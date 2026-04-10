// Copyright (c) DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.BuildMark.Cli;
using DemaConsulting.BuildMark.Configuration;

namespace DemaConsulting.BuildMark.Tests.Configuration;

/// <summary>
///     Subsystem-level tests for the Configuration subsystem.
/// </summary>
[TestClass]
public class ConfigurationSubsystemTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Configuration-Read
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Configuration subsystem reads a valid .buildmark.yaml file and returns a populated result.
    /// </summary>
    [TestMethod]
    public async Task Configuration_ReadAsync_ValidFile_ReturnsConfiguration()
    {
        // Arrange: create a temporary directory with a valid .buildmark.yaml file
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: github
              github:
                owner: test-owner
                repo: test-repo
            sections:
              - id: changes
                title: Changes
            rules:
              - match:
                  label: [feature]
                route: changes
            """);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: configuration is returned with expected structure
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("github", result.Config.Connector?.Type);
            Assert.AreEqual("test-owner", result.Config.Connector?.GitHub?.Owner);
            Assert.AreEqual("test-repo", result.Config.Connector?.GitHub?.Repo);
            Assert.HasCount(1, result.Config.Sections);
            Assert.HasCount(1, result.Config.Rules);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that the Configuration subsystem returns an empty result when the file is missing.
    /// </summary>
    [TestMethod]
    public async Task Configuration_ReadAsync_MissingFile_ReturnsEmptyResult()
    {
        // Arrange: create a temporary directory with no .buildmark.yaml file
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);

        try
        {
            // Act: read configuration from a directory without the file
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: result has null Config and no errors
            Assert.IsNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.IsEmpty(result.Issues);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that the Configuration subsystem reports errors for a malformed .buildmark.yaml file.
    /// </summary>
    [TestMethod]
    public async Task Configuration_ReadAsync_MalformedFile_ReportsError()
    {
        // Arrange: create a temporary directory with a malformed .buildmark.yaml file
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            "connector:\n\ttype: github\n");

        try
        {
            // Act: read the malformed configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: result contains errors and Config is null
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.IsNotEmpty(result.Issues);
            Assert.AreEqual(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Configuration-Issues
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Configuration subsystem sets the context exit code when an error issue is reported.
    /// </summary>
    [TestMethod]
    public void Configuration_Issues_ErrorIssue_SetsExitCode()
    {
        // Arrange: create a context and a result with an error issue
        using var context = Context.Create(["--silent"]);
        var result = new ConfigurationLoadResult(
            null,
            [
                new ConfigurationIssue(
                    "/tmp/.buildmark.yaml",
                    4,
                    ConfigurationIssueSeverity.Error,
                    "Subsystem test parse error")
            ]);

        // Act: report the issues to the context
        result.ReportTo(context);

        // Assert: exit code is set to 1 due to the error issue
        Assert.AreEqual(1, context.ExitCode);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Configuration-ConnectorConfig
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Configuration subsystem parses connector-specific settings from a valid file.
    /// </summary>
    [TestMethod]
    public async Task Configuration_ConnectorConfig_ValidFile_ParsesConnectorSettings()
    {
        // Arrange: create a temporary directory with a valid .buildmark.yaml containing connector settings
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: github
              github:
                owner: acme-org
                repo: my-project
                base-url: https://api.github.example.com
            """);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: connector settings are parsed correctly
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("github", result.Config.Connector?.Type);
            Assert.AreEqual("acme-org", result.Config.Connector?.GitHub?.Owner);
            Assert.AreEqual("my-project", result.Config.Connector?.GitHub?.Repo);
            Assert.AreEqual("https://api.github.example.com", result.Config.Connector?.GitHub?.BaseUrl);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }
}



