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
public class ConfigurationSubsystemTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Configuration-Read
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Configuration subsystem reads a valid .buildmark.yaml file and returns a populated result.
    /// </summary>
    [Fact]
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
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: configuration is returned with expected structure
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("github", result.Config.Connector?.Type);
            Assert.Equal("test-owner", result.Config.Connector?.GitHub?.Owner);
            Assert.Equal("test-repo", result.Config.Connector?.GitHub?.Repo);
            Assert.Single(result.Config.Sections);
            Assert.Single(result.Config.Rules);
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
    [Fact]
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
            Assert.Null(result.Config);
            Assert.False(result.HasErrors);
            Assert.Empty(result.Issues);
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
    [Fact]
    public async Task Configuration_ReadAsync_MalformedFile_ReportsError()
    {
        // Arrange: create a temporary directory with a malformed .buildmark.yaml file
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            "connector:\n\ttype: github\n",
            TestContext.Current.CancellationToken);

        try
        {
            // Act: read the malformed configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: result contains errors and Config is null
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.NotEmpty(result.Issues);
            Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
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
    [Fact]
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
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Test that the Configuration subsystem does not set the context exit code when only a warning issue is reported.
    /// </summary>
    [Fact]
    public void Configuration_Issues_WarningIssue_DoesNotSetExitCode()
    {
        // Arrange: create a context and a result with a warning-only issue
        using var context = Context.Create(["--silent"]);
        var result = new ConfigurationLoadResult(
            null,
            [
                new ConfigurationIssue(
                    "/tmp/.buildmark.yaml",
                    2,
                    ConfigurationIssueSeverity.Warning,
                    "Subsystem test warning")
            ]);

        // Act: report the issues to the context
        result.ReportTo(context);

        // Assert: exit code remains 0 for warning-only issues
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that the Configuration subsystem reports accurate 1-based line numbers for validation errors.
    /// </summary>
    [Fact]
    public async Task Configuration_Issues_ValidationError_ReportsAccurateLine()
    {
        // Arrange: create a YAML file where the unsupported key is on a known line number
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: github
              unsupported-key: value
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: the error is reported with the correct line number (3)
            Assert.NotNull(result.Issues);
            Assert.True(result.HasErrors);
            var issue = result.Issues[0];
            Assert.True(issue.Line == 3, $"Expected line 3 for 'unsupported-key' but got {issue.Line}");
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Configuration-ConnectorConfig
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Configuration subsystem parses connector-specific settings from a valid file.
    /// </summary>
    [Fact]
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
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: connector settings are parsed correctly
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("github", result.Config.Connector?.Type);
            Assert.Equal("acme-org", result.Config.Connector?.GitHub?.Owner);
            Assert.Equal("my-project", result.Config.Connector?.GitHub?.Repo);
            Assert.Equal("https://api.github.example.com", result.Config.Connector?.GitHub?.BaseUrl);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that the Configuration subsystem parses Azure DevOps connector settings from a valid file.
    /// </summary>
    [Fact]
    public async Task Configuration_ConnectorConfig_ValidFile_ParsesAzureDevOpsSettings()
    {
        // Arrange: create a temporary directory with a valid .buildmark.yaml containing Azure DevOps settings
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: azure-devops
              azure-devops:
                url: https://dev.azure.com/acme
                organization: acme
                project: my-project
                repository: my-repo
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act: read the configuration
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert: Azure DevOps connector settings are parsed correctly
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("azure-devops", result.Config.Connector?.Type);
            Assert.Equal("https://dev.azure.com/acme", result.Config.Connector?.AzureDevOps?.OrganizationUrl);
            Assert.Equal("acme", result.Config.Connector?.AzureDevOps?.Organization);
            Assert.Equal("my-project", result.Config.Connector?.AzureDevOps?.Project);
            Assert.Equal("my-repo", result.Config.Connector?.AzureDevOps?.Repository);
        }
        finally
        {
            // Cleanup temporary directory
            Directory.Delete(directory, recursive: true);
        }
    }
}
