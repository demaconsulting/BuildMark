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

using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

/// <summary>
///     HTTP client wrapper for Azure DevOps REST API operations.
/// </summary>
internal sealed class AzureDevOpsRestClient : IDisposable
{
    /// <summary>
    ///     Azure DevOps REST API version used for all requests.
    /// </summary>
    private const string ApiVersion = "7.1";

    /// <summary>
    ///     Shared JSON serializer options configured for Azure DevOps camelCase responses.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    ///     The HTTP client used for API requests.
    /// </summary>
    private readonly HttpClient _httpClient;

    /// <summary>
    ///     Indicates whether this instance owns the HTTP client and should dispose it.
    /// </summary>
    private readonly bool _ownsHttpClient;

    /// <summary>
    ///     The Azure DevOps organization URL (e.g., "https://dev.azure.com/myorg").
    /// </summary>
    private readonly string _organizationUrl;

    /// <summary>
    ///     The Azure DevOps project name.
    /// </summary>
    private readonly string _project;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureDevOpsRestClient"/> class.
    /// </summary>
    /// <param name="organizationUrl">Azure DevOps organization URL.</param>
    /// <param name="project">Azure DevOps project name.</param>
    /// <param name="token">Authentication token (PAT or Entra ID access token).</param>
    /// <param name="isBearer">True for Bearer token (Entra ID), false for Basic auth (PAT).</param>
    public AzureDevOpsRestClient(string organizationUrl, string project, string token, bool isBearer = false)
    {
        _organizationUrl = organizationUrl.TrimEnd('/');
        _project = project;
        _httpClient = new HttpClient();
        _ownsHttpClient = true;

        // Configure authentication header based on token type
        if (isBearer)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
        else
        {
            var credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{token}"));
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", credentials);
        }

        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureDevOpsRestClient"/> class with a pre-configured HTTP client.
    /// </summary>
    /// <param name="httpClient">Pre-configured HTTP client (for testing).</param>
    /// <param name="organizationUrl">Azure DevOps organization URL.</param>
    /// <param name="project">Azure DevOps project name.</param>
    internal AzureDevOpsRestClient(HttpClient httpClient, string organizationUrl, string project)
    {
        _httpClient = httpClient;
        _organizationUrl = organizationUrl.TrimEnd('/');
        _project = project;
        _ownsHttpClient = false;
    }

    /// <summary>
    ///     Fetches repository metadata for the specified repository.
    /// </summary>
    /// <param name="repository">Repository name.</param>
    /// <returns>Repository metadata record.</returns>
    public async Task<AzureDevOpsRepository> GetRepositoryAsync(string repository)
    {
        var url = $"{_organizationUrl}/{_project}/_apis/git/repositories/{repository}?api-version={ApiVersion}";
        var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureDevOpsRepository>(JsonOptions).ConfigureAwait(false);
        return result ?? throw new InvalidOperationException("Failed to deserialize repository response.");
    }

    /// <summary>
    ///     Fetches all tags (refs) for the specified repository.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <returns>List of tag references.</returns>
    public async Task<List<AzureDevOpsRef>> GetTagsAsync(string repositoryId)
    {
        var url = $"{_organizationUrl}/{_project}/_apis/git/repositories/{repositoryId}/refs?filter=tags&api-version={ApiVersion}";
        var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureDevOpsCollectionResponse<AzureDevOpsRef>>(JsonOptions).ConfigureAwait(false);
        return result?.Value ?? [];
    }

    /// <summary>
    ///     Fetches the complete paginated commit history for the repository.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <returns>List of commit records.</returns>
    public async Task<List<AzureDevOpsCommit>> GetCommitsAsync(string repositoryId)
    {
        List<AzureDevOpsCommit> allCommits = [];
        var skip = 0;
        const int top = 1000;

        while (true)
        {
            var url = $"{_organizationUrl}/{_project}/_apis/git/repositories/{repositoryId}/commits?$top={top}&$skip={skip}&api-version={ApiVersion}";
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AzureDevOpsCollectionResponse<AzureDevOpsCommit>>(JsonOptions).ConfigureAwait(false);
            if (result?.Value == null || result.Value.Count == 0)
            {
                break;
            }

            allCommits.AddRange(result.Value);

            // Stop when we received fewer items than requested (last page)
            if (result.Value.Count < top)
            {
                break;
            }

            skip += top;
        }

        return allCommits;
    }

    /// <summary>
    ///     Fetches all pull requests with the specified status for the repository.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="status">Pull request status filter (all, active, completed, abandoned).</param>
    /// <returns>List of pull request records.</returns>
    public async Task<List<AzureDevOpsPullRequest>> GetPullRequestsAsync(string repositoryId, string status = "all")
    {
        List<AzureDevOpsPullRequest> allPrs = [];
        var skip = 0;
        const int top = 1000;

        while (true)
        {
            var url = $"{_organizationUrl}/{_project}/_apis/git/repositories/{repositoryId}/pullrequests?searchCriteria.status={status}&$top={top}&$skip={skip}&api-version={ApiVersion}";
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AzureDevOpsCollectionResponse<AzureDevOpsPullRequest>>(JsonOptions).ConfigureAwait(false);
            if (result?.Value == null || result.Value.Count == 0)
            {
                break;
            }

            allPrs.AddRange(result.Value);

            // Stop when we received fewer items than requested (last page)
            if (result.Value.Count < top)
            {
                break;
            }

            skip += top;
        }

        return allPrs;
    }

    /// <summary>
    ///     Fetches the work items linked to a specific pull request.
    /// </summary>
    /// <param name="repositoryId">Repository identifier.</param>
    /// <param name="pullRequestId">Pull request identifier.</param>
    /// <returns>List of work item id references.</returns>
    public async Task<List<AzureDevOpsWorkItemRef>> GetPullRequestWorkItemsAsync(string repositoryId, int pullRequestId)
    {
        var url = $"{_organizationUrl}/{_project}/_apis/git/repositories/{repositoryId}/pullrequests/{pullRequestId}/workitems?api-version={ApiVersion}";
        var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureDevOpsCollectionResponse<AzureDevOpsWorkItemRef>>(JsonOptions).ConfigureAwait(false);
        return result?.Value ?? [];
    }

    /// <summary>
    ///     Batch-fetches work item details for a list of work item ids.
    ///     Splits requests into batches of 200 ids as required by the Azure DevOps API.
    /// </summary>
    /// <param name="workItemIds">Collection of work item ids to fetch.</param>
    /// <returns>List of work item records with all fields expanded.</returns>
    public async Task<List<AzureDevOpsWorkItem>> GetWorkItemsAsync(IEnumerable<int> workItemIds)
    {
        List<AzureDevOpsWorkItem> allWorkItems = [];
        var idList = workItemIds.ToList();

        // Process in batches of 200
        for (var i = 0; i < idList.Count; i += 200)
        {
            var batchIds = idList.Skip(i).Take(200);
            var ids = string.Join(",", batchIds);
            var url = $"{_organizationUrl}/{_project}/_apis/wit/workitems?ids={ids}&$expand=all&api-version={ApiVersion}";
            var response = await _httpClient.GetAsync(url).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AzureDevOpsCollectionResponse<AzureDevOpsWorkItem>>(JsonOptions).ConfigureAwait(false);
            if (result?.Value != null)
            {
                allWorkItems.AddRange(result.Value);
            }
        }

        return allWorkItems;
    }

    /// <summary>
    ///     Executes a WIQL query and returns the matching work item id references.
    /// </summary>
    /// <param name="wiql">WIQL query string.</param>
    /// <returns>Work item query result with matching id references.</returns>
    public async Task<AzureDevOpsWorkItemQuery> QueryWorkItemsAsync(string wiql)
    {
        var url = $"{_organizationUrl}/{_project}/_apis/wit/wiql?api-version={ApiVersion}";
        var content = new StringContent(
            JsonSerializer.Serialize(new { query = wiql }),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<AzureDevOpsWorkItemQuery>(JsonOptions).ConfigureAwait(false);
        return result ?? new AzureDevOpsWorkItemQuery([]);
    }

    /// <summary>
    ///     Disposes the HTTP client if this instance owns it.
    /// </summary>
    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }
}
