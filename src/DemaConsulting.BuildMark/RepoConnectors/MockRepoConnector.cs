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
        { "3", "Update documentation" }
    };

    private readonly Dictionary<string, string> _issueTypes = new()
    {
        { "1", "feature" },
        { "2", "bug" },
        { "3", "documentation" }
    };

    private readonly Dictionary<string, List<string>> _pullRequestIssues = new()
    {
        { "10", new List<string> { "1" } },
        { "11", new List<string> { "2" } },
        { "12", new List<string> { "3" } }
    };

    private readonly Dictionary<string, string> _tagHashes = new()
    {
        { "v1.0.0", "abc123def456" },
        { "v1.1.0", "def456ghi789" },
        { "v2.0.0-beta.1", "ghi789jkl012" },
        { "v2.0.0-rc.1", "jkl012mno345" },
        { "v2.0.0", "mno345pqr678" }
    };

    /// <summary>
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    public Task<List<string>> GetTagHistoryAsync()
    {
        return Task.FromResult(new List<string> { "v1.0.0", "v1.1.0", "v2.0.0-beta.1", "v2.0.0-rc.1", "v2.0.0" });
    }

    /// <summary>
    ///     Gets the list of pull request IDs between two tags.
    /// </summary>
    /// <param name="fromTag">Starting tag (null for start of history).</param>
    /// <param name="toTag">Ending tag (null for current state).</param>
    /// <returns>List of pull request IDs.</returns>
    public Task<List<string>> GetPullRequestsBetweenTagsAsync(string? fromTag, string? toTag)
    {
        // Deterministic mock data based on tag range
        if (fromTag == "v1.0.0" && toTag == "v1.1.0")
        {
            return Task.FromResult(new List<string> { "10" });
        }

        if (fromTag == "v1.1.0" && toTag == "v2.0.0")
        {
            return Task.FromResult(new List<string> { "11", "12" });
        }

        if (string.IsNullOrEmpty(fromTag) && toTag == "v1.0.0")
        {
            return Task.FromResult(new List<string> { "10" });
        }

        return Task.FromResult(new List<string> { "10", "11", "12" });
    }

    /// <summary>
    ///     Gets the issue IDs associated with a pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>List of issue IDs.</returns>
    public Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId)
    {
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
        if (string.IsNullOrEmpty(tag))
        {
            return Task.FromResult("current123hash456");
        }

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
        return Task.FromResult($"https://github.com/example/repo/issues/{issueId}");
    }
}
