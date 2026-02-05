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

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
///     Mock repository connector for deterministic testing.
/// </summary>
public class MockRepoConnector : RepoConnectorBase
{
    private readonly Dictionary<string, string> _issueTitles = new()
    {
        { "1", "Add feature X" },
        { "2", "Fix bug in Y" },
        { "3", "Update documentation" },
        { "4", "Known bug A" },
        { "5", "Known bug B" }
    };

    private readonly Dictionary<string, string> _issueTypes = new()
    {
        { "1", "feature" },
        { "2", "bug" },
        { "3", "documentation" },
        { "4", "bug" },
        { "5", "bug" }
    };

    private readonly Dictionary<string, List<string>> _pullRequestIssues = new()
    {
        { "10", new List<string> { "1" } },
        { "11", new List<string> { "2" } },
        { "12", new List<string> { "3" } },
        { "13", new List<string>() } // PR with no issues
    };

    private readonly Dictionary<string, string> _tagHashes = new()
    {
        { "v1.0.0", "abc123def456" },
        { "ver-1.1.0", "def456ghi789" },
        { "release_2.0.0-beta.1", "ghi789jkl012" },
        { "v2.0.0-rc.1", "jkl012mno345" },
        { "2.0.0", "mno345pqr678" }
    };

    private readonly List<string> _openIssues = ["4", "5"];

    /// <summary>
    ///     Gets build information for a release.
    /// </summary>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public override async Task<BuildInformation> GetBuildInformationAsync(Version? version = null)
    {
        // Retrieve tag history and current commit hash from the repository
        var tags = await GetTagHistoryAsync();
        var currentHash = await GetHashForTagAsync(null);

        // Determine the target version and hash for build information
        var (toTagInfo, toHash) = await DetermineTargetVersionAsync(version, tags, currentHash);

        // Determine the starting version for comparing changes
        var (fromTagInfo, fromHash) = await DetermineBaselineVersionAsync(toTagInfo, tags);

        // Collect all changes (issues and PRs) in version range
        var changes = await GetChangesBetweenTagsAsync(fromTagInfo, toTagInfo);

        // Categorize changes into bugs and non-bug changes
        var (bugs, nonBugChanges, allChangeIds) = CategorizeChanges(changes);

        // Collect known issues (open bugs not fixed in this build)
        var knownIssues = await CollectKnownIssuesAsync(allChangeIds);

        // Sort all lists by Index to ensure chronological order
        nonBugChanges.Sort((a, b) => a.Index.CompareTo(b.Index));
        bugs.Sort((a, b) => a.Index.CompareTo(b.Index));
        knownIssues.Sort((a, b) => a.Index.CompareTo(b.Index));

        // Create and return build information with all collected data
        return new BuildInformation(
            fromTagInfo,
            toTagInfo,
            fromHash?.Trim(),
            toHash.Trim(),
            nonBugChanges,
            bugs,
            knownIssues);
    }

