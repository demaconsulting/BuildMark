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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for configuration loading and reporting.
/// </summary>
[TestClass]
public class ConfigurationTests
{
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
            """);

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
            """);

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
}
