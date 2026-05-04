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

namespace DemaConsulting.BuildMark.RepoConnectors.GitHub;

/// <summary>
///     GitHub repository connector implementation using GraphQL.
/// </summary>
public class GitHubRepoConnector : RepoConnectorBase
{
    /// <summary>
    ///     The optional GitHub connector configuration overrides.
    /// </summary>
    private readonly GitHubConnectorConfig? _config;

    /// <summary>
    ///     Item type classification: feature.
    /// </summary>
    private const string ItemTypeFeature = "feature";

    /// <summary>
    ///     Item type classification: dependencies.
    /// </summary>
    private const string ItemTypeDependencies = "dependencies";

    /// <summary>
    ///     Item type classification: internal.
    /// </summary>
    private const string ItemTypeInternal = "internal";

    /// <summary>
    ///     Item visibility: public (force-include in report).
    /// </summary>
    private const string VisibilityPublic = "public";

    /// <summary>
    ///     Item visibility: internal (exclude from report).
    /// </summary>
    private const string VisibilityInternal = "internal";

    /// <summary>
    ///     Mapping of label keywords to their normalized item types.
    /// </summary>
    private static readonly Dictionary<string, string> LabelTypeMap = new()
    {
        { "bug", "bug" },
        { "defect", "bug" },
        { ItemTypeFeature, ItemTypeFeature },
        { "enhancement", ItemTypeFeature },
        { "documentation", "documentation" },
        { "performance", "performance" },
        { "security", "security" },
        { ItemTypeDependencies, ItemTypeDependencies },
        { "renovate", ItemTypeDependencies },
        { "dependabot", ItemTypeDependencies },
        { ItemTypeInternal, ItemTypeInternal },
        { "chore", ItemTypeInternal }
    };

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubRepoConnector"/> class.
    /// </summary>
    /// <param name="config">Optional GitHub connector overrides.</param>
    public GitHubRepoConnector(GitHubConnectorConfig? config = null)
    {
        _config = config;
    }

    /// <summary>
    ///     Gets the optional GitHub connector configuration overrides.
    /// </summary>
    internal GitHubConnectorConfig? ConfigurationOverrides => _config;

    /// <summary>
    ///     Creates a GitHub GraphQL client for API operations.
    /// </summary>
    /// <param name="token">GitHub personal access token for authentication.</param>
    /// <returns>A new GitHubGraphQLClient instance.</returns>
    /// <remarks>
    ///     This method is virtual to allow derived classes to override it for testing purposes.
    ///     Tests can provide a client configured with a mock HttpClient for controlled responses.
    /// </remarks>
    internal virtual GitHubGraphQLClient CreateGraphQLClient(string token)
    {
        return new GitHubGraphQLClient(token, ResolveGraphQLEndpoint(_config?.BaseUrl));
    }

