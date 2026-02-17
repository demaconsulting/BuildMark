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

using System.Net;
using System.Text;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Represents a mock commit for testing.
/// </summary>
/// <param name="Sha">Commit SHA.</param>
public record MockCommit(string Sha);

/// <summary>
///     Represents a mock release for testing.
/// </summary>
/// <param name="TagName">Release tag name.</param>
/// <param name="PublishedAt">Published date/time in ISO 8601 format.</param>
public record MockRelease(
    string TagName,
    string PublishedAt);

/// <summary>
///     Represents a mock tag for testing.
/// </summary>
/// <param name="Name">Tag name.</param>
/// <param name="TargetOid">Target commit OID.</param>
public record MockTag(
    string Name,
    string TargetOid);

/// <summary>
///     Represents a mock pull request for testing.
/// </summary>
/// <param name="Number">Pull request number.</param>
/// <param name="Title">Pull request title.</param>
/// <param name="Url">Pull request URL.</param>
/// <param name="Merged">Whether the PR is merged.</param>
/// <param name="MergeCommitSha">Merge commit SHA (null if not merged).</param>
/// <param name="HeadRefOid">Head reference OID.</param>
/// <param name="Labels">List of label names.</param>
public record MockPullRequest(
    int Number,
    string Title,
    string Url,
    bool Merged,
    string? MergeCommitSha,
    string? HeadRefOid,
    List<string> Labels);

/// <summary>
///     Represents a mock issue for testing.
/// </summary>
/// <param name="Number">Issue number.</param>
/// <param name="Title">Issue title.</param>
/// <param name="Url">Issue URL.</param>
/// <param name="State">Issue state (OPEN, CLOSED).</param>
/// <param name="Labels">List of label names.</param>
public record MockIssue(
    int Number,
    string Title,
    string Url,
    string State,
    List<string> Labels);

