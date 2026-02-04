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

using System.Text.Json;
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
        // Get commits using GitHub API
        string commitHashesOutput;

        if (from == null && to == null)
        {
            // No versions specified, get all commits using paginated API
            var output = await RunCommandAsync("gh", "api repos/:owner/:repo/commits --paginate");
            commitHashesOutput = ExtractShasFromCommitsJson(output);
        }
        else if (from == null)
        {
            // Only end version specified - get commits up to 'to' tag/HEAD
            var toExists = to != null && await TagExistsAsync(to.Tag);
            var toRef = toExists ? ValidateTag(to!.Tag) : "HEAD";
            var output = await RunCommandAsync("gh", $"api repos/:owner/:repo/commits?sha={toRef} --paginate");
            commitHashesOutput = ExtractShasFromCommitsJson(output);
        }
        else if (to == null)
        {
            // Only start version specified - compare from tag to HEAD
            var fromTag = ValidateTag(from.Tag);
            var output = await RunCommandAsync("gh", $"api repos/:owner/:repo/compare/{fromTag}...HEAD");
            commitHashesOutput = ExtractShasFromCompareJson(output);
        }
        else
        {
            // Both versions specified - compare from tag to to tag/HEAD
            var fromTag = ValidateTag(from.Tag);
            var toExists = await TagExistsAsync(to.Tag);
            var toRef = toExists ? ValidateTag(to.Tag) : "HEAD";
            var output = await RunCommandAsync("gh", $"api repos/:owner/:repo/compare/{fromTag}...{toRef}");
            commitHashesOutput = ExtractShasFromCompareJson(output);
        }

        // Batch fetch PR information with all details in one call
        string prDataOutput;
        try
        {
            // Fetch PRs with all required fields: number, title, url, labels, and closing issues with their details
            // Note: Not using --jq to avoid shell quoting issues on Windows
            prDataOutput = await RunCommandAsync(
                "gh",
                "pr list --state all --json number,title,url,labels,closingIssuesReferences",
                commitHashesOutput);
        }
        catch (InvalidOperationException)
        {
            // Fallback to empty result if batch query fails
            return new List<ItemInfo>();
        }

        // Parse the JSON array output
        List<JsonElement> prArray;
        try
        {
            var jsonDoc = JsonDocument.Parse(prDataOutput);
            prArray = jsonDoc.RootElement.EnumerateArray().ToList();
        }
        catch (JsonException)
        {
            // Fallback to empty result if JSON parsing fails
            return new List<ItemInfo>();
        }

        // Parse PR data and extract changes
        var changes = new List<ItemInfo>();
        var issueNumbers = new HashSet<string>();
        var prData = new List<(string number, string title, string url, List<string> issueNumbers)>();

        // First pass: collect issue numbers and PR data
        foreach (var prElement in prArray)
        {
            try
            {
                var prNumber = prElement.GetProperty("number").GetInt32().ToString();
                var prTitle = prElement.GetProperty("title").GetString() ?? $"PR #{prNumber}";
                var prUrl = prElement.GetProperty("url").GetString() ?? string.Empty;

                var prIssues = new List<string>();
                if (prElement.TryGetProperty("closingIssuesReferences", out var issuesElement) && issuesElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var issue in issuesElement.EnumerateArray())
                    {
                        if (issue.TryGetProperty("number", out var issueNumberElement))
                        {
                            var issueNumber = issueNumberElement.GetInt32().ToString();
                            issueNumbers.Add(issueNumber);
                            prIssues.Add(issueNumber);
                        }
                    }
                }

                prData.Add((prNumber, prTitle, prUrl, prIssues));
            }
            catch (JsonException)
            {
                // Skip malformed JSON
            }
        }

        // Second pass: batch fetch all issue details using issue list
        var issueDetailsMap = new Dictionary<string, ItemInfo>();
        if (issueNumbers.Count > 0)
        {
            try
            {
                var issuesOutput = await RunCommandAsync("gh", "issue list --state all --limit 1000 --json number,title,url,labels");
                var issuesDoc = JsonDocument.Parse(issuesOutput);

                foreach (var issueElement in issuesDoc.RootElement.EnumerateArray())
                {
                    var issueNumber = issueElement.GetProperty("number").GetInt32().ToString();

                    // Only process issues that are referenced by PRs
                    if (!issueNumbers.Contains(issueNumber))
                    {
                        continue;
                    }

                    var issueTitle = issueElement.GetProperty("title").GetString() ?? $"Issue #{issueNumber}";
                    var issueUrl = issueElement.GetProperty("url").GetString() ?? string.Empty;

                    // Determine type from labels
                    var issueType = "other";
                    if (issueElement.TryGetProperty("labels", out var labelsElement) && labelsElement.ValueKind == JsonValueKind.Array)
                    {
                        var labels = new List<string>();
                        foreach (var label in labelsElement.EnumerateArray())
                        {
                            if (label.TryGetProperty("name", out var labelName))
                            {
                                labels.Add(labelName.GetString() ?? string.Empty);
                            }
                        }

                        // Map labels to type
                        var matchingType = labels
                            .Select(label => label.ToLowerInvariant())
                            .SelectMany(lowerLabel => LabelTypeMap
                                .Where(kvp => lowerLabel.Contains(kvp.Key))
                                .Select(kvp => kvp.Value))
                            .FirstOrDefault();

                        if (matchingType != null)
                        {
                            issueType = matchingType;
                        }
                    }

                    issueDetailsMap[issueNumber] = new ItemInfo(issueNumber, issueTitle, issueUrl, issueType);
                }
            }
            catch (Exception)
            {
                // If we can't fetch issue list, create fallback entries
                foreach (var issueNumber in issueNumbers)
                {
                    issueDetailsMap[issueNumber] = new ItemInfo(issueNumber, $"Issue #{issueNumber}", string.Empty, "other");
                }
            }
        }

        // Third pass: add issues to changes list (avoiding duplicates)
        var addedIssues = new HashSet<string>();
        foreach (var (issueNumber, itemInfo) in issueDetailsMap)
        {
            if (!addedIssues.Contains(issueNumber))
            {
                changes.Add(itemInfo);
                addedIssues.Add(issueNumber);
            }
        }

        // Fourth pass: add PRs without issues
        foreach (var (prNumber, prTitle, prUrl, prIssues) in prData)
        {
            if (prIssues.Count == 0)
            {
                // PR has no issues - need to get labels from the pr list data
                // Find the PR element again to get its labels
                var prType = "other";
                var prElement = prArray.FirstOrDefault(pe =>
                    pe.TryGetProperty("number", out var numProp) &&
                    numProp.GetInt32().ToString() == prNumber);

                if (prElement.ValueKind != JsonValueKind.Undefined &&
                    prElement.TryGetProperty("labels", out var labelsElement) &&
                    labelsElement.ValueKind == JsonValueKind.Array)
                {
                    var labels = new List<string>();
                    foreach (var label in labelsElement.EnumerateArray())
                    {
                        if (label.TryGetProperty("name", out var labelName))
                        {
                            labels.Add(labelName.GetString() ?? string.Empty);
                        }
                    }

                    // Map labels to type
                    var matchingType = labels
                        .Select(label => label.ToLowerInvariant())
                        .SelectMany(lowerLabel => LabelTypeMap
                            .Where(kvp => lowerLabel.Contains(kvp.Key))
                            .Select(kvp => kvp.Value))
                        .FirstOrDefault();

                    if (matchingType != null)
                    {
                        prType = matchingType;
                    }
                }

                changes.Add(new ItemInfo($"#{prNumber}", prTitle, prUrl, prType));
            }
        }

        return changes;
    }

    /// <summary>
    ///     Checks if a git tag exists in the repository.
    /// </summary>
    /// <param name="tag">Tag name to check.</param>
    /// <returns>True if the tag exists, false otherwise.</returns>
    private async Task<bool> TagExistsAsync(string tag)
    {
        try
        {
            // Try to resolve the tag to a commit hash
            // If tag doesn't exist, RunCommandAsync will throw InvalidOperationException
            await RunCommandAsync("git", $"rev-parse --verify {ValidateTag(tag)}");
            return true;
        }
        catch (InvalidOperationException)
        {
            // Tag doesn't exist
            return false;
        }
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
        // Arguments: --state open (open issues only), --json to get all required fields
        // Note: Not using --jq to avoid shell quoting issues on Windows
        var output = await RunCommandAsync("gh", "issue list --state open --json number,title,url,labels");

        // Parse the JSON array output
        List<JsonElement> issueArray;
        try
        {
            var jsonDoc = JsonDocument.Parse(output);
            issueArray = jsonDoc.RootElement.EnumerateArray().ToList();
        }
        catch (JsonException)
        {
            // Return empty list if JSON parsing fails
            return new List<ItemInfo>();
        }

        var openIssues = new List<ItemInfo>();

        foreach (var issueElement in issueArray)
        {
            try
            {
                var issueNumber = issueElement.GetProperty("number").GetInt32().ToString();
                var issueTitle = issueElement.GetProperty("title").GetString() ?? $"Issue #{issueNumber}";
                var issueUrl = issueElement.GetProperty("url").GetString() ?? string.Empty;

                // Determine type from labels
                var issueType = "other";
                if (issueElement.TryGetProperty("labels", out var labelsElement) && labelsElement.ValueKind == JsonValueKind.Array)
                {
                    var labels = new List<string>();
                    foreach (var label in labelsElement.EnumerateArray())
                    {
                        if (label.TryGetProperty("name", out var labelName))
                        {
                            labels.Add(labelName.GetString() ?? string.Empty);
                        }
                    }

                    // Map labels to type
                    var matchingType = labels
                        .Select(label => label.ToLowerInvariant())
                        .SelectMany(lowerLabel => LabelTypeMap
                            .Where(kvp => lowerLabel.Contains(kvp.Key))
                            .Select(kvp => kvp.Value))
                        .FirstOrDefault();

                    if (matchingType != null)
                    {
                        issueType = matchingType;
                    }
                }

                openIssues.Add(new ItemInfo(issueNumber, issueTitle, issueUrl, issueType));
            }
            catch (System.Text.Json.JsonException)
            {
                // Skip malformed JSON
            }
        }

        return openIssues;
    }

    /// <summary>
    ///     Extracts SHA values from a commits API JSON response.
    /// </summary>
    /// <param name="json">JSON response from commits API.</param>
    /// <returns>Newline-separated SHA values.</returns>
    private static string ExtractShasFromCommitsJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var shas = new List<string>();

            foreach (var commit in doc.RootElement.EnumerateArray())
            {
                if (commit.TryGetProperty("sha", out var shaElement))
                {
                    var sha = shaElement.GetString();
                    if (!string.IsNullOrEmpty(sha))
                    {
                        shas.Add(sha);
                    }
                }
            }

            return string.Join('\n', shas);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     Extracts SHA values from a compare API JSON response.
    /// </summary>
    /// <param name="json">JSON response from compare API.</param>
    /// <returns>Newline-separated SHA values.</returns>
    private static string ExtractShasFromCompareJson(string json)
    {
        try
        {
            var doc = JsonDocument.Parse(json);
            var shas = new List<string>();

            if (doc.RootElement.TryGetProperty("commits", out var commitsElement))
            {
                foreach (var commit in commitsElement.EnumerateArray())
                {
                    if (commit.TryGetProperty("sha", out var shaElement))
                    {
                        var sha = shaElement.GetString();
                        if (!string.IsNullOrEmpty(sha))
                        {
                            shas.Add(sha);
                        }
                    }
                }
            }

            return string.Join('\n', shas);
        }
        catch (JsonException)
        {
            return string.Empty;
        }
    }

    /// <summary>
    ///     Regular expression to match valid tag names (alphanumeric, dots, hyphens, underscores, slashes).
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^[a-zA-Z0-9._/-]+$", RegexOptions.Compiled)]
    private static partial Regex TagNameRegex();
}
