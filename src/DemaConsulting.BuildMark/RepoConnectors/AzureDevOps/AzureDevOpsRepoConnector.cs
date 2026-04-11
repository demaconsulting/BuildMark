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

using System.Globalization;
using DemaConsulting.BuildMark.BuildNotes;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

/// <summary>
///     Azure DevOps repository connector implementation using REST API.
/// </summary>
public class AzureDevOpsRepoConnector : RepoConnectorBase
{
    /// <summary>
    ///     The optional Azure DevOps connector configuration overrides.
    /// </summary>
    private readonly AzureDevOpsConnectorConfig? _config;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AzureDevOpsRepoConnector"/> class.
    /// </summary>
    /// <param name="config">Optional Azure DevOps connector overrides.</param>
    public AzureDevOpsRepoConnector(AzureDevOpsConnectorConfig? config = null)
    {
        _config = config;
    }

    /// <summary>
    ///     Gets the optional Azure DevOps connector configuration overrides.
    /// </summary>
    internal AzureDevOpsConnectorConfig? ConfigurationOverrides => _config;

    /// <summary>
    ///     Creates an Azure DevOps REST client for API operations.
    /// </summary>
    /// <param name="organizationUrl">Azure DevOps organization URL.</param>
    /// <param name="project">Azure DevOps project name.</param>
    /// <param name="token">Authentication token.</param>
    /// <param name="isBearer">True for Bearer auth, false for Basic (PAT) auth.</param>
    /// <returns>A new AzureDevOpsRestClient instance.</returns>
    internal virtual AzureDevOpsRestClient CreateRestClient(
        string organizationUrl,
        string project,
        string token,
        bool isBearer)
    {
        return new AzureDevOpsRestClient(organizationUrl, project, token, isBearer);
    }

    /// <summary>
    ///     Gets build information for a release from Azure DevOps.
    /// </summary>
    /// <param name="version">Optional target version.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public override async Task<BuildInformation> GetBuildInformationAsync(VersionTag? version = null)
    {
        // Get repository metadata using git commands
        var repoUrl = await RunCommandAsync("git", "remote", "get-url", "origin");
        var currentCommitHash = await RunCommandAsync("git", "rev-parse", "HEAD");

        // Parse Azure DevOps organization, project, and repository from URL
        var (parsedOrgUrl, parsedProject, parsedRepo) = ParseAzureDevOpsUrl(repoUrl);
        var organizationUrl = _config?.OrganizationUrl ?? parsedOrgUrl;
        var project = _config?.Project ?? parsedProject;
        var repository = _config?.Repository ?? parsedRepo;

        // Get authentication token
        var (token, isBearer) = await GetAzureDevOpsTokenAsync();

        // Create REST client
        using var restClient = CreateRestClient(organizationUrl, project, token, isBearer);

        // Fetch all data from Azure DevOps
        var adoData = await FetchAzureDevOpsDataAsync(restClient, repository);

        // Build lookup dictionaries and mappings
        var lookupData = BuildLookupData(adoData, project, organizationUrl);

        // Determine the target version and hash
        var (toVersion, toHash) = DetermineTargetVersion(version, currentCommitHash.Trim(), lookupData);

        // Determine the baseline version
        var (fromVersion, fromHash) = DetermineBaselineVersion(toVersion, toHash, lookupData);

        // Get commits in range
        var commitsInRange = GetCommitsInRange(adoData.Commits, fromHash, toHash);

        // Collect changes from pull requests
        var (bugs, nonBugChanges, allChangeIds) = await CollectChangesFromPullRequestsAsync(
            restClient,
            commitsInRange,
            lookupData);

        // Collect known issues via WIQL query
        var knownIssues = await CollectKnownIssuesAsync(restClient, allChangeIds, lookupData);

        // Sort all lists by Index to ensure chronological order
        nonBugChanges.Sort((a, b) => a.Index.CompareTo(b.Index));
        bugs.Sort((a, b) => a.Index.CompareTo(b.Index));
        knownIssues.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Apply routing rules if configured
        IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>? routedSections = null;
        if (HasRules)
        {
            var allItems = nonBugChanges.Concat(bugs).Concat(knownIssues);
            routedSections = ApplyRules(allItems);
        }

        // Build version tags
        var currentTag = new VersionCommitTag(toVersion, toHash);
        var baselineTag = fromVersion != null && fromHash != null
            ? new VersionCommitTag(fromVersion, fromHash)
            : null;

        // Create and return build information
        return new BuildInformation(
            baselineTag,
            currentTag,
            nonBugChanges,
            bugs,
            knownIssues,
            null)
        {
            RoutedSections = routedSections
        };
    }