    /// <summary>
    ///     Gets build information for a release.
    /// </summary>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public override async Task<BuildInformation> GetBuildInformationAsync(VersionTag? version = null)
    {
        // Get repository metadata using git commands
        var repoUrl = await RunCommandAsync("git", "remote", "get-url", "origin");
        var branch = await RunCommandAsync("git", "rev-parse", "--abbrev-ref", "HEAD");
        var currentCommitHash = await RunCommandAsync("git", "rev-parse", "HEAD");

        // Parse owner and repo from URL
        var (parsedOwner, parsedRepo) = ParseGitHubUrl(repoUrl);
        var owner = _config?.Owner ?? parsedOwner;
        var repo = _config?.Repo ?? parsedRepo;

        // Get GitHub token
        var token = await GetGitHubTokenAsync();

        // Create GraphQL client
        using var graphqlClient = CreateGraphQLClient(token);

        // Fetch all data from GitHub
        var gitHubData = await FetchGitHubDataAsync(graphqlClient, owner, repo, branch.Trim());

        // Build lookup dictionaries and mappings
        var lookupData = BuildLookupData(gitHubData);

        // Determine the target version and hash
        var (toVersion, toHash) = DetermineTargetVersion(version, currentCommitHash.Trim(), lookupData);

        // Determine the starting release for comparing changes
        var (fromVersion, fromHash) = DetermineBaselineVersion(toVersion, toHash, lookupData);

        // Get commits in range
        var commitsInRange = GetCommitsInRange(gitHubData.Commits, fromHash, toHash);

        // Collect changes from PRs
        var (bugs, nonBugChanges, allChangeIds) = await CollectChangesFromPullRequestsAsync(
            graphqlClient,
            commitsInRange,
            lookupData,
            owner,
            repo);

        // Collect known issues
        var knownIssues = CollectKnownIssues(gitHubData.Issues, allChangeIds, toVersion);

        // Sort all lists by Index to ensure chronological order
        nonBugChanges.Sort((a, b) => a.Index.CompareTo(b.Index));
        bugs.Sort((a, b) => a.Index.CompareTo(b.Index));
        knownIssues.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Apply routing rules if configured, otherwise use legacy categorization
        IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>? routedSections = null;
        if (HasRules)
        {
            // Route all collected items into configured sections
            var allItems = nonBugChanges.Concat(bugs).Concat(knownIssues);
            routedSections = ApplyRules(allItems);
        }

        // Build version tags from version and hash info
        var currentTag = new VersionCommitTag(toVersion, toHash);
        var baselineTag = fromVersion != null && fromHash != null
            ? new VersionCommitTag(fromVersion, fromHash)
            : null;

        // Generate full changelog link for GitHub
        var changelogLink = GenerateGitHubChangelogLink(owner, repo, fromVersion?.Tag, toVersion.Tag, lookupData.BranchTagNames);

        // Create and return build information with all collected data
        return new BuildInformation(
            baselineTag,
            currentTag,
            nonBugChanges,
            bugs,
            knownIssues,
            changelogLink)
        {
            RoutedSections = routedSections
        };
    }

    /// <summary>
    ///     Resolves the configured base URL to a GraphQL endpoint.
    /// </summary>
    /// <param name="baseUrl">The configured base URL.</param>
    /// <returns>The GraphQL endpoint, or null for the default endpoint.</returns>
    private static string? ResolveGraphQLEndpoint(string? baseUrl)
    {
        // Leave the endpoint unchanged when no override was provided.
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return null;
        }

        // Preserve explicit GraphQL endpoints.
        var trimmed = baseUrl.TrimEnd('/');
        if (trimmed.EndsWith("/graphql", StringComparison.OrdinalIgnoreCase))
        {
            return trimmed;
        }

        // Handle the public GitHub API hostname directly.
        if (trimmed.Contains("api.github.com", StringComparison.OrdinalIgnoreCase))
        {
            return $"{trimmed}/graphql";
        }

