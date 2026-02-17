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

using System.Net.Http.Headers;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace DemaConsulting.BuildMark.RepoConnectors.GitHub;

/// <summary>
///     Helper class for executing GitHub GraphQL queries.
/// </summary>
internal sealed class GitHubGraphQLClient : IDisposable
{
    /// <summary>
    ///     Default GitHub GraphQL API endpoint.
    /// </summary>
    private const string DefaultGitHubGraphQLEndpoint = "https://api.github.com/graphql";

    /// <summary>
    ///     GraphQL HTTP client for making GraphQL requests.
    /// </summary>
    private readonly GraphQLHttpClient _graphqlClient;

    /// <summary>
    ///     Indicates whether this instance owns the GraphQL client and should dispose it.
    /// </summary>
    private readonly bool _ownsGraphQLClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubGraphQLClient"/> class.
    /// </summary>
    /// <param name="token">GitHub authentication token.</param>
    /// <param name="graphqlEndpoint">Optional GraphQL endpoint URL. Defaults to public GitHub API. For GitHub Enterprise, use https://your-github-enterprise/api/graphql.</param>
    public GitHubGraphQLClient(string token, string? graphqlEndpoint = null)
    {
        // Initialize HTTP client with authentication and user agent headers
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("BuildMark", "1.0"));