    /// <summary>
    ///     Container for Azure DevOps data fetched from the API.
    /// </summary>
    internal sealed record AzureDevOpsData(
        List<AzureDevOpsCommit> Commits,
        List<AzureDevOpsRef> Tags,
        List<AzureDevOpsPullRequest> PullRequests);

    /// <summary>
    ///     Container for lookup data structures built from Azure DevOps data.
    /// </summary>
    internal sealed record LookupData(
        Dictionary<string, AzureDevOpsPullRequest> CommitHashToPr,
        List<VersionTag> TagVersions,
        Dictionary<string, string> TagToCommitHash,
        string Project,
        string OrganizationUrl);

    /// <summary>
    ///     Fetches all required data from Azure DevOps API in parallel.
    /// </summary>
    /// <param name="restClient">Azure DevOps REST client.</param>
    /// <param name="repository">Repository name.</param>
    /// <returns>Container with all fetched Azure DevOps data.</returns>
    private static async Task<AzureDevOpsData> FetchAzureDevOpsDataAsync(
        AzureDevOpsRestClient restClient,
        string repository)
    {
        // Fetch all data in parallel
        var tagsTask = restClient.GetTagsAsync(repository);
        var commitsTask = restClient.GetCommitsAsync(repository);
        var prsTask = restClient.GetPullRequestsAsync(repository, "all");

        await Task.WhenAll(tagsTask, commitsTask, prsTask);

        return new AzureDevOpsData(
            await commitsTask,
            await tagsTask,
            await prsTask);
    }

    /// <summary>
    ///     Builds lookup data structures from Azure DevOps data.
    /// </summary>
    /// <param name="data">Azure DevOps data.</param>
    /// <param name="project">Azure DevOps project name.</param>
    /// <param name="organizationUrl">Azure DevOps organization URL.</param>
    /// <returns>Container with all lookup data structures.</returns>
    private static LookupData BuildLookupData(AzureDevOpsData data, string project, string organizationUrl)
    {
        // Build a set of commit hashes in the current branch
        var branchCommitHashes = new HashSet<string>(data.Commits.Select(c => c.CommitId), StringComparer.Ordinal);

        // Build tag-to-commit mapping, stripping "refs/tags/" prefix
        var tagToCommitHash = data.Tags
            .Where(t => branchCommitHashes.Contains(t.ObjectId))
            .ToDictionary(
                t => t.Name.StartsWith("refs/tags/", StringComparison.OrdinalIgnoreCase)
                    ? t.Name["refs/tags/".Length..]
                    : t.Name,
                t => t.ObjectId);

        // Parse tags into VersionTag objects, ordered by version (newest first)
        var tagVersions = tagToCommitHash.Keys
            .Select(VersionTag.TryCreate)
            .Where(v => v != null)
            .Cast<VersionTag>()
            .OrderByDescending(v => v.Semantic.Comparable)
            .ToList();

        // Build commit-to-PR mapping for completed PRs
        var commitHashToPr = data.PullRequests
            .Where(p => p is { Status: "completed", MergeCommitId: not null })
            .GroupBy(p => p.MergeCommitId!)
            .ToDictionary(g => g.Key, g => g.First());

        return new LookupData(commitHashToPr, tagVersions, tagToCommitHash, project, organizationUrl);
    }

