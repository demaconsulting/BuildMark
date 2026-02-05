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

namespace DemaConsulting.BuildMark;

/// <summary>
///     Mock repository connector for deterministic testing.
/// </summary>
public class MockRepoConnector : IRepoConnector
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

    private readonly List<string> _openIssues = new() { "4", "5" };

    /// <summary>
    ///     Gets build information for a release.
    /// </summary>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public async Task<BuildInformation> GetBuildInformationAsync(Version? version = null)
    {
        // Retrieve tag history and current commit hash from the repository
        var tags = await GetTagHistoryAsync();
        var currentHash = await GetHashForTagAsync(null);

        // Determine the target version and hash for build information
        Version toTagInfo;
        string toHash;
        if (version != null)
        {
            // Use explicitly specified version as target
            toTagInfo = version;
            toHash = currentHash;
        }
        else if (tags.Count > 0)
        {
            // Verify current commit matches latest tag when no version specified
            var latestTag = tags[^1];
            var latestTagHash = await GetHashForTagAsync(latestTag.Tag);

            if (latestTagHash.Trim() == currentHash.Trim())
            {
                // Current commit matches latest tag, use it as target
                toTagInfo = latestTag;
                toHash = currentHash;
            }
            else
            {
                // Current commit doesn't match any tag, cannot determine version
                throw new InvalidOperationException(
                    "Target version not specified and current commit does not match any tag. " +
                    "Please provide a version parameter.");
            }
        }
        else
        {
            // No tags in repository and no version provided
            throw new InvalidOperationException(
                "No tags found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Determine the starting version for comparing changes
        Version? fromTagInfo = null;
        string? fromHash = null;
        if (tags.Count > 0)
        {
            // Find the position of target version in tag history
            var toIndex = BuildInformation.FindTagIndex(tags, toTagInfo.FullVersion);

            if (toTagInfo.IsPreRelease)
            {
                // Pre-release versions use the immediately previous tag as baseline
                if (toIndex > 0)
                {
                    // Target version exists in history, use previous tag
                    fromTagInfo = tags[toIndex - 1];
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, use most recent tag as baseline
                    fromTagInfo = tags[^1];
                }
                // If toIndex == 0, this is the first tag, no baseline
            }
            else
            {
                // Release versions skip pre-releases and use previous release as baseline
                int startIndex;
                if (toIndex > 0)
                {
                    // Target version exists in history, start search from previous position
                    startIndex = toIndex - 1;
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, start from most recent tag
                    startIndex = tags.Count - 1;
                }
                else
                {
                    // Target is first tag, no previous release exists
                    startIndex = -1;
                }

                // Search backward for previous non-pre-release version
                for (var i = startIndex; i >= 0; i--)
                {
                    if (!tags[i].IsPreRelease)
                    {
                        fromTagInfo = tags[i];
                        break;
                    }
                }
            }

            // Get commit hash for baseline version if one was found
            if (fromTagInfo != null)
            {
                fromHash = await GetHashForTagAsync(fromTagInfo.Tag);
            }
        }

        // Collect all changes (issues and PRs) in version range
        var changes = await GetChangesBetweenTagsAsync(fromTagInfo, toTagInfo);
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

        // Collect known issues (open bugs not fixed in this build)
        var knownIssues = new List<ItemInfo>();
        var openIssues = await GetOpenIssuesAsync();
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
        List<string> prs;
        if (fromTagName == "v1.0.0" && toTagName == "ver-1.1.0")
        {
            prs = new List<string> { "10", "13" }; // Include PR without issues
        }
        else if (fromTagName == "ver-1.1.0" && (toTagName == "2.0.0" || toTagName == "v2.0.0"))
        {
            prs = new List<string> { "11", "12" };
        }
        else if (string.IsNullOrEmpty(fromTagName) && toTagName == "v1.0.0")
        {
            prs = new List<string> { "10" };
        }
        else
        {
            prs = new List<string> { "10", "11", "12", "13" };
        }

        // Build changes from PRs
        var changes = new List<ItemInfo>();
        foreach (var pr in prs)
        {
            // Get issues for this PR
            var issues = _pullRequestIssues.TryGetValue(pr, out var prIssues) ? prIssues : new List<string>();

            if (issues.Count > 0)
            {
                // PR has associated issues - add them as changes
                // Use PR number as index for chronological ordering
                foreach (var issueId in issues)
                {
                    var title = _issueTitles.TryGetValue(issueId, out var issueTitle) ? issueTitle : $"Issue {issueId}";
                    var url = $"https://github.com/example/repo/issues/{issueId}";
                    var type = _issueTypes.TryGetValue(issueId, out var issueType) ? issueType : "other";
                    changes.Add(new ItemInfo(issueId, title, url, type, int.Parse(pr)));
                }
            }
            else
            {
                // PR has no issues - treat the PR itself as a change
                changes.Add(new ItemInfo(
                    $"#{pr}",
                    $"PR #{pr}",
                    $"https://github.com/example/repo/pull/{pr}",
                    "other",
                    int.Parse(pr)));
            }
        }

        return Task.FromResult(changes);
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
        return Task.FromResult(openIssuesData);
    }
}
