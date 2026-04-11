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

namespace DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

/// <summary>
///     Repository metadata returned by the Azure DevOps repository lookup endpoint.
/// </summary>
/// <param name="Id">Repository identifier.</param>
/// <param name="Name">Repository name.</param>
/// <param name="RemoteUrl">Repository clone URL.</param>
internal sealed record AzureDevOpsRepository(
    string Id,
    string Name,
    string RemoteUrl);

/// <summary>
///     Commit data returned by the Azure DevOps commits endpoint.
/// </summary>
/// <param name="CommitId">Full commit SHA hash.</param>
/// <param name="Comment">Commit message.</param>
internal sealed record AzureDevOpsCommit(
    string CommitId,
    string Comment);

/// <summary>
///     Pull request data returned by the Azure DevOps pull requests endpoint.
/// </summary>
/// <param name="PullRequestId">Pull request identifier.</param>
/// <param name="Title">Pull request title.</param>
/// <param name="Url">Pull request web URL.</param>
/// <param name="Status">Pull request status (active, completed, abandoned).</param>
/// <param name="MergeCommitId">Merge commit SHA when completed, null otherwise.</param>
/// <param name="SourceRefName">Source branch reference name.</param>
/// <param name="Description">Pull request description body.</param>
internal sealed record AzureDevOpsPullRequest(
    int PullRequestId,
    string Title,
    string? Url,
    string Status,
    string? MergeCommitId,
    string? SourceRefName,
    string? Description);

/// <summary>
///     Work item data returned by the Azure DevOps work items endpoint with all fields expanded.
/// </summary>
/// <param name="Id">Work item identifier.</param>
/// <param name="Fields">Dictionary of field reference names to field values.</param>
internal sealed record AzureDevOpsWorkItem(
    int Id,
    Dictionary<string, object?> Fields);

/// <summary>
///     Result of a WIQL query, containing a list of work item id references.
/// </summary>
/// <param name="WorkItems">List of work item id references.</param>
internal sealed record AzureDevOpsWorkItemQuery(
    List<AzureDevOpsWorkItemRef> WorkItems);

/// <summary>
///     Work item id reference returned by WIQL queries and PR work item links.
/// </summary>
/// <param name="Id">Work item identifier.</param>
/// <param name="Url">Work item REST API URL.</param>
internal sealed record AzureDevOpsWorkItemRef(
    int Id,
    string? Url);

/// <summary>
///     Git reference (tag or branch) returned by the Azure DevOps refs endpoint.
/// </summary>
/// <param name="Name">Full reference name (e.g., refs/tags/v1.0.0).</param>
/// <param name="ObjectId">Object SHA that this reference points to (tag object for annotated tags, commit for lightweight tags).</param>
/// <param name="PeeledObjectId">Commit SHA for annotated tags (resolved through the tag object). Null for lightweight tags.</param>
internal sealed record AzureDevOpsRef(
    string Name,
    string ObjectId,
    string? PeeledObjectId = null)
{
    /// <summary>
    ///     Gets the commit SHA this reference resolves to, preferring the peeled object ID for annotated tags.
    /// </summary>
    public string CommitId => PeeledObjectId ?? ObjectId;
}

/// <summary>
///     Generic wrapper for paginated collection responses from the Azure DevOps REST API.
/// </summary>
/// <typeparam name="T">The type of items in the collection.</typeparam>
/// <param name="Count">Total count of items.</param>
/// <param name="Value">List of items in the current page.</param>
internal sealed record AzureDevOpsCollectionResponse<T>(
    int Count,
    List<T> Value);