    /// <summary>
    ///     Determines the target version and hash for the build.
    /// </summary>
    /// <param name="version">Optional target version provided by caller.</param>
    /// <param name="currentCommitHash">Current commit hash.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (toVersion, toHash).</returns>
    private static (VersionTag toVersion, string toHash) DetermineTargetVersion(
        VersionTag? version,
        string currentCommitHash,
        LookupData lookupData)
    {
        var toHash = currentCommitHash;

        // Return early if version was explicitly provided
        if (version != null)
        {
            return (version, toHash);
        }

        // Validate that repository has version tags
        if (lookupData.TagVersions.Count == 0)
        {
            throw new InvalidOperationException(
                "No version tags found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Find the latest tag that points to the current commit
        foreach (var tagVersion in lookupData.TagVersions)
        {
            if (lookupData.TagToCommitHash.TryGetValue(tagVersion.Tag, out var tagHash) &&
                tagHash == toHash)
            {
                return (tagVersion, toHash);
            }
        }

        throw new InvalidOperationException(
            "Target version not specified and current commit does not match any version tag. " +
            "Please provide a version parameter.");
    }

    /// <summary>
    ///     Determines the baseline version for comparing changes.
    /// </summary>
    /// <param name="toVersion">Target version.</param>
    /// <param name="toHash">Commit hash of target version.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (fromVersion, fromHash).</returns>
    private static (VersionTag? fromVersion, string? fromHash) DetermineBaselineVersion(
        VersionTag toVersion,
        string toHash,
        LookupData lookupData)
    {
        if (lookupData.TagVersions.Count == 0)
        {
            return (null, null);
        }

        var toIndex = FindVersionIndex(lookupData.TagVersions, toVersion);

        var fromVersion = toVersion.IsPreRelease
            ? DetermineBaselineForPreRelease(toIndex, toHash, lookupData)
            : DetermineBaselineForRelease(toIndex, lookupData.TagVersions);

        if (fromVersion != null &&
            lookupData.TagToCommitHash.TryGetValue(fromVersion.Tag, out var fromHash))
        {
            return (fromVersion, fromHash);
        }

        return (fromVersion, null);
    }

    /// <summary>
    ///     Determines the baseline version for a pre-release.
    /// </summary>
    /// <param name="toIndex">Index of target version in version list.</param>
    /// <param name="toHash">Commit hash of target version.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Baseline version or null.</returns>
    private static VersionTag? DetermineBaselineForPreRelease(int toIndex, string toHash, LookupData lookupData)
    {
        var versions = lookupData.TagVersions;
        int startIndex;

        if (toIndex >= 0 && toIndex < versions.Count - 1)
        {
            startIndex = toIndex + 1;
        }
        else if (toIndex == -1 && versions.Count > 0)
        {
            startIndex = 0;
        }
        else
        {
            return null;
        }

        // Search for previous version with different commit hash
        for (var i = startIndex; i < versions.Count; i++)
        {
            if (lookupData.TagToCommitHash.TryGetValue(versions[i].Tag, out var candidateHash) &&
                candidateHash != toHash)
            {
                return versions[i];
            }
        }

        return null;
    }

    /// <summary>
    ///     Determines the baseline version for a release (non-pre-release).
    /// </summary>
    /// <param name="toIndex">Index of target version in version list.</param>
    /// <param name="versions">List of version tags.</param>
    /// <returns>Baseline version or null.</returns>
    private static VersionTag? DetermineBaselineForRelease(int toIndex, List<VersionTag> versions)
    {
        int startIndex;

        if (toIndex >= 0 && toIndex < versions.Count - 1)
        {
            startIndex = toIndex + 1;
        }
        else if (toIndex == -1 && versions.Count > 0)
        {
            startIndex = 0;
        }
        else
        {
            return null;
        }

        // Search for previous non-pre-release version
        for (var i = startIndex; i < versions.Count; i++)
        {
            if (!versions[i].IsPreRelease)
            {
                return versions[i];
            }
        }

        return null;
    }

    /// <summary>
    ///     Gets commits in the range from fromHash (exclusive) to toHash (inclusive).
    /// </summary>
    /// <param name="commits">All commits.</param>
    /// <param name="fromHash">Starting commit hash (exclusive).</param>
    /// <param name="toHash">Ending commit hash (inclusive).</param>
    /// <returns>List of commits in range.</returns>
    private static List<AzureDevOpsCommit> GetCommitsInRange(
        List<AzureDevOpsCommit> commits,
        string? fromHash,
        string toHash)
    {
        List<AzureDevOpsCommit> result = [];
        var foundTo = false;

        foreach (var commit in commits)
        {
            if (commit.CommitId == toHash)
            {
                foundTo = true;
            }

            if (foundTo && commit.CommitId != fromHash)
            {
                result.Add(commit);
            }

            if (commit.CommitId == fromHash)
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    ///     Collects changes from pull requests in the commit range.
    /// </summary>
    /// <param name="restClient">Azure DevOps REST client.</param>
    /// <param name="commitsInRange">Commits in range.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (bugs, nonBugChanges, allChangeIds).</returns>
    private static async Task<(List<ItemInfo> bugs, List<ItemInfo> nonBugChanges, HashSet<string> allChangeIds)>
        CollectChangesFromPullRequestsAsync(
            AzureDevOpsRestClient restClient,
            List<AzureDevOpsCommit> commitsInRange,
            LookupData lookupData)
    {
        HashSet<string> allChangeIds = [];
        List<ItemInfo> bugs = [];
        List<ItemInfo> nonBugChanges = [];

        foreach (var pr in commitsInRange
            .Where(c => lookupData.CommitHashToPr.ContainsKey(c.CommitId))
            .Select(c => lookupData.CommitHashToPr[c.CommitId]))
        {
            // Get work items linked to this PR
            var workItemRefs = await restClient.GetPullRequestWorkItemsAsync(
                lookupData.Project, pr.PullRequestId);

            if (workItemRefs.Count > 0)
            {
                // Fetch full work item details
                var workItemIds = workItemRefs.Select(r => r.Id).ToList();
                var workItems = await restClient.GetWorkItemsAsync(workItemIds);

                ProcessLinkedWorkItems(workItems, pr, allChangeIds, bugs, nonBugChanges, lookupData);
            }
            else
            {
                ProcessPullRequestWithoutWorkItems(pr, allChangeIds, bugs, nonBugChanges, lookupData);
            }
        }

        return (bugs, nonBugChanges, allChangeIds);
    }

    /// <summary>
    ///     Processes work items linked to a pull request.
    /// </summary>
    /// <param name="workItems">Work items linked to the PR.</param>
    /// <param name="pr">The pull request.</param>
    /// <param name="allChangeIds">Set of all change IDs.</param>
    /// <param name="bugs">Bug items list.</param>
    /// <param name="nonBugChanges">Non-bug change items list.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    private static void ProcessLinkedWorkItems(
        List<AzureDevOpsWorkItem> workItems,
        AzureDevOpsPullRequest pr,
        HashSet<string> allChangeIds,
        List<ItemInfo> bugs,
        List<ItemInfo> nonBugChanges,
        LookupData lookupData)
    {
        foreach (var workItem in workItems)
        {
            var workItemId = workItem.Id.ToString(CultureInfo.InvariantCulture);
            if (!allChangeIds.Add(workItemId))
            {
                continue;
            }

            var workItemUrl = BuildWorkItemUrl(lookupData.OrganizationUrl, lookupData.Project, workItem.Id);
            var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, workItemUrl, pr.PullRequestId);
            if (itemInfo == null)
            {
                continue;
            }

            if (itemInfo.Type == "bug")
            {
                bugs.Add(itemInfo);
            }
            else
            {
                nonBugChanges.Add(itemInfo);
            }
        }
    }

    /// <summary>
    ///     Processes a pull request without linked work items.
    /// </summary>
    /// <param name="pr">The pull request.</param>
    /// <param name="allChangeIds">Set of all change IDs.</param>
    /// <param name="bugs">Bug items list.</param>
    /// <param name="nonBugChanges">Non-bug change items list.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    private static void ProcessPullRequestWithoutWorkItems(
        AzureDevOpsPullRequest pr,
        HashSet<string> allChangeIds,
        List<ItemInfo> bugs,
        List<ItemInfo> nonBugChanges,
        LookupData lookupData)
    {
        var prId = $"!{pr.PullRequestId}";
        if (!allChangeIds.Add(prId))
        {
            return;
        }

        // Parse item controls from PR description
        var controls = ItemControlsParser.Parse(pr.Description);

        // Exclude if internal
        var forceInclude = controls?.Visibility == "public";
        if (!forceInclude && controls?.Visibility == "internal")
        {
            return;
        }

        var type = controls?.Type ?? "other";
        var prUrl = pr.Url ?? BuildPullRequestUrl(lookupData.OrganizationUrl, lookupData.Project, pr.PullRequestId);

        var itemInfo = new ItemInfo(
            prId,
            pr.Title,
            prUrl,
            type,
            pr.PullRequestId,
            controls?.AffectedVersions);

        if (itemInfo.Type == "bug")
        {
            bugs.Add(itemInfo);
        }
        else
        {
            nonBugChanges.Add(itemInfo);
        }
    }

    /// <summary>
    ///     Collects known issues (open bugs not resolved) via a WIQL query.
    /// </summary>
    /// <param name="restClient">Azure DevOps REST client.</param>
    /// <param name="allChangeIds">Set of all change IDs already processed.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>List of known issues.</returns>
    private static async Task<List<ItemInfo>> CollectKnownIssuesAsync(
        AzureDevOpsRestClient restClient,
        HashSet<string> allChangeIds,
        LookupData lookupData)
    {
        // Query for open bugs and issues
        const string wiql = "SELECT [System.Id] FROM workitems " +
                            "WHERE [System.WorkItemType] IN ('Bug', 'Issue') " +
                            "AND [System.State] NOT IN ('Done', 'Closed', 'Resolved')";

        var queryResult = await restClient.QueryWorkItemsAsync(wiql);
        if (queryResult.WorkItems.Count == 0)
        {
            return [];
        }

        // Fetch full work item details
        var workItemIds = queryResult.WorkItems.Select(r => r.Id).ToList();
        var workItems = await restClient.GetWorkItemsAsync(workItemIds);

        List<ItemInfo> knownIssues = [];
        foreach (var workItem in workItems)
        {
            var workItemId = workItem.Id.ToString(CultureInfo.InvariantCulture);

            // Skip items already included as changes
            if (allChangeIds.Contains(workItemId))
            {
                continue;
            }

            // Skip resolved work items (defense in depth)
            if (WorkItemMapper.IsWorkItemResolved(workItem))
            {
                continue;
            }

            var workItemUrl = BuildWorkItemUrl(lookupData.OrganizationUrl, lookupData.Project, workItem.Id);
            var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, workItemUrl, workItem.Id);
            if (itemInfo != null && itemInfo.Type == "bug")
            {
                knownIssues.Add(itemInfo);
            }
        }

        return knownIssues;
    }

