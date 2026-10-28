using System.IO.Compression;
using System.Text.Json;
using Xunit;
using static Sbroenne.ExcelMcp.Tests.Infrastructure.PackagingScriptTestHelper;

namespace Sbroenne.ExcelMcp.Packaging.Tests;

[Collection("Sequential")]
[Trait("RequiresExcel", "false")]
[Trait("Category", "Integration")]
[Trait("Feature", "McpbPackaging")]
public sealed class McpbPackagingScriptTests
{
    private static readonly string[] BundleEntries = ["CHANGELOG.md", "LICENSE", "README.md", "icon-512.png", "manifest.json", "server/excel-mcp-server.exe"];

    // The fork ships the self-contained server inside the bundle, so tests
    // inject the current host executable and use its real product version.
    private static (string Runtime, string Version) InjectedRuntime()
    {
        var runtime = Environment.ProcessPath!;
        var version = System.Diagnostics.FileVersionInfo.GetVersionInfo(runtime).ProductVersion!.Split('+')[0];
        return (runtime, version);
    }

    [Fact]
    public async Task Build_CreatesExecutableBundleWithForkMetadata()
    {
        var sandbox = CreateSandbox("mcpb");
        try
        {
            var bundle = StageMcpbInputs(sandbox);
            var (runtime, version) = InjectedRuntime();
            var result = await RunPowerShellFileAsync(Path.Combine(bundle, "Build-McpBundle.ps1"),
                ["-Version", version, "-McpRuntimeExecutable", runtime]);
            Assert.True(result.ExitCode == 0, result.CombinedOutput);
            await AssertExecutableBundleAsync(Path.Combine(bundle, "artifacts", $"excel-mcp-ja-{version}.mcpb"), version);
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Fact]
    public async Task AggregatePackaging_McpbCreatesExecutableBundleWithoutPublishingRuntime()
    {
        var sandbox = CreateSandbox("mcpb-aggregate");
        try
        {
            StageMcpbInputs(sandbox);
            var output = Path.Combine(sandbox, "artifacts", "packages");
            var (runtime, version) = InjectedRuntime();
            var result = await RunPowerShellFileAsync(Path.Combine(sandbox, "scripts", "Build-ReleasePackages.ps1"),
                ["-Components", "Mcpb", "-Version", version, "-McpRuntimeExecutable", runtime, "-OutputDirectory", output]);
            Assert.True(result.ExitCode == 0, result.CombinedOutput);
            await AssertExecutableBundleAsync(Path.Combine(output, "mcpb", $"excel-mcp-ja-{version}.mcpb"), version);
            Assert.False(Directory.Exists(Path.Combine(output, "runtimes")));
            Assert.False(Directory.Exists(Path.Combine(output, "nuget")));
            Assert.False(Directory.Exists(Path.Combine(output, "npm")));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData("excel-cli", "wrong-version")]
    [InlineData("excel-cli", "")]
    [InlineData("excel-cli-report-formatting", "wrong-version")]
    [InlineData("excel-cli-report-formatting", "")]
    [InlineData("excel-mcp-report-formatting", "wrong-version")]
    [InlineData("excel-mcp-report-formatting", "")]
    public async Task AggregatePackaging_RejectsMismatchedPreparedSkillsBeforeCreatingOutput(string invalidSkill, string stamp)
    {
        var sandbox = CreateSandbox("prepared-skill-version");
        try
        {
            StageMcpbInputs(sandbox);
            var skills = Path.Combine(sandbox, "prepared");
            foreach (var name in new[] { "excel-cli", "excel-cli-report-formatting", "excel-mcp-report-formatting" })
            {
                var directory = Directory.CreateDirectory(Path.Combine(skills, name)).FullName;
                var version = name == invalidSkill ? stamp : "1.2.3";
                if (version.Length > 0) { File.WriteAllText(Path.Combine(directory, "VERSION"), version); }
            }
            var output = Path.Combine(sandbox, "artifacts", "packages");
            var result = await RunPowerShellFileAsync(Path.Combine(sandbox, "scripts", "Build-ReleasePackages.ps1"),
                ["-Components", "Plugins", "-Version", "1.2.3", "-SkillsDirectory", skills, "-OutputDirectory", output]);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("skill must match package version", result.CombinedOutput, StringComparison.Ordinal);
            Assert.Contains($"Prepared {invalidSkill} skill", result.CombinedOutput, StringComparison.Ordinal);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Fact]
    public async Task FailedBuild_PreservesExistingOutput()
    {
        var sandbox = CreateSandbox("mcpb-preserved-output");
        try
        {
            var bundle = StageMcpbInputs(sandbox);
            var output = Directory.CreateDirectory(Path.Combine(bundle, "artifacts")).FullName;
            var (runtime, version) = InjectedRuntime();
            var previous = Path.Combine(output, $"excel-mcp-ja-{version}.mcpb");
            var unrelated = Path.Combine(output, "keep.txt");
            File.WriteAllText(previous, "previous-good-package");
            File.WriteAllText(unrelated, "unrelated");
            File.Delete(Path.Combine(bundle, "icon-512.png"));
            var result = await RunPowerShellFileAsync(Path.Combine(bundle, "Build-McpBundle.ps1"),
                ["-Version", version, "-McpRuntimeExecutable", runtime, "-OutputDir", output]);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("icon-512.png", result.CombinedOutput, StringComparison.Ordinal);
            Assert.Equal("previous-good-package", File.ReadAllText(previous));
            Assert.Equal("unrelated", File.ReadAllText(unrelated));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData("src/output")]
    [InlineData("skills/generated")]
    [InlineData(".github/plugins/output")]
    public async Task PackageOutputs_RejectSourceDestinations(string relativePath)
    {
        var sandbox = CreateSandbox("package-source-destination");
        try
        {
            var output = Path.Combine(sandbox, relativePath);
            var result = await RunPowerShellCommandAsync($"""
                . '{Path.Combine(RepoRoot, "scripts", "PackageHelpers.ps1")}'
                Assert-PackageOutputPath '{output}' '{sandbox}'
                """);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Unsafe package output", result.CombinedOutput, StringComparison.Ordinal);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Fact]
    public async Task PublicationSync_RejectsItsOwnSourceTreeBeforeInspectingPayload()
    {
        var sandbox = CreateSandbox("publication-source-tree");
        try
        {
            var scripts = StagePublicationScripts(sandbox);
            var built = Directory.CreateDirectory(Path.Combine(sandbox, "built")).FullName;
            var result = await RunPowerShellFileAsync(Path.Combine(scripts, "Sync-PublishedPluginRepo.ps1"),
                ["-PublishedRepoDir", sandbox, "-BuiltPluginsDir", built, "-Version", "1.2.3"]);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Unsafe package output", result.Stderr, StringComparison.Ordinal);
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData("version.txt")]
    [InlineData("skills/excel-cli-report-formatting/SKILL.md")]
    public async Task PublicationSync_IncompletePayloadCannotReplaceExistingOutput(string missingFile)
    {
        var sandbox = CreateSandbox("publication-incomplete-payload");
        try
        {
            var scripts = StagePublicationScripts(sandbox);
            var overlay = Directory.CreateDirectory(Path.Combine(sandbox, ".github", "plugins", "marketplace-repo")).FullName;
            File.WriteAllText(Path.Combine(overlay, "README.md"), "new overlay");
            var built = Path.Combine(sandbox, "artifacts", "built");
            foreach (var name in new[] { "excel-cli", "excel-mcp" })
            {
                var plugin = Directory.CreateDirectory(Path.Combine(built, name)).FullName;
                var manifest = File.ReadAllText(Path.Combine(RepoRoot, ".github", "plugins", name, "plugin.json"));
                File.WriteAllText(Path.Combine(plugin, "plugin.json"), manifest.Replace("0.0.0", "1.2.3", StringComparison.Ordinal));
                foreach (var file in new[] {
                    "README.md", "version.txt", $"skills/{name}-report-formatting/SKILL.md",
                    $"skills/{name}-report-formatting/VERSION", $"skills/{name}-report-formatting/references/report-formatting.md"
                })
                {
                    var destination = Path.Combine(plugin, file);
                    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
                    File.WriteAllText(destination, "1.2.3");
                }
                if (name == "excel-cli")
                {
                    var wrapper = Path.Combine(plugin, "bin", "start-cli.ps1");
                    Directory.CreateDirectory(Path.GetDirectoryName(wrapper)!);
                    File.WriteAllText(wrapper, "1.2.3");
                }
                else { File.Copy(Path.Combine(RepoRoot, ".github", "plugins", "excel-mcp", "mcp.json"), Path.Combine(plugin, "mcp.json")); }
            }
            File.Delete(Path.Combine(built, "excel-cli", missingFile));
            var output = Directory.CreateDirectory(Path.Combine(sandbox, "artifacts", "published")).FullName;
            File.WriteAllText(Path.Combine(output, "README.md"), "previous output");
            var result = await RunPowerShellFileAsync(Path.Combine(scripts, "Sync-PublishedPluginRepo.ps1"),
                ["-PublishedRepoDir", output, "-BuiltPluginsDir", built, "-Version", "1.2.3"]);
            Assert.NotEqual(0, result.ExitCode);
            Assert.Contains("Incomplete plugin payload", result.Stderr, StringComparison.Ordinal);
            Assert.Equal("previous output", File.ReadAllText(Path.Combine(output, "README.md")));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    private static string StagePublicationScripts(string sandbox)
    {
        var scripts = Directory.CreateDirectory(Path.Combine(sandbox, "scripts")).FullName;
        foreach (var name in new[] { "Sync-PublishedPluginRepo.ps1", "PackageHelpers.ps1" })
        {
            File.Copy(Path.Combine(RepoRoot, "scripts", name), Path.Combine(scripts, name));
        }
        return scripts;
    }

    private static string StageMcpbInputs(string sandbox)
    {
        var bundle = Directory.CreateDirectory(Path.Combine(sandbox, "mcpb")).FullName;
        var scripts = Directory.CreateDirectory(Path.Combine(sandbox, "scripts")).FullName;
        foreach (var name in new[] { "Build-ReleasePackages.ps1", "PackageHelpers.ps1", "ExtensionPackaging.ps1" })
        {
            File.Copy(Path.Combine(RepoRoot, "scripts", name), Path.Combine(scripts, name));
        }
        foreach (var name in new[] { "Build-McpBundle.ps1", "manifest.json", "README.md", "icon-512.png" })
        {
            File.Copy(Path.Combine(RepoRoot, "mcpb", name), Path.Combine(bundle, name));
        }
        foreach (var name in new[] { "LICENSE", "CHANGELOG.md" }) { File.Copy(Path.Combine(RepoRoot, name), Path.Combine(sandbox, name)); }
        return bundle;
    }

    private static async Task AssertExecutableBundleAsync(string path, string version)
    {
        using var archive = ZipFile.OpenRead(path);
        foreach (var name in BundleEntries)
        {
            Assert.NotNull(archive.GetEntry(name));
        }
        var entry = archive.GetEntry("manifest.json");
        Assert.NotNull(entry);
        using var stream = entry.Open();
        using var manifest = await JsonDocument.ParseAsync(stream);
        Assert.Equal("excel-mcp-ja", manifest.RootElement.GetProperty("name").GetString());
        Assert.Equal(version, manifest.RootElement.GetProperty("version").GetString());
        var server = manifest.RootElement.GetProperty("server");
        Assert.Equal("binary", server.GetProperty("type").GetString());
        Assert.Equal("server/excel-mcp-server", server.GetProperty("entry_point").GetString());
        var config = server.GetProperty("mcp_config");
        Assert.Equal("${__dirname}/server/excel-mcp-server", config.GetProperty("command").GetString());
        Assert.Empty(config.GetProperty("args").EnumerateArray());
        Assert.Empty(config.GetProperty("env").EnumerateObject());
    }
}
