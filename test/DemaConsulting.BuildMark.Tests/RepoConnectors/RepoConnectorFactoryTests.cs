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

namespace DemaConsulting.BuildMark.Tests;

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
}
