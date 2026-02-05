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

namespace DemaConsulting.BuildMark;

/// <summary>
///     GitHub repository connector implementation using Octokit.Net.
/// </summary>
public partial class GitHubRepoConnector : RepoConnectorBase
{
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
    public override async Task<BuildInformation> GetBuildInformationAsync(DemaConsulting.BuildMark.Version? version = null)
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
        var client = new GitHubClient(new ProductHeaderValue("BuildMark"))
        {
            Credentials = new Credentials(token)
        };

        // Fetch all data from GitHub in parallel
        var commitsTask = GetAllCommitsAsync(client, owner, repo, branch.Trim());
        var releasesTask = client.Repository.Release.GetAll(owner, repo);
        var tagsTask = client.Repository.GetAllTags(owner, repo);
        var pullRequestsTask = client.PullRequest.GetAllForRepository(owner, repo, new PullRequestRequest { State = ItemStateFilter.All });
        var issuesTask = client.Issue.GetAllForRepository(owner, repo, new RepositoryIssueRequest { State = ItemStateFilter.All });

        await Task.WhenAll(commitsTask, releasesTask, tagsTask, pullRequestsTask, issuesTask);

        var commits = await commitsTask;
        var releases = await releasesTask;
        var tags = await tagsTask;
        var pullRequests = await pullRequestsTask;
        var issues = await issuesTask;

        // Build helper dictionaries
        var commitHashToPr = BuildCommitToPrMap(pullRequests);
        
        // Build a set of commit SHAs in the current branch for filtering
        var branchCommitShas = new HashSet<string>(commits.Select(c => c.Sha));
        
        // Build a mapping from tag name to tag object for quick lookup
        var tagsByName = tags.ToDictionary(t => t.Name, t => t);

        // Filter releases to only those with tags that point to commits in the current branch
        // This avoids confusion with releases from other branches (like LTS branches)
        var branchReleases = releases
            .Where(r => !string.IsNullOrEmpty(r.TagName) && 
                        tagsByName.TryGetValue(r.TagName, out var tag) && 
                        branchCommitShas.Contains(tag.Commit.Sha))
            .ToList();

        // Build a mapping from tag name to release for version lookup
        var tagToRelease = branchReleases.ToDictionary(r => r.TagName!, r => r);

        // Parse release tags into Version objects, maintaining release order (newest to oldest)
        var releaseVersions = branchReleases
            .Select(r => DemaConsulting.BuildMark.Version.TryCreate(r.TagName!))
            .Where(v => v != null)
            .Cast<DemaConsulting.BuildMark.Version>()
            .ToList();

