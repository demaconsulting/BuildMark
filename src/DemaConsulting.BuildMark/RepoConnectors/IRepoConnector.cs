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
    ///     Gets the history of releases leading to the current branch.
    /// </summary>
    /// <returns>List of release versions in chronological order.</returns>
    Task<List<Version>> GetReleaseHistoryAsync();

    /// <summary>
    ///     Gets the list of changes between two versions.
    /// </summary>
    /// <param name="from">Starting version (null for start of history).</param>
    /// <param name="to">Ending version (null for current state).</param>
    /// <returns>List of changes with full information.</returns>
    Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to);

    /// <summary>
    ///     Gets the git hash for a tag.
    /// </summary>
    /// <param name="tag">Tag name (null for current state).</param>
    /// <returns>Git hash.</returns>
    Task<string> GetHashForTagAsync(string? tag);

    /// <summary>
    ///     Gets the list of open issues with their details.
    /// </summary>
    /// <returns>List of open issues with full information.</returns>
    Task<List<ItemInfo>> GetOpenIssuesAsync();
}
