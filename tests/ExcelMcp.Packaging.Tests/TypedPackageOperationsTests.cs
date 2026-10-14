using System.Diagnostics;
using System.IO.Compression;
using Sbroenne.ExcelMcp.Build;
using Xunit;

namespace Sbroenne.ExcelMcp.Packaging.Tests;

[Collection("Sequential")]
[Trait("RequiresExcel", "false")]
[Trait("Category", "Integration")]
[Trait("Feature", "Packaging")]
public sealed class TypedPackageOperationsTests
{
    [Theory]
    [InlineData(false, null, false)]
    [InlineData(true, null, false)]
    [InlineData(false, "AGENTS.md", false)]
    [InlineData(false, "CLAUDE.md", false)]
    [InlineData(false, null, true)]
    [InlineData(true, null, true)]
    public async Task AggregatePackaging_VsixTargetsRequireMatchingBundledRuntime(
        bool corruptX64Payload, string? developerFile, bool skipExtensionTests)
    {
        var sandbox = NewSandbox();
        try
        {
            var version = FileVersionInfo.GetVersionInfo(typeof(TypedPackageOperationsTests).Assembly.Location).ProductVersion!.Split('+')[0];
            var payload = File.ReadAllBytes(typeof(TypedPackageOperationsTests).Assembly.Location);
            var offset = BitConverter.ToInt32(payload, 0x3c);
            var extension = Directory.CreateDirectory(Path.Combine(sandbox, "vscode-extension")).FullName;
            if (developerFile is not null) { File.WriteAllText(Path.Combine(extension, developerFile), "Developer-only instructions"); }
            File.WriteAllText(Path.Combine(extension, "package.json"), $$$"""
                {"version":"{{{version}}}","extensionKind":["ui"],"os":["win32"],"scripts":{"vscode:prepublish":"npm run compile"}}
                """);
            File.WriteAllText(Path.Combine(sandbox, "CHANGELOG.md"), "Fixture changelog");
            var skills = Path.Combine(sandbox, "prepared");
            var skill = Directory.CreateDirectory(Path.Combine(skills, "excel-mcp-report-formatting")).FullName;
            File.WriteAllText(Path.Combine(skill, "VERSION"), version);
            File.WriteAllText(Path.Combine(skill, "SKILL.md"), "Fixture skill");
            // Fork: the VSIX also bundles the source-managed Japanese skill.
            var jaSkill = Directory.CreateDirectory(Path.Combine(sandbox, "skills", "excel-mcp-ja")).FullName;
            File.WriteAllText(Path.Combine(jaSkill, "SKILL.md"), "Fixture skill");
            // Fork: only the x64 runtime is bundled; a corrupted payload must be rejected.
            var machine = corruptX64Payload ? (ushort)0xaa64 : (ushort)0x8664;
            var runtimeDir = Directory.CreateDirectory(Path.Combine(sandbox, "runtimes", "x64")).FullName;
            BitConverter.GetBytes(machine).CopyTo(payload, offset + 4);
            var runtime = Path.Combine(runtimeDir, "Sbroenne.ExcelMcp.McpServer.exe");
            File.WriteAllBytes(runtime, payload);
            var prepared = new Dictionary<string, string>(StringComparer.Ordinal) { ["Mcp"] = runtime };
            var output = Directory.CreateDirectory(Path.Combine(sandbox, "artifacts", "packages")).FullName;
            var calls = new List<PackageCommand>();
            var commands = new PackageCommands(call =>
            {
                calls.Add(call);
                Assert.Equal("npm", call.Executable);
                if (call.Arguments is ["run", "compile"])
                {
                    var compiled = Directory.CreateDirectory(Path.Combine(call.WorkingDirectory, "out")).FullName;
                    File.WriteAllText(Path.Combine(compiled, "extension.js"), "fixture");
                    File.WriteAllText(Path.Combine(compiled, "prerequisites.js"), "fixture");
                }
                if (call.Arguments[0] == "exec")
                {
                    var target = call.Arguments[Array.IndexOf(call.Arguments, "--target") + 1];
                    var destination = call.Arguments[Array.IndexOf(call.Arguments, "--out") + 1];
                    using var archive = ZipFile.Open(destination, ZipArchiveMode.Create);
                    foreach (var file in Directory.EnumerateFiles(call.WorkingDirectory, "*", SearchOption.AllDirectories))
                    {
                        var relative = Path.GetRelativePath(call.WorkingDirectory, file).Replace('\\', '/');
                        archive.CreateEntryFromFile(file, $"extension/{relative}");
                    }
                    using var writer = new StreamWriter(archive.CreateEntry("extension.vsixmanifest").Open());
                    writer.Write($"<PackageManifest><Metadata><Identity TargetPlatform='{target}'/></Metadata></PackageManifest>");
                }
                return Task.FromResult(new ProcessResult(0, "", ""));
            });
            var failure = await Record.ExceptionAsync(() => new PackageExecution(sandbox, commands).ExtensionAsync(version, skills, output, prepared, skipExtensionTests));
            Assert.Contains(calls, call => call.Arguments.SequenceEqual(["ci", "--ignore-scripts"]));
            Assert.Contains(calls, call => call.Arguments.SequenceEqual(["run", "compile"]));
            Assert.Contains(calls, call => call.Arguments.SequenceEqual(["run", "lint"]));
            Assert.Equal(!skipExtensionTests, calls.Any(call => call.Arguments.SequenceEqual(["run", "typecheck:tests"])));
            Assert.Equal(!skipExtensionTests, calls.Any(call => call.Arguments.SequenceEqual(["test"])));
            if (developerFile is not null)
            {
                Assert.NotNull(failure);
                Assert.Contains($"VSIX contains development files or the CLI: extension/{developerFile}", failure.Message, StringComparison.Ordinal);
            }
            else if (corruptX64Payload)
            {
                Assert.NotNull(failure);
                Assert.Contains("does not match x64", failure.Message, StringComparison.Ordinal);
            }
            else
            {
                Assert.Null(failure);
                // Fork contract: a single x64 VSIX under the excelmcp-ja name.
                using var archive = ZipFile.OpenRead(Path.Combine(output, $"excelmcp-ja-{version}.vsix"));
                var entry = archive.GetEntry("extension/bin/Sbroenne.ExcelMcp.McpServer.exe");
                Assert.NotNull(entry);
                using var reader = new BinaryReader(entry.Open());
                var expected = File.ReadAllBytes(runtime);
                var actual = reader.ReadBytes(expected.Length);
                Assert.Equal((ushort)0x8664, BitConverter.ToUInt16(actual, offset + 4));
                Assert.Equal(expected, actual);
                foreach (var skillName in new[] { "excel-mcp-ja", "excel-mcp-report-formatting" })
                {
                    Assert.NotNull(archive.GetEntry($"extension/skills/{skillName}/SKILL.md"));
                    Assert.NotNull(archive.GetEntry($"extension/skills/{skillName}/VERSION"));
                }
                var debug = File.ReadAllBytes(Path.Combine(output, "extension", "bin", "Sbroenne.ExcelMcp.McpServer.exe"));
                Assert.Equal((ushort)0x8664, BitConverter.ToUInt16(debug, offset + 4));
            }
            Assert.Empty(Directory.GetFiles(output, "*-server-inspection.exe"));
        }
        finally { Directory.Delete(sandbox, recursive: true); }
    }