        // Default GitHub Enterprise installations expose the GraphQL endpoint under /api/graphql.
        return $"{trimmed}/api/graphql";
    }

    /// <summary>
    ///     Simple commit representation containing only the SHA hash.
    /// </summary>
    internal sealed record Commit(
        string Sha);

    /// <summary>
    ///     Tag representation with name and commit information.
    /// </summary>
    internal sealed record Tag(
        string Name,
        TagCommit Commit);

    /// <summary>
    ///     Tag commit information containing the SHA hash.
    /// </summary>
    internal sealed record TagCommit(
        string Sha);

    /// <summary>
    ///     Pull request information.
    /// </summary>
    internal sealed record PullRequestInfo(
        int Number,
        string Title,
        string HtmlUrl,
        bool Merged,
        string? MergeCommitSha,
        string? HeadSha,
        IReadOnlyList<PullRequestLabelInfo> Labels,
        string? Body);

    /// <summary>
    ///     Pull request label information.
    /// </summary>
    internal sealed record PullRequestLabelInfo(
        string Name);

    /// <summary>
    ///     Issue information.
    /// </summary>
    internal sealed record IssueInfo(
        int Number,
        string Title,
        string HtmlUrl,
        string State,
        IReadOnlyList<IssueLabelInfo> Labels,
        string? Body);

    /// <summary>
    ///     Issue label information.
    /// </summary>
    internal sealed record IssueLabelInfo(
        string Name);

    /// <summary>
    ///     Container for GitHub data fetched from the API.
    /// </summary>
    internal sealed record GitHubData(
        IReadOnlyList<Commit> Commits,
        IReadOnlyList<ReleaseNode> Releases,
        IReadOnlyList<Tag> Tags,
        IReadOnlyList<PullRequestInfo> PullRequests,
        IReadOnlyList<IssueInfo> Issues);

    /// <summary>
    ///     Container for lookup data structures built from GitHub data.
    /// </summary>
    internal sealed record LookupData(
        Dictionary<int, IssueInfo> IssueById,
        Dictionary<string, PullRequestInfo> CommitHashToPr,
        List<ReleaseNode> BranchReleases,
        Dictionary<string, Tag> TagsByName,
        Dictionary<string, ReleaseNode> TagToRelease,
        List<VersionTag> ReleaseVersions,
        HashSet<string> BranchTagNames);

    /// <summary>
    ///     Fetches all required data from GitHub API in parallel.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>Container with all fetched GitHub data.</returns>
    private static async Task<GitHubData> FetchGitHubDataAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo,
        string branch)
    {
        // Fetch all data from GitHub in parallel
        var commitsTask = GetAllCommitsAsync(graphqlClient, owner, repo, branch);
        var releasesTask = GetAllReleasesAsync(graphqlClient, owner, repo);
        var tagsTask = GetAllTagsAsync(graphqlClient, owner, repo);
        var pullRequestsTask = GetAllPullRequestsAsync(graphqlClient, owner, repo);
        var issuesTask = GetAllIssuesAsync(graphqlClient, owner, repo);

        // Wait for all parallel fetches to complete
        await Task.WhenAll(commitsTask, releasesTask, tagsTask, pullRequestsTask, issuesTask);

        // Return collected data as a container
        return new GitHubData(
            await commitsTask,
            await releasesTask,
            await tagsTask,
            await pullRequestsTask,
            await issuesTask);
    }

    /// <summary>
    ///     Builds lookup data structures from GitHub data.
    /// </summary>
    /// <param name="data">GitHub data.</param>
    /// <returns>Container with all lookup data structures.</returns>
    private static LookupData BuildLookupData(GitHubData data)
    {
        // Build a mapping from issue number to issue for efficient lookup.
        // This is used to look up issue details when we find linked issue IDs.
        var issueById = data.Issues.ToDictionary(i => i.Number, i => i);

        // Build a mapping from commit SHA to pull request.
        // This is used to associate commits with their pull requests for change tracking.
        // For merged PRs, use MergeCommitSha; for open PRs, use head SHA.
        // Duplicate commit SHAs are handled gracefully by keeping the first PR in collection order per SHA.
        var commitHashToPr = data.PullRequests
            .Where(p => p is { Merged: true, MergeCommitSha: not null } or { Merged: false, HeadSha: not null })
            .GroupBy(p => p.Merged ? p.MergeCommitSha! : p.HeadSha!)
            .ToDictionary(g => g.Key, g => g.First());

        // Build a set of commit SHAs in the current branch.
        // This is used for efficient filtering of branch-related tags.
        var branchCommitShas = data.Commits.Select(c => c.Sha).ToHashSet();

        // Build a set of tags filtered to those on the current branch.
        // This is used for efficient filtering of branch-related releases.
        var branchTagNames = data.Tags.Where(t => branchCommitShas.Contains(t.Commit.Sha))
                .Select(t => t.Name).ToHashSet();

        // Build an ordered list of releases on the current branch.
        // This is used to select the prior release version for identifying changes in the build.
        var branchReleases = data.Releases
            .Where(r => r.TagName != null && branchTagNames.Contains(r.TagName))
            .ToList();

        // Build a mapping from tag name to tag object for quick lookup.
        // This is used to get commit SHAs for release tags.
        var tagsByName = data.Tags.ToDictionary(t => t.Name, t => t);

        // Build a mapping from tag name to release for version lookup.
        // This is used to match version objects back to their releases.
        var tagToRelease = branchReleases
            .GroupBy(r => r.TagName!)
            .ToDictionary(g => g.Key, g => g.First());

        // Parse release tags into VersionTag objects, maintaining release order (newest to oldest).
        // This is used to determine version history and find previous releases.
        var releaseVersions = branchReleases
            .Select(r => VersionTag.TryCreate(r.TagName!))
            .Where(v => v != null)
            .Cast<VersionTag>()
            .ToList();

        return new LookupData(
            issueById,
            commitHashToPr,
            branchReleases,
            tagsByName,
            tagToRelease,
            releaseVersions,
            branchTagNames);
    }

    /// <summary>
    ///     Determines the target version and hash for the build.
    /// </summary>
    /// <param name="version">Optional target version provided by caller.</param>
    /// <param name="currentCommitHash">Current commit hash.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (toVersion, toHash).</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    private static (VersionTag toVersion, string toHash) DetermineTargetVersion(
        VersionTag? version,
        string currentCommitHash,
        LookupData lookupData)
    {
        // Use provided version if specified
        var toVersion = version;
        var toHash = currentCommitHash;

        // Return early if version was explicitly provided
        if (toVersion != null)
        {
            return (toVersion, toHash);
        }

        // Validate that repository has releases
        if (lookupData.ReleaseVersions.Count == 0)
        {
            throw new InvalidOperationException(
                "No releases found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Use the most recent release (first in list since releases are newest to oldest)
        var latestRelease = lookupData.BranchReleases[0];
        var latestReleaseVersion = lookupData.ReleaseVersions[0];
        var latestTagCommit = lookupData.TagsByName[latestRelease.TagName!];

        // Check if current commit matches latest release tag
        if (latestTagCommit.Commit.Sha == toHash)
        {
            // Current commit matches latest release tag, use it as target
            return (latestReleaseVersion, toHash);
        }

        // Current commit doesn't match any release tag, cannot determine version
        throw new InvalidOperationException(
            "Target version not specified and current commit does not match any release tag. " +
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
        // Return null baseline if no releases exist
        if (lookupData.ReleaseVersions.Count == 0)
        {
            return (null, null);
        }

        // Find the position of target version in the newest-first release list
        var toIndex = FindVersionIndex(lookupData.ReleaseVersions, toVersion);

        // Build preceding versions list (oldest-first) for the base class selection methods.
        // ReleaseVersions is newest-first; preceding entries start at toIndex+1 (or 0 if target
        // not found) and span to the end of the list. Iterating from Count-1 down gives oldest-first.
        var startIndex = toIndex >= 0 ? toIndex + 1 : 0;
        var preceding = new List<VersionCommitTag>();
        for (var i = lookupData.ReleaseVersions.Count - 1; i >= startIndex; i--)
        {
            var tag = lookupData.ReleaseVersions[i];
            var hash = lookupData.TagToRelease.TryGetValue(tag.Tag, out var rel) &&
                       lookupData.TagsByName.TryGetValue(rel.TagName!, out var tagCommit)
                ? tagCommit.Commit.Sha
                : string.Empty;
            preceding.Add(new VersionCommitTag(tag, hash));
        }

        // Delegate selection to the base class algorithm
        var baseline = toVersion.IsPreRelease
            ? FindBaselineForPreRelease(preceding, toHash)
            : FindBaselineForRelease(preceding);

        return (baseline?.VersionTag, baseline?.CommitHash);
    }

    /// <summary>
    ///     Collects changes from pull requests in the commit range.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client for API operations.</param>
    /// <param name="commitsInRange">Commits in range.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>Tuple of (bugs, nonBugChanges, allChangeIds).</returns>
    private static async Task<(List<ItemInfo> bugs, List<ItemInfo> nonBugChanges, HashSet<string> allChangeIds)>
        CollectChangesFromPullRequestsAsync(
            GitHubGraphQLClient graphqlClient,
            List<Commit> commitsInRange,
            LookupData lookupData,
            string owner,
            string repo)
    {
        // Initialize collections for tracking changes
        HashSet<string> allChangeIds = [];
        List<ItemInfo> bugs = [];
        List<ItemInfo> nonBugChanges = [];

        // Process each commit that has an associated PR
        foreach (var pr in commitsInRange
            .Where(c => lookupData.CommitHashToPr.ContainsKey(c.Sha))
            .Select(c => lookupData.CommitHashToPr[c.Sha]))
        {

            // Find issue IDs that are linked to this PR using GitHub GraphQL API
            // All PRs are also issues, so we need to find the "real" issues (non-PR issues) that link to this PR
            var linkedIssueIds = await graphqlClient.FindIssueIdsLinkedToPullRequestAsync(owner, repo, pr.Number);

            // Process PR based on whether it has linked issues
            if (linkedIssueIds.Count > 0)
            {
                ProcessLinkedIssues(linkedIssueIds, lookupData.IssueById, pr, allChangeIds, bugs, nonBugChanges);
            }
            else
            {
                ProcessPullRequestWithoutIssues(pr, allChangeIds, bugs, nonBugChanges);
            }
        }

        // Return categorized changes
        return (bugs, nonBugChanges, allChangeIds);
    }

    /// <summary>
    ///     Processes issues linked to a pull request.
    /// </summary>
    /// <param name="linkedIssueIds">List of linked issue IDs.</param>
    /// <param name="issueById">Dictionary mapping issue IDs to issues.</param>
    /// <param name="pr">Pull request.</param>
    /// <param name="allChangeIds">Set of all change IDs (modified in place).</param>
    /// <param name="bugs">List of bug changes (modified in place).</param>
    /// <param name="nonBugChanges">List of non-bug changes (modified in place).</param>
    private static void ProcessLinkedIssues(
        IReadOnlyList<int> linkedIssueIds,
        Dictionary<int, IssueInfo> issueById,
        PullRequestInfo pr,
        HashSet<string> allChangeIds,
        List<ItemInfo> bugs,
        List<ItemInfo> nonBugChanges)
    {
        // PR has linked issues - add them (deduplicated)
        foreach (var issueId in linkedIssueIds)
        {
            // Check if issue already processed
            var issueIdStr = issueId.ToString(CultureInfo.InvariantCulture);
            if (allChangeIds.Contains(issueIdStr))
            {
                continue;
            }

            // Look up the issue in the master issues list
            if (issueById.TryGetValue(issueId, out var issue))
            {
                // Mark issue as processed and create item info
                allChangeIds.Add(issueIdStr);
                var itemInfo = CreateItemInfoFromIssue(issue, pr.Number);

                // Skip item if controls indicate internal visibility
                if (itemInfo == null)
                {
                    continue;
                }

                // Categorize by type
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
    }

    /// <summary>
    ///     Processes a pull request without linked issues.
    /// </summary>
    /// <param name="pr">Pull request.</param>
    /// <param name="allChangeIds">Set of all change IDs (modified in place).</param>
    /// <param name="bugs">List of bug changes (modified in place).</param>
    /// <param name="nonBugChanges">List of non-bug changes (modified in place).</param>
    private static void ProcessPullRequestWithoutIssues(
        PullRequestInfo pr,
        HashSet<string> allChangeIds,
        List<ItemInfo> bugs,
        List<ItemInfo> nonBugChanges)
    {
        // PR didn't close any issues - add the PR itself
        var prId = $"#{pr.Number}";
        if (allChangeIds.Add(prId))
        {
            // Create item info from PR
            var itemInfo = CreateItemInfoFromPullRequest(pr);

            // Skip item if controls indicate internal visibility
            if (itemInfo == null)
            {
                return;
            }

            // Categorize by type
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
    ///     Collects known issues from the full issue list.
    ///     When a bug declares <c>AffectedVersions</c>, it is a known issue if and only if
    ///     <c>AffectedVersions.Contains(targetVersion)</c> is true, regardless of its open/closed
    ///     state. When no <c>AffectedVersions</c> are declared, only open bugs are included.
    /// </summary>
    /// <param name="issues">All issues from GitHub (open and closed).</param>
    /// <param name="allChangeIds">Set of all change IDs already processed in this build.</param>
    /// <param name="targetVersion">The version being built, used for affected-versions filtering.</param>
    /// <returns>List of known issues.</returns>
    private static List<ItemInfo> CollectKnownIssues(
        IReadOnlyList<IssueInfo> issues,
        HashSet<string> allChangeIds,
        VersionTag targetVersion)
    {
        List<ItemInfo> knownIssues = [];

        foreach (var issue in issues)
        {
            // Skip issues already addressed in this build
            var issueId = issue.Number.ToString(CultureInfo.InvariantCulture);
            if (allChangeIds.Contains(issueId))
            {
                continue;
            }

            var itemInfo = CreateItemInfoFromIssue(issue, issue.Number);
            if (itemInfo == null || itemInfo.Type != "bug")
            {
                continue;
            }

            // With affected-versions: include if version matches, regardless of state.
            // Without affected-versions: only open bugs are included.
            var isKnownIssue = itemInfo.AffectedVersions != null
                ? itemInfo.AffectedVersions.Contains(targetVersion)
                : issue.State == "OPEN";

            if (isKnownIssue)
            {
                knownIssues.Add(itemInfo);
            }
        }

        return knownIssues;
    }

    /// <summary>
    ///     Gets all commits for a branch using GraphQL pagination.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>List of all commits.</returns>
    private static async Task<IReadOnlyList<Commit>> GetAllCommitsAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo,
        string branch)
    {
        // Fetch all commit SHAs for the branch using GraphQL
        var commitShas = await graphqlClient.GetCommitsAsync(owner, repo, branch);

        // Convert SHAs to Commit objects and return
        return commitShas.Select(sha => new Commit(sha)).ToList();
    }

    /// <summary>
    ///     Gets all releases for a repository using GraphQL pagination.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of all releases.</returns>
    private static async Task<IReadOnlyList<ReleaseNode>> GetAllReleasesAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo)
    {
        // Fetch all releases for the repository using GraphQL
        return await graphqlClient.GetReleasesAsync(owner, repo);
    }

    /// <summary>
    ///     Gets all tags for a repository using GraphQL pagination.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of all tags.</returns>
    private static async Task<IReadOnlyList<Tag>> GetAllTagsAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo)
    {
        // Fetch all tags for the repository using GraphQL
        var tagNodes = await graphqlClient.GetAllTagsAsync(owner, repo);

        // Convert TagNode objects to Tag objects with nested commit structure
        return tagNodes
            .Where(t => t is { Name: not null, Target.Oid: not null })
            .Select(t => new Tag(t.Name!, new TagCommit(t.Target!.Oid!)))
            .ToList();
    }

    /// <summary>
    ///     Gets all pull requests for a repository using GraphQL pagination.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of all pull requests.</returns>
    private static async Task<IReadOnlyList<PullRequestInfo>> GetAllPullRequestsAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo)
    {
        // Fetch all pull requests for the repository using GraphQL
        var prNodes = await graphqlClient.GetPullRequestsAsync(owner, repo);

        // Convert PullRequestNode objects to PullRequestInfo objects
        return prNodes
            .Where(pr => pr.Number.HasValue && !string.IsNullOrEmpty(pr.Title))
            .Select(pr => new PullRequestInfo(
                pr.Number!.Value,
                pr.Title!,
                pr.Url ?? string.Empty,
                pr.Merged,
                pr.MergeCommit?.Oid,
                pr.HeadRefOid,
                pr.Labels?.Nodes?
                    .Where(l => !string.IsNullOrEmpty(l.Name))
                    .Select(l => new PullRequestLabelInfo(l.Name!))
                    .ToList() ?? [],
                pr.Body))
            .ToList();
    }

    /// <summary>
    ///     Gets all issues for a repository using GraphQL.
    /// </summary>
    /// <param name="graphqlClient">GitHub GraphQL client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <returns>List of all issues.</returns>
    private static async Task<IReadOnlyList<IssueInfo>> GetAllIssuesAsync(
        GitHubGraphQLClient graphqlClient,
        string owner,
        string repo)
    {
        // Fetch all issues for the repository using GraphQL
        var issueNodes = await graphqlClient.GetAllIssuesAsync(owner, repo);

        // Convert IssueNode objects to IssueInfo objects
        return issueNodes
            .Where(issue => issue.Number.HasValue && !string.IsNullOrEmpty(issue.Title))
            .Select(issue => new IssueInfo(
                issue.Number!.Value,
                issue.Title!,
                issue.Url ?? string.Empty,
                issue.State ?? "UNKNOWN",
                issue.Labels?.Nodes?
                    .Where(l => !string.IsNullOrEmpty(l.Name))
                    .Select(l => new IssueLabelInfo(l.Name!))
                    .ToList() ?? [],
                issue.Body))
            .ToList();
    }

    /// <summary>
    ///     Gets commits in the range from fromHash (exclusive) to toHash (inclusive).
    /// </summary>
    /// <param name="commits">All commits.</param>
    /// <param name="fromHash">Starting commit hash (exclusive - not included in results; null for start of history).</param>
    /// <param name="toHash">Ending commit hash (inclusive - included in results).</param>
    /// <returns>List of commits in range, excluding fromHash but including toHash.</returns>
    private static List<Commit> GetCommitsInRange(IReadOnlyList<Commit> commits, string? fromHash, string toHash)
    {
        // Initialize collection and state tracking
        List<Commit> result = [];
        var foundTo = false;

        // Iterate through commits from newest to oldest
        foreach (var commit in commits)
        {
            // Mark when we've found the target commit
            if (commit.Sha == toHash)
            {
                foundTo = true;
            }

            // Collect commits in range, excluding the fromHash commit itself
            if (foundTo && commit.Sha != fromHash)
            {
                result.Add(commit);
            }

            // Stop when we reach the starting commit
            if (commit.Sha == fromHash)
            {
                break;
            }
        }

        // Return collected commits
        return result;
    }

    /// <summary>
    ///     Creates an ItemInfo from an issue, applying any item controls from the description.
    /// </summary>
    /// <param name="issue">GitHub issue.</param>
    /// <param name="index">Index for sorting.</param>
    /// <returns>ItemInfo instance, or null if the item controls indicate the item should be excluded.</returns>
    private static ItemInfo? CreateItemInfoFromIssue(IssueInfo issue, int index)
    {
        // Determine item type from issue labels
        var type = GetTypeFromLabels(issue.Labels);

        // Parse item controls from issue description body
        var controls = ItemControlsParser.Parse(issue.Body);

        // Exclude item if visibility is "internal"
        // Note: "public" explicitly force-includes the item, so it takes precedence
        var forceInclude = controls?.Visibility == VisibilityPublic;
        if (!forceInclude && controls?.Visibility == VisibilityInternal)
        {
            return null;
        }

        // Override type if item controls specify one
        type = ApplyTypeOverride(type, controls);

        // Create and return item info with issue details and affected versions
        return new ItemInfo(
            issue.Number.ToString(),
            issue.Title,
            issue.HtmlUrl,
            type,
            index,
            controls?.AffectedVersions);
    }

    /// <summary>
    ///     Creates an ItemInfo from a pull request, applying any item controls from the description.
    /// </summary>
    /// <param name="pr">GitHub pull request.</param>
    /// <returns>ItemInfo instance, or null if the item controls indicate the item should be excluded.</returns>
    private static ItemInfo? CreateItemInfoFromPullRequest(PullRequestInfo pr)
    {
        // Determine item type from PR labels
        var type = GetTypeFromLabels(pr.Labels);

        // Parse item controls from PR description body
        var controls = ItemControlsParser.Parse(pr.Body);

        // Exclude item if visibility is "internal"
        // Note: "public" explicitly force-includes the item, so it takes precedence
        var forceInclude = controls?.Visibility == VisibilityPublic;
        if (!forceInclude && controls?.Visibility == VisibilityInternal)
        {
            return null;
        }

        // Override type if item controls specify one
        type = ApplyTypeOverride(type, controls);

        // Create and return item info with PR details and affected versions
        return new ItemInfo(
            $"#{pr.Number}",
            pr.Title,
            pr.HtmlUrl,
            type,
            pr.Number,
            controls?.AffectedVersions);
    }

    /// <summary>
    ///     Applies type override from item controls if specified.
    /// </summary>
    /// <param name="type">Current item type from labels.</param>
    /// <param name="controls">Parsed item controls (maybe null).</param>
    /// <returns>Final item type string.</returns>
    private static string ApplyTypeOverride(string type, ItemControlsInfo? controls)
    {
        // Override type if item controls specify "bug" or "feature"
        if (controls?.Type == "bug")
        {
            return "bug";
        }

        if (controls?.Type == ItemTypeFeature)
        {
            return ItemTypeFeature;
        }

        return type;
    }

    /// <summary>
    ///     Determines item type from issue labels.
    /// </summary>
    /// <param name="labels">List of issue labels.</param>
    /// <returns>Item type string.</returns>
    private static string GetTypeFromLabels(IReadOnlyList<IssueLabelInfo> labels)
    {
        // Find first matching label type by checking label names against the type map
        var matchingType = labels
            .Select(label => label.Name.ToLowerInvariant())
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel == kvp.Key)
                .Select(kvp => kvp.Value))
            .FirstOrDefault();

        // Return matched type or default to "other"
        return matchingType ?? "other";
    }

    /// <summary>
    ///     Determines item type from pull request labels.
    /// </summary>
    /// <param name="labels">List of pull request labels.</param>
    /// <returns>Item type string.</returns>
    private static string GetTypeFromLabels(IReadOnlyList<PullRequestLabelInfo> labels)
    {
        // Find first matching label type by checking label names against the type map
        var matchingType = labels
            .Select(label => label.Name.ToLowerInvariant())
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel == kvp.Key)
                .Select(kvp => kvp.Value))
            .FirstOrDefault();

        // Return matched type or default to "other"
        return matchingType ?? "other";
    }

    /// <summary>
    ///     Gets GitHub token from environment or gh CLI.
    /// </summary>
    /// <returns>GitHub token.</returns>
    private async Task<string> GetGitHubTokenAsync()
    {
        // Try GH_TOKEN environment variable first
        var token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        // Try GITHUB_TOKEN environment variable next
        token = Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        // Fall back to gh auth token
        try
        {
            return await RunCommandAsync("gh", "auth", "token");
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Unable to get GitHub token. Set GH_TOKEN or GITHUB_TOKEN environment variable or authenticate with 'gh auth login'.");
        }
    }

    /// <summary>
    ///     Parses GitHub owner and repo from a git remote URL.
    /// </summary>
    /// <param name="url">Git remote URL.</param>
    /// <returns>Tuple of (owner, repo).</returns>
    /// <exception cref="ArgumentException">Thrown if URL format is invalid.</exception>
    private static (string owner, string repo) ParseGitHubUrl(string url)
    {
        // Normalize URL by trimming whitespace
        url = url.Trim();

        // Handle SSH URLs: git@github.com:owner/repo.git
        if (url.StartsWith("git@github.com:", StringComparison.OrdinalIgnoreCase))
        {
            var path = url["git@github.com:".Length..];
            return ParseOwnerRepo(path);
        }

        // Handle HTTPS URLs: https://github.com/owner/repo.git
        if (url.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
        {
            var path = url["https://github.com/".Length..];
            return ParseOwnerRepo(path);
        }

        throw new ArgumentException($"Unsupported GitHub URL format: {url}", nameof(url));
    }

    /// <summary>
    ///     Parses owner and repo from a path segment.
    /// </summary>
    /// <param name="path">Path segment (e.g., "owner/repo.git").</param>
    /// <returns>Tuple of (owner, repo).</returns>
    /// <exception cref="ArgumentException">Thrown if path format is invalid.</exception>
    private static (string owner, string repo) ParseOwnerRepo(string path)
    {
        // Remove .git suffix if present
        if (path.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            path = path[..^4];
        }

        // Split path into owner and repo components
        var parts = path.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid GitHub path format: {path}");
        }

        // Return parsed owner and repo
        return (parts[0], parts[1]);
    }

    /// <summary>
    ///     Generates a GitHub compare link for the full changelog.
    /// </summary>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="oldTag">Old tag name (null if from beginning).</param>
    /// <param name="newTag">New tag name.</param>
    /// <param name="branchTagNames">Set of tag names on the current branch.</param>
    /// <returns>WebLink to GitHub compare page, or null if no baseline tag or if tags not found in branch.</returns>
    private static WebLink? GenerateGitHubChangelogLink(string owner, string repo, string? oldTag, string newTag, HashSet<string> branchTagNames)
    {
        // Cannot generate comparison link without a baseline tag
        if (oldTag == null)
        {
            return null;
        }

        // Suppress changelog link if either tag is not in the branch
        if (!branchTagNames.Contains(oldTag) || !branchTagNames.Contains(newTag))
        {
            return null;
        }

        // Build comparison label and URL
        var comparisonLabel = $"{oldTag}...{newTag}";
        var comparisonUrl = $"https://github.com/{owner}/{repo}/compare/{comparisonLabel}";

        return new WebLink(comparisonLabel, comparisonUrl);
    }
}
