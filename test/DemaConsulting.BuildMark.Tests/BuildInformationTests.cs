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
///     Tests for the BuildInformation class.
/// </summary>
[TestClass]
public class BuildInformationTests
{
    /// <summary>
    ///     Test that CreateAsync throws when no version specified and no tags found.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_ThrowsWhenNoVersionAndNoTags()
    {
        // Arrange
        var connector = new MockRepoConnectorEmpty();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await BuildInformation.CreateAsync(connector));

        Assert.Contains("No tags found", exception.Message);
    }

    /// <summary>
    ///     Test that CreateAsync throws when no version specified and current commit doesn't match tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag()
    {
        // Arrange
        var connector = new MockRepoConnectorMismatch();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await BuildInformation.CreateAsync(connector));

        Assert.Contains("does not match any tag", exception.Message);
    }

    /// <summary>
    ///     Test that CreateAsync works with explicit version parameter.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_WorksWithExplicitVersion()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.1.0"));

        // Assert
        Assert.AreEqual("v2.1.0", buildInfo.ToVersion.Tag);
        Assert.AreEqual("current123hash456", buildInfo.ToHash);
        Assert.AreEqual("2.0.0", buildInfo.FromVersion?.Tag);
        Assert.AreEqual("mno345pqr678", buildInfo.FromHash);
    }

    /// <summary>
    ///     Test that CreateAsync works when current commit matches latest tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_WorksWhenCurrentCommitMatchesLatestTag()
    {
        // Arrange
        var connector = new MockRepoConnectorMatchingTag();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector);

        // Assert
        Assert.AreEqual("v2.0.0", buildInfo.ToVersion.Tag);
        Assert.AreEqual("mno345pqr678", buildInfo.ToHash);
        Assert.AreEqual("ver-1.1.0", buildInfo.FromVersion?.Tag);
    }

    /// <summary>
    ///     Test that CreateAsync correctly identifies pre-release and uses previous tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_PreReleaseUsesPreviousTag()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0-beta.1"));

        // Assert
        Assert.AreEqual("v2.0.0-beta.1", buildInfo.ToVersion.Tag);
        Assert.AreEqual("ver-1.1.0", buildInfo.FromVersion?.Tag);
    }

    /// <summary>
    ///     Test that CreateAsync correctly identifies release and skips pre-releases.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_ReleaseSkipsPreReleases()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Assert
        Assert.AreEqual("v2.0.0", buildInfo.ToVersion.Tag);
        Assert.AreEqual("ver-1.1.0", buildInfo.FromVersion?.Tag);
    }

    /// <summary>
    ///     Test that CreateAsync collects issues correctly.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_CollectsIssuesCorrectly()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("ver-1.1.0"));

        // Assert
        Assert.HasCount(1, buildInfo.ChangeIssues);
        Assert.AreEqual("1", buildInfo.ChangeIssues[0].Id);
        Assert.AreEqual("Add feature X", buildInfo.ChangeIssues[0].Title);
        Assert.AreEqual("https://github.com/example/repo/issues/1", buildInfo.ChangeIssues[0].Url);

        Assert.IsEmpty(buildInfo.BugIssues);

        // Known issues should include open bugs (4 and 5)
        Assert.HasCount(2, buildInfo.KnownIssues);
        Assert.AreEqual("4", buildInfo.KnownIssues[0].Id);
        Assert.AreEqual("5", buildInfo.KnownIssues[1].Id);
    }

    /// <summary>
    ///     Test that CreateAsync separates bug and change issues.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_SeparatesBugAndChangeIssues()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Assert
        Assert.HasCount(2, buildInfo.ChangeIssues);
        Assert.HasCount(1, buildInfo.BugIssues);
        Assert.AreEqual("2", buildInfo.BugIssues[0].Id);
        Assert.AreEqual("Fix bug in Y", buildInfo.BugIssues[0].Title);
    }

    /// <summary>
    ///     Test that CreateAsync handles first release correctly (no from version).
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_HandlesFirstReleaseCorrectly()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v1.0.0"));

        // Assert
        Assert.IsNull(buildInfo.FromVersion);
        Assert.IsNull(buildInfo.FromHash);
        Assert.AreEqual("v1.0.0", buildInfo.ToVersion.Tag);
    }

    /// <summary>
    ///     Mock connector with no tags.
    /// </summary>
    private class MockRepoConnectorEmpty : IRepoConnector
    {
        public Task<List<Version>> GetTagHistoryAsync() => Task.FromResult(new List<Version>());
        public Task<List<string>> GetPullRequestsBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<string>());
        public Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId) => Task.FromResult(new List<string>());
        public Task<string> GetIssueTitleAsync(string issueId) => Task.FromResult("Title");
        public Task<string> GetIssueTypeAsync(string issueId) => Task.FromResult("other");
        public Task<string> GetHashForTagAsync(string? tag) => Task.FromResult("hash123");
        public Task<string> GetIssueUrlAsync(string issueId) => Task.FromResult($"https://example.com/{issueId}");
        public Task<List<string>> GetOpenIssuesAsync() => Task.FromResult(new List<string>());
    }

    /// <summary>
    ///     Mock connector where current commit doesn't match latest tag.
    /// </summary>
    private class MockRepoConnectorMismatch : IRepoConnector
    {
        public Task<List<Version>> GetTagHistoryAsync()
        {
            return Task.FromResult(new List<Version> { Version.Create("v1.0.0") });
        }
        public Task<List<string>> GetPullRequestsBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<string>());
        public Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId) => Task.FromResult(new List<string>());
        public Task<string> GetIssueTitleAsync(string issueId) => Task.FromResult("Title");
        public Task<string> GetIssueTypeAsync(string issueId) => Task.FromResult("other");
        public Task<string> GetHashForTagAsync(string? tag) => Task.FromResult(tag == null ? "different123" : "hash123");
        public Task<string> GetIssueUrlAsync(string issueId) => Task.FromResult($"https://example.com/{issueId}");
        public Task<List<string>> GetOpenIssuesAsync() => Task.FromResult(new List<string>());
    }

    /// <summary>
    ///     Mock connector where current commit matches latest tag.
    /// </summary>
    private class MockRepoConnectorMatchingTag : IRepoConnector
    {
        public Task<List<Version>> GetTagHistoryAsync()
        {
            return Task.FromResult(new List<Version> 
            { 
                Version.Create("v1.0.0"), 
                Version.Create("ver-1.1.0"), 
                Version.Create("v2.0.0")
            });
        }
        public Task<List<string>> GetPullRequestsBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<string>());
        public Task<List<string>> GetIssuesForPullRequestAsync(string pullRequestId) => Task.FromResult(new List<string>());
        public Task<string> GetIssueTitleAsync(string issueId) => Task.FromResult("Title");
        public Task<string> GetIssueTypeAsync(string issueId) => Task.FromResult("other");

        public Task<string> GetHashForTagAsync(string? tag)
        {
            if (tag == null || tag == "v2.0.0")
            {
                return Task.FromResult("mno345pqr678");
            }

            if (tag == "ver-1.1.0")
            {
                return Task.FromResult("def456ghi789");
            }

            return Task.FromResult("abc123def456");
        }

        public Task<string> GetIssueUrlAsync(string issueId) => Task.FromResult($"https://example.com/{issueId}");
        public Task<List<string>> GetOpenIssuesAsync() => Task.FromResult(new List<string>());
    }
}
