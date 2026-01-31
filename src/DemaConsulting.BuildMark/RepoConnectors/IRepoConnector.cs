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
///     Interface for repository connectors that fetch repository information.
/// </summary>
public interface IRepoConnector
{
    /// <summary>
    ///     Gets the history of tags leading to the current branch.
    /// </summary>
    /// <returns>List of tags in chronological order.</returns>
    Task<List<TagInfo>> GetTagHistoryAsync();

    /// <summary>
    ///     Gets the list of pull request IDs between two tags.
    /// </summary>
    /// <param name="fromTag">Starting tag (null for start of history).</param>
    /// <param name="toTag">Ending tag (null for current state).</param>
    /// <returns>List of pull request IDs.</returns>
    Task<List<string>> GetPullRequestsBetweenTagsAsync(TagInfo? fromTag, TagInfo? toTag);

    /// <summary>
    ///     Gets the issue IDs associated with a pull request.
    /// </summary>
    /// <param name="pullRequestId">Pull request ID.</param>
    /// <returns>List of issue IDs.</returns>
    Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId);

    /// <summary>
    ///     Gets the title of an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue title.</returns>
    Task<string> GetIssueTitleAsync(string issueId);

    /// <summary>
    ///     Gets the type of an issue (bug, feature, etc.).
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue type.</returns>
    Task<string> GetIssueTypeAsync(string issueId);

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag information (null for current state).</param>
    /// <returns>Git hash.</returns>
    Task<string> GetHashForTagAsync(TagInfo? tag);

    /// <summary>
    ///     Gets the URL for an issue.
    /// </summary>
    /// <param name="issueId">Issue ID.</param>
    /// <returns>Issue URL.</returns>
    Task<string> GetIssueUrlAsync(string issueId);

    /// <summary>
    ///     Gets the list of open issue IDs.
    /// </summary>
    /// <returns>List of open issue IDs.</returns>
    Task<List<string>> GetOpenIssuesAsync();
}
