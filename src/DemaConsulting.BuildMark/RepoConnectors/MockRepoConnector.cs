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
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    public Task<List<Version>> GetTagHistoryAsync()
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
    public Task<List<ChangeData>> GetChangesBetweenTagsAsync(Version? from, Version? to)
    {
        // Get the PRs in the range
        var prs = GetPullRequestsBetweenTagsAsync(from, to).Result;
        var changes = new List<ChangeData>();

        foreach (var pr in prs)
        {
            // Get issues for this PR
            var issues = GetIssuesForPullRequestAsync(pr).Result;

            if (issues.Count > 0)
            {
                // PR has associated issues - add them as changes
                foreach (var issueId in issues)
                {
                    var title = GetIssueTitleAsync(issueId).Result;
                    var url = GetIssueUrlAsync(issueId).Result;
                    var type = GetIssueTypeAsync(issueId).Result;
                    changes.Add(new ChangeData(issueId, title, url, type));
                }
            }
            else
            {
                // PR has no issues - treat the PR itself as a change
                changes.Add(new ChangeData(
                    $"#{pr}",
                    $"PR #{pr}",
                    $"https://github.com/example/repo/pull/{pr}",
                    "other"));
            }
        }

        return Task.FromResult(changes);
    }

    /// <summary>
    ///     Gets the list of pull request IDs between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of pull request IDs.</returns>
    public Task<List<string>> GetPullRequestsBetweenTagsAsync(Version? from, Version? to)
    {
        // Extract tag names from version objects
        var fromTagName = from?.Tag;
        var toTagName = to?.Tag;

        // Return pull requests based on specific tag ranges
        if (fromTagName == "v1.0.0" && toTagName == "ver-1.1.0")
        {
            return Task.FromResult(new List<string> { "10", "13" }); // Include PR without issues
        }

        if (fromTagName == "ver-1.1.0" && (toTagName == "2.0.0" || toTagName == "v2.0.0"))
        {
            return Task.FromResult(new List<string> { "11", "12" });
        }

        if (string.IsNullOrEmpty(fromTagName) && toTagName == "v1.0.0")
        {
            return Task.FromResult(new List<string> { "10" });
        }

        // Default case returns all pull requests
        return Task.FromResult(new List<string> { "10", "11", "12", "13" });
    }

    /// <summary>
    ///     Gets the issue IDs associated with a pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>List of issue IDs.</returns>
    public Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId)
    {
        // Return issues for known pull requests
        return Task.FromResult(
            _pullRequestIssues.TryGetValue(pullRequestId, out var issues)
                ? issues
                : new List<string>());
    }

    /// <summary>
    ///     Gets the title of an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue title.</returns>
    public Task<string> GetIssueTitleAsync(string issueId)
    {
        // Return title for known issues or default value
        return Task.FromResult(
            _issueTitles.TryGetValue(issueId, out var title)
                ? title
                : $"Issue {issueId}");
    }

    /// <summary>
    ///     Gets the type of an issue (bug, feature, etc.).
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue type.</returns>
    public Task<string> GetIssueTypeAsync(string issueId)
    {
        // Return type for known issues or default to "other"
        return Task.FromResult(
            _issueTypes.TryGetValue(issueId, out var type)
                ? type
                : "other");
    }

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name (null for current state).</param>
    /// <returns>Git hash.</returns>
    public Task<string> GetHashForTagAsync(string? tag)
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
    ///     Gets the URL for an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue URL.</returns>
    public Task<string> GetIssueUrlAsync(string issueId)
    {
        // Generate mock URL for issue
        return Task.FromResult($"https://github.com/example/repo/issues/{issueId}");
    }

    /// <summary>
    ///     Gets the list of open issue IDs.
    /// </summary>
    /// <returns>List of open issue IDs.</returns>
    public Task<List<string>> GetOpenIssuesAsync()
    {
        // Return predefined list of open issues
        return Task.FromResult(_openIssues);
    }
}
