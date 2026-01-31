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
    Version? FromVersion,
    Version ToVersion,
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
    public static async Task<BuildInformation> CreateAsync(IRepoConnector connector, Version? version = null)
    {
        // Get tag history and current hash
        var tags = await connector.GetTagHistoryAsync();
        var currentHash = await connector.GetHashForTagAsync(null);

        // Determine the "To" version
        Version toTagInfo;
        string toHash;

        if (version != null)
        {
            // Use the provided version
            toTagInfo = version;
            toHash = currentHash;
        }
        else if (tags.Count > 0)
        {
            // Check if current commit matches the most recent tag
            var latestTag = tags[^1];
            var latestTagHash = await connector.GetHashForTagAsync(latestTag.Tag);

            if (latestTagHash.Trim() == currentHash.Trim())
            {
                toTagInfo = latestTag;
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

        // Determine the "From" version
        Version? fromTagInfo = null;
        string? fromHash = null;

        if (tags.Count > 0)
        {
            var toIndex = FindTagIndex(tags, toTagInfo.FullVersion);

            if (toTagInfo.IsPreRelease)
            {
                // For pre-release: use the previous tag (any type)
                if (toIndex > 0)
                {
                    // The to version exists in tag history, use the previous tag
                    fromTagInfo = tags[toIndex - 1];
                }
                else if (toIndex == -1)
                {
                    // The to version doesn't exist in tag history, use the most recent tag
                    fromTagInfo = tags[^1];
                }
                // If toIndex == 0, fromTagInfo stays null (first release)
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
                    if (!tags[i].IsPreRelease)
                    {
                        fromTagInfo = tags[i];
                        break;
                    }
                }
            }

            if (fromTagInfo != null)
            {
                fromHash = await connector.GetHashForTagAsync(fromTagInfo.Tag);
            }
        }

        // Get pull requests and issues between versions
        var pullRequests = await connector.GetPullRequestsBetweenTagsAsync(fromTagInfo, toTagInfo);

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

        // Get known issues (open bugs that are not already fixed in this build)
        var knownIssues = new List<IssueInfo>();
        var openIssueIds = await connector.GetOpenIssuesAsync();

        foreach (var issueId in openIssueIds)
        {
            // Skip if already included in fixed bugs
            if (allIssues.Contains(issueId))
            {
                continue;
            }

            var type = await connector.GetIssueTypeAsync(issueId);
            if (type == "bug")
            {
                var title = await connector.GetIssueTitleAsync(issueId);
                var url = await connector.GetIssueUrlAsync(issueId);
                knownIssues.Add(new IssueInfo(issueId, title, url));
            }
        }

        return new BuildInformation(
            fromTagInfo,
            toTagInfo,
            fromHash?.Trim(),
            toHash.Trim(),
            changeIssues,
            bugIssues,
            knownIssues);
    }

    /// <summary>
    ///     Finds the index of a tag in the tag history by normalized version.
    /// </summary>
    /// <param name="tags">List of tags.</param>
    /// <param name="normalizedVersion">Normalized version to find.</param>
    /// <returns>Index of the tag, or -1 if not found.</returns>
    private static int FindTagIndex(List<Version> tags, string normalizedVersion)
    {
        for (var i = 0; i < tags.Count; i++)
        {
            if (tags[i].FullVersion.Equals(normalizedVersion, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }
}
