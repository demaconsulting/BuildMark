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

namespace DemaConsulting.BuildMark;

/// <summary>
///     GitHub repository connector implementation.
/// </summary>
public partial class GitHubRepoConnector : RepoConnectorBase
{
    /// <summary>
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    public override async Task<List<string>> GetTagHistoryAsync()
    {
        var output = await RunCommandAsync("git", "tag --sort=creatordate --merged HEAD");
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToList();
    }

    /// <summary>
    ///     Gets the list of pull request IDs between two tags.
    /// </summary>
    /// <param name="fromTag">Starting tag (null for start of history).</param>
    /// <param name="toTag">Ending tag (null for current state).</param>
    /// <returns>List of pull request IDs.</returns>
    public override async Task<List<string>> GetPullRequestsBetweenTagsAsync(string? fromTag, string? toTag)
    {
        string range;
        if (string.IsNullOrEmpty(fromTag) && string.IsNullOrEmpty(toTag))
        {
            range = "HEAD";
        }
        else if (string.IsNullOrEmpty(fromTag))
        {
            range = toTag!;
        }
        else if (string.IsNullOrEmpty(toTag))
        {
            range = $"{fromTag}..HEAD";
        }
        else
        {
            range = $"{fromTag}..{toTag}";
        }

        var output = await RunCommandAsync("git", $"log --oneline --merges {range}");
        var pullRequests = new List<string>();
        var regex = PullRequestRegex();

        foreach (var line in output.Split('\n', StringSplitOptions.RemoveEmptyEntries))
        {
            var match = regex.Match(line);
            if (match.Success)
            {
                pullRequests.Add(match.Groups[1].Value);
            }
        }

        return pullRequests;
    }

    /// <summary>
    ///     Gets the issue IDs associated with a pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>List of issue IDs.</returns>
    public override async Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId)
    {
        var output = await RunCommandAsync("gh", $"pr view {pullRequestId} --json body --jq .body");
        var issues = new List<string>();
        var regex = IssueReferenceRegex();

        foreach (Match match in regex.Matches(output))
        {
            issues.Add(match.Groups[1].Value);
        }

        return issues;
    }

    /// <summary>
    ///     Gets the title of an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue title.</returns>
    public override async Task<string> GetIssueTitleAsync(string issueId)
    {
        return await RunCommandAsync("gh", $"issue view {issueId} --json title --jq .title");
    }

    /// <summary>
    ///     Gets the type of an issue (bug, feature, etc.).
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue type.</returns>
    public override async Task<string> GetIssueTypeAsync(string issueId)
    {
        var output = await RunCommandAsync("gh", $"issue view {issueId} --json labels --jq '.labels[].name'");
        var labels = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Look for common type labels
        foreach (var label in labels)
        {
            var lowerLabel = label.ToLowerInvariant();
            if (lowerLabel.Contains("bug") || lowerLabel.Contains("defect"))
            {
                return "bug";
            }

            if (lowerLabel.Contains("feature") || lowerLabel.Contains("enhancement"))
            {
                return "feature";
            }

            if (lowerLabel.Contains("documentation"))
            {
                return "documentation";
            }

            if (lowerLabel.Contains("performance"))
            {
                return "performance";
            }

            if (lowerLabel.Contains("security"))
            {
                return "security";
            }
        }

        return "other";
    }

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name (null for current state).</param>
    /// <returns>Git hash.</returns>
    public override async Task<string> GetHashForTagAsync(string? tag)
    {
        var refName = string.IsNullOrEmpty(tag) ? "HEAD" : tag;
        return await RunCommandAsync("git", $"rev-parse {refName}");
    }

    /// <summary>
    ///     Regular expression to match pull request numbers in merge commit messages.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"#(\d+)", RegexOptions.Compiled)]
    private static partial Regex PullRequestRegex();

    /// <summary>
    ///     Regular expression to match issue references in text.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"#(\d+)", RegexOptions.Compiled)]
    private static partial Regex IssueReferenceRegex();
}
