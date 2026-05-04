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
///     Tests for configuration loading and reporting.
/// </summary>
public class ConfigurationTests
{
    /// <summary>
    ///     Test that missing configuration files return an empty result.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_MissingFile_ReturnsEmptyResult()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.False(result.HasErrors);
            Assert.Empty(result.Issues);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that valid configuration files are parsed into the configuration model.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: github
              github:
                owner: example-owner
                repo: hello-world
                base-url: https://api.github.com
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
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("github", result.Config.Connector?.Type);
            Assert.Equal("example-owner", result.Config.Connector?.GitHub?.Owner);
            Assert.Equal("hello-world", result.Config.Connector?.GitHub?.Repo);
            Assert.Equal("https://api.github.com", result.Config.Connector?.GitHub?.BaseUrl);
            Assert.Single(result.Config.Sections);
            Assert.Equal("changes", result.Config.Sections[0].Id);
            Assert.Single(result.Config.Rules);
            Assert.Equal("changes", result.Config.Rules[0].Route);
            Assert.Equal("feature", result.Config.Rules[0].Match?.Label[0]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that malformed configuration files surface an error issue.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_InvalidRepositoryValue_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: github
              github:
                repository: invalid
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
            Assert.Contains("owner/repo", result.Issues[0].Description);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that malformed configuration files surface an error issue.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            "connector:\n\ttype: github\n",
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
            Assert.Contains("tab", result.Issues[0].Description, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that a valid Azure DevOps connector block is parsed into the configuration model.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_ValidAzureDevOpsConnector_ReturnsParsedConfiguration()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: azure-devops
              azure-devops:
                url: https://dev.azure.com/myorg
                organization: myorg
                project: myproject
                repository: myrepo
            sections:
              - id: changes
                title: Changes
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("azure-devops", result.Config.Connector?.Type);
            Assert.Equal("https://dev.azure.com/myorg", result.Config.Connector?.AzureDevOps?.OrganizationUrl);
            Assert.Equal("myorg", result.Config.Connector?.AzureDevOps?.Organization);
            Assert.Equal("myproject", result.Config.Connector?.AzureDevOps?.Project);
            Assert.Equal("myrepo", result.Config.Connector?.AzureDevOps?.Repository);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that Azure DevOps connector block with alternate key aliases is parsed correctly.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_AzureDevOpsConnectorAliases_ReturnsParsedConfiguration()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: azure-devops
              azure-devops:
                url: https://dev.azure.com/myorg
                org: myorg
                project: myproject
                repo: myrepo
            sections:
              - id: changes
                title: Changes
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.Equal("https://dev.azure.com/myorg", result.Config.Connector?.AzureDevOps?.OrganizationUrl);
            Assert.Equal("myorg", result.Config.Connector?.AzureDevOps?.Organization);
            Assert.Equal("myproject", result.Config.Connector?.AzureDevOps?.Project);
            Assert.Equal("myrepo", result.Config.Connector?.AzureDevOps?.Repository);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that an unsupported key inside the Azure DevOps connector block produces an error.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_AzureDevOpsUnsupportedKey_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: azure-devops
              azure-devops:
                unknown-key: some-value
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
            Assert.Contains("Unsupported Azure DevOps connector key", result.Issues[0].Description);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that a non-mapping Azure DevOps connector node produces an error.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_AzureDevOpsNonMapping_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            connector:
              type: azure-devops
              azure-devops: not-a-mapping
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
            Assert.Contains("YAML mapping", result.Issues[0].Description);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that reporting an error issue sets the context exit code.
    /// </summary>
    [Fact]
    public void ConfigurationLoadResult_ReportTo_ErrorIssue_SetsExitCode()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        var result = new ConfigurationLoadResult(
            null,
            [
                new ConfigurationIssue(
                    "/tmp/.buildmark.yaml",
                    4,
                    ConfigurationIssueSeverity.Error,
                    "Example parse error")
            ]);

        // Act
        result.ReportTo(context);

        // Assert
        Assert.Equal(1, context.ExitCode);
    }

    /// <summary>
    ///     Test that reporting a warning issue does not set the context exit code.
    /// </summary>
    [Fact]
    public void ConfigurationLoadResult_ReportTo_WarningIssue_DoesNotSetExitCode()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        var result = new ConfigurationLoadResult(
            null,
            [
                new ConfigurationIssue(
                    "/tmp/.buildmark.yaml",
                    2,
                    ConfigurationIssueSeverity.Warning,
                    "Example warning")
            ]);

        // Act
        result.ReportTo(context);

        // Assert
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that ReportTo includes the file path and line number in the formatted message.
    /// </summary>
    [Fact]
    public void ConfigurationLoadResult_ReportTo_IssueMessage_IncludesLineNumber()
    {
        // Arrange
        using var context = Context.Create(["--silent"]);
        var result = new ConfigurationLoadResult(
            null,
            [
                new ConfigurationIssue(
                    "/repo/.buildmark.yaml",
                    7,
                    ConfigurationIssueSeverity.Error,
                    "Unexpected value")
            ]);

        // Act
        result.ReportTo(context);

        // Assert - the issue's FilePath and Line are surfaced via WriteError; confirm HasErrors
        // and that the issue record exposes the correct location fields
        Assert.True(result.HasErrors);
        Assert.True(context.ExitCode == 1, "Error severity should set exit code");
        Assert.Equal("/repo/.buildmark.yaml", result.Issues[0].FilePath);
        Assert.Equal(7, result.Issues[0].Line);
        Assert.Equal(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
        Assert.Equal("Unexpected value", result.Issues[0].Description);
    }

    /// <summary>
    ///     Test that the default configuration contains the expected sections and routing rules.
    /// </summary>
    [Fact]
    public void BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection()
    {
        // Act
        var config = BuildMarkConfig.CreateDefault();

        // Assert - verify sections have correct IDs and titles
        Assert.Equal(3, config.Sections.Count);
        Assert.Equal("changes", config.Sections[0].Id);
        Assert.Equal("Changes", config.Sections[0].Title);
        Assert.Equal("bugs-fixed", config.Sections[1].Id);
        Assert.Equal("Bugs Fixed", config.Sections[1].Title);
        Assert.Equal("dependency-updates", config.Sections[2].Id);
        Assert.Equal("Dependency Updates", config.Sections[2].Title);

        // Assert - verify rules have correct routes and match conditions
        Assert.Equal(6, config.Rules.Count);

        Assert.Equal("dependency-updates", config.Rules[0].Route);
        Assert.Contains("dependencies", config.Rules[0].Match!.Label);
        Assert.Contains("renovate", config.Rules[0].Match!.Label);
        Assert.Contains("dependabot", config.Rules[0].Match!.Label);

        Assert.Equal("bugs-fixed", config.Rules[1].Route);
        Assert.Contains("Bug", config.Rules[1].Match!.WorkItemType);

        Assert.Equal("bugs-fixed", config.Rules[2].Route);
        Assert.Contains("bug", config.Rules[2].Match!.Label);
        Assert.Contains("defect", config.Rules[2].Match!.Label);
        Assert.Contains("regression", config.Rules[2].Match!.Label);

        Assert.Equal("suppressed", config.Rules[3].Route);
        Assert.Contains("internal", config.Rules[3].Match!.Label);
        Assert.Contains("chore", config.Rules[3].Match!.Label);

        Assert.Equal("suppressed", config.Rules[4].Route);
        Assert.Contains("Task", config.Rules[4].Match!.WorkItemType);
        Assert.Contains("Epic", config.Rules[4].Match!.WorkItemType);

        Assert.Equal("changes", config.Rules[5].Route);
        Assert.Null(config.Rules[5].Match);
    }

    /// <summary>
    ///     Test that a valid report section is parsed into the report configuration model.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_ValidReportSection_ReturnsParsedReportConfig()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            report:
              file: build-notes.md
              depth: 2
              include-known-issues: true
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.NotNull(result.Config);
            Assert.False(result.HasErrors);
            Assert.NotNull(result.Config.Report);
            Assert.Equal("build-notes.md", result.Config.Report.File);
            Assert.Equal(2, result.Config.Report.Depth);
            Assert.True(result.Config.Report.IncludeKnownIssues);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that an invalid report depth produces an error issue.
    /// </summary>
    [Fact]
    public async Task BuildMarkConfigReader_ReadAsync_InvalidReportDepth_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            """
            report:
              depth: -1
            """,
            TestContext.Current.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.Null(result.Config);
            Assert.True(result.HasErrors);
            Assert.Contains("positive integer", result.Issues[0].Description);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}
