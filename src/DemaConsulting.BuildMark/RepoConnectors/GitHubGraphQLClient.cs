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
using System.Text;
using System.Text.Json;

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
/// Helper class for executing GitHub GraphQL queries.
/// </summary>
internal sealed class GitHubGraphQLClient : IDisposable
{
    /// <summary>
    /// Default GitHub GraphQL API endpoint.
    /// </summary>
    private const string DefaultGitHubGraphQLEndpoint = "https://api.github.com/graphql";

    /// <summary>
    /// HTTP client for making GraphQL requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    /// GraphQL endpoint URL.
    /// </summary>
    private readonly string _graphqlEndpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubGraphQLClient"/> class.
    /// </summary>
    /// <param name="token">GitHub authentication token.</param>
    /// <param name="graphqlEndpoint">Optional GraphQL endpoint URL. Defaults to public GitHub API. For GitHub Enterprise, use https://your-github-enterprise/api/graphql.</param>
    public GitHubGraphQLClient(string token, string? graphqlEndpoint = null)
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);
        _httpClient.DefaultRequestHeaders.UserAgent.Add(
            new ProductInfoHeaderValue("BuildMark", "1.0"));
        _graphqlEndpoint = graphqlEndpoint ?? DefaultGitHubGraphQLEndpoint;
    }

    /// <summary>
    /// Finds issue IDs linked to a pull request via closingIssuesReferences.
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
            // GraphQL query to get closing issues for a pull request
            var graphqlQuery = new
            {
                query = @"
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
                variables = new
                {
                    owner,
                    repo,
                    prNumber
                }
            };

            var jsonContent = JsonSerializer.Serialize(graphqlQuery);
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_graphqlEndpoint, content);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(responseBody);

            // Extract issue numbers from the GraphQL response
            var issueNumbers = new List<int>();
            if (jsonDoc.RootElement.TryGetProperty("data", out var data) &&
                data.TryGetProperty("repository", out var repository) &&
                repository.TryGetProperty("pullRequest", out var pullRequest) &&
                pullRequest.TryGetProperty("closingIssuesReferences", out var closingIssues) &&
                closingIssues.TryGetProperty("nodes", out var nodes))
            {
                foreach (var node in nodes.EnumerateArray().Where(n => n.TryGetProperty("number", out _)))
                {
                    node.TryGetProperty("number", out var number);
                    issueNumbers.Add(number.GetInt32());
                }
            }

            return issueNumbers;
        }
        catch
        {
            // If GraphQL query fails, return empty list
            return [];
        }
    }

    /// <summary>
    /// Disposes the HTTP client.
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }
}