    /// <summary>
    ///     Gets Azure DevOps authentication token from environment or Azure CLI.
    /// </summary>
    /// <returns>Tuple of (token, isBearer).</returns>
    private async Task<(string token, bool isBearer)> GetAzureDevOpsTokenAsync()
    {
        // Check explicit user tokens first
        var token = Environment.GetEnvironmentVariable("AZURE_DEVOPS_PAT");
        if (!string.IsNullOrEmpty(token))
        {
            return (token, false);
        }

        token = Environment.GetEnvironmentVariable("AZURE_DEVOPS_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return (token, false);
        }

        token = Environment.GetEnvironmentVariable("AZURE_DEVOPS_EXT_PAT");
        if (!string.IsNullOrEmpty(token))
        {
            return (token, false);
        }

        // Check Azure Pipelines system token
        token = Environment.GetEnvironmentVariable("SYSTEM_ACCESSTOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return (token, true);
        }

        // Fall back to Azure CLI
        try
        {
            var result = await RunCommandAsync("az", "account", "get-access-token", "--resource", "499b84ac-1321-427f-aa17-267ca6975798", "--query", "accessToken", "-o", "tsv");
            if (!string.IsNullOrEmpty(result?.Trim()))
            {
                return (result.Trim(), true);
            }
        }
        catch (InvalidOperationException)
        {
            // Azure CLI not available or not logged in
        }

        throw new InvalidOperationException(
            "Unable to get Azure DevOps token. " +
            "Set AZURE_DEVOPS_PAT, AZURE_DEVOPS_TOKEN, or SYSTEM_ACCESSTOKEN environment variable, " +
            "or authenticate with 'az login'.");
    }

