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

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubRepoConnector" /> class.
    /// </summary>
    /// <param name="client">GitHub API client.</param>
    /// <param name="owner">Repository owner.</param>
    /// <param name="repo">Repository name.</param>
    public GitHubRepoConnector(GitHubClient client, string owner, string repo)
    {
        _client = client;
        _owner = owner;
        _repo = repo;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="GitHubRepoConnector" /> class for testing.
    /// </summary>
    protected GitHubRepoConnector()
    {
        _client = null!;
        _owner = string.Empty;
        _repo = string.Empty;
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
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    public override async Task<List<Version>> GetTagHistoryAsync()
    {
        // Get all tags merged into current branch, sorted by creation date
        // Arguments: --sort=creatordate (chronological order), --merged HEAD (reachable from HEAD)
        // Output format: one tag name per line
        var output = await RunCommandAsync("git", "tag --sort=creatordate --merged HEAD");

        // Split output into individual tag names
        var tagNames = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();

        // Parse and filter to valid version tags only
        return tagNames
            .Select(Version.TryCreate)
            .Where(t => t != null)
            .Cast<Version>()
            .ToList();
    }

    /// <summary>
    ///     Gets the list of changes between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of changes with full information.</returns>
    public override async Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to)
    {
        // Note: Currently fetches all PRs and issues regardless of version range.
        // Version filtering would require mapping commits to PRs, which is complex.
        // The from/to parameters are preserved for future enhancement.

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

        // Parse PR data and extract changes
        var changes = new List<ItemInfo>();
        var issueNumbers = new HashSet<int>();
        var prData = new List<(int number, string title, string url, List<int> issueNumbers, IReadOnlyList<Label> labels)>();

        // First pass: collect issue numbers and PR data
        foreach (var pr in allPullRequests)
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
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name (null for current state).</param>
    /// <returns>Git hash.</returns>
    public override async Task<string> GetHashForTagAsync(string? tag)
    {
        // Get commit hash for tag or HEAD using git rev-parse
        // Arguments: tag name or "HEAD" for current commit
        // Output: full 40-character commit SHA
        var refName = tag == null ? "HEAD" : ValidateTag(tag);
        return await RunCommandAsync("git", $"rev-parse {refName}");
    }

    /// <summary>
    ///     Gets the list of open issues with their details.
    /// </summary>
    /// <returns>List of open issues with full information.</returns>
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
