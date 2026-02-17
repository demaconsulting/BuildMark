// Copyright (c) 2026 DEMA Consulting
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

using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for GitHubRepoConnector testability improvements.
/// </summary>
[TestClass]
public class GitHubRepoConnectorTestabilityTests
{
    /// <summary>
    ///     Mock implementation of GitHubRepoConnector for testing purposes.
    /// </summary>
    private sealed class MockableGitHubRepoConnector : GitHubRepoConnector
    {
        /// <summary>
        ///     Mock responses for RunCommandAsync.
        /// </summary>
        private readonly Dictionary<string, string> _commandResponses = new();

        /// <summary>
        ///     Mock HttpClient for GraphQL requests.
        /// </summary>
        private readonly HttpClient? _mockHttpClient;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MockableGitHubRepoConnector"/> class.
        /// </summary>
        /// <param name="mockHttpClient">Optional mock HttpClient for GraphQL requests.</param>
        public MockableGitHubRepoConnector(HttpClient? mockHttpClient = null)
        {
            _mockHttpClient = mockHttpClient;
        }

        /// <summary>
        ///     Sets the mock response for a specific command.
        /// </summary>
        /// <param name="command">Command to mock.</param>
        /// <param name="response">Response to return.</param>
        public void SetCommandResponse(string command, string response)
        {
            _commandResponses[command] = response;
        }

        /// <summary>
        ///     Runs a command and returns its output.
        /// </summary>
        /// <param name="command">Command to run.</param>
        /// <param name="arguments">Command arguments.</param>
        /// <returns>Command output.</returns>
        protected override Task<string> RunCommandAsync(string command, string arguments)
        {
            var key = $"{command} {arguments}";
            return Task.FromResult(_commandResponses.TryGetValue(key, out var response) ? response : string.Empty);
        }

        /// <summary>
        ///     Creates a GitHub GraphQL client for API operations.
        /// </summary>
        /// <param name="token">GitHub personal access token for authentication.</param>
        /// <returns>A new GitHubGraphQLClient instance.</returns>
        internal override GitHubGraphQLClient CreateGraphQLClient(string token)
        {
            return _mockHttpClient != null
                ? new GitHubGraphQLClient(_mockHttpClient)
                : base.CreateGraphQLClient(token);
        }
    }

    /// <summary>
    ///     Test that CreateGraphQLClient can be overridden for testing.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_CreateGraphQLClient_CanBeOverridden()
    {
        // Arrange
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler();
        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Act - Create a client using the virtual method
        using var client = connector.CreateGraphQLClient("test-token");

        // Assert
        Assert.IsNotNull(client);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync uses the overridden CreateGraphQLClient method.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_UsesOverriddenCreateGraphQLClient()
    {
        // Arrange - Create mock responses using helper methods
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse(new[] { "commit123" })
            .AddReleasesResponse(new[] { ("v1.0.0", "2024-01-01T00:00:00Z") })
            .AddPullRequestsResponse(Array.Empty<object>())
            .AddIssuesResponse(Array.Empty<object>())
            .AddTagsResponse(new[] { ("v1.0.0", "commit123") });

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - This should use the overridden CreateGraphQLClient method
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit123", buildInfo.CurrentVersionTag.CommitHash);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can be configured with multiple responses.
    /// </summary>
    [TestMethod]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleResponses_ReturnsCorrectResponse()
    {
        // Arrange - Use helper methods for standard responses
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse(new[] { "commit1" })
            .AddReleasesResponse(new[] { ("v1.0.0", "2024-01-01T00:00:00Z") });

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act & Assert - GetCommitsAsync should return commit1
        var commits = await client.GetCommitsAsync("owner", "repo", "main");
        Assert.HasCount(1, commits);
        Assert.AreEqual("commit1", commits[0]);

        // Act & Assert - GetReleasesAsync should return different data
        var releases = await client.GetReleasesAsync("owner", "repo");
        Assert.HasCount(1, releases);
        Assert.AreEqual("v1.0.0", releases[0].TagName);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler returns default response when no pattern matches.
    /// </summary>
    [TestMethod]
    public async Task MockGitHubGraphQLHttpMessageHandler_NoPatternMatches_ReturnsDefaultResponse()
    {
        // Arrange - Use helper method for empty repository response
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddEmptyRepositoryResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act
        var commits = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert - Should return empty list since repository is null
        Assert.IsNotNull(commits);
        Assert.IsEmpty(commits);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler supports method chaining.
    /// </summary>
    [TestMethod]
    public void MockGitHubGraphQLHttpMessageHandler_MethodChaining_WorksCorrectly()
    {
        // Arrange & Act - Chain multiple AddResponse calls
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddResponse("pattern1", "response1")
            .AddResponse("pattern2", "response2")
            .SetDefaultResponse("default");

        // Assert - Verify handler was created
        Assert.IsNotNull(mockHandler);
    }
}
