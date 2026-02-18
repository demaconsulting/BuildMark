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
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.Bugs.Count, $"Expected at least 1 bug, got {buildInfo.Bugs.Count}");
        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.IsNotNull(bugPR, "PR 100 should be categorized as a bug");
        Assert.AreEqual("Fix critical bug", bugPR.Title);

        // PR 101 with "feature" label should be in changes
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.Changes.Count, $"Expected at least 1 change, got {buildInfo.Changes.Count}");
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
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.KnownIssues.Count, $"Expected at least 1 known issue, got {buildInfo.KnownIssues.Count}");
        
        // Verify at least one known issue is present
        var knownIssueTitles = buildInfo.KnownIssues.Select(i => i.Title).ToList();
        var hasExpectedIssue = knownIssueTitles.Exists(t => t.Contains("Known bug") || t.Contains("Feature request"));
        Assert.IsTrue(hasExpectedIssue, "Should have at least one of the open issues as a known issue");
    }

    /// <summary>
    ///     Test that pre-release baseline selection skips tags with the same commit hash.
    ///     Example: 1.1.2-rc.1 (hash a1b2c3d4) and 1.1.2-beta.2 (hash a1b2c3d4) are re-tags.
    ///     When processing 1.1.2-rc.1, it should skip 1.1.2-beta.2 and use 1.1.2-beta.1 (hash 734713bc).
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_PreReleaseWithSameCommitHash_SkipsToNextDifferentHash()
    {
        // Arrange - Create mock responses with multiple pre-releases on same and different hashes
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("a1b2c3d4", "734713bc", "commit1")
            .AddReleasesResponse(
                new MockRelease("1.1.2-rc.1", "2024-03-03T00:00:00Z"),     // Same hash as beta.2
                new MockRelease("1.1.2-beta.2", "2024-03-02T00:00:00Z"),   // Same hash as rc.1
                new MockRelease("1.1.2-beta.1", "2024-03-01T00:00:00Z"),   // Different hash
                new MockRelease("v1.1.1", "2024-02-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("1.1.2-rc.1", "a1b2c3d4"),      // rc.1 and beta.2 on same hash
                new MockTag("1.1.2-beta.2", "a1b2c3d4"),    // Same hash as rc.1
                new MockTag("1.1.2-beta.1", "734713bc"),    // Different hash
                new MockTag("v1.1.1", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "a1b2c3d4");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - Process 1.1.2-rc.1
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("1.1.2-rc.1"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.2-rc.1", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("a1b2c3d4", buildInfo.CurrentVersionTag.CommitHash);
        
        // Should have skipped 1.1.2-beta.2 (same hash) and selected 1.1.2-beta.1 (different hash)
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.1.2-beta.1", buildInfo.BaselineVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("734713bc", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link between beta.1 and rc.1
        Assert.IsNotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("1.1.2-beta.1...1.1.2-rc.1", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that release baseline selection skips all pre-release versions.
    ///     Example: 1.1.2 should skip 1.1.2-rc.1, 1.1.2-beta.2, 1.1.2-beta.1 and use 1.1.1.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_ReleaseVersion_SkipsAllPreReleases()
    {
        // Arrange - Create mock responses with release and multiple pre-releases
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit5", "commit4", "commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("1.1.2", "2024-03-05T00:00:00Z"),
                new MockRelease("1.1.2-rc.1", "2024-03-04T00:00:00Z"),
                new MockRelease("1.1.2-beta.2", "2024-03-03T00:00:00Z"),
                new MockRelease("1.1.2-beta.1", "2024-03-02T00:00:00Z"),
                new MockRelease("v1.1.1", "2024-02-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("1.1.2", "commit5"),
                new MockTag("1.1.2-rc.1", "commit4"),
                new MockTag("1.1.2-beta.2", "commit3"),
                new MockTag("1.1.2-beta.1", "commit2"),
                new MockTag("v1.1.1", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit5");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - Process 1.1.2
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("1.1.2"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.2", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit5", buildInfo.CurrentVersionTag.CommitHash);
        
        // Should have skipped all pre-releases and selected 1.1.1
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.1.1", buildInfo.BaselineVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit1", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link between 1.1.1 and 1.1.2
        Assert.IsNotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("v1.1.1...1.1.2", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that pre-release baseline selection works correctly when target is not in release history.
    ///     This happens when generating build notes for a version that hasn't been tagged yet.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_PreReleaseNotInHistory_UsesLatestDifferentHash()
    {
        // Arrange - Create mock responses where target version doesn't exist yet
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("new-hash-123", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("1.1.2-beta.1", "2024-03-01T00:00:00Z"),
                new MockRelease("v1.1.1", "2024-02-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("1.1.2-beta.1", "commit2"),
                new MockTag("v1.1.1", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "new-hash-123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - Process 1.1.2-beta.2 which doesn't exist in releases yet
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("1.1.2-beta.2"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.2-beta.2", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("new-hash-123", buildInfo.CurrentVersionTag.CommitHash);
        
        // Should use most recent release with different hash
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.1.2-beta.1", buildInfo.BaselineVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("commit2", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Test that pre-release baseline selection returns null when all previous versions have the same hash.
    ///     This is an edge case where all previous tags are re-tags of the current commit.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_PreReleaseAllPreviousSameHash_ReturnsNullBaseline()
    {
        // Arrange - Create mock responses where all versions are on the same commit
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("same-hash-123")
            .AddReleasesResponse(
                new MockRelease("1.1.2-rc.1", "2024-03-03T00:00:00Z"),
                new MockRelease("1.1.2-beta.2", "2024-03-02T00:00:00Z"),
                new MockRelease("1.1.2-beta.1", "2024-03-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("1.1.2-rc.1", "same-hash-123"),
                new MockTag("1.1.2-beta.2", "same-hash-123"),
                new MockTag("1.1.2-beta.1", "same-hash-123"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "same-hash-123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - Process 1.1.2-rc.1
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("1.1.2-rc.1"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.2-rc.1", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.AreEqual("same-hash-123", buildInfo.CurrentVersionTag.CommitHash);
        
        // Should have null baseline since all previous versions are on the same hash
        Assert.IsNull(buildInfo.BaselineVersionTag);
    }
}