        // Create GraphQL HTTP client with the configured HTTP client
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(graphqlEndpoint ?? DefaultGitHubGraphQLEndpoint)
        };
        _graphqlClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), httpClient);
        _ownsGraphQLClient = true;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubGraphQLClient"/> class with a pre-configured HTTP client.
    /// </summary>
    /// <param name="httpClient">Pre-configured HTTP client for making requests. Useful for testing with mocked responses.</param>
    /// <param name="graphqlEndpoint">Optional GraphQL endpoint URL. Defaults to public GitHub API. For GitHub Enterprise, use https://your-github-enterprise/api/graphql.</param>
    /// <remarks>
    ///     This constructor is intended for testing scenarios where you need to inject a mocked HttpClient with pre-canned responses.
    ///     The caller is responsible for disposing the provided HttpClient.
    /// </remarks>
    internal GitHubGraphQLClient(HttpClient httpClient, string? graphqlEndpoint = null)
    {
        // Use provided HTTP client (typically a mocked one for testing)
        var options = new GraphQLHttpClientOptions
        {
            EndPoint = new Uri(graphqlEndpoint ?? DefaultGitHubGraphQLEndpoint)
        };
        _graphqlClient = new GraphQLHttpClient(options, new SystemTextJsonSerializer(), httpClient);
        _ownsGraphQLClient = false;
    }

    /// <summary>
    ///     Gets all commits for a branch using GraphQL with pagination.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Branch name (e.g., 'main'). Will be automatically converted to fully qualified ref name.</param>
    /// <returns>List of commit SHAs on the branch.</returns>
    public async Task<List<string>> GetCommitsAsync(
        string owner,
        string repo,
        string branch)
    {
        try
        {
            var allCommitShas = new List<string>();
            string? afterCursor = null;
            bool hasNextPage;

            // Convert branch name to fully qualified ref name if needed
            var qualifiedBranch = branch.StartsWith("refs/") ? branch : $"refs/heads/{branch}";

            // Paginate through all commits on the branch
            do
            {
                // Create GraphQL request to get commits for a branch with pagination support
                var request = new GraphQLRequest
                {
                    Query = @"
                        query($owner: String!, $repo: String!, $branch: String!, $after: String) {
                            repository(owner: $owner, name: $repo) {
                                ref(qualifiedName: $branch) {
                                    target {
                                        ... on Commit {
                                            history(first: 100, after: $after) {
                                                nodes {
                                                    oid
                                                }
                                                pageInfo {
                                                    hasNextPage
                                                    endCursor
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }",
                    Variables = new
                    {
                        owner,
                        repo,
                        branch = qualifiedBranch,
                        after = afterCursor
                    }
                };

                // Execute GraphQL query
                var response = await _graphqlClient.SendQueryAsync<GetCommitsResponse>(request);

                // Extract commit SHAs from the GraphQL response, filtering out null or invalid values
                var pageCommitShas = response.Data?.Repository?.Ref?.Target?.History?.Nodes?
                    .Where(n => !string.IsNullOrEmpty(n.Oid))
                    .Select(n => n.Oid!)
                    .ToList() ?? [];

                allCommitShas.AddRange(pageCommitShas);

                // Check if there are more pages
                var pageInfo = response.Data?.Repository?.Ref?.Target?.History?.PageInfo;
                hasNextPage = pageInfo?.HasNextPage ?? false;
                afterCursor = pageInfo?.EndCursor;
            }
            while (hasNextPage);

            // Return list of all commit SHAs
            return allCommitShas;
        }
        catch
        {
            // If GraphQL query fails, return empty list
            return [];
        }
    }

    /// <summary>
    ///     Gets all releases for a repository using GraphQL with pagination.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of release nodes.</returns>
    public async Task<List<ReleaseNode>> GetReleasesAsync(
        string owner,
        string repo)
    {
        try
        {
            var allReleaseNodes = new List<ReleaseNode>();
            string? afterCursor = null;
            bool hasNextPage;

            // Paginate through all releases
            do
            {
                // Create GraphQL request to get releases for a repository with pagination support
                var request = new GraphQLRequest
                {
                    Query = @"
                        query($owner: String!, $repo: String!, $after: String) {
                            repository(owner: $owner, name: $repo) {
                                releases(first: 100, after: $after, orderBy: {field: CREATED_AT, direction: DESC}) {
                                    nodes {
                                        tagName
                                    }
                                    pageInfo {
                                        hasNextPage
                                        endCursor
                                    }
                                }
                            }
                        }",
                    Variables = new
                    {
                        owner,
                        repo,
                        after = afterCursor
                    }
                };

                // Execute GraphQL query
                var response = await _graphqlClient.SendQueryAsync<GetReleasesResponse>(request);

                // Extract release nodes from the GraphQL response, filtering out null or invalid values
                var pageReleaseNodes = response.Data?.Repository?.Releases?.Nodes?
                    .Where(n => !string.IsNullOrEmpty(n.TagName))
                    .ToList() ?? [];

                allReleaseNodes.AddRange(pageReleaseNodes);

                // Check if there are more pages
                var pageInfo = response.Data?.Repository?.Releases?.PageInfo;
                hasNextPage = pageInfo?.HasNextPage ?? false;
                afterCursor = pageInfo?.EndCursor;
            }
            while (hasNextPage);

            // Return list of all release nodes
            return allReleaseNodes;
        }
        catch
        {
            // If GraphQL query fails, return empty list
            return [];
        }
    }

    /// <summary>
    ///     Finds issue IDs linked to a pull request via closingIssuesReferences.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="prNumber">Pull request number.</param>
    /// <returns>List of issue IDs linked to the pull request.</returns>
    public async Task<List<int>> FindIssueIdsLinkedToPullRequestAsync(
        string owner,
        string repo,
        int prNumber)
    {
        try
        {
            var allIssueNumbers = new List<int>();
            string? afterCursor = null;
            bool hasNextPage;

            // Paginate through all closing issues
            do
            {
                // Create GraphQL request to get closing issues for a pull request with pagination support
                var request = new GraphQLRequest
                {
                    Query = @"
                        query($owner: String!, $repo: String!, $prNumber: Int!, $after: String) {
                            repository(owner: $owner, name: $repo) {
                                pullRequest(number: $prNumber) {
                                    closingIssuesReferences(first: 100, after: $after) {
                                        nodes {
                                            number
                                        }
                                        pageInfo {
                                            hasNextPage
                                            endCursor
                                        }
                                    }
                                }
                            }
                        }",
                    Variables = new
                    {
                        owner,
                        repo,
                        prNumber,
                        after = afterCursor
                    }
                };

                // Execute GraphQL query
                var response = await _graphqlClient.SendQueryAsync<FindIssueIdsResponse>(request);

                // Extract issue numbers from the GraphQL response, filtering out null or invalid values
                var pageIssueNumbers = response.Data?.Repository?.PullRequest?.ClosingIssuesReferences?.Nodes?
                    .Where(n => n.Number.HasValue)
                    .Select(n => n.Number!.Value)
                    .ToList() ?? [];

                allIssueNumbers.AddRange(pageIssueNumbers);

                // Check if there are more pages
                var pageInfo = response.Data?.Repository?.PullRequest?.ClosingIssuesReferences?.PageInfo;
                hasNextPage = pageInfo?.HasNextPage ?? false;
                afterCursor = pageInfo?.EndCursor;
            }
            while (hasNextPage);

            // Return list of all linked issue numbers
            return allIssueNumbers;
        }
        catch
        {
            // If GraphQL query fails, return empty list
            return [];
        }
    }

    /// <summary>
    ///     Gets all tags for a repository using GraphQL with pagination.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of tag nodes.</returns>
    public async Task<List<TagNode>> GetAllTagsAsync(
        string owner,
        string repo)
    {
        try
        {
            var allTagNodes = new List<TagNode>();
            string? afterCursor = null;
            bool hasNextPage;

            // Paginate through all tags
            do
            {
                // Create GraphQL request to get tags for a repository with pagination support
                var request = new GraphQLRequest
                {
                    Query = @"
                        query($owner: String!, $repo: String!, $after: String) {
                            repository(owner: $owner, name: $repo) {
                                refs(refPrefix: ""refs/tags/"", first: 100, after: $after) {
                                    nodes {
                                        name
                                        target {
                                            oid
                                        }
                                    }
                                    pageInfo {
                                        hasNextPage
                                        endCursor
                                    }
                                }
                            }
                        }",
                    Variables = new
                    {
                        owner,
                        repo,
                        after = afterCursor
                    }
                };

                // Execute GraphQL query
                var response = await _graphqlClient.SendQueryAsync<GetAllTagsResponse>(request);

                // Extract tag nodes from the GraphQL response, filtering out null or invalid values
                var pageTagNodes = response.Data?.Repository?.Refs?.Nodes?
                    .Where(n => !string.IsNullOrEmpty(n.Name))
                    .ToList() ?? [];

                allTagNodes.AddRange(pageTagNodes);

                // Check if there are more pages
                var pageInfo = response.Data?.Repository?.Refs?.PageInfo;
                hasNextPage = pageInfo?.HasNextPage ?? false;
                afterCursor = pageInfo?.EndCursor;
            }
            while (hasNextPage);

            // Return list of all tag nodes
            return allTagNodes;
        }
        catch
        {
            // If GraphQL query fails, return empty list
            return [];
        }
    }

    /// <summary>
    ///     Disposes the GraphQL client if owned by this instance.
    /// </summary>
    public void Dispose()
    {
        // Clean up GraphQL client resources only if we own it
        if (_ownsGraphQLClient)
        {
            _graphqlClient.Dispose();
        }
    }
}
