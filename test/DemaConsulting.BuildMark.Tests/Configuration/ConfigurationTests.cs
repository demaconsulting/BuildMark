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
[TestClass]
public class ConfigurationTests
{
    /// <summary>
    ///     Gets or sets the test context for the current test run.
    /// </summary>
    public TestContext TestContext { get; set; } = null!;

    /// <summary>
    ///     Test that missing configuration files return an empty result.
    /// </summary>
    [TestMethod]
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
            Assert.IsNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.IsEmpty(result.Issues);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that valid configuration files are parsed into the configuration model.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("github", result.Config.Connector?.Type);
            Assert.AreEqual("example-owner", result.Config.Connector?.GitHub?.Owner);
            Assert.AreEqual("hello-world", result.Config.Connector?.GitHub?.Repo);
            Assert.AreEqual("https://api.github.com", result.Config.Connector?.GitHub?.BaseUrl);
            Assert.HasCount(1, result.Config.Sections);
            Assert.AreEqual("changes", result.Config.Sections[0].Id);
            Assert.HasCount(1, result.Config.Rules);
            Assert.AreEqual("changes", result.Config.Rules[0].Route);
            Assert.AreEqual("feature", result.Config.Rules[0].Match?.Label[0]);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that malformed configuration files surface an error issue.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
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
    [TestMethod]
    public async Task BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue()
    {
        // Arrange
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        Directory.CreateDirectory(directory);
        var filePath = Path.Combine(directory, ".buildmark.yaml");
        await File.WriteAllTextAsync(
            filePath,
            "connector:\n\ttype: github\n",
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
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
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("azure-devops", result.Config.Connector?.Type);
            Assert.AreEqual("https://dev.azure.com/myorg", result.Config.Connector?.AzureDevOps?.OrganizationUrl);
            Assert.AreEqual("myorg", result.Config.Connector?.AzureDevOps?.Organization);
            Assert.AreEqual("myproject", result.Config.Connector?.AzureDevOps?.Project);
            Assert.AreEqual("myrepo", result.Config.Connector?.AzureDevOps?.Repository);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that Azure DevOps connector block with alternate key aliases is parsed correctly.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.AreEqual("https://dev.azure.com/myorg", result.Config.Connector?.AzureDevOps?.OrganizationUrl);
            Assert.AreEqual("myorg", result.Config.Connector?.AzureDevOps?.Organization);
            Assert.AreEqual("myproject", result.Config.Connector?.AzureDevOps?.Project);
            Assert.AreEqual("myrepo", result.Config.Connector?.AzureDevOps?.Repository);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that an unsupported key inside the Azure DevOps connector block produces an error.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
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
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.AreEqual(ConfigurationIssueSeverity.Error, result.Issues[0].Severity);
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
    [TestMethod]
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
        Assert.AreEqual(1, context.ExitCode);
    }

    /// <summary>
    ///     Test that the default configuration contains the expected sections and routing rules.
    /// </summary>
    [TestMethod]
    public void BuildMarkConfig_CreateDefault_ContainsDependencyUpdatesSection()
    {
        // Act
        var config = BuildMarkConfig.CreateDefault();

        // Assert - verify sections have correct IDs and titles
        Assert.HasCount(3, config.Sections);
        Assert.AreEqual("changes", config.Sections[0].Id);
        Assert.AreEqual("Changes", config.Sections[0].Title);
        Assert.AreEqual("bugs-fixed", config.Sections[1].Id);
        Assert.AreEqual("Bugs Fixed", config.Sections[1].Title);
        Assert.AreEqual("dependency-updates", config.Sections[2].Id);
        Assert.AreEqual("Dependency Updates", config.Sections[2].Title);

        // Assert - verify rules have correct routes and match conditions
        Assert.HasCount(6, config.Rules);

        Assert.AreEqual("dependency-updates", config.Rules[0].Route);
        Assert.Contains("dependencies", config.Rules[0].Match!.Label);
        Assert.Contains("renovate", config.Rules[0].Match!.Label);
        Assert.Contains("dependabot", config.Rules[0].Match!.Label);

        Assert.AreEqual("bugs-fixed", config.Rules[1].Route);
        Assert.Contains("Bug", config.Rules[1].Match!.WorkItemType);

        Assert.AreEqual("bugs-fixed", config.Rules[2].Route);
        Assert.Contains("bug", config.Rules[2].Match!.Label);
        Assert.Contains("defect", config.Rules[2].Match!.Label);
        Assert.Contains("regression", config.Rules[2].Match!.Label);

        Assert.AreEqual("suppressed", config.Rules[3].Route);
        Assert.Contains("internal", config.Rules[3].Match!.Label);
        Assert.Contains("chore", config.Rules[3].Match!.Label);

        Assert.AreEqual("suppressed", config.Rules[4].Route);
        Assert.Contains("Task", config.Rules[4].Match!.WorkItemType);
        Assert.Contains("Epic", config.Rules[4].Match!.WorkItemType);

        Assert.AreEqual("changes", config.Rules[5].Route);
        Assert.IsNull(config.Rules[5].Match);
    }
    /// <summary>
    ///     Test that a valid report section is parsed into the report configuration model.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNotNull(result.Config);
            Assert.IsFalse(result.HasErrors);
            Assert.IsNotNull(result.Config.Report);
            Assert.AreEqual("build-notes.md", result.Config.Report.File);
            Assert.AreEqual(2, result.Config.Report.Depth);
            Assert.IsTrue(result.Config.Report.IncludeKnownIssues);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    /// <summary>
    ///     Test that an invalid report depth produces an error issue.
    /// </summary>
    [TestMethod]
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
            TestContext.CancellationToken);

        try
        {
            // Act
            var result = await BuildMarkConfigReader.ReadAsync(directory);

            // Assert
            Assert.IsNull(result.Config);
            Assert.IsTrue(result.HasErrors);
            Assert.Contains("positive integer", result.Issues[0].Description);
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }
}



