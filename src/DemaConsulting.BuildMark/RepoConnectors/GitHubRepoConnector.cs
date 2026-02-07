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

using Octokit;

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
///     GitHub repository connector implementation using Octokit.Net.
/// </summary>
public class GitHubRepoConnector : RepoConnectorBase
{
    /// <summary>
    ///     Mapping of label keywords to their normalized item types.
    /// </summary>
    private static readonly Dictionary<string, string> LabelTypeMap = new()
    {
        { "bug", "bug" },
        { "defect", "bug" },
        { "feature", "feature" },
        { "enhancement", "feature" },
        { "documentation", "documentation" },
        { "performance", "performance" },
        { "security", "security" }
    };

    /// <summary>
    ///     Gets build information for a release.
    /// </summary>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public override async Task<BuildInformation> GetBuildInformationAsync(Version? version = null)
    {
        // Get repository metadata using git commands
        var repoUrl = await RunCommandAsync("git", "remote get-url origin");
        var branch = await RunCommandAsync("git", "rev-parse --abbrev-ref HEAD");
        var currentCommitHash = await RunCommandAsync("git", "rev-parse HEAD");

        // Parse owner and repo from URL
        var (owner, repo) = ParseGitHubUrl(repoUrl);

        // Get GitHub token
        var token = await GetGitHubTokenAsync();

        // Create Octokit client
        var client = new GitHubClient(new Octokit.ProductHeaderValue("BuildMark"))
        {
            Credentials = new Credentials(token)
        };

        // Fetch all data from GitHub
        var gitHubData = await FetchGitHubDataAsync(client, owner, repo, branch.Trim());

        // Build lookup dictionaries and mappings
        var lookupData = BuildLookupData(gitHubData);

        // Determine the target version and hash
        var (toVersion, toHash) = DetermineTargetVersion(version, currentCommitHash.Trim(), lookupData);

        // Determine the starting release for comparing changes
        var (fromVersion, fromHash) = DetermineBaselineVersion(toVersion, lookupData);

        // Get commits in range
        var commitsInRange = GetCommitsInRange(gitHubData.Commits, fromHash, toHash);

        // Collect changes from PRs
        var (bugs, nonBugChanges, allChangeIds) = await CollectChangesFromPullRequestsAsync(
            commitsInRange,
            lookupData,
            owner,
            repo,
            token);

        // Collect known issues
        var knownIssues = CollectKnownIssues(gitHubData.Issues, allChangeIds);

        // Sort all lists by Index to ensure chronological order
        nonBugChanges.Sort((a, b) => a.Index.CompareTo(b.Index));
        bugs.Sort((a, b) => a.Index.CompareTo(b.Index));
        knownIssues.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Build version tags from version and hash info
        var currentTag = new VersionTag(toVersion, toHash);
        var baselineTag = fromVersion != null && fromHash != null
            ? new VersionTag(fromVersion, fromHash)
            : null;

        // Generate full changelog link for GitHub
        var changelogLink = GenerateGitHubChangelogLink(owner, repo, fromVersion?.Tag, toVersion.Tag);

        // Create and return build information with all collected data
        return new BuildInformation(
            baselineTag,
            currentTag,
            nonBugChanges,
            bugs,
            knownIssues,
            changelogLink);
    }

    /// <summary>
    ///     Container for GitHub data fetched from the API.
    /// </summary>
    internal sealed record GitHubData(
        IReadOnlyList<GitHubCommit> Commits,
        IReadOnlyList<Release> Releases,
        IReadOnlyList<RepositoryTag> Tags,
        IReadOnlyList<PullRequest> PullRequests,
        IReadOnlyList<Issue> Issues);

    /// <summary>
    ///     Container for lookup data structures built from GitHub data.
    /// </summary>
    internal sealed record LookupData(
        Dictionary<int, Issue> IssueById,
        Dictionary<string, PullRequest> CommitHashToPr,
        List<Release> BranchReleases,
        Dictionary<string, RepositoryTag> TagsByName,
        Dictionary<string, Release> TagToRelease,
        List<Version> ReleaseVersions);