    [Fact]
    public void InstallPackageOutput_WhenInstallAndRestoreFail_PreservesBothErrorsAndRecoveryBackup()
    {
        var sandbox = NewSandbox();
        try
        {
            var source = Directory.CreateDirectory(Path.Combine(sandbox, "source")).FullName;
            var destination = Directory.CreateDirectory(Path.Combine(sandbox, "destination")).FullName;
            File.WriteAllText(Path.Combine(source, "payload.txt"), "replacement");
            File.WriteAllText(Path.Combine(destination, "payload.txt"), "previous");
            var attempts = 0;
            var warnings = new List<string>();
            var error = Assert.Throws<AggregateException>(() => PackageFiles.Install(source, destination,
                move: (from, to) =>
                {
                    attempts++;
                    if (attempts == 2) { throw new IOException("installation root cause"); }
                    if (attempts == 3) { throw new IOException("restore root cause"); }
                    Directory.Move(from, to);
                },
                delete: path =>
                {
                    if (path.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase)) { throw new IOException("temporary cleanup root cause"); }
                    Directory.Delete(path, recursive: true);
                },
                warning: warnings.Add));
            var messages = error.Message + "\n" + string.Join('\n', warnings);
            Assert.Contains("installation root cause", messages, StringComparison.Ordinal);
            Assert.Contains("restore root cause", messages, StringComparison.Ordinal);
            Assert.Contains("Recovery backup retained at", messages, StringComparison.Ordinal);
            Assert.Contains("temporary cleanup root cause", messages, StringComparison.Ordinal);
            Assert.Contains("Temporary output retained at", messages, StringComparison.Ordinal);
            Assert.False(Directory.Exists(destination));
            var backup = Assert.Single(Directory.GetDirectories(sandbox, "*.bak"));
            Assert.Equal("previous", File.ReadAllText(Path.Combine(backup, "payload.txt")));
            Assert.Contains(backup, messages, StringComparison.OrdinalIgnoreCase);
            var temporary = Assert.Single(Directory.GetDirectories(sandbox, "*.tmp"));
            Assert.Equal("replacement", File.ReadAllText(Path.Combine(temporary, "payload.txt")));
            Assert.Contains(temporary, messages, StringComparison.OrdinalIgnoreCase);
        }
        finally { Directory.Delete(sandbox, recursive: true); }
    }

    [Fact]
    public void InstallPackageOutput_WhenBackupCleanupFails_ReportsSuccessAndRetainedBackup()
    {
        var sandbox = NewSandbox();
        try
        {
            var source = Directory.CreateDirectory(Path.Combine(sandbox, "source")).FullName;
            var destination = Directory.CreateDirectory(Path.Combine(sandbox, "destination")).FullName;
            File.WriteAllText(Path.Combine(source, "payload.txt"), "replacement");
            File.WriteAllText(Path.Combine(destination, "payload.txt"), "previous");
            var warnings = new List<string>();
            PackageFiles.Install(source, destination, delete: path =>
            {
                if (path.EndsWith(".bak", StringComparison.OrdinalIgnoreCase)) { throw new IOException("backup cleanup root cause"); }
                Directory.Delete(path, recursive: true);
            }, warning: warnings.Add);
            var messages = string.Join('\n', warnings);
            Assert.Contains("installed successfully", messages, StringComparison.Ordinal);
            Assert.Contains("backup cleanup root cause", messages, StringComparison.Ordinal);
            Assert.Contains("Backup retained at", messages, StringComparison.Ordinal);
            Assert.Equal("replacement", File.ReadAllText(Path.Combine(destination, "payload.txt")));
            var backup = Assert.Single(Directory.GetDirectories(sandbox, "*.bak"));
            Assert.Equal("previous", File.ReadAllText(Path.Combine(backup, "payload.txt")));
            Assert.Contains(backup, messages, StringComparison.OrdinalIgnoreCase);
        }
        finally { Directory.Delete(sandbox, recursive: true); }
    }

    [Fact]
    [Trait("Feature", "McpbPackaging")]
    public void RemoveStagingDirectory_RetriesTransientFileLockUntilDirectoryIsGone()
    {
        var sandbox = NewSandbox();
        try
        {
            var attempts = 0;
            PackageFiles.RemoveStaging(sandbox, TimeSpan.FromSeconds(1), TimeSpan.Zero, path =>
            {
                attempts++;
                if (attempts < 3) { throw new IOException("locked by scanner"); }
                Directory.Delete(path, recursive: true);
            });
            Assert.Equal(3, attempts);
            Assert.False(Directory.Exists(sandbox));
        }
        finally { if (Directory.Exists(sandbox)) { Directory.Delete(sandbox, recursive: true); } }
    }

    [Fact]
    [Trait("Feature", "McpbPackaging")]
    public void RemoveStagingDirectory_WhenTimeoutExpires_ReportsTerminalState()
    {
        var sandbox = NewSandbox();
        try
        {
            var error = Assert.Throws<IOException>(() => PackageFiles.RemoveStaging(sandbox, TimeSpan.Zero, TimeSpan.Zero,
                _ => throw new UnauthorizedAccessException("locked by scanner")));
            Assert.True(Directory.Exists(sandbox));
            foreach (var text in new[] { "Failed to remove MCPB staging directory", sandbox, "after 1 attempt", "within 0 ms", "stale staging remains", "locked by scanner" })
            {
                Assert.Contains(text, error.Message, StringComparison.Ordinal);
            }
        }
        finally { Directory.Delete(sandbox, recursive: true); }
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [Trait("Feature", "McpbPackaging")]
    public void RemoveStagingDirectory_WhenTimeoutOrRetryIntervalIsNegative_UsesMatchingArgumentName(int timeoutMs, int retryMs)
    {
        var sandbox = NewSandbox();
        try
        {
            var timeout = TimeSpan.FromMilliseconds(timeoutMs);
            var retryInterval = TimeSpan.FromMilliseconds(retryMs);
            var error = Assert.ThrowsAny<ArgumentOutOfRangeException>(() => PackageFiles.RemoveStaging(sandbox, timeout, retryInterval));
            var expectedName = timeoutMs < 0 ? nameof(timeout) : nameof(retryInterval);
            Assert.Equal(expectedName, error.ParamName);
        }
        finally { if (Directory.Exists(sandbox)) { Directory.Delete(sandbox, recursive: true); } }
    }

    private static string NewSandbox() => Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), $"ExcelMcp.TypedPackages.{Guid.NewGuid():N}")).FullName;
}
