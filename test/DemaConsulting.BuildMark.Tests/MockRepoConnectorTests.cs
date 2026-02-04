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
    ///     Test that GetReleaseHistoryAsync returns expected releases.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetReleaseHistoryAsync_ReturnsExpectedReleases()
    {
        // Get release history from mock connector
        var connector = new MockRepoConnector();
        var tags = await connector.GetReleaseHistoryAsync();

        // Verify all expected releases are returned
        Assert.HasCount(5, tags);
        Assert.AreEqual("v1.0.0", tags[0].Tag);
        Assert.AreEqual("ver-1.1.0", tags[1].Tag);
        Assert.AreEqual("release_2.0.0-beta.1", tags[2].Tag);
        Assert.AreEqual("v2.0.0-rc.1", tags[3].Tag);
        Assert.AreEqual("2.0.0", tags[4].Tag);
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
        var hash = await connector.GetHashForTagAsync(Version.Create("v1.0.0").Tag);

        // Assert
        Assert.AreEqual("abc123def456", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns current hash for null tag.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetCurrentHashAsync_ReturnsCurrentHash()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var hash = await connector.GetCurrentHashAsync();

        // Assert
        Assert.AreEqual("current123hash456", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync throws for null tag.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetHashForTagAsync_ThrowsForNullTag()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await connector.GetHashForTagAsync(null));
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
        var hash = await connector.GetHashForTagAsync(Version.Create("v999.0.0").Tag);

        // Assert
        Assert.AreEqual("unknown000hash000", hash);
    }

    /// <summary>
    ///     Test that GetOpenIssuesAsync returns expected open issues with details.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetOpenIssuesAsync_ReturnsExpectedOpenIssues()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var openIssues = await connector.GetOpenIssuesAsync();

        // Assert
        Assert.HasCount(2, openIssues);
        Assert.AreEqual("4", openIssues[0].Id);
        Assert.AreEqual("Known bug A", openIssues[0].Title);
        Assert.AreEqual("bug", openIssues[0].Type);
        Assert.AreEqual("5", openIssues[1].Id);
        Assert.AreEqual("Known bug B", openIssues[1].Title);
        Assert.AreEqual("bug", openIssues[1].Type);
    }

    /// <summary>
    ///     Test that GetChangesBetweenTagsAsync returns changes including PRs without issues.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetChangesBetweenTagsAsync_IncludesPRsWithoutIssues()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var fromVersion = Version.Create("v1.0.0");
        var toVersion = Version.Create("ver-1.1.0");

        // Act
        var changes = await connector.GetChangesBetweenTagsAsync(fromVersion, toVersion);

        // Assert - Should have both issue #1 (from PR #10) and PR #13 (without issues)
        Assert.HasCount(2, changes);

        // First change should be issue #1
        Assert.AreEqual("1", changes[0].Id);
        Assert.AreEqual("Add feature X", changes[0].Title);
        Assert.AreEqual("feature", changes[0].Type);

        // Second change should be PR #13
        Assert.AreEqual("#13", changes[1].Id);
        Assert.AreEqual("PR #13", changes[1].Title);
        Assert.Contains("pull/13", changes[1].Url);
        Assert.AreEqual("other", changes[1].Type);
    }
}
