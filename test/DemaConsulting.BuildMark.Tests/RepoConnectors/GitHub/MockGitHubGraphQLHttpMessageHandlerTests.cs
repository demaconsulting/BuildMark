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

using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the MockGitHubGraphQLHttpMessageHandler class.
/// </summary>
[TestClass]
public class MockGitHubGraphQLHttpMessageHandlerTests
{
    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can be configured with multiple responses.
    /// </summary>
    [TestMethod]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleResponses_ReturnsCorrectResponse()
    {
        // Arrange - Use helper methods for standard responses
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse(new[] { "commit1" })
            .AddReleasesResponse(new[] { new MockRelease("v1.0.0", "2024-01-01T00:00:00Z") });

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

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can handle multiple commits.
    /// </summary>
    [TestMethod]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleCommits_ReturnsAllCommits()
    {
        // Arrange
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse(new[] { "commit1", "commit2", "commit3" });

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act
        var commits = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.HasCount(3, commits);
        Assert.AreEqual("commit1", commits[0]);
        Assert.AreEqual("commit2", commits[1]);
        Assert.AreEqual("commit3", commits[2]);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can handle multiple releases.
    /// </summary>
    [TestMethod]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleReleases_ReturnsAllReleases()
    {
        // Arrange
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddReleasesResponse(new[]
            {
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v2.0.0", "2024-03-01T00:00:00Z")
            });

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act
        var releases = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.HasCount(3, releases);
        Assert.AreEqual("v1.0.0", releases[0].TagName);
        Assert.AreEqual("v1.1.0", releases[1].TagName);
        Assert.AreEqual("v2.0.0", releases[2].TagName);
    }
}