    /// <summary>
    ///     Determines the target version and hash for the build.
    /// </summary>
    /// <param name="version">Optional target version provided by caller.</param>
    /// <param name="tags">List of tag history.</param>
    /// <param name="currentHash">Current commit hash.</param>
    /// <returns>Tuple of (toTagInfo, toHash).</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    private async Task<(Version toTagInfo, string toHash)> DetermineTargetVersionAsync(
        Version? version,
        List<Version> tags,
        string currentHash)
    {
        // Use explicitly specified version if provided
        if (version != null)
        {
            return (version, currentHash);
        }

        // Validate that repository has tags
        if (tags.Count == 0)
        {
            // No tags in repository and no version provided
            throw new InvalidOperationException(
                "No tags found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Verify current commit matches latest tag when no version specified
        var latestTag = tags[^1];
        var latestTagHash = await GetHashForTagAsync(latestTag.Tag);

        // Check if current commit matches the latest tag
        if (latestTagHash.Trim() == currentHash.Trim())
        {
            // Current commit matches latest tag, use it as target
            return (latestTag, currentHash);
        }

        // Current commit doesn't match any tag, cannot determine version
        throw new InvalidOperationException(
            "Target version not specified and current commit does not match any tag. " +
            "Please provide a version parameter.");
    }

    /// <summary>
    ///     Determines the baseline version for comparing changes.
    /// </summary>
    /// <param name="toTagInfo">Target version.</param>
    /// <param name="tags">List of tag history.</param>
    /// <returns>Tuple of (fromTagInfo, fromHash).</returns>
    private async Task<(Version? fromTagInfo, string? fromHash)> DetermineBaselineVersionAsync(
        Version toTagInfo,
        List<Version> tags)
    {
        // Return null baseline if no tags exist
        if (tags.Count == 0)
        {
            return (null, null);
        }

        // Find the position of target version in tag history
        var toIndex = FindVersionIndex(tags, toTagInfo.FullVersion);

        // Determine baseline version based on whether target is pre-release
        Version? fromTagInfo;
        if (toTagInfo.IsPreRelease)
        {
            fromTagInfo = DetermineBaselineForPreRelease(toIndex, tags);
        }
        else
        {
            fromTagInfo = DetermineBaselineForRelease(toIndex, tags);
        }

        // Get commit hash for baseline version if one was found
        if (fromTagInfo != null)
        {
            var fromHash = await GetHashForTagAsync(fromTagInfo.Tag);
            return (fromTagInfo, fromHash);
        }

        // Return baseline version with null hash
        return (fromTagInfo, null);
    }

    /// <summary>
    ///     Determines the baseline version for a pre-release.
    /// </summary>
    /// <param name="toIndex">Index of target version in tag history.</param>
    /// <param name="tags">List of tags.</param>
    /// <returns>Baseline version or null.</returns>
    private static Version? DetermineBaselineForPreRelease(int toIndex, List<Version> tags)
    {
        // Pre-release versions use the immediately previous tag as baseline
        if (toIndex > 0)
        {
            // Target version exists in history, use previous tag
            return tags[toIndex - 1];
        }

        // Target version not in history, use most recent tag as baseline
        if (toIndex == -1)
        {
            return tags[^1];
        }

        // If toIndex == 0, this is the first tag, no baseline
        return null;
    }

    /// <summary>
    ///     Determines the baseline version for a release (non-pre-release).
    /// </summary>
    /// <param name="toIndex">Index of target version in tag history.</param>
    /// <param name="tags">List of tags.</param>
    /// <returns>Baseline version or null.</returns>
    private static Version? DetermineBaselineForRelease(int toIndex, List<Version> tags)
    {
        // Release versions skip pre-releases and use previous release as baseline
        var startIndex = DetermineSearchStartIndex(toIndex, tags.Count);

        // Search backward for previous non-pre-release version
        for (var i = startIndex; i >= 0; i--)
        {
            if (!tags[i].IsPreRelease)
            {
                return tags[i];
            }
        }

        // No previous non-pre-release version found
        return null;
    }

    /// <summary>
    ///     Determines the starting index for searching for previous releases.
    /// </summary>
    /// <param name="toIndex">Index of target version in tag history.</param>
    /// <param name="tagCount">Total number of tags.</param>
    /// <returns>Starting index for search, or -1 if no search needed.</returns>
    private static int DetermineSearchStartIndex(int toIndex, int tagCount)
    {
        // Target version exists in history, start search from previous position
        if (toIndex > 0)
        {
            return toIndex - 1;
        }

        // Target version not in history, start from most recent tag
        if (toIndex == -1)
        {
            return tagCount - 1;
        }

        // Target is first tag, no previous release exists
        return -1;
    }

    /// <summary>
    ///     Categorizes changes into bugs and non-bug changes.
    /// </summary>
    /// <param name="changes">List of all changes.</param>
    /// <returns>Tuple of (bugs, nonBugChanges, allChangeIds).</returns>
    private static (List<ItemInfo> bugs, List<ItemInfo> nonBugChanges, HashSet<string> allChangeIds)
        CategorizeChanges(List<ItemInfo> changes)
    {
        // Initialize collections for categorized changes
        var allChangeIds = new HashSet<string>();
        var bugs = new List<ItemInfo>();
        var nonBugChanges = new List<ItemInfo>();

        // Process and categorize each change
        foreach (var change in changes)
        {
            // Skip changes already processed
            if (allChangeIds.Contains(change.Id))
            {
                continue;
            }

            // Mark change as processed
            allChangeIds.Add(change.Id);

            // Categorize change by type
            if (change.Type == "bug")
            {
                bugs.Add(change);
            }
            else
            {
                nonBugChanges.Add(change);
            }
        }

        // Return categorized changes
        return (bugs, nonBugChanges, allChangeIds);
    }

    /// <summary>
    ///     Collects known issues (open bugs not fixed in this build).
    /// </summary>
    /// <param name="allChangeIds">Set of all change IDs already processed.</param>
    /// <returns>List of known issues.</returns>
    private async Task<List<ItemInfo>> CollectKnownIssuesAsync(HashSet<string> allChangeIds)
    {
        // Initialize collection for known issues
        var knownIssues = new List<ItemInfo>();
        var openIssues = await GetOpenIssuesAsync();

        // Process each open issue
        foreach (var issue in openIssues)
        {
            // Skip issues already fixed in this build
            if (allChangeIds.Contains(issue.Id))
            {
                continue;
            }

            // Only include bugs in known issues list
            if (issue.Type == "bug")
            {
                knownIssues.Add(issue);
            }
        }

        // Return collected known issues
        return knownIssues;
    }

    /// <summary>
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    private Task<List<Version>> GetTagHistoryAsync()
    {
        // Parse all mock tag names into Version objects
        var tagInfoList = _tagHashes.Keys
            .Select(Version.TryCreate)
            .Where(t => t != null)
            .Cast<Version>()
            .ToList();

        // Return parsed tag history
        return Task.FromResult(tagInfoList);
    }

    /// <summary>
    ///     Gets the list of changes between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of changes with full information.</returns>
    private Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to)
    {
        // Extract tag names from version objects
        var fromTagName = from?.Tag;
        var toTagName = to?.Tag;

        // Get PRs based on tag range
        var prs = GetPullRequestsForTagRange(fromTagName, toTagName);

        // Build changes from PRs
        var changes = BuildChangesFromPullRequests(prs);

        // Return collected changes
        return Task.FromResult(changes);
    }

    /// <summary>
    ///     Gets the list of pull requests for a given tag range.
    /// </summary>
    /// <param name="fromTagName">Starting tag name.</param>
    /// <param name="toTagName">Ending tag name.</param>
    /// <returns>List of pull request IDs.</returns>
    private static List<string> GetPullRequestsForTagRange(string? fromTagName, string? toTagName)
    {
        // Return PRs for specific tag ranges based on mock data
        if (fromTagName == "v1.0.0" && toTagName == "ver-1.1.0")
        {
            return ["10", "13"]; // Include PR without issues
        }

        // Return PRs for version 2.0.0 range
        if (fromTagName == "ver-1.1.0" && (toTagName == "2.0.0" || toTagName == "v2.0.0"))
        {
            return ["11", "12"];
        }

        // Return PRs for initial version
        if (string.IsNullOrEmpty(fromTagName) && toTagName == "v1.0.0")
        {
            return ["10"];
        }

        // Default: return all PRs
        return ["10", "11", "12", "13"];
    }

    /// <summary>
    ///     Builds a list of changes from pull requests.
    /// </summary>
    /// <param name="prs">List of pull request IDs.</param>
    /// <returns>List of changes.</returns>
    private List<ItemInfo> BuildChangesFromPullRequests(List<string> prs)
    {
        // Initialize collection for changes
        var changes = new List<ItemInfo>();

        // Process each pull request
        foreach (var pr in prs)
        {
            // Get issues for this PR
            var issues = _pullRequestIssues.TryGetValue(pr, out var prIssues) ? prIssues : [];

            // Process PR based on whether it has associated issues
            if (issues.Count > 0)
            {
                AddIssuesAsChanges(changes, issues, pr);
            }
            else
            {
                AddPullRequestAsChange(changes, pr);
            }
        }

        // Return collected changes
        return changes;
    }

    /// <summary>
    ///     Adds issues as changes to the changes list.
    /// </summary>
    /// <param name="changes">List of changes (modified in place).</param>
    /// <param name="issues">List of issue IDs.</param>
    /// <param name="pr">Pull request ID.</param>
    private void AddIssuesAsChanges(List<ItemInfo> changes, List<string> issues, string pr)
    {
        // PR has associated issues - add them as changes
        // Use PR number as index for chronological ordering
        foreach (var issueId in issues)
        {
            // Get issue details from mock data
            var title = _issueTitles.TryGetValue(issueId, out var issueTitle) ? issueTitle : $"Issue {issueId}";
            var url = $"https://github.com/example/repo/issues/{issueId}";
            var type = _issueTypes.TryGetValue(issueId, out var issueType) ? issueType : "other";

            // Add issue as a change
            changes.Add(new ItemInfo(issueId, title, url, type, int.Parse(pr)));
        }
    }

    /// <summary>
    ///     Adds a pull request as a change to the changes list.
    /// </summary>
    /// <param name="changes">List of changes (modified in place).</param>
    /// <param name="pr">Pull request ID.</param>
    private static void AddPullRequestAsChange(List<ItemInfo> changes, string pr)
    {
        // PR has no issues - treat the PR itself as a change
        changes.Add(new ItemInfo(
            $"#{pr}",
            $"PR #{pr}",
            $"https://github.com/example/repo/pull/{pr}",
            "other",
            int.Parse(pr)));
    }

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name (null for current state).</param>
    /// <returns>Git hash.</returns>
    private Task<string> GetHashForTagAsync(string? tag)
    {
        // Return current hash for null tag
        if (tag == null)
        {
            return Task.FromResult("current123hash456");
        }

        // Return hash for known tags or default value
        return Task.FromResult(
            _tagHashes.TryGetValue(tag, out var hash)
                ? hash
                : "unknown000hash000");
    }

    /// <summary>
    ///     Gets the list of open issues with their details.
    /// </summary>
    /// <returns>List of open issues with full information.</returns>
    private Task<List<ItemInfo>> GetOpenIssuesAsync()
    {
        // Return predefined list of open issues with full details
        var openIssuesData = _openIssues
            .Select(issueId => new ItemInfo(
                issueId,
                _issueTitles.TryGetValue(issueId, out var title) ? title : $"Issue {issueId}",
                $"https://github.com/example/repo/issues/{issueId}",
                _issueTypes.TryGetValue(issueId, out var type) ? type : "other",
                int.Parse(issueId)))
            .ToList();

        // Return task with open issues data
        return Task.FromResult(openIssuesData);
    }
}
