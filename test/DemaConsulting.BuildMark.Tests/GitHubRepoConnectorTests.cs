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

using DemaConsulting.BuildMark.RepoConnectors;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the GitHubRepoConnector class.
/// </summary>
[TestClass]
public class GitHubRepoConnectorTests
{
    /// <summary>
    ///     Test that GitHubRepoConnector can be instantiated.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_Constructor_CreatesInstance()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify instance
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GitHubRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ImplementsInterface_ReturnsTrue()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify interface implementation
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }



    /// <summary>
    ///     Test that GetBuildInformationAsync works with mocked data.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange - Create mock responses using helper methods
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123def456")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(new MockTag("v1.0.0", "abc123def456"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123def456");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("abc123def456", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsNotNull(buildInfo.Bugs);
        Assert.IsNotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly selects previous version and generates changelog link.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithMultipleVersions_SelectsCorrectPreviousVersionAndGeneratesChangelogLink()
    {
        // Arrange - Create mock responses with multiple versions
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v2.0.0", "2024-03-01T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v2.0.0", "commit3"),
                new MockTag("v1.1.0", "commit2"),
                new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit3", buildInfo.CurrentVersionTag.CommitHash);
        
        // Should have selected v1.1.0 as baseline (previous non-prerelease)
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit2", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link
        Assert.IsNotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("v1.1.0...v2.0.0", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly gathers changes from PRs with labels.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithPullRequests_GathersChangesCorrectly()
    {
        // Arrange - Create mock responses with PRs containing different label types
        // We need commits in range between v1.0.0 and v1.1.0
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse(
                new MockPullRequest(
                    Number: 101,
                    Title: "Add new feature",
                    Url: "https://github.com/test/repo/pull/101",
                    Merged: true,
                    MergeCommitSha: "commit3",
                    HeadRefOid: "feature-branch",
                    Labels: ["feature", "enhancement"]),
                new MockPullRequest(
                    Number: 100,
                    Title: "Fix critical bug",
                    Url: "https://github.com/test/repo/pull/100",
                    Merged: true,
                    MergeCommitSha: "commit2",
                    HeadRefOid: "bugfix-branch",
                    Labels: ["bug"]))
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v1.1.0", "commit3"),
                new MockTag("v1.0.0", "commit1"))
            // Mock the linked issues query to return empty (PRs are treated as standalone changes)
            .AddResponse("closingIssuesReferences", @"{""data"":{""repository"":{""pullRequest"":{""closingIssuesReferences"":{""nodes"":[],""pageInfo"":{""hasNextPage"":false,""endCursor"":null}}}}}}");

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.1.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);

        // PRs without linked issues are treated based on their labels
        // PR 100 with "bug" label should be in bugs
        Assert.IsNotNull(buildInfo.Bugs);
        Assert.IsGreaterThanOrEqualTo(buildInfo.Bugs.Count, 1, $"Expected at least 1 bug, got {buildInfo.Bugs.Count}");
        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.IsNotNull(bugPR, "PR 100 should be categorized as a bug");
        Assert.AreEqual("Fix critical bug", bugPR.Title);

        // PR 101 with "feature" label should be in changes
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsGreaterThanOrEqualTo(buildInfo.Changes.Count, 1, $"Expected at least 1 change, got {buildInfo.Changes.Count}");
        var featurePR = buildInfo.Changes.FirstOrDefault(c => c.Index == 101);
        Assert.IsNotNull(featurePR, "PR 101 should be categorized as a change");
        Assert.AreEqual("Add new feature", featurePR.Title);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly identifies known issues.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithOpenIssues_IdentifiesKnownIssues()
    {
        // Arrange - Create mock responses with open and closed issues
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse(
                new MockIssue(
                    Number: 201,
                    Title: "Known bug in feature X",
                    Url: "https://github.com/test/repo/issues/201",
                    State: "OPEN",
                    Labels: ["bug"]),
                new MockIssue(
                    Number: 202,
                    Title: "Feature request for Y",
                    Url: "https://github.com/test/repo/issues/202",
                    State: "OPEN",
                    Labels: ["feature"]),
                new MockIssue(
                    Number: 203,
                    Title: "Fixed bug",
                    Url: "https://github.com/test/repo/issues/203",
                    State: "CLOSED",
                    Labels: ["bug"]))
            .AddTagsResponse(new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        
        // Known issues are open issues that aren't linked to any changes in this release
        Assert.IsNotNull(buildInfo.KnownIssues);
        // Since we have no PRs, all open issues should be known issues
        Assert.IsGreaterThanOrEqualTo(buildInfo.KnownIssues.Count, 1, $"Expected at least 1 known issue, got {buildInfo.KnownIssues.Count}");
        
        // Verify at least one known issue is present
        var knownIssueTitles = buildInfo.KnownIssues.Select(i => i.Title).ToList();
        var hasExpectedIssue = knownIssueTitles.Exists(t => t.Contains("Known bug") || t.Contains("Feature request"));
        Assert.IsTrue(hasExpectedIssue, "Should have at least one of the open issues as a known issue");
    }
}
