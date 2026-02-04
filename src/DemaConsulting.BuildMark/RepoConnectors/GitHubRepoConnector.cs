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

using System.Text.RegularExpressions;
using Octokit;

namespace DemaConsulting.BuildMark;

/// <summary>
///     GitHub repository connector implementation.
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

    private readonly GitHubClient _client;
    private readonly string _owner;
    private readonly string _repo;
    private readonly string _branch;
    private readonly string _commitSha;

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubRepoConnector" /> class.
    /// </summary>
    /// <param name="client">GitHub API client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    /// <param name="branch">Current branch name.</param>
    /// <param name="commitSha">Current commit SHA.</param>
    public GitHubRepoConnector(GitHubClient client, string owner, string repo, string branch, string commitSha)
    {
        _client = client;
        _owner = owner;
        _repo = repo;
        _branch = branch;
        _commitSha = commitSha;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubRepoConnector" /> class for testing.
    /// </summary>
    protected GitHubRepoConnector()
    {
        _client = null!;
        _owner = string.Empty;
        _repo = string.Empty;
        _branch = string.Empty;
        _commitSha = string.Empty;
    }

    /// <summary>
    ///     Validates and sanitizes a tag name to prevent command injection.
    /// </summary>
    /// <param name="tag">Tag name to validate.</param>
    /// <returns>Sanitized tag name.</returns>
    /// <exception cref="ArgumentException">Thrown if tag name is invalid.</exception>
    private static string ValidateTag(string tag)
    {
        // Ensure tag name matches allowed pattern to prevent injection attacks
        if (!TagNameRegex().IsMatch(tag))
        {
            throw new ArgumentException($"Invalid tag name: {tag}", nameof(tag));
        }

        return tag;
    }

    /// <summary>
    ///     Gets the history of releases leading to the current branch.
    /// </summary>
    /// <returns>List of release versions in chronological order.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Fetch all commits on the current branch using Octokit Repository.Commit.GetAll API (newest to oldest)</item>
    /// <item>Start collecting commits once we reach the current commit SHA</item>
    /// <item>Continue collecting all older commits (the history) until pagination completes</item>
    /// <item>Fetch all repository tags using Octokit Repository.GetAllTags API</item>
    /// <item>Filter tags to only those whose commit is in the branch history to build branchTags set</item>
    /// <item>Fetch all releases using Octokit Repository.Release.GetAll API (already in correct order)</item>
    /// <item>Filter releases to only those whose tag is in the branchTags set and return as Version objects</item>
    /// </list>
    /// This approach focuses on releases (which are tagged) rather than just tags.
    /// </remarks>
    public override async Task<List<Version>> GetReleaseHistoryAsync()
    {
        // Get all commits on the current branch up to the current commit
        var commitRequest = new CommitRequest
        {
            Sha = _branch
        };
        
        var commitOptions = new ApiOptions
        {
            PageSize = 100,
            PageCount = 1
        };
        
        // Fetch all commits on the branch
        var branchCommits = new HashSet<string>();
        var page = 1;
        var foundCurrentCommit = false;
        
        while (true)
        {
            commitOptions.StartPage = page;
            var commits = await _client.Repository.Commit.GetAll(_owner, _repo, commitRequest, commitOptions);
            if (commits.Count == 0)
            {
                break;
            }
            
            // Add commit SHAs and track if we've seen the current commit
            #pragma warning disable S3267 // Cannot simplify with Select due to early termination logic
            foreach (var commit in commits)
            {
                // Once we find the current commit, start including commits in history
                if (commit.Sha == _commitSha)
                {
                    foundCurrentCommit = true;
                }
                
                // Include this commit and all older commits (after finding current commit)
                if (foundCurrentCommit)
                {
                    branchCommits.Add(commit.Sha);
                }
            }
            #pragma warning restore S3267
            
            page++;
        }
        
        // Get all tags from the repository and filter to branch tags
        var allTags = await _client.Repository.GetAllTags(_owner, _repo);
        var branchTags = allTags
            .Where(tag => branchCommits.Contains(tag.Commit.Sha))
            .Select(tag => tag.Name)
            .ToHashSet();
        
        // Get all releases and filter by branch tags
        // Releases are already in correct order from GetAll, just filter and return
        var allReleases = await _client.Repository.Release.GetAll(_owner, _repo);
        return allReleases
            .Where(r => branchTags.Contains(r.TagName))
            .Select(r => Version.TryCreate(r.TagName))
            .Where(v => v != null)
            .Cast<Version>()
            .ToList();
    }

    /// <summary>
    ///     Gets the list of changes between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of changes with full information.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Determine commit range based on from/to parameters using Octokit Compare or Commit APIs</item>
    /// <item>Fetch all pull requests from repository (paginated)</item>
    /// <item>Filter PRs to only those with commits in the determined range</item>
    /// <item>Extract issue numbers from PR bodies using closing keyword regex (closes #123, fixes #456, etc.)</item>
    /// <item>Batch fetch all referenced issues with their labels and metadata</item>
    /// <item>Map issue types from labels (bug, feature, documentation, etc.)</item>
    /// <item>Return combined list of issues and PRs without issues, ordered by number</item>
    /// </list>
    /// Server-side filtering is used where possible (Compare API for commit ranges) to minimize data transfer.
    /// </remarks>
    public override async Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to)
    {
        // Get commits in the range using GitHub API
        HashSet<string> commitShas;

        if (from == null && to == null)
        {
            // No versions specified, get all commits using paginated API
            commitShas = await GetAllCommitsAsync(null);
        }
        else if (from == null)
        {
            // Only end version specified - get commits up to 'to' tag/HEAD
            var toExists = to != null && await TagExistsAsync(to.Tag);
            var toRef = toExists ? ValidateTag(to!.Tag) : _commitSha;
            commitShas = await GetAllCommitsAsync(toRef);
        }
        else if (to == null)
        {
            // Only start version specified - compare from tag to current commit
            var fromTag = ValidateTag(from.Tag);
            commitShas = await GetCompareCommitsAsync(fromTag, _commitSha);
        }
        else
        {
            // Both versions specified - compare from tag to to tag/HEAD
            var fromTag = ValidateTag(from.Tag);
            var toExists = await TagExistsAsync(to.Tag);
            var toRef = toExists ? ValidateTag(to.Tag) : _commitSha;
            commitShas = await GetCompareCommitsAsync(fromTag, toRef);
        }

        // Batch fetch PR information with all details in one call
        List<PullRequest> allPullRequests;
        try
        {
            // Fetch all PRs (open and closed)
            var request = new PullRequestRequest
            {
                State = ItemStateFilter.All
            };
            allPullRequests = new List<PullRequest>();
            var options = new ApiOptions
            {
                PageSize = 100,
                PageCount = 1
            };
            
            // Fetch all pages of PRs
            var page = 1;
            while (true)
            {
                options.StartPage = page;
                var prs = await _client.PullRequest.GetAllForRepository(_owner, _repo, request, options);
                if (prs.Count == 0)
                {
                    break;
                }
                allPullRequests.AddRange(prs);
                page++;
            }
        }
        catch (RateLimitExceededException)
        {
            // API rate limit exceeded - return empty result
            return new List<ItemInfo>();
        }
        catch (ApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            // Repository not found or not accessible - return empty result
            return new List<ItemInfo>();
        }
        catch (ApiException)
        {
            // Other API errors - return empty result
            return new List<ItemInfo>();
        }

        // Filter PRs to only those with commits in the range
        var relevantPrs = new List<PullRequest>();
        foreach (var pr in allPullRequests)
        {
            // Check if this PR has any commits in the range
            // We need to get the commits for this PR
            try
            {
                var prCommits = await _client.PullRequest.Commits(_owner, _repo, pr.Number);
                var hasRelevantCommit = prCommits.Any(c => commitShas.Contains(c.Sha));
                
                if (hasRelevantCommit)
                {
                    relevantPrs.Add(pr);
                }
            }
            catch
            {
                // If we can't get PR commits, skip this PR
                continue;
            }
        }

        // Parse PR data and extract changes
        var changes = new List<ItemInfo>();
        var issueNumbers = new HashSet<int>();
        var prData = new List<(int number, string title, string url, List<int> issueNumbers, IReadOnlyList<Label> labels)>();

        // First pass: collect issue numbers and PR data
        foreach (var pr in relevantPrs)
        {
            var prIssues = new List<int>();
            
            // Extract issue numbers from PR body using closing keywords
            if (!string.IsNullOrEmpty(pr.Body))
            {
                var issueMatches = ClosingIssueRegex().Matches(pr.Body);
                foreach (Match match in issueMatches)
                {
                    if (int.TryParse(match.Groups[1].Value, out var issueNum))
                    {
                        issueNumbers.Add(issueNum);
                        prIssues.Add(issueNum);
                    }
                }
            }

            prData.Add((pr.Number, pr.Title, pr.HtmlUrl, prIssues, pr.Labels));
        }

        // Build a map from issue number to PR number for Index assignment
        var issueToPrMap = new Dictionary<int, int>();
        foreach (var (prNumber, _, _, prIssues, _) in prData)
        {
            foreach (var issueNumber in prIssues)
            {
                // Use the first PR that references this issue (smallest PR number)
                if (!issueToPrMap.ContainsKey(issueNumber) || prNumber < issueToPrMap[issueNumber])
                {
                    issueToPrMap[issueNumber] = prNumber;
                }
            }
        }

        // Second pass: batch fetch all issue details using issue list
        var issueDetailsMap = new Dictionary<int, ItemInfo>();
        if (issueNumbers.Count > 0)
        {
            try
            {
                var issueRequest = new RepositoryIssueRequest
                {
                    State = ItemStateFilter.All
                };
                var issueOptions = new ApiOptions
                {
                    PageSize = 100,
                    PageCount = 1
                };
                
                // Fetch all pages of issues
                var issuePage = 1;
                while (true)
                {
                    issueOptions.StartPage = issuePage;
                    var issues = await _client.Issue.GetAllForRepository(_owner, _repo, issueRequest, issueOptions);
                    if (issues.Count == 0)
                    {
                        break;
                    }
                    
                    foreach (var issue in issues)
                    {
                        // Only process issues that are referenced by PRs and are not PRs themselves
                        if (!issueNumbers.Contains(issue.Number) || issue.PullRequest != null)
                        {
                            continue;
                        }

                        var issueNumber = issue.Number;
                        var issueTitle = issue.Title;
                        var issueUrl = issue.HtmlUrl;

                        // Determine type from labels
                        var issueType = DetermineTypeFromLabels(issue.Labels);

                        // Use PR number as Index for ordering
                        var index = issueToPrMap.TryGetValue(issueNumber, out var prNum) ? prNum : issueNumber;
                        issueDetailsMap[issueNumber] = new ItemInfo(issueNumber.ToString(), issueTitle, issueUrl, issueType, index);
                    }
                    
                    issuePage++;
                }
            }
            catch (RateLimitExceededException)
            {
                // If rate limit exceeded, create fallback entries
                foreach (var issueNumber in issueNumbers)
                {
                    var index = issueToPrMap.TryGetValue(issueNumber, out var prNum) ? prNum : issueNumber;
                    issueDetailsMap[issueNumber] = new ItemInfo(issueNumber.ToString(), $"Issue #{issueNumber}", string.Empty, "other", index);
                }
            }
            catch (ApiException)
            {
                // If we can't fetch issue list, create fallback entries
                foreach (var issueNumber in issueNumbers)
                {
                    var index = issueToPrMap.TryGetValue(issueNumber, out var prNum) ? prNum : issueNumber;
                    issueDetailsMap[issueNumber] = new ItemInfo(issueNumber.ToString(), $"Issue #{issueNumber}", string.Empty, "other", index);
                }
            }
        }

        // Third pass: add issues to changes list (avoiding duplicates)
        var addedIssues = new HashSet<int>();
        foreach (var (issueNumber, itemInfo) in issueDetailsMap)
        {
            if (!addedIssues.Contains(issueNumber))
            {
                changes.Add(itemInfo);
                addedIssues.Add(issueNumber);
            }
        }

        // Fourth pass: add PRs without issues
        foreach (var (prNumber, prTitle, prUrl, prIssues, labels) in prData)
        {
            if (prIssues.Count == 0)
            {
                // PR has no issues - determine type from labels
                var prType = DetermineTypeFromLabels(labels);

                changes.Add(new ItemInfo(
                    $"#{prNumber}",
                    prTitle,
                    prUrl,
                    prType,
                    prNumber));
            }
        }

        return changes;
    }

    /// <summary>
    ///     Gets the current commit hash.
    /// </summary>
    /// <returns>Current commit hash.</returns>
    /// <remarks>
    /// For GitHubRepoConnector, the current commit hash is already known at construction time,
    /// so this method simply returns the cached value without making any API calls.
    /// </remarks>
    public override Task<string> GetCurrentHashAsync()
    {
        return Task.FromResult(_commitSha);
    }

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name.</param>
    /// <returns>Git hash.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Fetch all repository tags using Octokit Repository.GetAllTags API</item>
    /// <item>Find the tag matching the provided tag name</item>
    /// <item>Return the commit SHA for that tag</item>
    /// </list>
    /// Uses Octokit API for consistent type-safe access to GitHub data.
    /// </remarks>
    public override async Task<string> GetHashForTagAsync(string? tag)
    {
        if (tag == null)
        {
            throw new ArgumentNullException(nameof(tag));
        }

        // Get all tags from the repository
        var allTags = await _client.Repository.GetAllTags(_owner, _repo);
        
        // Find the tag matching the provided name
        var matchingTag = allTags.FirstOrDefault(t => t.Name == tag);
        if (matchingTag == null)
        {
            throw new InvalidOperationException($"Tag '{tag}' not found in repository");
        }
        
        return matchingTag.Commit.Sha;
    }

    /// <summary>
    ///     Gets the list of open issues with their details.
    /// </summary>
    /// <returns>List of open issues with full information.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Query repository for open issues using Octokit API with server-side filtering (State=Open)</item>
    /// <item>Paginate through all results (100 per page)</item>
    /// <item>Filter out pull requests (they appear in issues list but have PullRequest property)</item>
    /// <item>Extract issue number, title, and URL from each issue</item>
    /// <item>Determine issue type from labels (bug, feature, documentation, etc.)</item>
    /// <item>Return list of ItemInfo objects with all metadata</item>
    /// </list>
    /// Uses server-side State filter to minimize data transfer.
    /// </remarks>
    public override async Task<List<ItemInfo>> GetOpenIssuesAsync()
    {
        // Fetch all open issues with full details in a single batch call
        var issueRequest = new RepositoryIssueRequest
        {
            State = ItemStateFilter.Open
        };
        var issueOptions = new ApiOptions
        {
            PageSize = 100,
            PageCount = 1
        };

        var openIssues = new List<ItemInfo>();

        try
        {
            // Fetch all pages of open issues
            var page = 1;
            while (true)
            {
                issueOptions.StartPage = page;
                var issues = await _client.Issue.GetAllForRepository(_owner, _repo, issueRequest, issueOptions);
                if (issues.Count == 0)
                {
                    break;
                }

                foreach (var issue in issues)
                {
                    // Skip pull requests (they also appear in issues list)
                    if (issue.PullRequest != null)
                    {
                        continue;
                    }

                    var issueNumber = issue.Number;
                    var issueTitle = issue.Title;
                    var issueUrl = issue.HtmlUrl;

                    // Determine type from labels
                    var issueType = DetermineTypeFromLabels(issue.Labels);

                    openIssues.Add(new ItemInfo(issueNumber.ToString(), issueTitle, issueUrl, issueType, issueNumber));
                }

                page++;
            }
        }
        catch (RateLimitExceededException)
        {
            // Return empty list if rate limit exceeded
            return new List<ItemInfo>();
        }
        catch (ApiException)
        {
            // Return empty list if API query fails
            return new List<ItemInfo>();
        }

        return openIssues;
    }

    /// <summary>
    ///     Gets all commits for the repository or up to a specific ref.
    /// </summary>
    /// <param name="sha">Optional SHA or ref to get commits up to.</param>
    /// <returns>Set of commit SHAs.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Create commit request with optional SHA filter for server-side filtering</item>
    /// <item>Paginate through all commits (100 per page) using Octokit API</item>
    /// <item>Collect all commit SHAs into a HashSet for fast lookups</item>
    /// <item>Return complete set of commit SHAs in the range</item>
    /// </list>
    /// </remarks>
    private async Task<HashSet<string>> GetAllCommitsAsync(string? sha)
    {
        var commitShas = new HashSet<string>();
        var request = new CommitRequest();
        if (sha != null)
        {
            request.Sha = sha;
        }
        
        var options = new ApiOptions
        {
            PageSize = 100,
            PageCount = 1
        };
        
        // Fetch all pages of commits
        var page = 1;
        while (true)
        {
            options.StartPage = page;
            var commits = await _client.Repository.Commit.GetAll(_owner, _repo, request, options);
            if (commits.Count == 0)
            {
                break;
            }
            
            foreach (var commit in commits)
            {
                commitShas.Add(commit.Sha);
            }
            
            page++;
        }
        
        return commitShas;
    }

    /// <summary>
    ///     Gets commits between two refs using compare API.
    /// </summary>
    /// <param name="fromRef">Starting ref.</param>
    /// <param name="toRef">Ending ref.</param>
    /// <returns>Set of commit SHAs.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Call Octokit Compare API with from and to refs for server-side filtering</item>
    /// <item>Extract commit SHAs from comparison result</item>
    /// <item>Return as HashSet for fast lookups when filtering PRs</item>
    /// </list>
    /// Uses server-side Compare API to minimize data transfer - much more efficient than fetching all commits.
    /// </remarks>
    private async Task<HashSet<string>> GetCompareCommitsAsync(string fromRef, string toRef)
    {
        var comparison = await _client.Repository.Commit.Compare(_owner, _repo, fromRef, toRef);
        var commitShas = new HashSet<string>();
        
        foreach (var commit in comparison.Commits)
        {
            commitShas.Add(commit.Sha);
        }
        
        return commitShas;
    }

    /// <summary>
    ///     Checks if a git tag exists in the repository.
    /// </summary>
    /// <param name="tag">Tag name to check.</param>
    /// <returns>True if the tag exists, false otherwise.</returns>
    /// <remarks>
    /// Workflow:
    /// <list type="number">
    /// <item>Attempt to get the tag reference using Octokit Git.Reference.Get API</item>
    /// <item>If successful, tag exists - return true</item>
    /// <item>If NotFoundException is thrown, tag doesn't exist - return false</item>
    /// </list>
    /// </remarks>
    private async Task<bool> TagExistsAsync(string tag)
    {
        try
        {
            // Try to get the tag reference
            await _client.Git.Reference.Get(_owner, _repo, $"tags/{tag}");
            return true;
        }
        catch (NotFoundException)
        {
            // Tag doesn't exist
            return false;
        }
    }

    /// <summary>
    ///     Determines the type from a list of labels.
    /// </summary>
    /// <param name="labels">Labels to check.</param>
    /// <returns>Type string.</returns>
    private static string DetermineTypeFromLabels(IReadOnlyList<Label> labels)
    {
        var labelNames = labels.Select(l => l.Name.ToLowerInvariant()).ToList();
        
        // Map labels to type
        var matchingType = labelNames
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel.Contains(kvp.Key))
                .Select(kvp => kvp.Value))
            .FirstOrDefault();
        
        return matchingType ?? "other";
    }

    /// <summary>
    ///     Regular expression to match closing issue keywords in PR bodies.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"(?:close[sd]?|fix(?:e[sd])?|resolve[sd]?)\s+#(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex ClosingIssueRegex();

    /// <summary>
    ///     Regular expression to match valid tag names (alphanumeric, dots, hyphens, underscores, slashes).
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9._/-]+$", RegexOptions.Compiled)]
    private static partial Regex TagNameRegex();
}
