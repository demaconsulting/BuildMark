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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the MockRepoConnector class.
/// </summary>
[TestClass]
public class MockRepoConnectorTests
{
    /// <summary>
    ///     Test that GetTagHistoryAsync returns expected tags.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetTagHistoryAsync_ReturnsExpectedTags()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var tags = await connector.GetTagHistoryAsync();

        // Assert
        Assert.HasCount(5, tags);
        Assert.AreEqual("v1.0.0", tags[0]);
        Assert.AreEqual("v1.1.0", tags[1]);
        Assert.AreEqual("v2.0.0-beta.1", tags[2]);
        Assert.AreEqual("v2.0.0-rc.1", tags[3]);
        Assert.AreEqual("v2.0.0", tags[4]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync returns expected PRs for specific range.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetPullRequestsBetweenTagsAsync_ReturnsExpectedPRsForRange()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync("v1.0.0", "v1.1.0");

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("10", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync returns expected PRs for different range.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetPullRequestsBetweenTagsAsync_ReturnsExpectedPRsForDifferentRange()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync("v1.1.0", "v2.0.0");

        // Assert
        Assert.HasCount(2, prs);
        Assert.AreEqual("11", prs[0]);
        Assert.AreEqual("12", prs[1]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync returns all PRs when no tags specified.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetPullRequestsBetweenTagsAsync_ReturnsAllPRsWhenNoTags()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(3, prs);
    }

    /// <summary>
    ///     Test that GetIssuesForPullRequestAsync returns expected issues.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssuesForPullRequestAsync_ReturnsExpectedIssues()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var issues = await connector.GetIssuesForPullRequestAsync("10");

        // Assert
        Assert.HasCount(1, issues);
        Assert.AreEqual("1", issues[0]);
    }

    /// <summary>
    ///     Test that GetIssuesForPullRequestAsync returns empty for unknown PR.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssuesForPullRequestAsync_ReturnsEmptyForUnknownPR()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var issues = await connector.GetIssuesForPullRequestAsync("999");

        // Assert
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Test that GetIssueTitleAsync returns expected title.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssueTitleAsync_ReturnsExpectedTitle()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var title = await connector.GetIssueTitleAsync("1");

        // Assert
        Assert.AreEqual("Add feature X", title);
    }

    /// <summary>
    ///     Test that GetIssueTitleAsync returns default for unknown issue.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssueTitleAsync_ReturnsDefaultForUnknownIssue()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var title = await connector.GetIssueTitleAsync("999");

        // Assert
        Assert.AreEqual("Issue 999", title);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync returns expected type.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssueTypeAsync_ReturnsExpectedType()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var type = await connector.GetIssueTypeAsync("2");

        // Assert
        Assert.AreEqual("bug", type);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync returns other for unknown issue.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssueTypeAsync_ReturnsOtherForUnknownIssue()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var type = await connector.GetIssueTypeAsync("999");

        // Assert
        Assert.AreEqual("other", type);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns expected hash.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetHashForTagAsync_ReturnsExpectedHash()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var hash = await connector.GetHashForTagAsync("v1.0.0");

        // Assert
        Assert.AreEqual("abc123def456", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns current hash for null tag.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetHashForTagAsync_ReturnsCurrentHashForNullTag()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var hash = await connector.GetHashForTagAsync(null);

        // Assert
        Assert.AreEqual("current123hash456", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns unknown hash for unknown tag.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetHashForTagAsync_ReturnsUnknownHashForUnknownTag()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var hash = await connector.GetHashForTagAsync("v999.0.0");

        // Assert
        Assert.AreEqual("unknown000hash000", hash);
    }

    /// <summary>
    ///     Test that GetIssueUrlAsync returns expected URL.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetIssueUrlAsync_ReturnsExpectedUrl()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var url = await connector.GetIssueUrlAsync("1");

        // Assert
        Assert.AreEqual("https://github.com/example/repo/issues/1", url);
    }
}
