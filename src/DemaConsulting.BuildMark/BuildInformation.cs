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
///     Represents build information for a release.
/// </summary>
/// <param name="FromVersion">Starting version (null if from beginning of history).</param>
/// <param name="ToVersion">Ending version.</param>
/// <param name="FromHash">Starting git hash (null if from beginning of history).</param>
/// <param name="ToHash">Ending git hash.</param>
/// <param name="Changes">Non-bug changes performed between versions.</param>
/// <param name="Bugs">Bugs fixed between versions.</param>
/// <param name="KnownIssues">Known issues (unfixed or fixed but not in this build).</param>
public record BuildInformation(
    Version? FromVersion,
    Version ToVersion,
    string? FromHash,
    string ToHash,
    List<ItemInfo> Changes,
    List<ItemInfo> Bugs,
    List<ItemInfo> KnownIssues)
{
    /// <summary>
    ///     Creates a BuildInformation record from a repository connector.
    /// </summary>
    /// <param name="connector">Repository connector to fetch information from.</param>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public static async Task<BuildInformation> CreateAsync(IRepoConnector connector, Version? version = null)
    {
        // Retrieve release history and current commit hash from the repository
        var releases = await connector.GetReleaseHistoryAsync();
        var currentHash = await connector.GetCurrentHashAsync();

        // Determine the target version and hash for build information
        Version toTagInfo;
        string toHash;
        if (version != null)
        {
            // Use explicitly specified version as target
            toTagInfo = version;
            toHash = currentHash;
        }
        else if (releases.Count > 0)
        {
            // Verify current commit matches latest release when no version specified
            // Releases are ordered newest first, so index 0 is the most recent
            var latestTag = releases[0];
            var latestTagHash = await connector.GetHashForTagAsync(latestTag.Tag);

            if (latestTagHash.Trim() == currentHash.Trim())
            {
                // Current commit matches latest release, use it as target
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
        if (releases.Count > 0)
        {
            // Find the position of target version in release history
            var toIndex = FindTagIndex(releases, toTagInfo.FullVersion);

            if (toTagInfo.IsPreRelease)
            {
                // Pre-release versions use the immediately previous tag as baseline
                // Releases are ordered newest first (index 0 = most recent)
                if (toIndex > 0)
                {
                    // Target version exists in history, use next older tag (higher index)
                    fromTagInfo = releases[toIndex + 1];
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, use most recent tag as baseline
                    fromTagInfo = releases[0];
                }
                // If toIndex == 0, this is the most recent tag, no baseline
            }
            else
            {
                // Release versions skip pre-releases and use previous release as baseline
                // Releases are ordered newest first (index 0 = most recent)
                int startIndex;
                if (toIndex >= 0)
                {
                    // Target version exists in history, start search from next older position
                    startIndex = toIndex + 1;
                }
                else if (toIndex == -1)
                {
                    // Target version not in history, start from most recent tag
                    startIndex = 0;
                }
                else
                {
                    // Should not reach here
                    startIndex = -1;
                }

                // Search forward (increasing index = older releases) for previous non-pre-release version
                for (var i = startIndex; i < releases.Count; i++)
                {
                    if (!releases[i].IsPreRelease)
                    {
                        fromTagInfo = releases[i];
                        break;
                    }
                }
            }

            // Get commit hash for baseline version if one was found
            if (fromTagInfo != null)
            {
                fromHash = await connector.GetHashForTagAsync(fromTagInfo.Tag);
            }
        }

        // Collect all changes (issues and PRs) in version range
        var changes = await connector.GetChangesBetweenTagsAsync(fromTagInfo, toTagInfo);
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
        var openIssues = await connector.GetOpenIssuesAsync();
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
    ///     Generates a Markdown build report from this build information.
    /// </summary>
    /// <param name="headingDepth">Root markdown heading depth (default 1).</param>
    /// <param name="includeKnownIssues">Flag for whether to include known issues (default false).</param>
    /// <returns>Markdown-formatted build report.</returns>
    public string ToMarkdown(int headingDepth = 1, bool includeKnownIssues = false)
    {
        // Build heading prefix based on requested depth
        var heading = new string('#', headingDepth);
        var subHeading = new string('#', headingDepth + 1);

        // Start building the markdown report
        var markdown = new System.Text.StringBuilder();

        // Add title section
        markdown.AppendLine($"{heading} Build Report");
        markdown.AppendLine();

        // Add version information section
        markdown.AppendLine($"{subHeading} Version Information");
        markdown.AppendLine();
        markdown.AppendLine("| Field | Value |");
        markdown.AppendLine("|-------|-------|");
        markdown.AppendLine($"| **Version** | {ToVersion.Tag} |");
        markdown.AppendLine($"| **Commit Hash** | {ToHash} |");
        if (FromVersion != null)
        {
            markdown.AppendLine($"| **Previous Version** | {FromVersion.Tag} |");
            markdown.AppendLine($"| **Previous Commit Hash** | {FromHash} |");
        }
        else
        {
            markdown.AppendLine("| **Previous Version** | N/A |");
            markdown.AppendLine("| **Previous Commit Hash** | N/A |");
        }
        markdown.AppendLine();

        // Add changes section
        markdown.AppendLine($"{subHeading} Changes");
        markdown.AppendLine();
        markdown.AppendLine("| Issue | Title |");
        markdown.AppendLine("|-------|-------|");
        if (Changes.Count > 0)
        {
            foreach (var issue in Changes)
            {
                markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
            }
        }
        else
        {
            markdown.AppendLine("| N/A | N/A |");
        }
        markdown.AppendLine();

        // Add bugs fixed section
        markdown.AppendLine($"{subHeading} Bugs Fixed");
        markdown.AppendLine();
        markdown.AppendLine("| Issue | Title |");
        markdown.AppendLine("|-------|-------|");
        if (Bugs.Count > 0)
        {
            foreach (var issue in Bugs)
            {
                markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
            }
        }
        else
        {
            markdown.AppendLine("| N/A | N/A |");
        }
        markdown.AppendLine();

        // Add known issues section if requested
        if (includeKnownIssues)
        {
            markdown.AppendLine($"{subHeading} Known Issues");
            markdown.AppendLine();
            markdown.AppendLine("| Issue | Title |");
            markdown.AppendLine("|-------|-------|");
            if (KnownIssues.Count > 0)
            {
                foreach (var issue in KnownIssues)
                {
                    markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
                }
            }
            else
            {
                markdown.AppendLine("| N/A | N/A |");
            }
            markdown.AppendLine();
        }

        // Return the complete markdown report
        return markdown.ToString();
    }

    /// <summary>
    ///     Finds the index of a tag in the tag history by normalized version.
    /// </summary>
    /// <param name="tags">List of tags.</param>
    /// <param name="normalizedVersion">Normalized version to find.</param>
    /// <returns>Index of the tag, or -1 if not found.</returns>
    private static int FindTagIndex(List<Version> tags, string normalizedVersion)
    {
        // Search for tag matching the normalized version
        for (var i = 0; i < tags.Count; i++)
        {
            if (tags[i].FullVersion.Equals(normalizedVersion, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        // Tag not found in history
        return -1;
    }
}
