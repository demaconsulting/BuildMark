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
        // Ensure tag name matches allowed pattern to prevent injection attacks
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
        // Ensure ID is numeric to prevent injection attacks
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
    ///     Gets the list of pull request IDs between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of pull request IDs.</returns>
    public override async Task<List<string>> GetPullRequestsBetweenTagsAsync(Version? from, Version? to)
    {
        // Build git log range based on provided versions
        string range;
        if (from == null && to == null)
        {
            // No versions specified, use all of HEAD
            range = "HEAD";
        }
        else if (from == null)
        {
            // Only end version specified (to is not null here)
            range = ValidateTag(to!.Tag);
        }
        else if (to == null)
        {
            // Only start version specified, range to HEAD (from is not null here)
            range = $"{ValidateTag(from.Tag)}..HEAD";
        }
        else
        {
            // Both versions specified (both from and to are not null here)
            range = $"{ValidateTag(from.Tag)}..{ValidateTag(to.Tag)}";
        }

        // Get merge commits in range using git log
        // Arguments: --oneline (one line per commit), --merges (only merge commits)
        // Output format: "<short-hash> Merge pull request #<number> from <branch>"
        var output = await RunCommandAsync("git", $"log --oneline --merges {range}");

        // Extract pull request numbers from merge commit messages
        // Each line is parsed for "#<number>" pattern to identify the PR
        var regex = NumberReferenceRegex();

        var pullRequests = output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => regex.Match(line))
            .Where(match => match.Success)
            .Select(match => match.Groups[1].Value)
            .ToList();

        return pullRequests;
    }

    /// <summary>
    ///     Gets the issue IDs associated with a pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>List of issue IDs.</returns>
    public override async Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId)
    {
        // Validate and fetch PR body using GitHub CLI
        // Arguments: --json body (get body field), --jq .body (extract body value)
        // Output: raw PR description text which may contain issue references
        var validatedId = ValidateId(pullRequestId, nameof(pullRequestId));
        var output = await RunCommandAsync("gh", $"pr view {validatedId} --json body --jq .body");

        // Extract issue references (e.g., #123, #456) from PR body text
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
        // Validate and fetch issue title using GitHub CLI
        // Arguments: --json title (get title field), --jq .title (extract title value)
        // Output: issue title as plain text
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
        // Validate and fetch issue labels using GitHub CLI
        // Arguments: --json labels (get labels array), --jq '.labels[].name' (extract label names)
        // Output: one label name per line
        var validatedId = ValidateId(issueId, nameof(issueId));
        var output = await RunCommandAsync("gh", $"issue view {validatedId} --json labels --jq '.labels[].name'");
        var labels = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        // Map labels to standardized issue types
        var matchingType = labels
            .Select(label => label.ToLowerInvariant())
            .SelectMany(lowerLabel => LabelTypeMap
                .Where(kvp => lowerLabel.Contains(kvp.Key))
                .Select(kvp => kvp.Value))
            .FirstOrDefault();

        // Return matching type or default when no recognized label found
        return matchingType ?? "other";
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
    ///     Gets the URL for an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue URL.</returns>
    public override async Task<string> GetIssueUrlAsync(string issueId)
    {
        // Validate and fetch issue URL using GitHub CLI
        // Arguments: --json url (get url field), --jq .url (extract url value)
        // Output: full HTTPS URL to the issue
        var validatedId = ValidateId(issueId, nameof(issueId));
        return await RunCommandAsync("gh", $"issue view {validatedId} --json url --jq .url");
    }

    /// <summary>
    ///     Gets the list of open issue IDs.
    /// </summary>
    /// <returns>List of open issue IDs.</returns>
    public override async Task<List<string>> GetOpenIssuesAsync()
    {
        // Fetch all open issue numbers using GitHub CLI
        // Arguments: --state open (open issues only), --json number (get number field), --jq '.[].number' (extract numbers from array)
        // Output: one issue number per line
        var output = await RunCommandAsync("gh", "issue list --state open --json number --jq '.[].number'");

        // Parse output into list of issue IDs
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
