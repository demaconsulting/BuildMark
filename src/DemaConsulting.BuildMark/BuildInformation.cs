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
///     Represents information about an issue.
/// </summary>
/// <param name="Id">Issue ID.</param>
/// <param name="Title">Issue title.</param>
/// <param name="Url">Issue URL.</param>
public record IssueInfo(string Id, string Title, string Url);

/// <summary>
///     Represents build information for a release.
/// </summary>
/// <param name="FromVersion">Starting version (null if from beginning of history).</param>
/// <param name="ToVersion">Ending version.</param>
/// <param name="FromHash">Starting git hash (null if from beginning of history).</param>
/// <param name="ToHash">Ending git hash.</param>
/// <param name="ChangeIssues">Non-bug changes performed between versions.</param>
/// <param name="BugIssues">Bugs fixed between versions.</param>
/// <param name="KnownIssues">Known issues (unfixed or fixed but not in this build).</param>
public record BuildInformation(
    string? FromVersion,
    string ToVersion,
    string? FromHash,
    string ToHash,
    List<IssueInfo> ChangeIssues,
    List<IssueInfo> BugIssues,
    List<IssueInfo> KnownIssues)
{
    /// <summary>
    ///     Creates a BuildInformation record from a repository connector.
    /// </summary>
    /// <param name="connector">Repository connector to fetch information from.</param>
    /// <param name="version">Optional target version. If not provided, uses the most recent tag if it matches current commit.</param>
    /// <returns>BuildInformation record with all collected data.</returns>
    /// <exception cref="InvalidOperationException">Thrown if version cannot be determined.</exception>
    public static async Task<BuildInformation> CreateAsync(IRepoConnector connector, string? version = null)
    {
        // Get tag history and current hash
        var tags = await connector.GetTagHistoryAsync();
        var currentHash = await connector.GetHashForTagAsync(null);

        // Determine the "To" version
        string toVersion;
        string toHash;

        if (!string.IsNullOrEmpty(version))
        {
            // Use the provided version
            toVersion = version;
            toHash = currentHash;
        }
        else if (tags.Count > 0)
        {
            // Check if current commit matches the most recent tag
            var latestTag = tags[^1];
            var latestTagHash = await connector.GetHashForTagAsync(latestTag);

            if (latestTagHash.Trim() == currentHash.Trim())
            {
                toVersion = latestTag;
                toHash = currentHash;
            }
            else
            {
                throw new InvalidOperationException(
                    "Target version not specified and current commit does not match any tag. " +
                    "Please provide a version parameter.");
            }
        }
        else
        {
            throw new InvalidOperationException(
                "No tags found in repository and no version specified. " +
                "Please provide a version parameter.");
        }

        // Determine if the "To" version is a pre-release
        var isPreRelease = IsPreRelease(toVersion);

        // Determine the "From" version
        string? fromVersion = null;
        string? fromHash = null;

        if (tags.Count > 0)
        {
            var normalizedToVersion = NormalizeVersion(toVersion);
            var toIndex = FindTagIndex(tags, normalizedToVersion);

            if (isPreRelease)
            {
                // For pre-release: use the previous tag (any type)
                if (toIndex > 0)
                {
                    // The to version exists in tag history, use the previous tag
                    fromVersion = tags[toIndex - 1];
                }
                else if (toIndex == -1)
                {
                    // The to version doesn't exist in tag history, use the most recent tag
                    fromVersion = tags[^1];
                }
                // If toIndex == 0, fromVersion stays null (first release)
            }
            else
            {
                // For release: use the previous release tag (skip pre-releases)
                int startIndex;
                if (toIndex > 0)
                {
                    // The to version exists in tag history
                    startIndex = toIndex - 1;
                }
                else if (toIndex == -1)
                {
                    // The to version doesn't exist in tag history, use the most recent tag
                    startIndex = tags.Count - 1;
                }
                else
                {
                    // toIndex == 0, this is the first tag, no previous release
                    startIndex = -1;
                }
                
                for (var i = startIndex; i >= 0; i--)
                {
                    if (!IsPreRelease(tags[i]))
                    {
                        fromVersion = tags[i];
                        break;
                    }
                }
            }

            if (fromVersion != null)
            {
                fromHash = await connector.GetHashForTagAsync(fromVersion);
            }
        }

        // Get pull requests and issues between versions
        var pullRequests = await connector.GetPullRequestsBetweenTagsAsync(fromVersion, toVersion);

        var allIssues = new HashSet<string>();
        var bugIssues = new List<IssueInfo>();
        var changeIssues = new List<IssueInfo>();

        foreach (var pr in pullRequests)
        {
            var issueIds = await connector.GetIssuesForPullRequestAsync(pr);
            foreach (var issueId in issueIds)
            {
                if (allIssues.Contains(issueId))
                {
                    continue;
                }

                allIssues.Add(issueId);

                var title = await connector.GetIssueTitleAsync(issueId);
                var url = await connector.GetIssueUrlAsync(issueId);
                var type = await connector.GetIssueTypeAsync(issueId);

                var issueInfo = new IssueInfo(issueId, title, url);

                if (type == "bug")
                {
                    bugIssues.Add(issueInfo);
                }
                else
                {
                    changeIssues.Add(issueInfo);
                }
            }
        }

        // For known issues, we would need to query open bugs, but that's not in the current interface
        // For now, return an empty list
        var knownIssues = new List<IssueInfo>();

        return new BuildInformation(
            fromVersion,
            toVersion,
            fromHash?.Trim(),
            toHash.Trim(),
            changeIssues,
            bugIssues,
            knownIssues);
    }

    /// <summary>
    ///     Determines if a version string represents a pre-release.
    /// </summary>
    /// <param name="version">Version string to check.</param>
    /// <returns>True if the version is a pre-release, false otherwise.</returns>
    private static bool IsPreRelease(string version)
    {
        var normalized = version.ToLowerInvariant();
        return normalized.Contains("alpha") ||
               normalized.Contains("beta") ||
               normalized.Contains("rc") ||
               normalized.Contains("-pre");
    }

    /// <summary>
    ///     Normalizes a version string by removing leading non-numeric characters (alphabetic, dashes, underscores).
    /// </summary>
    /// <param name="version">Version string to normalize.</param>
    /// <returns>Normalized version string starting with the semantic version.</returns>
    private static string NormalizeVersion(string version)
    {
        // Remove any leading alphabetic characters, dashes, and underscores
        // This supports various tag naming conventions like "v1.0.0", "ver-1.0.0", "release_1.0.0", etc.
        var startIndex = 0;
        while (startIndex < version.Length)
        {
            var c = version[startIndex];
            if (char.IsDigit(c))
            {
                break;
            }

            if (c != '-' && c != '_' && !char.IsLetter(c))
            {
                break;
            }

            startIndex++;
        }

        return startIndex < version.Length ? version.Substring(startIndex) : version;
    }

    /// <summary>
    ///     Finds the index of a tag in the tag history by normalized version.
    /// </summary>
    /// <param name="tags">List of tags.</param>
    /// <param name="normalizedVersion">Normalized version to find.</param>
    /// <returns>Index of the tag, or -1 if not found.</returns>
    private static int FindTagIndex(List<string> tags, string normalizedVersion)
    {
        for (var i = 0; i < tags.Count; i++)
        {
            if (NormalizeVersion(tags[i]).Equals(normalizedVersion, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
