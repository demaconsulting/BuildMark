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

using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.GitHub;

/// <summary>
///     Tests for the MockGitHubGraphQLHttpMessageHandler class.
/// </summary>
public class MockGitHubGraphQLHttpMessageHandlerTests
{
    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can be configured with multiple responses.
    /// </summary>
    [Fact]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleResponses_ReturnsCorrectResponse()
    {
        // Arrange - Use helper methods for standard responses
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"));

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act & Assert - GetCommitsAsync should return commit1
        var commits = await client.GetCommitsAsync("owner", "repo", "main");
        Assert.Single(commits);
        Assert.Equal("commit1", commits[0]);

        // Act & Assert - GetReleasesAsync should return different data
        var releases = await client.GetReleasesAsync("owner", "repo");
        Assert.Single(releases);
        Assert.Equal("v1.0.0", releases[0].TagName);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler returns default response when no pattern matches.
    /// </summary>
    [Fact]
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
        Assert.NotNull(commits);
        Assert.Empty(commits);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler supports method chaining.
    /// </summary>
    [Fact]
    public void MockGitHubGraphQLHttpMessageHandler_MethodChaining_WorksCorrectly()
    {
        // Arrange & Act - Chain multiple AddResponse calls
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddResponse("pattern1", "response1")
            .AddResponse("pattern2", "response2")
            .SetDefaultResponse("default");

        // Assert - Verify handler was created
        Assert.NotNull(mockHandler);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can handle multiple commits.
    /// </summary>
    [Fact]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleCommits_ReturnsAllCommits()
    {
        // Arrange
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1", "commit2", "commit3");

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act
        var commits = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.Equal(3, commits.Count);
        Assert.Equal("commit1", commits[0]);
        Assert.Equal("commit2", commits[1]);
        Assert.Equal("commit3", commits[2]);
    }

    /// <summary>
    ///     Test that MockGitHubGraphQLHttpMessageHandler can handle multiple releases.
    /// </summary>
    [Fact]
    public async Task MockGitHubGraphQLHttpMessageHandler_MultipleReleases_ReturnsAllReleases()
    {
        // Arrange
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddReleasesResponse(
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v2.0.0", "2024-03-01T00:00:00Z"));

        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(mockHttpClient);

        // Act
        var releases = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.Equal(3, releases.Count);
        Assert.Equal("v1.0.0", releases[0].TagName);
        Assert.Equal("v1.1.0", releases[1].TagName);
        Assert.Equal("v2.0.0", releases[2].TagName);
    }
}



