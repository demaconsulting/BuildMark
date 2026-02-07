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

using DemaConsulting.BuildMark.RepoConnectors;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the BuildInformation class.
/// </summary>
[TestClass]
public class BuildInformationTests
{
    /// <summary>
    ///     Test that GetBuildInformationAsync throws when no version specified and no tags found.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndNoTags()
    {
        // Create mock connector that throws for no tags
        var connector = Substitute.For<IRepoConnector>();
        connector.GetBuildInformationAsync(Arg.Any<Version?>())
            .Throws(new InvalidOperationException(
                "No tags found in repository and no version specified. " +
                "Please provide a version parameter."));

        // Verify exception is thrown when no version and no tags
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.GetBuildInformationAsync());

        // Verify exception message contains expected text
        Assert.Contains("No tags found", exception.Message);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync throws when no version specified and current commit doesn't match tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag()
    {
        // Create mock connector that throws for commit mismatch
        var connector = Substitute.For<IRepoConnector>();
        connector.GetBuildInformationAsync(Arg.Any<Version?>())
            .Throws(new InvalidOperationException(
                "Target version not specified and current commit does not match any tag. " +
                "Please provide a version parameter."));

        // Verify exception is thrown
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await connector.GetBuildInformationAsync());

        // Verify exception message contains expected text
        Assert.Contains("does not match any tag", exception.Message);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync works with explicit version parameter.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_WorksWithExplicitVersion()
    {
        // Create build information with explicit version
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.1.0"));

        // Verify version and hashes are set correctly
        Assert.AreEqual("v2.1.0", buildInfo.CurrentVersionTag.VersionInfo.Tag);
        Assert.AreEqual("current123hash456", buildInfo.CurrentVersionTag.CommitHash);
        Assert.AreEqual("2.0.0", buildInfo.BaselineVersionTag?.VersionInfo.Tag);
        Assert.AreEqual("mno345pqr678", buildInfo.BaselineVersionTag?.CommitHash);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync works when current commit matches latest tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_WorksWhenCurrentCommitMatchesLatestTag()
    {
        // Create mock connector that returns data for matching tag
        var connector = Substitute.For<IRepoConnector>();
        connector.GetBuildInformationAsync(Arg.Any<Version?>())
            .Returns(Task.FromResult(new BuildInformation(
                new VersionTag(Version.Create("ver-1.1.0"), "def456ghi789"),
                new VersionTag(Version.Create("v2.0.0"), "mno345pqr678"),
                [],
                [],
                [],
                null)));

        // Act
        var buildInfo = await connector.GetBuildInformationAsync();

        // Assert
        Assert.AreEqual("v2.0.0", buildInfo.CurrentVersionTag.VersionInfo.Tag);
        Assert.AreEqual("mno345pqr678", buildInfo.CurrentVersionTag.CommitHash);
        Assert.AreEqual("ver-1.1.0", buildInfo.BaselineVersionTag?.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly identifies pre-release and uses previous tag.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_PreReleaseUsesPreviousTag()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0-beta.1"));

        // Assert
        Assert.AreEqual("v2.0.0-beta.1", buildInfo.CurrentVersionTag.VersionInfo.Tag);
        Assert.AreEqual("ver-1.1.0", buildInfo.BaselineVersionTag?.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly identifies release and skips pre-releases.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_ReleaseSkipsPreReleases()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Assert
        Assert.AreEqual("v2.0.0", buildInfo.CurrentVersionTag.VersionInfo.Tag);
        Assert.AreEqual("ver-1.1.0", buildInfo.BaselineVersionTag?.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync collects issues correctly.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_CollectsIssuesCorrectly()
    {
        // Create build information for version with issues
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("ver-1.1.0"));

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
    ///     Test that GetBuildInformationAsync orders changes by Index (PR number).
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex()
    {
        // Create build information for version with issues
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("ver-1.1.0"));

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
    ///     Test that GetBuildInformationAsync separates bug and change issues.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_SeparatesBugAndChangeIssues()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Assert - verify bugs and changes are properly separated
        Assert.HasCount(1, buildInfo.Changes);
        Assert.HasCount(1, buildInfo.Bugs);
        Assert.AreEqual("2", buildInfo.Bugs[0].Id);
        Assert.AreEqual("Fix bug in Y", buildInfo.Bugs[0].Title);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync handles first release correctly (no from version).
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_GetBuildInformationAsync_HandlesFirstReleaseCorrectly()
    {
        // Arrange
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Assert - verify first release has no previous version
        Assert.IsNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("v1.0.0", buildInfo.CurrentVersionTag.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that ToMarkdown generates correct markdown with default parameters.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - verify all required sections are present
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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown(includeKnownIssues: true);

        // Assert - verify known issues section is included
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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown(headingDepth: 3);

        // Assert - verify heading depth is correct
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
            new VersionTag(Version.Create("v1.0.0"), "abc123"),
            new VersionTag(Version.Create("v1.1.0"), "def456"),
            [], // No changes
            [new ItemInfo("2", "Bug fix", "https://example.com/2", "bug")],
            [],
            null);

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
            new VersionTag(Version.Create("v1.0.0"), "abc123"),
            new VersionTag(Version.Create("v1.1.0"), "def456"),
            [new ItemInfo("1", "Feature", "https://example.com/1", "feature")],
            [], // No bugs
            [],
            null);

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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - verify issue links are properly formatted
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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - verify previous version shows N/A for first release
        var versionInfoStart = markdown.IndexOf("## Version Information", StringComparison.Ordinal);
        var changesStart = markdown.IndexOf("## Changes", StringComparison.Ordinal);
        var versionInfo = markdown.Substring(versionInfoStart, changesStart - versionInfoStart);
        Assert.Contains("| **Previous Version** | N/A |", versionInfo);
        Assert.Contains("| **Previous Commit Hash** | N/A |", versionInfo);
    }

    /// <summary>
    ///     Test that ToMarkdown includes Full Changelog section when link is present.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_IncludesFullChangelogWhenLinkPresent()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - verify full changelog section is included
        Assert.Contains("## Full Changelog", markdown);
        Assert.Contains("See the full changelog at", markdown);
        Assert.Contains("ver-1.1.0...v2.0.0", markdown);
        Assert.Contains("https://github.com/example/repo/compare/ver-1.1.0...v2.0.0", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown excludes Full Changelog section when no baseline version.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_ExcludesFullChangelogWhenNoBaseline()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown();

        // Assert - verify full changelog section is not included
        Assert.DoesNotContain("## Full Changelog", markdown);
    }

    /// <summary>
    ///     Test that ToMarkdown uses correct table widths with centered Issue column.
    /// </summary>
    [TestMethod]
    public async Task BuildInformation_ToMarkdown_UsesCorrectTableWidths()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Act
        var markdown = buildInfo.ToMarkdown(includeKnownIssues: true);

        // Assert - verify table separators use correct width format (10:1 ratio with centered Issue)
        // The separator should have centered Issue column (:-:) and wide left-aligned Title column (:----------)

        // Verify the table separator appears in Changes section
        var changesStart = markdown.IndexOf("## Changes", StringComparison.Ordinal);
        var bugsStart = markdown.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        var changesSection = markdown.Substring(changesStart, bugsStart - changesStart);
        Assert.Contains("| :-: | :---------- |", changesSection);

        // Verify the table separator appears in Bugs Fixed section
        var knownIssuesStart = markdown.IndexOf("## Known Issues", StringComparison.Ordinal);
        var bugsSection = markdown.Substring(bugsStart, knownIssuesStart - bugsStart);
        Assert.Contains("| :-: | :---------- |", bugsSection);

        // Verify the table separator appears in Known Issues section
        var knownIssuesSection = markdown.Substring(knownIssuesStart);
        Assert.Contains("| :-: | :---------- |", knownIssuesSection);
    }

    /// <summary>
    ///     Test that VersionTag correctly stores version and hash.
    /// </summary>
    [TestMethod]
    public void VersionTag_Constructor_StoresVersionAndHash()
    {
        // Arrange
        var version = Version.Create("v1.0.0");
        var hash = "abc123def456";

        // Act
        var versionTag = new VersionTag(version, hash);

        // Assert
        Assert.AreEqual(version, versionTag.VersionInfo);
        Assert.AreEqual(hash, versionTag.CommitHash);
    }

    /// <summary>
    ///     Test that WebLink correctly stores text and URL.
    /// </summary>
    [TestMethod]
    public void WebLink_Constructor_StoresTextAndUrl()
    {
        // Arrange
        var text = "v1.0.0...v2.0.0";
        var url = "https://github.com/owner/repo/compare/v1.0.0...v2.0.0";

        // Act
        var webLink = new WebLink(text, url);

        // Assert
        Assert.AreEqual(text, webLink.LinkText);
        Assert.AreEqual(url, webLink.TargetUrl);
    }
}
