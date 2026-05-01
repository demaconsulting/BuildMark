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
using DemaConsulting.BuildMark.RepoConnectors.GitHub;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.GitHub;

/// <summary>
///     Sub-subsystem tests for the GitHub sub-subsystem.
///     These tests verify the contract exposed by the GitHub sub-subsystem as a whole,
///     exercising the connector through its public IRepoConnector interface.
/// </summary>
public class GitHubTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-GitHub-SubSystem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the GitHub sub-subsystem provides a connector that implements IRepoConnector.
    /// </summary>
    [Fact]
    public void GitHub_ImplementsInterface_ReturnsTrue()
    {
        // Arrange: create a GitHubRepoConnector instance from the GitHub sub-subsystem
        var connector = new GitHubRepoConnector();

        // Assert: the sub-subsystem connector satisfies the shared IRepoConnector interface
        Assert.IsAssignableFrom<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the GitHub sub-subsystem returns valid build information from mocked API data.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHub sub-subsystem end-to-end build information retrieval
    ///     What the assertions prove: Build information is complete and accurate for a single release
    /// </remarks>
    [Fact]
    public async Task GitHub_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange: set up a mocked GraphQL handler with a single release and commit
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123def456")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(new MockTag("v1.0.0", "abc123def456"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123def456");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: build information is complete and accurate
        Assert.NotNull(buildInfo);
        Assert.Equal("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("abc123def456", buildInfo.CurrentVersionTag.CommitHash);
        Assert.NotNull(buildInfo.Changes);
        Assert.NotNull(buildInfo.Bugs);
        Assert.NotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that the GitHub sub-subsystem selects the correct previous version as baseline.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHub sub-subsystem baseline version selection
    ///     What the assertions prove: The connector picks the most recent prior release as the baseline
    /// </remarks>
    [Fact]
    public async Task GitHub_GetBuildInformation_WithMultipleVersions_SelectsCorrectBaseline()
    {
        // Arrange: set up three release tags so the connector can pick v1.1.0 as baseline for v2.0.0
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
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information for v2.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: v1.1.0 is selected as baseline
        Assert.NotNull(buildInfo);
        Assert.Equal("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.True(buildInfo.BaselineVersionTag != null, "Previous version should be identified");
        Assert.Equal("1.1.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
    }

    /// <summary>
    ///     Test that the GitHub sub-subsystem correctly categorizes pull requests into changes and bugs.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHub sub-subsystem PR classification at the sub-subsystem level
    ///     What the assertions prove: Feature PRs appear in Changes, bug PRs appear in Bugs
    /// </remarks>
    [Fact]
    public async Task GitHub_GetBuildInformation_WithPullRequests_GathersChanges()
    {
        // Arrange: two PRs — one labelled "feature", one labelled "bug"
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
                    Labels: ["feature"]),
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
            .AddResponse(
                "closingIssuesReferences",
                @"{""data"":{""repository"":{""pullRequest"":{""closingIssuesReferences"":{""nodes"":[],""pageInfo"":{""hasNextPage"":false,""endCursor"":null}}}}}}");

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert: feature PR is in Changes, bug PR is in Bugs
        Assert.NotNull(buildInfo);
        var featurePR = buildInfo.Changes.FirstOrDefault(c => c.Index == 101);
        Assert.True(featurePR != null, "Feature PR should be in Changes");

        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.True(bugPR != null, "Bug PR should be in Bugs");
    }

    /// <summary>
    ///     Test that the GitHub sub-subsystem correctly identifies open issues as known issues.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHub sub-subsystem known-issues identification
    ///     What the assertions prove: Open issues with "bug" label appear in KnownIssues
    /// </remarks>
    [Fact]
    public async Task GitHub_GetBuildInformation_WithOpenIssues_IdentifiesKnownIssues()
    {
        // Arrange: one open issue that is not resolved in this release
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
                    Labels: ["bug"]))
            .AddTagsResponse(new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: open issue surfaces as a known issue
        Assert.NotNull(buildInfo);
        Assert.True(buildInfo.KnownIssues.Count > 0, "Should have at least one known issue");
        var knownIssue = buildInfo.KnownIssues.FirstOrDefault(i => i.Index == 201);
        Assert.True(knownIssue != null, "Open issue 201 should appear in KnownIssues");
        Assert.Equal("Known bug in feature X", knownIssue.Title);
    }

    /// <summary>
    ///     Test that the GitHub sub-subsystem skips pre-releases when building the version baseline.
    /// </summary>
    /// <remarks>
    ///     What is being tested: GitHub sub-subsystem pre-release handling
    ///     What the assertions prove: A release version uses only prior release tags as its baseline
    /// </remarks>
    [Fact]
    public async Task GitHub_GetBuildInformation_ReleaseVersion_SkipsPreReleases()
    {
        // Arrange: mix of release and pre-release tags
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit4", "commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v2.0.0", "2024-04-01T00:00:00Z"),
                new MockRelease("v2.0.0-rc.1", "2024-03-15T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v2.0.0", "commit4"),
                new MockTag("v2.0.0-rc.1", "commit3"),
                new MockTag("v1.1.0", "commit2"),
                new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit4");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information for v2.0.0 (a release version)
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: baseline should be v1.1.0, not the pre-release v2.0.0-rc.1
        Assert.NotNull(buildInfo);
        Assert.Equal("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.True(buildInfo.BaselineVersionTag != null, "Baseline version should be set");
        Assert.True(buildInfo.BaselineVersionTag.VersionTag.FullVersion == "1.1.0", "Release version should skip pre-releases when selecting baseline");
    }
}