/// <summary>
///     Mock HTTP message handler for testing GitHub GraphQL API interactions.
/// </summary>
/// <remarks>
///     This class provides a reusable way to mock GitHub GraphQL API responses
///     for testing purposes. It can be configured to return specific responses
///     based on the request content or return a default response for all requests.
///     Includes helper methods for common GitHub GraphQL response types.
/// </remarks>
public sealed class MockGitHubGraphQLHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    ///     Dictionary mapping request patterns to response content.
    /// </summary>
    private readonly Dictionary<string, (string content, HttpStatusCode statusCode)> _responses = new();

    /// <summary>
    ///     Default response content to return when no pattern matches.
    /// </summary>
    private string? _defaultResponse;

    /// <summary>
    ///     Default HTTP status code to return with the default response.
    /// </summary>
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;

    /// <summary>
    ///     Adds a response that will be returned when the request body contains the specified pattern.
    /// </summary>
    /// <param name="requestPattern">Pattern to look for in the request body.</param>
    /// <param name="responseContent">Response content to return.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddResponse(
        string requestPattern,
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses[requestPattern] = (responseContent, statusCode);
        return this;
    }

    /// <summary>
    ///     Sets the default response that will be returned when no pattern matches.
    /// </summary>
    /// <param name="responseContent">Default response content.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler SetDefaultResponse(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _defaultResponse = responseContent;
        _defaultStatusCode = statusCode;
        return this;
    }

    /// <summary>
    ///     Adds a mock response for GetCommitsAsync with a collection of commits.
    /// </summary>
    /// <param name="commitShas">Commit SHAs to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddCommitsResponse(params string[] commitShas)
    {
        return AddCommitsResponse(commitShas, false, null);
    }

    /// <summary>
    ///     Adds a mock response for GetCommitsAsync with a collection of commits and pagination.
    /// </summary>
    /// <param name="commitShas">Collection of commit SHAs to return.</param>
    /// <param name="hasNextPage">Whether there are more pages.</param>
    /// <param name="endCursor">End cursor for pagination.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddCommitsResponse(
        IEnumerable<string> commitShas,
        bool hasNextPage,
        string? endCursor)
    {
        var commitNodes = string.Join(",\n                                    ", 
            commitShas.Select(sha => $@"{{ ""oid"": ""{sha}"" }}"));
        
        var response = $@"{{
            ""data"": {{
                ""repository"": {{
                    ""ref"": {{
                        ""target"": {{
                            ""history"": {{
                                ""nodes"": [
                                    {commitNodes}
                                ],
                                ""pageInfo"": {{
                                    ""hasNextPage"": {hasNextPage.ToString().ToLowerInvariant()},
                                    ""endCursor"": {(endCursor != null ? $@"""{endCursor}""" : "null")}
                                }}
                            }}
                        }}
                    }}
                }}
            }}
        }}";
        return AddResponse("ref(qualifiedName:", response);
    }

    /// <summary>
    ///     Adds a mock response for GetReleasesAsync with a collection of releases.
    /// </summary>
    /// <param name="releases">Mock releases to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddReleasesResponse(params MockRelease[] releases)
    {
        return AddReleasesResponse(releases, false, null);
    }

    /// <summary>
    ///     Adds a mock response for GetReleasesAsync with a collection of releases and pagination.
    /// </summary>
    /// <param name="releases">Collection of mock releases to return.</param>
    /// <param name="hasNextPage">Whether there are more pages.</param>
    /// <param name="endCursor">End cursor for pagination.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddReleasesResponse(
        IEnumerable<MockRelease> releases,
        bool hasNextPage,
        string? endCursor)
    {
        var releaseNodes = string.Join(",\n                            ",
            releases.Select(r => $@"{{
                                ""tagName"": ""{r.TagName}"",
                                ""publishedAt"": ""{r.PublishedAt}""
                            }}"));

        var response = $@"{{
            ""data"": {{
                ""repository"": {{
                    ""releases"": {{
                        ""nodes"": [
                            {releaseNodes}
                        ],
                        ""pageInfo"": {{
                            ""hasNextPage"": {hasNextPage.ToString().ToLowerInvariant()},
                            ""endCursor"": {(endCursor != null ? $@"""{endCursor}""" : "null")}
                        }}
                    }}
                }}
            }}
        }}";
        return AddResponse("releases(", response);
    }

    /// <summary>
    ///     Adds a mock response for GetAllTagsAsync with a collection of tags.
    /// </summary>
    /// <param name="tags">Mock tags to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddTagsResponse(params MockTag[] tags)
    {
        return AddTagsResponse(tags, false, null);
    }

    /// <summary>
    ///     Adds a mock response for GetAllTagsAsync with a collection of tags and pagination.
    /// </summary>
    /// <param name="tags">Collection of mock tags to return.</param>
    /// <param name="hasNextPage">Whether there are more pages.</param>
    /// <param name="endCursor">End cursor for pagination.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddTagsResponse(
        IEnumerable<MockTag> tags,
        bool hasNextPage,
        string? endCursor)
    {
        var tagNodes = string.Join(",\n                            ",
            tags.Select(t => $@"{{
                                ""name"": ""{t.Name}"",
                                ""target"": {{
                                    ""oid"": ""{t.TargetOid}""
                                }}
                            }}"));

        var response = $@"{{
            ""data"": {{
                ""repository"": {{
                    ""refs"": {{
                        ""nodes"": [
                            {tagNodes}
                        ],
                        ""pageInfo"": {{
                            ""hasNextPage"": {hasNextPage.ToString().ToLowerInvariant()},
                            ""endCursor"": {(endCursor != null ? $@"""{endCursor}""" : "null")}
                        }}
                    }}
                }}
            }}
        }}";
        return AddResponse("refs(refPrefix:", response);
    }

    /// <summary>
    ///     Adds a mock response for GetPullRequestsAsync with a collection of pull requests.
    /// </summary>
    /// <param name="pullRequests">Pull request data to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddPullRequestsResponse(params MockPullRequest[] pullRequests)
    {
        return AddPullRequestsResponse(pullRequests, false, null);
    }

    /// <summary>
    ///     Adds a mock response for GetPullRequestsAsync with a collection of pull requests and pagination.
    /// </summary>
    /// <param name="pullRequests">Collection of pull request data to return.</param>
    /// <param name="hasNextPage">Whether there are more pages.</param>
    /// <param name="endCursor">End cursor for pagination.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddPullRequestsResponse(
        IEnumerable<MockPullRequest> pullRequests,
        bool hasNextPage,
        string? endCursor)
    {
        var prNodes = string.Join(",\n                            ",
            pullRequests.Select(pr =>
            {
                var labelsJson = pr.Labels.Any()
                    ? string.Join(",\n                                        ", pr.Labels.Select(l => $@"{{ ""name"": ""{l}"" }}"))
                    : string.Empty;

                return $@"{{
                                ""number"": {pr.Number},
                                ""title"": ""{pr.Title}"",
                                ""url"": ""{pr.Url}"",
                                ""merged"": {pr.Merged.ToString().ToLowerInvariant()},
                                ""mergeCommit"": {(pr.MergeCommitSha != null ? $@"{{ ""oid"": ""{pr.MergeCommitSha}"" }}" : "null")},
                                ""headRefOid"": {(pr.HeadRefOid != null ? $@"""{pr.HeadRefOid}""" : "null")},
                                ""labels"": {{
                                    ""nodes"": [{labelsJson}]
                                }}
                            }}";
            }));

        var response = $@"{{
            ""data"": {{
                ""repository"": {{
                    ""pullRequests"": {{
                        ""nodes"": [
                            {prNodes}
                        ],
                        ""pageInfo"": {{
                            ""hasNextPage"": {hasNextPage.ToString().ToLowerInvariant()},
                            ""endCursor"": {(endCursor != null ? $@"""{endCursor}""" : "null")}
                        }}
                    }}
                }}
            }}
        }}";
        return AddResponse("pullRequests(", response);
    }

    /// <summary>
    ///     Adds a mock response for GetAllIssuesAsync with a collection of issues.
    /// </summary>
    /// <param name="issues">Issue data to return.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddIssuesResponse(params MockIssue[] issues)
    {
        return AddIssuesResponse(issues, false, null);
    }

    /// <summary>
    ///     Adds a mock response for GetAllIssuesAsync with a collection of issues and pagination.
    /// </summary>
    /// <param name="issues">Collection of issue data to return.</param>
    /// <param name="hasNextPage">Whether there are more pages.</param>
    /// <param name="endCursor">End cursor for pagination.</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddIssuesResponse(
        IEnumerable<MockIssue> issues,
        bool hasNextPage,
        string? endCursor)
    {
        var issueNodes = string.Join(",\n                            ",
            issues.Select(issue =>
            {
                var labelsJson = issue.Labels.Any()
                    ? string.Join(",\n                                        ", issue.Labels.Select(l => $@"{{ ""name"": ""{l}"" }}"))
                    : string.Empty;

                return $@"{{
                                ""number"": {issue.Number},
                                ""title"": ""{issue.Title}"",
                                ""url"": ""{issue.Url}"",
                                ""state"": ""{issue.State}"",
                                ""labels"": {{
                                    ""nodes"": [{labelsJson}]
                                }}
                            }}";
            }));

        var response = $@"{{
            ""data"": {{
                ""repository"": {{
                    ""issues"": {{
                        ""nodes"": [
                            {issueNodes}
                        ],
                        ""pageInfo"": {{
                            ""hasNextPage"": {hasNextPage.ToString().ToLowerInvariant()},
                            ""endCursor"": {(endCursor != null ? $@"""{endCursor}""" : "null")}
                        }}
                    }}
                }}
            }}
        }}";
        return AddResponse("issues(", response);
    }

    /// <summary>
    ///     Adds an empty response that returns repository as null (for error scenarios).
    /// </summary>
    /// <returns>This instance for method chaining.</returns>
    public MockGitHubGraphQLHttpMessageHandler AddEmptyRepositoryResponse()
    {
        return SetDefaultResponse(@"{""data"":{""repository"":null}}");
    }

    /// <summary>
    ///     Sends a mock HTTP response based on the request content.
    /// </summary>
    /// <param name="request">HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mock HTTP response.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Read request body to determine which response to return
        var requestBody = request.Content != null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;

        // Find matching response based on request pattern
        foreach (var (pattern, (content, statusCode)) in _responses)
        {
            if (requestBody.Contains(pattern, StringComparison.Ordinal))
            {
                return CreateResponse(content, statusCode);
            }
        }

        // Return default response if no pattern matches
        if (_defaultResponse != null)
        {
            return CreateResponse(_defaultResponse, _defaultStatusCode);
        }

        // Return 500 error if no response is configured
        return CreateResponse(
            @"{""errors"":[{""message"":""No mock response configured""}]}",
            HttpStatusCode.InternalServerError);
    }

    /// <summary>
    ///     Creates an HTTP response message with the specified content and status code.
    /// </summary>
    /// <param name="content">Response content.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>HTTP response message.</returns>
    private static HttpResponseMessage CreateResponse(string content, HttpStatusCode statusCode)
    {
        // Create response with content
        // Note: The returned HttpResponseMessage will be disposed by HttpClient,
        // which also disposes the Content. This is the expected pattern for HttpMessageHandler.
        var responseContent = new StringContent(content, Encoding.UTF8, "application/json");
        return new HttpResponseMessage(statusCode)
        {
            Content = responseContent
        };
    }
}
