using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using Sbroenne.ExcelMcp.Build;
using Xunit;
using static Sbroenne.ExcelMcp.Tests.Infrastructure.PackagingScriptTestHelper;

namespace Sbroenne.ExcelMcp.Packaging.Tests;

[Collection("Sequential")]
[Trait("RequiresExcel", "false")]
[Trait("Category", "Integration")]
[Trait("Feature", "McpbPackaging")]
public sealed class McpbPackagingScriptTests
{
    // Fork: the bundle additionally embeds the compiled server executable.
    private static readonly string[] BundleEntries =
        ["CHANGELOG.md", "LICENSE", "README.md", "icon-512.png", "manifest.json", "server/excel-mcp-server.exe"];

    [Fact]
    public async Task Build_CreatesExecutableBundleWithForkMetadata()
    {
        var sandbox = CreateSandbox("mcpb");
        try
        {
            var bundle = StageMcpbInputs(sandbox);
            var result = await RunPowerShellFileAsync(Path.Combine(bundle, "Build-McpBundle.ps1"),
                ["-Version", "1.2.3", "-McpRuntimeExecutable", CurrentProcessExe()]);
            Assert.True(result.ExitCode == 0, result.CombinedOutput);
            await AssertForkExeBundleAsync(Path.Combine(bundle, "artifacts", "excel-mcp-ja-1.2.3.mcpb"), "1.2.3");
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Fact]
    public async Task AggregatePackaging_McpbCreatesExecutableBundle()
    {
        var sandbox = CreateSandbox("mcpb-aggregate");
        try
        {
            StageMcpbInputs(sandbox);
            var output = Path.Combine(sandbox, "artifacts", "packages");
            var runtime = StageVersionedX64Runtime(sandbox, out var version);
            var commands = new PackageCommands(_ => Task.FromResult(new Sbroenne.ExcelMcp.Build.ProcessResult(0, "", "")));
            await new PackageExecution(sandbox, commands).ReleaseAsync(new PackageOptions
            {
                Components = ["Mcpb"],
                Version = version,
                McpRuntimeExecutable = runtime,
                OutputDirectory = output
            });
            await AssertForkExeBundleAsync(Path.Combine(output, "mcpb", $"excel-mcp-ja-{version}.mcpb"), version);
            Assert.False(Directory.Exists(Path.Combine(output, "nuget")));
            Assert.False(Directory.Exists(Path.Combine(output, "npm")));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("AGENTS.md")]
    [InlineData("CLAUDE.md")]
    public async Task AggregatePackaging_VsixTargetsRequireMatchingBundledRuntime(string? developerFile)
    {
        var sandbox = CreateSandbox("vsix-runtime");
        try
        {
            // Use a versioned PE fixture so the extension package exercises the real version check.
            var runtime = StageVersionedX64Runtime(sandbox, out var version);
            var extension = Directory.CreateDirectory(Path.Combine(sandbox, "vscode-extension")).FullName;
            if (developerFile is not null)
            {
                File.WriteAllText(Path.Combine(extension, developerFile), "Developer-only instructions");
            }
            File.WriteAllText(Path.Combine(extension, "package.json"), $$$"""
                {"version":"{{{version}}}","extensionKind":["ui"],"os":["win32"],"scripts":{"vscode:prepublish":"npm run compile"}}
                """);
            File.WriteAllText(Path.Combine(sandbox, "CHANGELOG.md"), "Fixture changelog");
            var skills = Directory.CreateDirectory(Path.Combine(sandbox, "prepared", "excel-mcp-report-formatting")).FullName;
            File.WriteAllText(Path.Combine(skills, "VERSION"), version);
            File.WriteAllText(Path.Combine(skills, "SKILL.md"), "Fixture skill");
            // The fork bundles the source-managed Japanese skill from skills/ (not from prepared output).
            var jaSkill = Directory.CreateDirectory(Path.Combine(sandbox, "skills", "excel-mcp-ja")).FullName;
            File.WriteAllText(Path.Combine(jaSkill, "SKILL.md"), "Fixture skill");
            var output = Directory.CreateDirectory(Path.Combine(sandbox, "artifacts", "packages")).FullName;

            var commands = new PackageCommands(command => Task.FromResult(FakeExtensionCommand(command)));
            var prepared = new Dictionary<string, string>(StringComparer.Ordinal) { ["Mcp"] = runtime };
            var act = () => new PackageExecution(sandbox, commands)
                .ExtensionAsync(version, Path.Combine(sandbox, "prepared"), output, prepared, skipTests: true);

            if (developerFile is not null)
            {
                var error = await Assert.ThrowsAsync<InvalidOperationException>(act);
                Assert.Equal($"VSIX contains development files or the CLI: extension/{developerFile}", error.Message);
                return;
            }

            await act();
            // Fork contract: a single x64 VSIX named like the other fork artifacts.
            using var archive = ZipFile.OpenRead(Path.Combine(output, $"excelmcp-ja-{version}.vsix"));
            var entry = archive.GetEntry("extension/bin/Sbroenne.ExcelMcp.McpServer.exe");
            Assert.NotNull(entry);
            Assert.Equal(File.ReadAllBytes(runtime), ReadAll(entry));
            foreach (var skillName in new[] { "excel-mcp-ja", "excel-mcp-report-formatting" })
            {
                Assert.NotNull(archive.GetEntry($"extension/skills/{skillName}/SKILL.md"));
                Assert.NotNull(archive.GetEntry($"extension/skills/{skillName}/VERSION"));
            }
            var debugRuntime = Path.Combine(output, "extension", "bin", "Sbroenne.ExcelMcp.McpServer.exe");
            Assert.Equal(File.ReadAllBytes(runtime), File.ReadAllBytes(debugRuntime));
            Assert.Empty(Directory.GetFiles(output, "*-server-inspection.exe"));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData("wrong-version")]
    [InlineData("")]
    public async Task AggregatePackaging_RejectsMismatchedPreparedSkillsBeforeCreatingOutput(string stamp)
    {
        var sandbox = CreateSandbox("prepared-skill-version");
        try
        {
            var skills = Path.Combine(sandbox, "prepared");
            foreach (var name in new[] { "excel-cli", "excel-mcp" })
            {
                var directory = Directory.CreateDirectory(Path.Combine(skills, name)).FullName;
                if (stamp.Length > 0) { File.WriteAllText(Path.Combine(directory, "VERSION"), stamp); }
            }
            var output = Path.Combine(sandbox, "artifacts", "packages");
            var commands = new PackageCommands(_ => throw new InvalidOperationException("Payload packaging was reached."));
            var error = await Assert.ThrowsAsync<InvalidOperationException>(() => new PackageExecution(sandbox, commands).ReleaseAsync(new PackageOptions
            {
                Components = ["Plugins"],
                Version = "1.2.3",
                SkillsDirectory = skills,
                OutputDirectory = output
            }));
            Assert.Contains("skill must match package version", error.Message, StringComparison.Ordinal);
            Assert.False(Directory.Exists(output));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Fact]
    public void FailedBuild_PreservesExistingOutput()
    {
        var sandbox = CreateSandbox("mcpb-preserved-output");
        try
        {
            var bundle = StageMcpbInputs(sandbox);
            var output = Directory.CreateDirectory(Path.Combine(bundle, "artifacts")).FullName;
            var previous = Path.Combine(output, "excel-mcp-ja-1.2.3.mcpb");
            var unrelated = Path.Combine(output, "keep.txt");
            File.WriteAllText(previous, "previous-good-package");
            File.WriteAllText(unrelated, "unrelated");
            var error = Assert.Throws<IOException>(() => new AuthoredPackages(sandbox, (_, _) =>
                throw new IOException("archive-root-cause")).Mcpb("1.2.3", output, CurrentProcessExe()));
            Assert.Contains("archive-root-cause", error.Message, StringComparison.Ordinal);
            Assert.Equal("previous-good-package", File.ReadAllText(previous));
            Assert.Equal("unrelated", File.ReadAllText(unrelated));
        }
        finally { DeleteDirectoryIfExists(sandbox); }
    }

    [Theory]
    [InlineData("src/output")]
    [InlineData("skills/generated")]
    [InlineData(".github/plugins/output")]
    public void PackageOutputs_RejectSourceDestinations(string relativePath)
    {
        var sandbox = CreateSandbox("package-source-destination");
        try
        {
            var output = Path.Combine(sandbox, relativePath);
            var error = Assert.Throws<ArgumentException>(() => PackageFiles.AssertOutput(output, sandbox));
            Assert.Contains("Unsafe package output", error.Message, StringComparison.Ordinal);
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
        foreach (var name in new[] { "Build-ReleasePackages.ps1", "PackageHelpers.ps1" })
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

    private static string CurrentProcessExe() => Environment.ProcessPath!;

    // Copies the test assembly as a self-contained x64 runtime fixture whose
    // ProductVersion matches, so the package version check exercises real data.
    private static string StageVersionedX64Runtime(string sandbox, out string version)
    {
        var assemblyPath = typeof(McpbPackagingScriptTests).Assembly.Location;
        var productVersion = FileVersionInfo.GetVersionInfo(assemblyPath).ProductVersion;
        Assert.NotNull(productVersion);
        version = productVersion.Split('+')[0];
        Assert.NotEmpty(version);
        var payload = File.ReadAllBytes(assemblyPath);
        var headerOffset = BitConverter.ToInt32(payload, 0x3c);
        BitConverter.GetBytes((ushort)0x8664).CopyTo(payload, headerOffset + 4);
        var runtime = Directory.CreateDirectory(Path.Combine(sandbox, "runtimes", "x64")).FullName;
        var executable = Path.Combine(runtime, "Sbroenne.ExcelMcp.McpServer.exe");
        File.WriteAllBytes(executable, payload);
        return executable;
    }

    // Simulates the npm/vsce steps inside the staged extension directory:
    // 'npm run compile' emits the out/ payload and 'vsce package' archives the
    // whole stage under extension/ plus the target's extension.vsixmanifest.
    private static Sbroenne.ExcelMcp.Build.ProcessResult FakeExtensionCommand(PackageCommand command)
    {
        var arguments = command.Arguments;
        if (command.Executable == "npm" && arguments is ["run", "compile"])
        {
            var outDirectory = Directory.CreateDirectory(Path.Combine(command.WorkingDirectory, "out")).FullName;
            File.WriteAllText(Path.Combine(outDirectory, "extension.js"), "fixture");
            File.WriteAllText(Path.Combine(outDirectory, "prerequisites.js"), "fixture");
            return new Sbroenne.ExcelMcp.Build.ProcessResult(0, "", "");
        }
        if (command.Executable == "npm" && arguments.Length > 2 && arguments[0] == "exec" && arguments.Contains("vsce"))
        {
            var target = arguments[Array.IndexOf(arguments, "--target") + 1];
            var destination = arguments[Array.IndexOf(arguments, "--out") + 1];
            Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
            using var archive = ZipFile.Open(destination, ZipArchiveMode.Create);
            foreach (var file in Directory.EnumerateFiles(command.WorkingDirectory, "*", SearchOption.AllDirectories))
            {
                var relative = Path.GetRelativePath(command.WorkingDirectory, file).Replace('\\', '/');
                archive.CreateEntryFromFile(file, $"extension/{relative}");
            }
            var manifest = archive.CreateEntry("extension.vsixmanifest");
            using (var writer = new StreamWriter(manifest.Open()))
            {
                writer.Write($"<PackageManifest><Metadata><Identity TargetPlatform='{target}'/></Metadata></PackageManifest>");
            }
            return new Sbroenne.ExcelMcp.Build.ProcessResult(0, "", "");
        }
        return new Sbroenne.ExcelMcp.Build.ProcessResult(0, "", "");
    }

    private static byte[] ReadAll(ZipArchiveEntry entry)
    {
        using var stream = entry.Open();
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    private static async Task AssertForkExeBundleAsync(string path, string version)
    {
        using var archive = ZipFile.OpenRead(path);
        Assert.Equal(BundleEntries,
            archive.Entries.Select(entry => entry.FullName).Order(StringComparer.Ordinal));
        var entry = archive.GetEntry("manifest.json");
        Assert.NotNull(entry);
        using var stream = entry.Open();
        using var manifest = await JsonDocument.ParseAsync(stream);
        Assert.Equal(version, manifest.RootElement.GetProperty("version").GetString());
        Assert.Equal("excel-mcp-ja", manifest.RootElement.GetProperty("name").GetString());
        var server = manifest.RootElement.GetProperty("server");
        Assert.Equal("binary", server.GetProperty("type").GetString());
        Assert.Equal("server/excel-mcp-server", server.GetProperty("entry_point").GetString());
        var config = server.GetProperty("mcp_config");
        Assert.Equal("${__dirname}/server/excel-mcp-server", config.GetProperty("command").GetString());
        Assert.Empty(config.GetProperty("args").EnumerateArray());
        Assert.Empty(config.GetProperty("env").EnumerateObject());
    }
}
