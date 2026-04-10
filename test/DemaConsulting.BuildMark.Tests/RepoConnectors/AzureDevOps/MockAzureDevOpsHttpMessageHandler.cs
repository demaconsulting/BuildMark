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

using System.Net;
using System.Text;
using System.Text.Json;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <summary>
///     Represents a mock commit for Azure DevOps testing.
/// </summary>
/// <param name="CommitId">Commit SHA hash.</param>
/// <param name="Comment">Commit message.</param>
internal record MockAdoCommit(string CommitId, string Comment = "commit");

/// <summary>
///     Represents a mock tag reference for Azure DevOps testing.
/// </summary>
/// <param name="Name">Tag name (without refs/tags/ prefix).</param>
/// <param name="ObjectId">Commit SHA hash that this tag points to.</param>
internal record MockAdoTag(string Name, string ObjectId);

/// <summary>
///     Represents a mock pull request for Azure DevOps testing.
/// </summary>
/// <param name="PullRequestId">Pull request identifier.</param>
/// <param name="Title">Pull request title.</param>
/// <param name="Status">Pull request status (active, completed, abandoned).</param>
/// <param name="MergeCommitId">Merge commit SHA when completed.</param>
/// <param name="Description">Pull request description body.</param>
internal record MockAdoPullRequest(
    int PullRequestId,
    string Title,
    string Status = "completed",
    string? MergeCommitId = null,
    string? Description = null);

/// <summary>
///     Represents a mock work item for Azure DevOps testing.
/// </summary>
/// <param name="Id">Work item identifier.</param>
/// <param name="Title">Work item title.</param>
/// <param name="WorkItemType">Work item type (Bug, User Story, Task, etc.).</param>
/// <param name="State">Work item state (Active, Resolved, Closed, Done, etc.).</param>
/// <param name="Description">Work item description body.</param>
/// <param name="CustomVisibility">Custom.Visibility field value.</param>
/// <param name="CustomAffectedVersions">Custom.AffectedVersions field value.</param>
internal record MockAdoWorkItem(
    int Id,
    string Title,
    string WorkItemType = "User Story",
    string State = "Active",
    string? Description = null,
    string? CustomVisibility = null,
    string? CustomAffectedVersions = null);