    /// <summary>
    ///     Fetches all required data from GitHub API in parallel.
    /// </summary>
    /// <param name="client">GitHub client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>Container with all fetched GitHub data.</returns>
    private static async Task<GitHubData> FetchGitHubDataAsync(GitHubClient client, string owner, string repo, string branch)
    {
        // Fetch all data from GitHub in parallel
        var commitsTask = GetAllCommitsAsync(client, owner, repo, branch);
        var releasesTask = client.Repository.Release.GetAll(owner, repo);
        var tagsTask = client.Repository.GetAllTags(owner, repo);
        var pullRequestsTask = client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest { State = ItemStateFilter.All });
        var issuesTask = client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest { State = ItemStateFilter.All });

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
    internal static LookupData BuildLookupData(GitHubData data)
    {
        // Build a mapping from issue number to issue for efficient lookup.
        // This is used to look up issue details when we find linked issue IDs.
        var issueById = data.Issues.ToDictionary(i => i.Number, i => i);

        // Build a mapping from commit SHA to pull request.
        // This is used to associate commits with their pull requests for change tracking.
        // For merged PRs, use MergeCommitSha; for open PRs, use head SHA.
        var commitHashToPr = data.PullRequests
            .Where(p => (p.Merged && p.MergeCommitSha != null) || (!p.Merged && p.Head?.Sha != null))
            .ToDictionary(p => p.Merged ? p.MergeCommitSha! : p.Head.Sha, p => p);

        // Build a set of commit SHAs in the current branch.
        // This is used for efficient filtering of branch-related tags.
        var branchCommitShas = new HashSet<string>(data.Commits.Select(c => c.Sha));

        // Build a set of tags filtered to those on the current branch.
        // This is used for efficient filtering of branch-related releases.
        var branchTagNames = new HashSet<string>(
            data.Tags.Where(t => branchCommitShas.Contains(t.Commit.Sha))
                .Select(t => t.Name));

        // Build an ordered list of releases on the current branch.
        // This is used to select the prior release version for identifying changes in the build.
        var branchReleases = data.Releases
            .Where(r => !string.IsNullOrEmpty(r.TagName) && branchTagNames.Contains(r.TagName))
            .ToList();

        // Build a mapping from tag name to tag object for quick lookup.
        // This is used to get commit SHAs for release tags.
        var tagsByName = data.Tags.ToDictionary(t => t.Name, t => t);

        // Build a mapping from tag name to release for version lookup.
        // This is used to match version objects back to their releases.
        var tagToRelease = branchReleases.ToDictionary(r => r.TagName!, r => r);

        // Parse release tags into Version objects, maintaining release order (newest to oldest).
        // This is used to determine version history and find previous releases.
        var releaseVersions = branchReleases
            .Select(r => Version.TryCreate(r.TagName!))
            .Where(v => v != null)
            .Cast<Version>()
            .ToList();

        return new LookupData(
            issueById,
            commitHashToPr,
            branchReleases,
            tagsByName,
            tagToRelease,
            releaseVersions);
    }

    /// <summary>
    ///     Determines the target version and hash for the build.
    /// </summary>
    /// <param name="version">Optional target version provided by caller.</param>
    /// <param name="currentCommitHash">Current commit hash.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (toVersion, toHash).</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    internal static (Version toVersion, string toHash) DetermineTargetVersion(
        Version? version,
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
    /// <param name="lookupData">Lookup data structures.</param>
    /// <returns>Tuple of (fromVersion, fromHash).</returns>
    internal static (Version? fromVersion, string? fromHash) DetermineBaselineVersion(
        Version toVersion,
        LookupData lookupData)
    {
        // Return null baseline if no releases exist
        if (lookupData.ReleaseVersions.Count == 0)
        {
            return (null, null);
        }

        // Find the position of target version in release history
        var toIndex = FindVersionIndex(lookupData.ReleaseVersions, toVersion.FullVersion);

        // Determine baseline version based on whether target is pre-release
        var fromVersion = toVersion.IsPreRelease
            ? DetermineBaselineForPreRelease(toIndex, lookupData.ReleaseVersions)
            : DetermineBaselineForRelease(toIndex, lookupData.ReleaseVersions);

        // Get commit hash for baseline version if one was found
        if (fromVersion != null &&
            lookupData.TagToRelease.TryGetValue(fromVersion.Tag, out var fromRelease) &&
            lookupData.TagsByName.TryGetValue(fromRelease.TagName!, out var fromTagCommit))
        {
            return (fromVersion, fromTagCommit.Commit.Sha);
        }

        // Return baseline version with null hash if commit not found
        return (fromVersion, null);
    }

    /// <summary>
    ///     Determines the baseline version for a pre-release.
    /// </summary>
    /// <param name="toIndex">Index of target version in release history.</param>
    /// <param name="releaseVersions">List of release versions.</param>
    /// <returns>Baseline version or null.</returns>
    private static Version? DetermineBaselineForPreRelease(int toIndex, List<Version> releaseVersions)
    {
        // Pre-release versions use the immediately previous (older) release as baseline
        if (toIndex >= 0 && toIndex < releaseVersions.Count - 1)
        {
            // Target version exists in history, use next older release (higher index)
            return releaseVersions[toIndex + 1];
        }

        // Target version not in history, use most recent release as baseline
        if (toIndex == -1 && releaseVersions.Count > 0)
        {
            return releaseVersions[0];
        }

        // If toIndex is last in list, this is the oldest release, no baseline
        return null;
    }

    /// <summary>
    ///     Determines the baseline version for a release (non-pre-release).
    /// </summary>
    /// <param name="toIndex">Index of target version in release history.</param>
    /// <param name="releaseVersions">List of release versions.</param>
    /// <returns>Baseline version or null.</returns>
    private static Version? DetermineBaselineForRelease(int toIndex, List<Version> releaseVersions)
    {
        // Release versions skip pre-releases and use previous non-pre-release as baseline
        var startIndex = DetermineSearchStartIndex(toIndex, releaseVersions.Count);

        // Search forward through older releases (incrementing index) for previous non-pre-release version
        if (startIndex >= 0)
        {
            for (var i = startIndex; i < releaseVersions.Count; i++)
            {
                if (!releaseVersions[i].IsPreRelease)
                {
                    return releaseVersions[i];
                }
            }
        }

        return null;
    }

    /// <summary>
    ///     Determines the starting index for searching for previous releases.
    /// </summary>
    /// <param name="toIndex">Index of target version in release history.</param>
    /// <param name="releaseCount">Total number of releases.</param>
    /// <returns>Starting index for search, or -1 if no search needed.</returns>
    private static int DetermineSearchStartIndex(int toIndex, int releaseCount)
    {
        // Target version exists in history, start search from next older release
        if (toIndex >= 0 && toIndex < releaseCount - 1)
        {
            return toIndex + 1;
        }

        // Target version not in history, start from most recent release
        if (toIndex == -1 && releaseCount > 0)
        {
            // Target version not in history, start from most recent release
            // The target is newer than all existing releases, so use the most recent as baseline
            return 0;
        }

        // Target is oldest release or no releases exist, no previous release exists
        return -1;
    }

    /// <summary>
    ///     Collects changes from pull requests in the commit range.
    /// </summary>
    /// <param name="commitsInRange">Commits in range.</param>
    /// <param name="lookupData">Lookup data structures.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="token">GitHub token.</param>
    /// <returns>Tuple of (bugs, nonBugChanges, allChangeIds).</returns>
    private static async Task<(List<ItemInfo> bugs, List<ItemInfo> nonBugChanges, HashSet<string> allChangeIds)>
        CollectChangesFromPullRequestsAsync(
            List<GitHubCommit> commitsInRange,
            LookupData lookupData,
            string owner,
            string repo,
            string token)
    {
        // Initialize collections for tracking changes
        var allChangeIds = new HashSet<string>();
        var bugs = new List<ItemInfo>();
        var nonBugChanges = new List<ItemInfo>();

        // Create GraphQL client for finding linked issues (reused across multiple PR queries)
        using var graphqlClient = new GitHubGraphQLClient(token);

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
        Dictionary<int, Issue> issueById,
        PullRequest pr,
        HashSet<string> allChangeIds,
        List<ItemInfo> bugs,
        List<ItemInfo> nonBugChanges)
    {
        // PR has linked issues - add them (deduplicated)
        foreach (var issueId in linkedIssueIds)
        {
            // Check if issue already processed
            var issueIdStr = issueId.ToString();
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
        PullRequest pr,
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
    ///     Collects known issues (open bugs not fixed in this build).
    /// </summary>
    /// <param name="issues">All issues from GitHub.</param>
    /// <param name="allChangeIds">Set of all change IDs already processed.</param>
    /// <returns>List of known issues.</returns>
    private static List<ItemInfo> CollectKnownIssues(IReadOnlyList<Issue> issues, HashSet<string> allChangeIds)
    {
        return issues
            .Where(i => i.State == ItemState.Open)
            .Select(issue => (issue, issueId: issue.Number.ToString()))
            .Where(tuple => !allChangeIds.Contains(tuple.issueId))
            .Select(tuple => CreateItemInfoFromIssue(tuple.issue, tuple.issue.Number))
            .Where(itemInfo => itemInfo.Type == "bug")
            .ToList();
    }

    /// <summary>
    ///     Gets all commits for a branch using pagination.
    /// </summary>
    /// <param name="client">GitHub client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Branch name.</param>
    /// <returns>List of all commits.</returns>
    private static async Task<IReadOnlyList<GitHubCommit>> GetAllCommitsAsync(GitHubClient client, string owner, string repo, string branch)
    {
        // Create request for branch commits
        var request = new CommitRequest { Sha = branch };

        // Fetch and return all commits for the branch
        return await client.Repository.Commit.GetAll(owner, repo, request);
    }

    /// <summary>
    ///     Gets commits in the range from fromHash (exclusive) to toHash (inclusive).
    /// </summary>
    /// <param name="commits">All commits.</param>
    /// <param name="fromHash">Starting commit hash (exclusive - not included in results; null for start of history).</param>
    /// <param name="toHash">Ending commit hash (inclusive - included in results).</param>
    /// <returns>List of commits in range, excluding fromHash but including toHash.</returns>
    internal static List<GitHubCommit> GetCommitsInRange(IReadOnlyList<GitHubCommit> commits, string? fromHash, string toHash)
    {
        // Initialize collection and state tracking
        var result = new List<GitHubCommit>();
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
    ///     Creates an ItemInfo from an issue.
    /// </summary>
    /// <param name="issue">GitHub issue.</param>
    /// <param name="index">Index for sorting.</param>
    /// <returns>ItemInfo instance.</returns>
    internal static ItemInfo CreateItemInfoFromIssue(Issue issue, int index)
    {
        // Determine item type from issue labels
        var type = GetTypeFromLabels(issue.Labels);

        // Create and return item info with issue details
        return new ItemInfo(
            issue.Number.ToString(),
            issue.Title,
            issue.HtmlUrl,
            type,
            index);
    }

    /// <summary>
    ///     Creates an ItemInfo from a pull request.
    /// </summary>
    /// <param name="pr">GitHub pull request.</param>
    /// <returns>ItemInfo instance.</returns>
    internal static ItemInfo CreateItemInfoFromPullRequest(PullRequest pr)
    {
        // Determine item type from PR labels
        var type = GetTypeFromLabels(pr.Labels);

        // Create and return item info with PR details
        return new ItemInfo(
            $"#{pr.Number}",
            pr.Title,
            pr.HtmlUrl,
            type,
            pr.Number);
    }

    /// <summary>
    ///     Determines item type from labels.
    /// </summary>
    /// <param name="labels">List of labels.</param>
    /// <returns>Item type string.</returns>
    internal static string GetTypeFromLabels(IReadOnlyList<Label> labels)
    {
        // Find first matching label type by checking label names against the type map
        var matchingType = labels
            .Select(label => label.Name.ToLowerInvariant())
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel.Contains(kvp.Key))
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
        // Try to get token from environment variable first
        var token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }

        // Fall back to gh auth token
        try
        {
            return await RunCommandAsync("gh", "auth token");
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("Unable to get GitHub token. Set GH_TOKEN environment variable or authenticate with 'gh auth login'.");
        }
    }

    /// <summary>
    ///     Parses GitHub owner and repo from a git remote URL.
    /// </summary>
    /// <param name="url">Git remote URL.</param>
    /// <returns>Tuple of (owner, repo).</returns>
    /// <exception cref="ArgumentException">Thrown if URL format is invalid.</exception>
    internal static (string owner, string repo) ParseGitHubUrl(string url)
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
    internal static (string owner, string repo) ParseOwnerRepo(string path)
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
    /// <returns>WebLink to GitHub compare page, or null if no baseline tag.</returns>
    internal static WebLink? GenerateGitHubChangelogLink(string owner, string repo, string? oldTag, string newTag)
    {
        // Cannot generate comparison link without a baseline tag
        if (oldTag == null)
        {
            return null;
        }

        // Build comparison label and URL
        var comparisonLabel = $"{oldTag}...{newTag}";
        var comparisonUrl = $"https://github.com/{owner}/{repo}/compare/{comparisonLabel}";

        return new WebLink(comparisonLabel, comparisonUrl);
    }
}
