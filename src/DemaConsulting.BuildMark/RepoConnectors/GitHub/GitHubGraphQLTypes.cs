// Copyright (c) 2026 DEMA Consulting
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

namespace DemaConsulting.BuildMark.RepoConnectors.GitHub;

/// <summary>
///     Response for finding issues linked to a pull request.
/// </summary>
/// <param name="Repository">Repository data.</param>
internal record FindIssueIdsResponse(
    RepositoryData? Repository);

/// <summary>
///     Repository data containing pull request information.
/// </summary>
/// <param name="PullRequest">Pull request data.</param>
internal record RepositoryData(
    PullRequestData? PullRequest);

/// <summary>
///     Pull request data containing closing issues.
/// </summary>
/// <param name="ClosingIssuesReferences">Closing issues references.</param>
internal record PullRequestData(
    ClosingIssuesReferencesData? ClosingIssuesReferences);

/// <summary>
///     Closing issues references data containing nodes and page info.
/// </summary>
/// <param name="Nodes">Issue nodes.</param>
/// <param name="PageInfo">Pagination information.</param>
internal record ClosingIssuesReferencesData(
    List<IssueNode>? Nodes,
    PageInfo? PageInfo);

/// <summary>
///     Issue node containing issue number.
/// </summary>
/// <param name="Number">Issue number.</param>
internal record IssueNode(
    int? Number);

/// <summary>
///     Pagination information for GraphQL queries.
/// </summary>
/// <param name="HasNextPage">Indicates whether there are more pages.</param>
/// <param name="EndCursor">Cursor for the next page.</param>
internal record PageInfo(
    bool HasNextPage,
    string? EndCursor);

/// <summary>
///     Response for getting commits from a repository.
/// </summary>
/// <param name="Repository">Repository data containing commit information.</param>
internal record GetCommitsResponse(
    CommitRepositoryData? Repository);

/// <summary>
///     Repository data containing ref information for commits.
/// </summary>
/// <param name="Ref">Git reference data.</param>
internal record CommitRepositoryData(
    RefData? Ref);

/// <summary>
///     Git reference data containing commit history.
/// </summary>
/// <param name="Target">Target commit information.</param>
internal record RefData(
    TargetData? Target);

/// <summary>
///     Target commit data containing history.
/// </summary>
/// <param name="History">Commit history with pagination.</param>
internal record TargetData(
    CommitHistoryData? History);

/// <summary>
///     Commit history data containing nodes and page info.
/// </summary>
/// <param name="Nodes">Commit nodes.</param>
/// <param name="PageInfo">Pagination information.</param>
internal record CommitHistoryData(
    List<CommitNode>? Nodes,
    PageInfo? PageInfo);

/// <summary>
///     Commit node containing commit SHA.
/// </summary>
/// <param name="Oid">Git object ID (SHA).</param>
internal record CommitNode(
    string? Oid);

/// <summary>
///     Response for getting releases from a repository.
/// </summary>
/// <param name="Repository">Repository data containing release information.</param>
internal record GetReleasesResponse(
    ReleaseRepositoryData? Repository);

/// <summary>
///     Repository data containing releases information.
/// </summary>
/// <param name="Releases">Releases connection data.</param>
internal record ReleaseRepositoryData(
    ReleasesConnectionData? Releases);

/// <summary>
///     Releases connection data containing nodes and page info.
/// </summary>
/// <param name="Nodes">Release nodes.</param>
/// <param name="PageInfo">Pagination information.</param>
internal record ReleasesConnectionData(
    List<ReleaseNode>? Nodes,
    PageInfo? PageInfo);

/// <summary>
///     Release node containing release information.
/// </summary>
/// <param name="TagName">Tag name associated with the release.</param>
internal record ReleaseNode(
    string? TagName);

/// <summary>
///     Response for getting tags from a repository.
/// </summary>
/// <param name="Repository">Repository data containing tags information.</param>
internal record GetAllTagsResponse(
    TagRepositoryData? Repository);

/// <summary>
///     Repository data containing tags information.
/// </summary>
/// <param name="Refs">Git references connection data.</param>
internal record TagRepositoryData(
    TagsConnectionData? Refs);

/// <summary>
///     Tags connection data containing nodes and page info.
/// </summary>
/// <param name="Nodes">Tag nodes.</param>
/// <param name="PageInfo">Pagination information.</param>
internal record TagsConnectionData(
    List<TagNode>? Nodes,
    PageInfo? PageInfo);

/// <summary>
///     Tag node containing tag information.
/// </summary>
/// <param name="Name">Tag name.</param>
/// <param name="Target">Target commit information.</param>
internal record TagNode(
    string? Name,
    TagTargetData? Target);

/// <summary>
///     Target data for a tag containing commit SHA.
/// </summary>
/// <param name="Oid">Git object ID (SHA) of the commit.</param>
internal record TagTargetData(
    string? Oid);
