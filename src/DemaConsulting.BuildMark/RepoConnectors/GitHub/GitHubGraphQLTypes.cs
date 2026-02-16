// Copyright (c) 2024-2025 Dema Consulting
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
