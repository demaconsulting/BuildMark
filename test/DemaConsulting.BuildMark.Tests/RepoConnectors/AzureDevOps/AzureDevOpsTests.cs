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
using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <summary>
///     Sub-subsystem tests for the AzureDevOps sub-subsystem.
///     These tests verify the contract exposed by the AzureDevOps sub-subsystem as a whole,
///     exercising the connector through its public IRepoConnector interface.
/// </summary>
public class AzureDevOpsTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-SubSystem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the AzureDevOps sub-subsystem provides a connector that implements IRepoConnector.
    /// </summary>
    [Fact]
    public void AzureDevOps_ImplementsInterface_ReturnsTrue()
    {
        // Arrange: create an AzureDevOpsRepoConnector instance from the AzureDevOps sub-subsystem
        var connector = new AzureDevOpsRepoConnector();

        // Assert: the sub-subsystem connector satisfies the shared IRepoConnector interface
        Assert.IsAssignableFrom<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the AzureDevOps sub-subsystem returns valid build information from mocked API data.
    /// </summary>
    /// <remarks>
    ///     What is being tested: AzureDevOps sub-subsystem end-to-end build information retrieval
    ///     What the assertions prove: Build information is complete and accurate for a single tag
    /// </remarks>
    [Fact]
    public async Task AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange: set up a mocked REST handler with a single tag and commit
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
            .AddCommitsResponse(new MockAdoCommit("abc123"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "abc123");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: build information is complete and accurate
        Assert.NotNull(buildInfo);
        Assert.Equal("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.Equal("abc123", buildInfo.CurrentVersionTag.CommitHash);
        Assert.NotNull(buildInfo.Changes);
        Assert.NotNull(buildInfo.Bugs);
        Assert.NotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that the AzureDevOps sub-subsystem gathers changes from pull requests.
    /// </summary>
    /// <remarks>
    ///     What is being tested: AzureDevOps sub-subsystem PR-based change gathering
    ///     What the assertions prove: Merged PR work items appear in the Changes collection
    /// </remarks>
    [Fact]
    public async Task AzureDevOps_GetBuildInformation_WithPullRequests_GathersChanges()
    {
        // Arrange: set up two versions with a PR merged between them
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Add feature", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Feature work item", "User Story"))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "commit2");

        // Act: retrieve build information for v1.1.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert: changes include the work item from the PR
        Assert.NotNull(buildInfo);
        Assert.NotEmpty(buildInfo.Changes);
    }

    /// <summary>
    ///     Test that the AzureDevOps sub-subsystem identifies open work items as known issues.
    /// </summary>
    /// <remarks>
    ///     What is being tested: AzureDevOps sub-subsystem known-issues identification
    ///     What the assertions prove: Open bug work items from WIQL queries appear in KnownIssues
    /// </remarks>
    [Fact]
    public async Task AzureDevOps_GetBuildInformation_WithOpenWorkItems_IdentifiesKnownIssues()
    {
        // Arrange: set up a version with an open bug from WIQL query
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
            .AddCommitsResponse(new MockAdoCommit("abc123"))
            .AddPullRequestsResponse()
            .AddWiqlResponse(500)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(500, "Open bug", "Bug", "Active"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "abc123");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: known issues include the open bug
        Assert.NotNull(buildInfo);
        Assert.NotEmpty(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that the AzureDevOps sub-subsystem skips pre-release tags for release versions.
    /// </summary>
    /// <remarks>
    ///     What is being tested: AzureDevOps sub-subsystem pre-release tag handling
    ///     What the assertions prove: A release version uses only prior release tags as its baseline
    /// </remarks>
    [Fact]
    public async Task AzureDevOps_GetBuildInformation_ReleaseVersion_SkipsPreReleases()
    {
        // Arrange: a release version with a pre-release between it and the previous release
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v2.0.0", "commit3"),
                new MockAdoTag("v2.0.0-rc.1", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "commit3");

        // Act: retrieve build information for v2.0.0 (a release version)
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: baseline should be v1.0.0, skipping the pre-release v2.0.0-rc.1
        Assert.NotNull(buildInfo);
        Assert.NotNull(buildInfo.BaselineVersionTag);
        Assert.Equal("1.0.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
    }

    /// <summary>
    ///     Creates a mock Azure DevOps connector with pre-configured git command responses.
    /// </summary>
    /// <param name="mockHttpClient">Mock HTTP client for REST API.</param>
    /// <param name="currentCommitHash">Current commit hash to return from git rev-parse HEAD.</param>
    /// <returns>Configured MockableAzureDevOpsRepoConnector.</returns>
    private static MockableAzureDevOpsRepoConnector CreateMockAdoConnector(
        HttpClient mockHttpClient,
        string currentCommitHash)
    {
        var connector = new MockableAzureDevOpsRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin",
            "https://dev.azure.com/org/project/_git/repo");
        connector.SetCommandResponse("git rev-parse HEAD", currentCommitHash);
        connector.SetCommandResponse(
            "az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv",
            "mock-token");
        return connector;
    }
}
