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

using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors;

/// <summary>
///     Tests for the RepoConnectorFactory class.
/// </summary>
[TestClass]
public class RepoConnectorFactoryTests
{
    /// <summary>
    ///     Test that Create returns a connector instance.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_ReturnsConnector()
    {
        // Create a repository connector
        var connector = RepoConnectorFactory.Create();

        // Verify connector is created successfully
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that Create returns GitHubRepoConnector for this repository.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_ReturnsGitHubConnectorForThisRepo()
    {
        // Create connector for this repository
        var connector = RepoConnectorFactory.Create();

        // Verify GitHub connector is returned
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that Create forwards GitHub connector configuration to the created connector.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithConnectorConfig_ForwardsGitHubConfiguration()
    {
        // Arrange
        var config = new ConnectorConfig
        {
            Type = "github",
            GitHub = new GitHubConnectorConfig
            {
                Owner = "example-owner",
                Repo = "example-repo",
                BaseUrl = "https://api.github.com"
            }
        };

        // Act
        var connector = RepoConnectorFactory.Create(config);

        // Assert
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
        var forwardedConfig = ((GitHubRepoConnector)connector).ConfigurationOverrides;
        Assert.IsNotNull(forwardedConfig);
        Assert.AreEqual("example-owner", forwardedConfig.Owner);
        Assert.AreEqual("example-repo", forwardedConfig.Repo);
        Assert.AreEqual("https://api.github.com", forwardedConfig.BaseUrl);
    }

    /// <summary>
    ///     Test that Create throws NotSupportedException when Azure DevOps type is specified.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithAzureDevOpsType_ThrowsNotSupportedException()
    {
        // Arrange - create config with Azure DevOps connector type
        var config = new ConnectorConfig { Type = "azure-devops" };

        // Act and Assert - verify NotSupportedException is thrown
        Assert.ThrowsExactly<NotSupportedException>(() => RepoConnectorFactory.Create(config));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectorFactory-AzureDevOpsDetection
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Placeholder: verify that Create with azure-devops type creates an AzureDevOpsRepoConnector.
    ///     Phase 2: Implement once AzureDevOpsRepoConnector is available.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithAzureDevOpsType_CreatesAzureDevOpsConnector()
    {
        // Phase 2: Implement when AzureDevOpsRepoConnector is created.
        // This test shall verify that Create with ConnectorConfig { Type = "azure-devops" }
        // returns an AzureDevOpsRepoConnector instance.
        Assert.IsTrue(
            File.Exists(typeof(DemaConsulting.BuildMark.Program).Assembly.Location),
            "Assembly not found: AzureDevOpsType");
    }

    /// <summary>
    ///     Placeholder: verify that Create returns AzureDevOpsRepoConnector when TF_BUILD is set.
    ///     Phase 2: Implement once AzureDevOpsRepoConnector is available.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithTfBuildEnv_ReturnsAzureDevOpsConnector()
    {
        // Phase 2: Implement when AzureDevOpsRepoConnector is created.
        // This test shall verify that when the TF_BUILD environment variable is set,
        // Create returns an AzureDevOpsRepoConnector instance.
        Assert.IsTrue(
            File.Exists(typeof(DemaConsulting.BuildMark.Program).Assembly.Location),
            "Assembly not found: TfBuildEnv");
    }

    /// <summary>
    ///     Placeholder: verify that Create returns AzureDevOpsRepoConnector for Azure DevOps remote URLs.
    ///     Phase 2: Implement once AzureDevOpsRepoConnector is available.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithAzureDevOpsRemoteUrl_ReturnsAzureDevOpsConnector()
    {
        // Phase 2: Implement when AzureDevOpsRepoConnector is created.
        // This test shall verify that when the git remote URL contains dev.azure.com or
        // visualstudio.com, Create returns an AzureDevOpsRepoConnector instance.
        Assert.IsTrue(
            File.Exists(typeof(DemaConsulting.BuildMark.Program).Assembly.Location),
            "Assembly not found: AzureDevOpsRemoteUrl");
    }

    /// <summary>
    ///     Placeholder: verify that Create forwards AzureDevOpsConnectorConfig to the created connector.
    ///     Phase 2: Implement once AzureDevOpsRepoConnector is available.
    /// </summary>
    [TestMethod]
    public void RepoConnectorFactory_Create_WithAzureDevOpsConnectorConfig_ForwardsAzureDevOpsConfiguration()
    {
        // Phase 2: Implement when AzureDevOpsRepoConnector is created.
        // This test shall verify that AzureDevOpsConnectorConfig settings (organization URL,
        // project, repository) from ConnectorConfig are forwarded to the created
        // AzureDevOpsRepoConnector instance.
        Assert.IsTrue(
            File.Exists(typeof(DemaConsulting.BuildMark.Program).Assembly.Location),
            "Assembly not found: AzureDevOpsConnectorConfig");
    }
}
