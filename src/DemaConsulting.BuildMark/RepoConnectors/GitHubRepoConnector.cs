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
    ///     Validates and sanitizes a tag name to prevent command injection.
    /// </summary>
    /// <param name="tag">Tag name to validate.</param>
    /// <returns>Sanitized tag name.</returns>
    /// <exception cref="ArgumentException">Thrown if tag name is invalid.</exception>
    private static string ValidateTag(string tag)
    {
        if (!TagNameRegex().IsMatch(tag))
        {
            throw new ArgumentException($"Invalid tag name: {tag}", nameof(tag));
        }

        return tag;
    }

    /// <summary>
    ///     Validates and sanitizes an issue or PR ID to prevent command injection.
    /// </summary>
    /// <param name="id">ID to validate.</param>
    /// <param name="paramName">Parameter name for exception message.</param>
    /// <returns>Sanitized ID.</returns>
    /// <exception cref="ArgumentException">Thrown if ID is invalid.</exception>
    private static string ValidateId(string id, string paramName)
    {
        if (!NumericIdRegex().IsMatch(id))
        {
            throw new ArgumentException($"Invalid ID: {id}", paramName);
        }

        return id;
    }

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
            range = ValidateTag(toTag!);
        }
        else if (string.IsNullOrEmpty(toTag))
        {
            range = $"{ValidateTag(fromTag)}..HEAD";
        }
        else
        {
            range = $"{ValidateTag(fromTag)}..{ValidateTag(toTag)}";
        }

        var output = await RunCommandAsync("git", $"log --oneline --merges {range}");
        var pullRequests = new List<string>();
        var regex = NumberReferenceRegex();

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
        var validatedId = ValidateId(pullRequestId, nameof(pullRequestId));
        var output = await RunCommandAsync("gh", $"pr view {validatedId} --json body --jq .body");
        var issues = new List<string>();
        var regex = NumberReferenceRegex();

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
        var validatedId = ValidateId(issueId, nameof(issueId));
        return await RunCommandAsync("gh", $"issue view {validatedId} --json title --jq .title");
    }

    /// <summary>
    ///     Gets the type of an issue (bug, feature, etc.).
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue type.</returns>
    public override async Task<string> GetIssueTypeAsync(string issueId)
    {
        var validatedId = ValidateId(issueId, nameof(issueId));
        var output = await RunCommandAsync("gh", $"issue view {validatedId} --json labels --jq '.labels[].name'");
        var labels = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Look for common type labels
        foreach (var label in labels)
        {
            var lowerLabel = label.ToLowerInvariant();
            foreach (var (key, value) in LabelTypeMap)
            {
                if (lowerLabel.Contains(key))
                {
                    return value;
                }
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
        var refName = string.IsNullOrEmpty(tag) ? "HEAD" : ValidateTag(tag);
        return await RunCommandAsync("git", $"rev-parse {refName}");
    }

    /// <summary>
    ///     Gets the URL for an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue URL.</returns>
    public override async Task<string> GetIssueUrlAsync(string issueId)
    {
        var validatedId = ValidateId(issueId, nameof(issueId));
        return await RunCommandAsync("gh", $"issue view {validatedId} --json url --jq .url");
    }

    /// <summary>
    ///     Gets the list of open issue IDs.
    /// </summary>
    /// <returns>List of open issue IDs.</returns>
    public override async Task<List<string>> GetOpenIssuesAsync()
    {
        var output = await RunCommandAsync("gh", "issue list --state open --json number --jq '.[].number'");
        return output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(n => n.Trim())
            .ToList();
    }

    /// <summary>
    ///     Regular expression to match valid tag names (alphanumeric, dots, hyphens, underscores, slashes).
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9._/-]+$", RegexOptions.Compiled)]
    private static partial Regex TagNameRegex();

    /// <summary>
    ///     Regular expression to match numeric IDs.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^\d+$", RegexOptions.Compiled)]
    private static partial Regex NumericIdRegex();

    /// <summary>
    ///     Regular expression to match number references (#123).
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"#(\d+)", RegexOptions.Compiled)]
    private static partial Regex NumberReferenceRegex();
}
