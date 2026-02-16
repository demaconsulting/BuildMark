// Copyright (c) 2024-2025 Dema Consulting
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
            // Create GraphQL request to get closing issues for a pull request.
            // Note: Limited to first 100 issues per GitHub API. In practice, PRs rarely have more than 100 linked issues.
            var request = new GraphQLRequest
            {
                Query = @"
                    query($owner: String!, $repo: String!, $prNumber: Int!) {
                        repository(owner: $owner, name: $repo) {
                            pullRequest(number: $prNumber) {
                                closingIssuesReferences(first: 100) {
                                    nodes {
                                        number
                                    }
                                }
                            }
                        }
                    }",
                Variables = new
                {
                    owner,
                    repo,
                    prNumber
                }
            };

            // Execute GraphQL query
            var response = await _graphqlClient.SendQueryAsync<GitHubGraphQLTypes.FindIssueIdsResponse>(request);

            // Extract issue numbers from the GraphQL response, filtering out null or invalid values
            var issueNumbers = response.Data?.Repository?.PullRequest?.ClosingIssuesReferences?.Nodes?
                .Where(n => n.Number.HasValue)
                .Select(n => n.Number!.Value)
                .ToList() ?? [];

            // Return list of linked issue numbers
            return issueNumbers;
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
