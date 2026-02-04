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
        // Create connector with no tags
        var connector = new MockRepoConnectorEmpty();

        // Verify exception is thrown when no version and no tags
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
        // Create build information with explicit version
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.1.0"));

        // Verify version and hashes are set correctly
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
        // Create build information for version with issues
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("ver-1.1.0"));

        // Verify change issues are collected (including PR without issues)
        Assert.HasCount(2, buildInfo.Changes);
        Assert.AreEqual("1", buildInfo.Changes[0].Id);
        Assert.AreEqual("Add feature X", buildInfo.Changes[0].Title);
        Assert.AreEqual("https://github.com/example/repo/issues/1", buildInfo.Changes[0].Url);

        // Second change should be PR #13 (without issues)
        Assert.AreEqual("#13", buildInfo.Changes[1].Id);

        // Verify no bug issues for this version
        Assert.IsEmpty(buildInfo.Bugs);

        // Verify known issues include open bugs
        Assert.HasCount(2, buildInfo.KnownIssues);
        Assert.AreEqual("4", buildInfo.KnownIssues[0].Id);
        Assert.AreEqual("5", buildInfo.KnownIssues[1].Id);
    }

    /// <summary>
    ///     Test that CreateAsync orders changes by Index (PR number).
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_CreateAsync_OrdersChangesByIndex()
    {
        // Create build information for version with issues
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("ver-1.1.0"));

        // Verify changes are ordered by Index (PR number)
        // Issue #1 from PR #10 should come before PR #13
        Assert.HasCount(2, buildInfo.Changes);
        Assert.AreEqual("1", buildInfo.Changes[0].Id);
        Assert.AreEqual(10, buildInfo.Changes[0].Index);
        Assert.AreEqual("#13", buildInfo.Changes[1].Id);
        Assert.AreEqual(13, buildInfo.Changes[1].Index);
        
        // Verify Index values are in ascending order
        for (var i = 0; i < buildInfo.Changes.Count - 1; i++)
        {
            Assert.IsLessThanOrEqualTo(buildInfo.Changes[i + 1].Index, buildInfo.Changes[i].Index, 
                $"Changes should be ordered by Index. Found {buildInfo.Changes[i].Index} before {buildInfo.Changes[i + 1].Index}");
        }
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
        Assert.HasCount(1, buildInfo.Changes);
        Assert.HasCount(1, buildInfo.Bugs);
        Assert.AreEqual("2", buildInfo.Bugs[0].Id);
        Assert.AreEqual("Fix bug in Y", buildInfo.Bugs[0].Title);
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
    ///     Test that ToMarkdown generates correct markdown with default parameters.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert
        Assert.Contains("# Build Report", markdown);
        Assert.Contains("## Version Information", markdown);
        Assert.Contains("## Changes", markdown);
        Assert.Contains("## Bugs Fixed", markdown);
        Assert.DoesNotContain("## Known Issues", markdown);
        Assert.Contains("v2.0.0", markdown);
        Assert.Contains("ver-1.1.0", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown includes known issues when requested.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_IncludesKnownIssuesWhenRequested()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown(includeKnownIssues: true);

        // Assert
        Assert.Contains("## Known Issues", markdown);
        Assert.Contains("Known bug A", markdown);
        Assert.Contains("Known bug B", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown respects custom heading depth.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_RespectsCustomHeadingDepth()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown(headingDepth: 3);

        // Assert
        Assert.Contains("### Build Report", markdown);
        Assert.Contains("#### Version Information", markdown);
        Assert.Contains("#### Changes", markdown);
        Assert.Contains("#### Bugs Fixed", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown displays N/A for empty changes table.
    /// </summary>
    [TestMethod]
    public void BuildInformation_ToMarkdown_DisplaysNAForEmptyChanges()
    {
        // Arrange - Create build info with no change issues
        var buildInfo = new BuildInformation(
            Version.Create("v1.0.0"),
            Version.Create("v1.1.0"),
            "abc123",
            "def456",
            new List<ItemInfo>(), // No changes
            new List<ItemInfo> { new ItemInfo("2", "Bug fix", "https://example.com/2", "bug") },
            new List<ItemInfo>());

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - Check that Changes section contains N/A
        var changesSectionStart = markdown.IndexOf("## Changes", StringComparison.Ordinal);
        var bugsSectionStart = markdown.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        var changesSection = markdown.Substring(changesSectionStart, bugsSectionStart - changesSectionStart);
        Assert.Contains("| N/A | N/A |", changesSection);
    }

    /// <summary>
    ///     Test that ToMarkdown displays N/A for empty bugs table.
    /// </summary>
    [TestMethod]
    public void BuildInformation_ToMarkdown_DisplaysNAForEmptyBugs()
    {
        // Arrange - Create build info with no bug issues
        var buildInfo = new BuildInformation(
            Version.Create("v1.0.0"),
            Version.Create("v1.1.0"),
            "abc123",
            "def456",
            new List<ItemInfo> { new ItemInfo("1", "Feature", "https://example.com/1", "feature") },
            new List<ItemInfo>(), // No bugs
            new List<ItemInfo>());

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - Check that Bugs Fixed section contains N/A
        var bugsSectionStart = markdown.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        var bugsSection = markdown.Substring(bugsSectionStart);
        Assert.Contains("| N/A | N/A |", bugsSection);
    }

    /// <summary>
    ///     Test that ToMarkdown includes issue links in tables.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_IncludesIssueLinks()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert
        Assert.Contains("[3](https://github.com/example/repo/issues/3)", markdown);
        Assert.Contains("[2](https://github.com/example/repo/issues/2)", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown handles first release with N/A for previous version.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_HandlesFirstReleaseWithNA()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await BuildInformation.CreateAsync(connector, Version.Create("v1.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert
        var versionInfoStart = markdown.IndexOf("## Version Information", StringComparison.Ordinal);
        var changesStart = markdown.IndexOf("## Changes", StringComparison.Ordinal);
        var versionInfo = markdown.Substring(versionInfoStart, changesStart - versionInfoStart);
        Assert.Contains("| **Previous Version** | N/A |", versionInfo);
        Assert.Contains("| **Previous Commit Hash** | N/A |", versionInfo);
    }

    /// <summary>
    ///     Mock connector with no tags.
    /// </summary>
    private class MockRepoConnectorEmpty : IRepoConnector
    {
        public Task<List<Version>> GetReleaseHistoryAsync() => Task.FromResult(new List<Version>());
        public Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<ItemInfo>());
        public Task<string> GetCurrentHashAsync() => Task.FromResult("hash123");
        public Task<string> GetHashForTagAsync(string? tag) => Task.FromResult("hash123");
        public Task<List<ItemInfo>> GetOpenIssuesAsync() => Task.FromResult(new List<ItemInfo>());
    }

    /// <summary>
    ///     Mock connector where current commit doesn't match latest tag.
    /// </summary>
    private class MockRepoConnectorMismatch : IRepoConnector
    {
        public Task<List<Version>> GetReleaseHistoryAsync()
        {
            return Task.FromResult(new List<Version> { Version.Create("v1.0.0") });
        }
        public Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<ItemInfo>());
        public Task<string> GetCurrentHashAsync() => Task.FromResult("different123");
        public Task<string> GetHashForTagAsync(string? tag) => Task.FromResult("hash123");
        public Task<List<ItemInfo>> GetOpenIssuesAsync() => Task.FromResult(new List<ItemInfo>());
    }

    /// <summary>
    ///     Mock connector where current commit matches latest tag.
    /// </summary>
    private class MockRepoConnectorMatchingTag : IRepoConnector
    {
        public Task<List<Version>> GetReleaseHistoryAsync()
        {
            // Releases are ordered newest first to match Octokit API behavior
            return Task.FromResult(new List<Version>
            {
                Version.Create("v2.0.0"),
                Version.Create("ver-1.1.0"),
                Version.Create("v1.0.0")
            });
        }
        public Task<List<ItemInfo>> GetChangesBetweenTagsAsync(Version? from, Version? to) => Task.FromResult(new List<ItemInfo>());

        public Task<string> GetCurrentHashAsync() => Task.FromResult("mno345pqr678");

        public Task<string> GetHashForTagAsync(string? tag)
        {
            if (tag == "v2.0.0")
            {
                return Task.FromResult("mno345pqr678");
            }

            if (tag == "ver-1.1.0")
            {
                return Task.FromResult("def456ghi789");
            }

            return Task.FromResult("abc123def456");
        }

        public Task<List<ItemInfo>> GetOpenIssuesAsync() => Task.FromResult(new List<ItemInfo>());
    }
}