/// <summary>
///     Mock HTTP message handler for testing Azure DevOps REST API interactions.
/// </summary>
/// <remarks>
///     This class provides a reusable way to mock Azure DevOps REST API responses
///     for testing purposes. It can be configured to return specific responses
///     based on URL patterns or return a default response for all requests.
/// </remarks>
internal sealed class MockAzureDevOpsHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    ///     Dictionary mapping URL patterns to response content.
    /// </summary>
    private readonly Dictionary<string, (string Content, HttpStatusCode StatusCode)> _responses = [];

    /// <summary>
    ///     Default response content to return when no pattern matches.
    /// </summary>
    private string? _defaultResponse;

    /// <summary>
    ///     Default HTTP status code to return with the default response.
    /// </summary>
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;

    /// <summary>
    ///     Adds a response that will be returned when the request URL contains the specified pattern.
    /// </summary>
    /// <param name="urlPattern">URL pattern to match.</param>
    /// <param name="responseContent">Response content to return.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddResponse(
        string urlPattern,
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses[urlPattern] = (responseContent, statusCode);
        return this;
    }

    /// <summary>
    ///     Sets the default response that will be returned when no URL pattern matches.
    /// </summary>
    /// <param name="responseContent">Default response content.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler SetDefaultResponse(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _defaultResponse = responseContent;
        _defaultStatusCode = statusCode;
        return this;
    }

    /// <summary>
    ///     Adds a mock response for the tags (refs) endpoint.
    /// </summary>
    /// <param name="tags">Mock tags to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddTagsResponse(params MockAdoTag[] tags)
    {
        var tagValues = tags.Select(t => new
        {
            name = $"refs/tags/{t.Name}",
            objectId = t.ObjectId
        });

        var json = JsonSerializer.Serialize(new { count = tags.Length, value = tagValues });
        return AddResponse("refs?filter=tags", json);
    }

    /// <summary>
    ///     Adds a mock response for the commits endpoint.
    /// </summary>
    /// <param name="commits">Mock commits to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddCommitsResponse(params MockAdoCommit[] commits)
    {
        var commitValues = commits.Select(c => new { commitId = c.CommitId, comment = c.Comment });
        var json = JsonSerializer.Serialize(new { count = commits.Length, value = commitValues });
        return AddResponse("/commits?", json);
    }

    /// <summary>
    ///     Adds a mock response for the pull requests endpoint.
    /// </summary>
    /// <param name="pullRequests">Mock pull requests to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddPullRequestsResponse(params MockAdoPullRequest[] pullRequests)
    {
        var prValues = pullRequests.Select(pr => new
        {
            pullRequestId = pr.PullRequestId,
            title = pr.Title,
            url = $"https://dev.azure.com/org/project/_git/repo/pullrequest/{pr.PullRequestId}",
            status = pr.Status,
            mergeCommitId = pr.MergeCommitId,
            sourceRefName = "refs/heads/feature",
            description = pr.Description
        });

        var json = JsonSerializer.Serialize(new { count = pullRequests.Length, value = prValues });
        return AddResponse("pullrequests?searchCriteria", json);
    }

    /// <summary>
    ///     Adds a mock response for the pull request work items endpoint.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID to match.</param>
    /// <param name="workItemIds">Work item IDs to return as linked.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddPullRequestWorkItemsResponse(
        int pullRequestId,
        params int[] workItemIds)
    {
        var refs = workItemIds.Select(id => new { id, url = $"https://dev.azure.com/org/project/_apis/wit/workitems/{id}" });
        var json = JsonSerializer.Serialize(new { count = workItemIds.Length, value = refs });
        return AddResponse($"pullrequests/{pullRequestId}/workitems", json);
    }

    /// <summary>
    ///     Adds a mock response for the work items batch endpoint.
    /// </summary>
    /// <param name="workItems">Mock work items to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddWorkItemsResponse(params MockAdoWorkItem[] workItems)
    {
        var wiValues = workItems.Select(wi =>
        {
            var fields = new Dictionary<string, string?>
            {
                ["System.Title"] = wi.Title,
                ["System.WorkItemType"] = wi.WorkItemType,
                ["System.State"] = wi.State,
                ["System.Description"] = wi.Description,
                ["Custom.Visibility"] = wi.CustomVisibility,
                ["Custom.AffectedVersions"] = wi.CustomAffectedVersions
            };

            return new { id = wi.Id, fields };
        });

        var json = JsonSerializer.Serialize(new { count = workItems.Length, value = wiValues });
        return AddResponse("wit/workitems?ids=", json);
    }

    /// <summary>
    ///     Adds a mock response for the WIQL query endpoint.
    /// </summary>
    /// <param name="workItemIds">Work item IDs to return from the query.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddWiqlResponse(params int[] workItemIds)
    {
        var refs = workItemIds.Select(id => new { id, url = $"https://dev.azure.com/org/project/_apis/wit/workitems/{id}" });
        var json = JsonSerializer.Serialize(new { workItems = refs });
        return AddResponse("wit/wiql", json);
    }

    /// <summary>
    ///     Adds a mock response for the repository endpoint.
    /// </summary>
    /// <param name="id">Repository ID.</param>
    /// <param name="name">Repository name.</param>
    /// <param name="remoteUrl">Repository remote URL.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockAzureDevOpsHttpMessageHandler AddRepositoryResponse(
        string id = "repo-id",
        string name = "repo",
        string remoteUrl = "https://dev.azure.com/org/project/_git/repo")
    {
        var json = JsonSerializer.Serialize(new { id, name, remoteUrl });
        return AddResponse("git/repositories/", json);
    }

    /// <summary>
    ///     Processes an HTTP request and returns the mock response.
    /// </summary>
    /// <param name="request">HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mock HTTP response message.</returns>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var requestUrl = request.RequestUri?.ToString() ?? string.Empty;

        // Check for matching URL patterns
        foreach (var (pattern, (content, statusCode)) in _responses)
        {
            if (requestUrl.Contains(pattern, StringComparison.OrdinalIgnoreCase))
            {
                return Task.FromResult(new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(content, Encoding.UTF8, "application/json")
                });
            }
        }

        // Return default response if configured
        if (_defaultResponse != null)
        {
            return Task.FromResult(new HttpResponseMessage(_defaultStatusCode)
            {
                Content = new StringContent(_defaultResponse, Encoding.UTF8, "application/json")
            });
        }

        // Return 404 when no response configured
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(
                $"{{\"message\":\"No mock response configured for URL: {requestUrl}\"}}",
                Encoding.UTF8,
                "application/json")
        });
    }
}