    /// <summary>
    ///     Parses Azure DevOps organization URL, project, and repository from a git remote URL.
    /// </summary>
    /// <remarks>
    ///     Supports Azure DevOps Services (dev.azure.com, visualstudio.com) and on-premises
    ///     Azure DevOps Server instances. HTTPS URLs are detected by locating the <c>_git</c>
    ///     path segment: the segment before <c>_git</c> is the project, the segment after is the
    ///     repository, and everything before the project forms the organization URL.
    ///     SSH URLs use the <c>git@ssh.dev.azure.com:v3/org/project/repo</c> format.
    /// </remarks>
    /// <param name="url">Git remote URL.</param>
    /// <returns>Tuple of (organizationUrl, project, repository).</returns>
    internal static (string organizationUrl, string project, string repository) ParseAzureDevOpsUrl(string url)
    {
        url = url.Trim();

        // Handle SSH URLs: git@ssh.dev.azure.com:v3/org/project/repo
        if (url.StartsWith("git@ssh.dev.azure.com:", StringComparison.OrdinalIgnoreCase))
        {
            var path = url["git@ssh.dev.azure.com:".Length..];
            var parts = path.Split('/');
            if (parts.Length >= 4 && parts[0] == "v3")
            {
                var repo = parts[3];
                if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                {
                    repo = repo[..^4];
                }

                return ($"https://dev.azure.com/{parts[1]}", parts[2], repo);
            }
        }

        // Handle HTTPS URLs by locating the _git path segment
        // Supports dev.azure.com, visualstudio.com, and on-premises Azure DevOps Server
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            var uri = new Uri(url);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            // Find the _git segment to anchor the parse
            var gitIndex = Array.FindIndex(segments,
                s => s.Equals("_git", StringComparison.OrdinalIgnoreCase));

            if (gitIndex >= 1 && gitIndex + 1 < segments.Length)
            {
                var project = segments[gitIndex - 1];
                var repo = segments[gitIndex + 1];
                if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                {
                    repo = repo[..^4];
                }

                // Build organization URL from scheme://authority and path segments before the project
                var baseUrl = uri.GetLeftPart(UriPartial.Authority);
                var orgPath = string.Join("/", segments.Take(gitIndex - 1));
                var orgUrl = orgPath.Length > 0
                    ? $"{baseUrl}/{orgPath}"
                    : baseUrl;

                return (orgUrl, project, repo);
            }
        }

        throw new ArgumentException($"Unsupported Azure DevOps URL format: {url}", nameof(url));
    }

    /// <summary>
    ///     Builds a web URL for an Azure DevOps work item.
    /// </summary>
    /// <param name="organizationUrl">Organization URL.</param>
    /// <param name="project">Project name.</param>
    /// <param name="workItemId">Work item ID.</param>
    /// <returns>Work item web URL.</returns>
    private static string BuildWorkItemUrl(string organizationUrl, string project, int workItemId)
    {
        return $"{organizationUrl}/{project}/_workitems/edit/{workItemId}";
    }

    /// <summary>
    ///     Builds a web URL for an Azure DevOps pull request.
    /// </summary>
    /// <param name="organizationUrl">Organization URL.</param>
    /// <param name="project">Project name.</param>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>Pull request web URL.</returns>
    private static string BuildPullRequestUrl(string organizationUrl, string project, int pullRequestId)
    {
        return $"{organizationUrl}/{project}/_git/pullrequest/{pullRequestId}";
    }
}