        // Determine the target version and hash for build information
        DemaConsulting.BuildMark.Version toTagInfo;
        string toHash = currentCommitHash.Trim();
        if (version != null)
        {
            // Use explicitly specified version as target
            toTagInfo = version;
        }
        else if (releaseVersions.Count > 0)
        {
            // Use the most recent release (first in list since releases are newest to oldest)
            var latestRelease = branchReleases[0];
            var latestReleaseVersion = releaseVersions[0];
            var latestTagCommit = tagsByName[latestRelease.TagName!];

            if (latestTagCommit.Commit.Sha == toHash)
            {
                // Current commit matches latest release tag, use it as target
                toTagInfo = latestReleaseVersion;
            }
            else
            {
                // Current commit doesn't match any release tag, cannot determine version
                throw new InvalidOperationException(
                    "Target version not specified and current commit does not match any release tag. " +
                    "Please provide a version parameter.");
            }
        }
        else
        {
            // No releases in repository and no version provided
            throw new InvalidOperationException(
                "No releases found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Determine the starting release for comparing changes
        DemaConsulting.BuildMark.Version? fromTagInfo = null;
        string? fromHash = null;
        
        if (releaseVersions.Count > 0)
        {
            // Find the position of target version in release history
            var toIndex = FindVersionIndex(releaseVersions, toTagInfo.FullVersion);

            if (toTagInfo.IsPreRelease)
            {
                // Pre-release versions use the immediately previous release as baseline
                if (toIndex > 0)
                {
                    // Target version exists in history, use previous release
                    fromTagInfo = releaseVersions[toIndex - 1];
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, use most recent release as baseline
                    fromTagInfo = releaseVersions[0];
                }
                // If toIndex == 0, this is the first release, no baseline
            }
            else
            {
                // Release versions skip pre-releases and use previous non-pre-release as baseline
                int startIndex;
                if (toIndex > 0)
                {
                    // Target version exists in history, start search from previous position
                    startIndex = toIndex - 1;
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, start from most recent release
                    startIndex = 0;
                }
                else
                {
                    // Target is first release, no previous release exists
                    startIndex = -1;
                }

                // Search forward (toward older releases) for previous non-pre-release version
                for (var i = startIndex; i >= 0 && i < releaseVersions.Count; i++)
                {
                    if (!releaseVersions[i].IsPreRelease)
                    {
                        fromTagInfo = releaseVersions[i];
                        break;
                    }
                }
            }

            // Get commit hash for baseline version if one was found
            if (fromTagInfo != null && 
                tagToRelease.TryGetValue(fromTagInfo.Tag, out var fromRelease) &&
                tagsByName.TryGetValue(fromRelease.TagName!, out var fromTagCommit))
            {
                fromHash = fromTagCommit.Commit.Sha;
            }
        }

        // Get commits in range
        var commitsInRange = GetCommitsInRange(commits, fromHash, toHash);

        // Collect changes from PRs
        var allChangeIds = new HashSet<string>();
        var bugs = new List<ItemInfo>();
        var nonBugChanges = new List<ItemInfo>();

        foreach (var commit in commitsInRange)
        {
            if (commitHashToPr.TryGetValue(commit.Sha, out var pr))
            {
                // Find issues that are linked to this PR
                // Use Issue.PullRequest.HtmlUrl to match with PullRequest.HtmlUrl
                var linkedIssues = issues.Where(i => 
                    i.State == ItemState.Closed && 
                    i.PullRequest != null &&
                    i.PullRequest.HtmlUrl == pr.HtmlUrl).ToList();

                if (linkedIssues.Count > 0)
                {
                    // PR closed issues - add them
                    foreach (var issue in linkedIssues)
                    {
                        var issueId = issue.Number.ToString();
                        if (allChangeIds.Contains(issueId))
                        {
                            continue;
                        }

                        allChangeIds.Add(issueId);
                        var itemInfo = CreateItemInfoFromIssue(issue, pr.Number);
                        
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
                else
                {
                    // PR didn't close any issues - add the PR itself
                    var prId = $"#{pr.Number}";
                    if (!allChangeIds.Contains(prId))
                    {
                        allChangeIds.Add(prId);
                        var itemInfo = CreateItemInfoFromPullRequest(pr);
                        
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
        }

        // Collect known issues (open bugs not fixed in this build)
        var knownIssues = issues
            .Where(i => i.State == ItemState.Open)
            .Select(issue => (issue, issueId: issue.Number.ToString()))
            .Where(tuple => !allChangeIds.Contains(tuple.issueId))
            .Select(tuple => CreateItemInfoFromIssue(tuple.issue, tuple.issue.Number))
            .Where(itemInfo => itemInfo.Type == "bug")
            .ToList();

        // Sort all lists by Index to ensure chronological order
        nonBugChanges.Sort((a, b) => a.Index.CompareTo(b.Index));
        bugs.Sort((a, b) => a.Index.CompareTo(b.Index));
        knownIssues.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Create and return build information with all collected data
        return new BuildInformation(
            fromTagInfo,
            toTagInfo,
            fromHash,
            toHash,
            nonBugChanges,
            bugs,
            knownIssues);
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
        var request = new CommitRequest { Sha = branch };
        return await client.Repository.Commit.GetAll(owner, repo, request);
    }

    /// <summary>
    ///     Builds a map from commit hash (merge SHA) to pull request.
    /// </summary>
    /// <param name="pullRequests">List of pull requests.</param>
    /// <returns>Dictionary mapping commit hash to pull request.</returns>
    private static Dictionary<string, PullRequest> BuildCommitToPrMap(IReadOnlyList<PullRequest> pullRequests)
    {
        var map = new Dictionary<string, PullRequest>();
        
        foreach (var pr in pullRequests.Where(p => p.Merged && p.MergeCommitSha != null))
        {
            map[pr.MergeCommitSha] = pr;
        }

        return map;
    }

    /// <summary>
    ///     Gets commits in the range from fromHash to toHash.
    /// </summary>
    /// <param name="commits">All commits.</param>
    /// <param name="fromHash">Starting commit hash (null for start of history).</param>
    /// <param name="toHash">Ending commit hash.</param>
    /// <returns>List of commits in range.</returns>
    private static List<GitHubCommit> GetCommitsInRange(IReadOnlyList<GitHubCommit> commits, string? fromHash, string toHash)
    {
        var result = new List<GitHubCommit>();
        var foundTo = false;

        // Iterate through commits from newest to oldest
        foreach (var commit in commits)
        {
            if (commit.Sha == toHash)
            {
                foundTo = true;
            }

            if (foundTo)
            {
                result.Add(commit);
            }

            if (commit.Sha == fromHash)
            {
                break;
            }
        }

        return result;
    }

    /// <summary>
    ///     Creates an ItemInfo from an issue.
    /// </summary>
    /// <param name="issue">GitHub issue.</param>
    /// <param name="index">Index for sorting.</param>
    /// <returns>ItemInfo instance.</returns>
    private static ItemInfo CreateItemInfoFromIssue(Issue issue, int index)
    {
        var type = GetTypeFromLabels(issue.Labels);
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
    private static ItemInfo CreateItemInfoFromPullRequest(PullRequest pr)
    {
        var type = GetTypeFromLabels(pr.Labels);
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
    private static string GetTypeFromLabels(IReadOnlyList<Label> labels)
    {
        var matchingType = labels
            .Select(label => label.Name.ToLowerInvariant())
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel.Contains(kvp.Key))
                .Select(kvp => kvp.Value))
            .FirstOrDefault();

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
    private static (string owner, string repo) ParseGitHubUrl(string url)
    {
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

        var parts = path.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException($"Invalid GitHub path format: {path}");
        }

        return (parts[0], parts[1]);
    }
}
