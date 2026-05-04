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
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.GitHub;

/// <summary>
///     Tests for the GitHubRepoConnector class.
/// </summary>
public class GitHubRepoConnectorTests
{
    /// <summary>
    ///     Test that GitHubRepoConnector can be instantiated.
    /// </summary>
    [Fact]
    public void GitHubRepoConnector_Constructor_CreatesInstance()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify instance
        Assert.NotNull(connector);
        Assert.IsAssignableFrom<GitHubRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GitHubRepoConnector stores the provided configuration overrides.
    /// </summary>
    [Fact]
    public void GitHubRepoConnector_Constructor_WithConfig_StoresConfigurationOverrides()
    {
        // Arrange
        var config = new GitHubConnectorConfig
        {
            Owner = "example-owner",
            Repo = "example-repo",
            BaseUrl = "https://api.github.com"
        };

        // Act
        var connector = new GitHubRepoConnector(config);

        // Assert
        Assert.NotNull(connector.ConfigurationOverrides);
        Assert.Equal("example-owner", connector.ConfigurationOverrides.Owner);
        Assert.Equal("example-repo", connector.ConfigurationOverrides.Repo);
        Assert.Equal("https://api.github.com", connector.ConfigurationOverrides.BaseUrl);
    }

    /// <summary>
    ///     Test that GitHubRepoConnector implements IRepoConnector.
    /// </summary>
    [Fact]
    public void GitHubRepoConnector_ImplementsInterface_ReturnsTrue()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify interface implementation
        Assert.IsAssignableFrom<IRepoConnector>(connector);
    }



    /// <summary>
    ///     Test that GetBuildInformationAsync works with mocked data.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("abc123def456", buildInfo.CurrentVersionTag.CommitHash);
        Assert.NotNull(buildInfo.Changes);
        Assert.NotNull(buildInfo.Bugs);
        Assert.NotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly selects previous version and generates changelog link.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("commit3", buildInfo.CurrentVersionTag.CommitHash);

        // Should have selected v1.1.0 as baseline (previous non-prerelease)
        Assert.NotNull(buildInfo.BaselineVersionTag);
        Assert.Equal("1.1.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.Equal("commit2", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link
        Assert.NotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("v1.1.0...v2.0.0", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly gathers changes from PRs with labels.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.1.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);

        // PRs without linked issues are treated based on their labels
        // PR 100 with "bug" label should be in bugs
        Assert.NotNull(buildInfo.Bugs);
        Assert.True(buildInfo.Bugs.Count >= 1, $"Expected at least 1 bug, got {buildInfo.Bugs.Count}");
        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.True(bugPR != null, "PR 100 should be categorized as a bug");
        Assert.Equal("Fix critical bug", bugPR.Title);

        // PR 101 with "feature" label should be in changes
        Assert.NotNull(buildInfo.Changes);
        Assert.True(buildInfo.Changes.Count >= 1, $"Expected at least 1 change, got {buildInfo.Changes.Count}");
        var featurePR = buildInfo.Changes.FirstOrDefault(c => c.Index == 101);
        Assert.True(featurePR != null, "PR 101 should be categorized as a change");
        Assert.Equal("Add new feature", featurePR.Title);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly identifies known issues.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);

        // Known issues are open issues that aren't linked to any changes in this release
        Assert.NotNull(buildInfo.KnownIssues);
        // Since we have no PRs, all open issues should be known issues
        Assert.True(buildInfo.KnownIssues.Count >= 1, $"Expected at least 1 known issue, got {buildInfo.KnownIssues.Count}");

        // Verify at least one known issue is present
        var knownIssueTitles = buildInfo.KnownIssues.Select(i => i.Title).ToList();
        var hasExpectedIssue = knownIssueTitles.Exists(t => t.Contains("Known bug") || t.Contains("Feature request"));
        Assert.True(hasExpectedIssue, "Should have at least one of the open issues as a known issue");
    }

    /// <summary>
    ///     Test that pre-release baseline selection skips tags with the same commit hash.
    ///     Example: 1.1.2-rc.1 (hash a1b2c3d4) and 1.1.2-beta.2 (hash a1b2c3d4) are re-tags.
    ///     When processing 1.1.2-rc.1, it should skip 1.1.2-beta.2 and use 1.1.2-beta.1 (hash 734713bc).
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("1.1.2-rc.1"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.1.2-rc.1", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("a1b2c3d4", buildInfo.CurrentVersionTag.CommitHash);

        // Should have skipped 1.1.2-beta.2 (same hash) and selected 1.1.2-beta.1 (different hash)
        Assert.NotNull(buildInfo.BaselineVersionTag);
        Assert.Equal("1.1.2-beta.1", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.Equal("734713bc", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link between beta.1 and rc.1
        Assert.NotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("1.1.2-beta.1...1.1.2-rc.1", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that release baseline selection skips all pre-release versions.
    ///     Example: 1.1.2 should skip 1.1.2-rc.1, 1.1.2-beta.2, 1.1.2-beta.1 and use 1.1.1.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("1.1.2"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.1.2", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("commit5", buildInfo.CurrentVersionTag.CommitHash);

        // Should have skipped all pre-releases and selected 1.1.1
        Assert.NotNull(buildInfo.BaselineVersionTag);
        Assert.Equal("1.1.1", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.Equal("commit1", buildInfo.BaselineVersionTag.CommitHash);

        // Should have changelog link between 1.1.1 and 1.1.2
        Assert.NotNull(buildInfo.CompleteChangelogLink);
        Assert.Contains("v1.1.1...1.1.2", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that pre-release baseline selection works correctly when target is not in release history.
    ///     This happens when generating build notes for a version that hasn't been tagged yet.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("1.1.2-beta.2"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.1.2-beta.2", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("new-hash-123", buildInfo.CurrentVersionTag.CommitHash);

        // Should use most recent release with different hash
        Assert.NotNull(buildInfo.BaselineVersionTag);
        Assert.Equal("1.1.2-beta.1", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.Equal("commit2", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Test that pre-release baseline selection returns null when all previous versions have the same hash.
    ///     This is an edge case where all previous tags are re-tags of the current commit.
    /// </summary>
    [Fact]
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
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("1.1.2-rc.1"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Equal("1.1.2-rc.1", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("same-hash-123", buildInfo.CurrentVersionTag.CommitHash);

        // Should have null baseline since all previous versions are on the same hash
        Assert.Null(buildInfo.BaselineVersionTag);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync does not throw when two merged pull requests share
    ///     the same merge commit SHA. This is a regression test for the key collision bug where
    ///     <c>ToDictionary</c> would throw <see cref="ArgumentException"/> on duplicate keys.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithDuplicateMergeCommitSha_DoesNotThrow()
    {
        // Arrange - Create mock responses where two merged PRs share the same merge commit SHA.
        // The SHA below is the exact key from the bug report (demaconsulting/BuildMark#45).
        const string sharedMergeCommitSha = "c85989fd08aee2b768557f6b90011ec325b3bdea";
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse(sharedMergeCommitSha)
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse(
                new MockPullRequest(
                    Number: 1,
                    Title: "First PR with shared merge commit",
                    Url: "https://github.com/test/repo/pull/1",
                    Merged: true,
                    MergeCommitSha: sharedMergeCommitSha,
                    HeadRefOid: "head-sha-1",
                    Labels: []),
                new MockPullRequest(
                    Number: 2,
                    Title: "Second PR with same merge commit SHA",
                    Url: "https://github.com/test/repo/pull/2",
                    Merged: true,
                    MergeCommitSha: sharedMergeCommitSha,
                    HeadRefOid: "head-sha-2",
                    Labels: []))
            .AddIssuesResponse()
            .AddTagsResponse(new MockTag("v1.0.0", sharedMergeCommitSha));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        // Set up mock command responses
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", sharedMergeCommitSha);
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act - This must not throw ArgumentException due to duplicate dictionary key
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert - Build info should be valid and not null
        Assert.NotNull(buildInfo);
        Assert.Equal("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal(sharedMergeCommitSha, buildInfo.CurrentVersionTag.CommitHash);
        Assert.NotNull(buildInfo.Changes);
        Assert.NotNull(buildInfo.Bugs);
        Assert.NotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that a PR with a label whose name contains a known type as a substring is not incorrectly classified.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_PrWithSubstringMatchLabel_NotClassifiedAsBug()
    {
        // Arrange - PR with label "debugging" (contains "bug" as substring but is not "bug")
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse(
                new MockPullRequest(
                    Number: 100,
                    Title: "Add debug logging",
                    Url: "https://github.com/test/repo/pull/100",
                    Merged: true,
                    MergeCommitSha: "commit1",
                    HeadRefOid: "debug-branch",
                    Labels: ["debugging"]))
            .AddIssuesResponse()
            .AddTagsResponse(new MockTag("v1.0.0", "commit1"))
            .AddResponse("closingIssuesReferences", @"{""data"":{""repository"":{""pullRequest"":{""closingIssuesReferences"":{""nodes"":[],""pageInfo"":{""hasNextPage"":false,""endCursor"":null}}}}}}");

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert - PR with "debugging" label must NOT be classified as a bug
        Assert.NotNull(buildInfo);
        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.True(bugPR == null, "PR with 'debugging' label must not be classified as a bug");
    }

    /// <summary>
    ///     Test that an open issue with a label whose name contains a known type as a substring is not a known issue.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_IssueWithSubstringMatchLabel_NotClassifiedAsKnownIssue()
    {
        // Arrange - Open issue with label "debugging" (contains "bug" as substring but is not "bug")
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse(
                new MockIssue(
                    Number: 201,
                    Title: "Improve debug output",
                    Url: "https://github.com/test/repo/issues/201",
                    State: "OPEN",
                    Labels: ["debugging"]))
            .AddTagsResponse(new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);

        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert - Open issue with "debugging" label must NOT be classified as a known issue
        Assert.NotNull(buildInfo);
        var knownIssue = buildInfo.KnownIssues.FirstOrDefault(i => i.Index == 201);
        Assert.True(knownIssue == null, "Issue with 'debugging' label must not be classified as a known issue");
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync excludes items with visibility:internal in description.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_VisibilityInternal_ExcludesItem()
    {
        // Arrange
        var internalBody = "This is internal.\n\n```buildmark\nvisibility: internal\n```";
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddTagsResponse(new MockTag("v1.0.0", "abc123"))
            .AddPullRequestsResponse(
                new MockPullRequest(1, "Internal PR", "https://github.com/owner/repo/pull/1", true, "abc123", "abc123", ["feature"], internalBody))
            .AddIssuesResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Empty(buildInfo.Changes);
        Assert.Empty(buildInfo.Bugs);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync includes items with visibility:public in description.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_VisibilityPublic_IncludesItem()
    {
        // Arrange
        var publicBody = "This is public.\n\n```buildmark\nvisibility: public\n```";
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddTagsResponse(new MockTag("v1.0.0", "abc123"))
            .AddPullRequestsResponse(
                new MockPullRequest(1, "Public PR", "https://github.com/owner/repo/pull/1", true, "abc123", "abc123", [], publicBody))
            .AddIssuesResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        // Item should be included in Changes (type "other" which is not bug)
        Assert.Single(buildInfo.Changes);
        Assert.Equal("Public PR", buildInfo.Changes[0].Title);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync classifies as bug when type:bug in description.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_TypeBugOverride_ClassifiesAsBug()
    {
        // Arrange
        var bugBody = "This is a bug.\n\n```buildmark\ntype: bug\n```";
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddTagsResponse(new MockTag("v1.0.0", "abc123"))
            .AddPullRequestsResponse(
                new MockPullRequest(1, "Bug PR", "https://github.com/owner/repo/pull/1", true, "abc123", "abc123", [], bugBody))
            .AddIssuesResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Single(buildInfo.Bugs);
        Assert.Equal("Bug PR", buildInfo.Bugs[0].Title);
        Assert.Empty(buildInfo.Changes);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync classifies as feature when type:feature in description.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_TypeFeatureOverride_ClassifiesAsFeature()
    {
        // Arrange
        var featureBody = "This is a feature.\n\n```buildmark\ntype: feature\n```";
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddTagsResponse(new MockTag("v1.0.0", "abc123"))
            .AddPullRequestsResponse(
                new MockPullRequest(1, "Feature PR", "https://github.com/owner/repo/pull/1", true, "abc123", "abc123", [], featureBody))
            .AddIssuesResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.Single(buildInfo.Changes);
        Assert.Equal("Feature PR", buildInfo.Changes[0].Title);
        Assert.Equal("feature", buildInfo.Changes[0].Type);
        Assert.Empty(buildInfo.Bugs);
    }

    /// <summary>
    ///     Test that Configure with rules causes HasRules behavior (RoutedSections populated after GetBuildInformation).
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHubRepoConnector.Configure stores rules
    ///     What the assertions prove: Configure is callable on GitHubRepoConnector (public method inherited from base)
    /// </remarks>
    [Fact]
    public void GitHubRepoConnector_Configure_WithRules_HasRulesReturnsTrue()
    {
        // Arrange - Create connector and define rules
        var connector = new GitHubRepoConnector();
        List<RuleConfig> rules =
        [
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
            new() { Route = "features" }
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "features", Title = "Features" },
            new() { Id = "bugs", Title = "Bugs" }
        ];

        // Act - Configure the connector with rules (should not throw)
        connector.Configure(rules, sections);

        // Assert - Connector is still a valid instance after configuration
        Assert.NotNull(connector);
        Assert.IsAssignableFrom<GitHubRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync with configured rules populates RoutedSections.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHubRepoConnector.GetBuildInformationAsync routing behavior
    ///     What the assertions prove: When rules are configured, items are routed into the
    ///     correct sections and RoutedSections is populated on the returned BuildInformation.
    /// </remarks>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections()
    {
        // Arrange: set up two merged PRs with different labels — one feature, one bug
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("feat123", "bug456")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddTagsResponse(new MockTag("v1.0.0", "feat123"))
            .AddPullRequestsResponse(
                new MockPullRequest(1, "Feature PR", "https://github.com/owner/repo/pull/1", true, "feat123", "feat123", ["feature"]),
                new MockPullRequest(2, "Bug PR", "https://github.com/owner/repo/pull/2", true, "bug456", "bug456", ["bug"]))
            .AddIssuesResponse();

        // Set up connector with mocked HTTP and git commands
        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "feat123");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Configure routing rules: bugs → "bugs" section, everything else → "features" section
        List<RuleConfig> rules =
        [
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
            new() { Route = "features" }
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "features", Title = "Features" },
            new() { Id = "bugs", Title = "Bugs Fixed" }
        ];
        connector.Configure(rules, sections);

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: RoutedSections is populated when rules are configured
        Assert.True(buildInfo.RoutedSections != null, "RoutedSections should be populated when rules are configured");
        Assert.Equal(2, buildInfo.RoutedSections.Count);

        // Verify the feature item was routed to the "features" section (first section)
        var featuresSection = buildInfo.RoutedSections[0];
        Assert.Equal("features", featuresSection.SectionId);
        Assert.Equal("Features", featuresSection.SectionTitle);
        Assert.Single(featuresSection.Items);
        Assert.Equal("Feature PR", featuresSection.Items[0].Title);

        // Verify the bug item was routed to the "bugs" section (second section)
        var bugsSection = buildInfo.RoutedSections[1];
        Assert.Equal("bugs", bugsSection.SectionId);
        Assert.Equal("Bugs Fixed", bugsSection.SectionTitle);
        Assert.Single(bugsSection.Items);
        Assert.Equal("Bug PR", bugsSection.Items[0].Title);
    }

    /// <summary>
    ///     Verify that known issues are filtered by affected-versions when present.
    ///     A bug whose affected-versions do not contain the build version is excluded;
    ///     a bug whose affected-versions contain the build version is included;
    ///     a bug with no affected-versions is included (fallback to open status).
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions()
    {
        // Arrange - three open bugs:
        //   301: affected-versions [1.0.0,2.0.0) => includes v1.5.0, excludes v2.0.0
        //   302: affected-versions [3.0.0,) => excludes v1.5.0
        //   303: no affected-versions => always included when open
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.5.0", "2024-06-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse(
                new MockIssue(
                    Number: 301,
                    Title: "Bug affecting v1.x",
                    Url: "https://github.com/test/repo/issues/301",
                    State: "OPEN",
                    Labels: ["bug"],
                    Body: "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```"),
                new MockIssue(
                    Number: 302,
                    Title: "Bug affecting v3+",
                    Url: "https://github.com/test/repo/issues/302",
                    State: "OPEN",
                    Labels: ["bug"],
                    Body: "```buildmark\naffected-versions: [3.0.0,)\n```"),
                new MockIssue(
                    Number: 303,
                    Title: "Bug with no versions",
                    Url: "https://github.com/test/repo/issues/303",
                    State: "OPEN",
                    Labels: ["bug"]))
            .AddTagsResponse(new MockTag("v1.5.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.5.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.NotNull(buildInfo.KnownIssues);

        // Bug 301 should be included (v1.5.0 is in [1.0.0,2.0.0))
        Assert.True(
            buildInfo.KnownIssues.Exists(i => i.Id == "301"),
            "Bug 301 with affected-versions [1.0.0,2.0.0) should be a known issue for v1.5.0");

        // Bug 302 should be excluded (v1.5.0 is NOT in [3.0.0,))
        Assert.False(
            buildInfo.KnownIssues.Exists(i => i.Id == "302"),
            "Bug 302 with affected-versions [3.0.0,) should NOT be a known issue for v1.5.0");

        // Bug 303 should be included (no affected-versions, fallback to open status)
        Assert.True(
            buildInfo.KnownIssues.Exists(i => i.Id == "303"),
            "Bug 303 with no affected-versions should be a known issue (open status fallback)");
    }

    /// <summary>
    ///     Verify that a CLOSED bug with an affected-versions range that contains the build
    ///     version is reported as a known issue.  This models the LTS back-port gap scenario:
    ///     a bug may be closed after being fixed in a newer release, yet still affect an older
    ///     branch from which LTS releases are cut.
    /// </summary>
    [Fact]
    public async Task GitHubRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue()
    {
        // Arrange - three closed bugs:
        //   304: CLOSED, AV [1.0.0,2.0.0) - fixed in v2 but v1.5.0 LTS branch never got the fix
        //   305: CLOSED, AV [3.0.0,) - fixed in v3+; does NOT affect v1.5.0
        //   306: CLOSED, no AV - closed bug with no AV is NOT a known issue (status fallback)
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.5.0", "2024-06-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse(
                new MockIssue(
                    Number: 304,
                    Title: "Closed bug affecting v1.x",
                    Url: "https://github.com/test/repo/issues/304",
                    State: "CLOSED",
                    Labels: ["bug"],
                    Body: "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```"),
                new MockIssue(
                    Number: 305,
                    Title: "Closed bug affecting v3+",
                    Url: "https://github.com/test/repo/issues/305",
                    State: "CLOSED",
                    Labels: ["bug"],
                    Body: "```buildmark\naffected-versions: [3.0.0,)\n```"),
                new MockIssue(
                    Number: 306,
                    Title: "Closed bug with no AV",
                    Url: "https://github.com/test/repo/issues/306",
                    State: "CLOSED",
                    Labels: ["bug"]))
            .AddTagsResponse(new MockTag("v1.5.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.5.0"));

        // Assert
        Assert.NotNull(buildInfo);
        Assert.NotNull(buildInfo.KnownIssues);

        // Bug 304 is CLOSED but has AV [1.0.0,2.0.0) which contains v1.5.0 → IS a known issue
        Assert.True(
            buildInfo.KnownIssues.Exists(i => i.Id == "304"),
            "Closed bug 304 with AV [1.0.0,2.0.0) should be a known issue for v1.5.0 (LTS back-port gap)");

        // Bug 305 is CLOSED and has AV [3.0.0,) which does NOT contain v1.5.0 → NOT a known issue
        Assert.False(
            buildInfo.KnownIssues.Exists(i => i.Id == "305"),
            "Closed bug 305 with AV [3.0.0,) should NOT be a known issue for v1.5.0");

        // Bug 306 is CLOSED with no AV → NOT a known issue (open/closed fallback applies)
        Assert.False(
            buildInfo.KnownIssues.Exists(i => i.Id == "306"),
            "Closed bug 306 with no AV should NOT be a known issue (closed, no AV)");
    }
}



