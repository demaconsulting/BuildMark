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

using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <summary>
///     Unit tests for the AzureDevOpsRestClient class.
/// </summary>
public class AzureDevOpsRestClientTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-RestClient
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that GetRepositoryAsync returns a repository record from a valid response.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetRepositoryAsync_ValidResponse_ReturnsRepository()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddRepositoryResponse("repo-123", "MyRepo", "https://dev.azure.com/org/project/_git/MyRepo");
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var repo = await client.GetRepositoryAsync("MyRepo");

        // Assert
        Assert.NotNull(repo);
        Assert.Equal("repo-123", repo.Id);
        Assert.Equal("MyRepo", repo.Name);
        Assert.Equal("https://dev.azure.com/org/project/_git/MyRepo", repo.RemoteUrl);
    }

    /// <summary>
    ///     Verify that GetCommitsAsync returns commits from a valid response.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddCommitsResponse(
                new MockAdoCommit("abc123", "First commit"),
                new MockAdoCommit("def456", "Second commit"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var commits = await client.GetCommitsAsync("repo-id");

        // Assert
        Assert.NotNull(commits);
        Assert.Equal(2, commits.Count);
        Assert.Equal("abc123", commits[0].CommitId);
        Assert.Equal("First commit", commits[0].Comment);
        Assert.Equal("def456", commits[1].CommitId);
    }

    /// <summary>
    ///     Verify that GetPullRequestsAsync returns pull requests from a valid response.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddPullRequestsResponse(
                new MockAdoPullRequest(101, "Feature PR", "completed", "merge-commit-1"),
                new MockAdoPullRequest(102, "Bug PR", "active"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var prs = await client.GetPullRequestsAsync("repo-id");

        // Assert
        Assert.NotNull(prs);
        Assert.Equal(2, prs.Count);
        Assert.Equal(101, prs[0].PullRequestId);
        Assert.Equal("Feature PR", prs[0].Title);
        Assert.Equal("completed", prs[0].Status);
        Assert.Equal("merge-commit-1", prs[0].MergeCommitId);
    }

    /// <summary>
    ///     Verify that GetPullRequestWorkItemsAsync returns work item references from a valid response.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_ValidResponse_ReturnsWorkItemRefs()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddPullRequestWorkItemsResponse("repo-id", 101, 200, 201);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItemRefs = await client.GetPullRequestWorkItemsAsync("repo-id", 101);

        // Assert
        Assert.NotNull(workItemRefs);
        Assert.Equal(2, workItemRefs.Count);
        Assert.Equal(200, workItemRefs[0].Id);
        Assert.Equal(201, workItemRefs[1].Id);
    }

    /// <summary>
    ///     Verify that GetWorkItemsAsync returns work item details from a valid response.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Bug work item", "Bug", "Active"),
                new MockAdoWorkItem(201, "Feature work item", "User Story", "Resolved"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItems = await client.GetWorkItemsAsync([200, 201]);

        // Assert
        Assert.NotNull(workItems);
        Assert.Equal(2, workItems.Count);
        Assert.Equal(200, workItems[0].Id);
        Assert.Equal(201, workItems[1].Id);
    }

    /// <summary>
    ///     Verify that QueryWorkItemsAsync returns work item ids for a valid WIQL query.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_QueryWorkItemsAsync_ValidWiql_ReturnsWorkItemIds()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddWiqlResponse(300, 301, 302);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var query = await client.QueryWorkItemsAsync("SELECT [System.Id] FROM workitems WHERE [System.WorkItemType] = 'Bug'");

        // Assert
        Assert.NotNull(query);
        Assert.Equal(3, query.WorkItems.Count);
        Assert.Equal(300, query.WorkItems[0].Id);
        Assert.Equal(301, query.WorkItems[1].Id);
        Assert.Equal(302, query.WorkItems[2].Id);
    }

    /// <summary>
    ///     Verify that GetPullRequestWorkItemsAsync deserializes string-valued ids
    ///     as returned by the Azure DevOps PR work items endpoint.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_StringValuedIds_DeserializesCorrectly()
    {
        // Arrange - raw JSON matching the real Azure DevOps response format where id is a string
        const string json = """{"count":2,"value":[{"id":"1234","url":"https://dev.azure.com/org/project/_apis/wit/workItems/1234"},{"id":"5678","url":"https://dev.azure.com/org/project/_apis/wit/workItems/5678"}]}""";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddResponse("pullrequests/101/workitems", json);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItemRefs = await client.GetPullRequestWorkItemsAsync("repo-id", 101);

        // Assert
        Assert.NotNull(workItemRefs);
        Assert.Equal(2, workItemRefs.Count);
        Assert.Equal(1234, workItemRefs[0].Id);
        Assert.Equal(5678, workItemRefs[1].Id);
    }

    /// <summary>
    ///     Verify that QueryWorkItemsAsync deserializes string-valued ids
    ///     when the WIQL endpoint returns them as strings.
    /// </summary>
    [Fact]
    public async Task AzureDevOpsRestClient_QueryWorkItemsAsync_StringValuedIds_DeserializesCorrectly()
    {
        // Arrange - raw JSON with string-valued ids
        const string json = """{"workItems":[{"id":"300","url":"https://dev.azure.com/org/project/_apis/wit/workItems/300"},{"id":"301","url":"https://dev.azure.com/org/project/_apis/wit/workItems/301"}]}""";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddResponse("wit/wiql", json);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var query = await client.QueryWorkItemsAsync("SELECT [System.Id] FROM workitems WHERE [System.WorkItemType] = 'Bug'");

        // Assert
        Assert.NotNull(query);
        Assert.Equal(2, query.WorkItems.Count);
        Assert.Equal(300, query.WorkItems[0].Id);
        Assert.Equal(301, query.WorkItems[1].Id);
    }
}
