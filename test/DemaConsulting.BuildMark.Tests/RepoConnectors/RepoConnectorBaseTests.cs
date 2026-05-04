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

using DemaConsulting.BuildMark.BuildNotes;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors;

/// <summary>
///     Tests for the RepoConnectorBase class.
/// </summary>
public class RepoConnectorBaseTests
{
    /// <summary>
    ///     Concrete testable subclass of RepoConnectorBase for testing protected members.
    /// </summary>
    private sealed class TestableRepoConnector : RepoConnectorBase
    {
        /// <summary>
        ///     Exposes HasRules for test assertions.
        /// </summary>
        public bool ExposedHasRules => HasRules;

        /// <summary>
        ///     Exposes ApplyRules for test assertions.
        /// </summary>
        /// <param name="items">Items to route.</param>
        /// <returns>Routed sections.</returns>
        public IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)> ExposedApplyRules(
            IEnumerable<ItemInfo> items) => ApplyRules(items);

        /// <summary>
        ///     Exposes FindVersionIndex for test assertions.
        /// </summary>
        /// <param name="versions">Version list to search.</param>
        /// <param name="targetVersion">Target version to find.</param>
        /// <returns>Index of the matching version, or -1 if not found.</returns>
        public static int ExposedFindVersionIndex(List<VersionTag> versions, VersionTag targetVersion) =>
            FindVersionIndex(versions, targetVersion);

        /// <inheritdoc/>
        public override Task<BuildInformation> GetBuildInformationAsync(VersionTag? version = null)
        {
            // Return a minimal but valid BuildInformation for stub testing purposes
            var versionTag = version ?? VersionTag.Create("1.0.0");
            var versionCommitTag = new VersionCommitTag(versionTag, "stub-hash");
            return Task.FromResult(new BuildInformation(null, versionCommitTag, [], [], [], null));
        }
    }

    /// <summary>
    ///     Test that Configure with rules sets HasRules to true.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_Configure_StoresRulesAndSections_HasRulesReturnsTrue()
    {
        // Arrange - Create testable connector and define rules
        var connector = new TestableRepoConnector();
        List<RuleConfig> rules =
        [
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" }
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "bugs", Title = "Bugs" }
        ];

        // Act - Configure with non-empty rules
        connector.Configure(rules, sections);

        // Assert - HasRules is true after configuration
        Assert.True(connector.ExposedHasRules, "HasRules should be true when rules are configured");
    }

    /// <summary>
    ///     Test that Configure with empty rules sets HasRules to false.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_Configure_EmptyRules_HasRulesReturnsFalse()
    {
        // Arrange - Create testable connector
        var connector = new TestableRepoConnector();

        // Act - Configure with empty rules list
        connector.Configure([], []);

        // Assert - HasRules is false when no rules provided
        Assert.False(connector.ExposedHasRules, "HasRules should be false when no rules are configured");
    }

    /// <summary>
    ///     Test that ApplyRules routes items into configured sections.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_ApplyRules_RoutesItemsToConfiguredSections()
    {
        // Arrange - Create testable connector with rules that route bugs to bugs and everything else to features
        var connector = new TestableRepoConnector();
        connector.Configure(
            [
                new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
                new() { Route = "features" }
            ],
            [
                new() { Id = "features", Title = "Features" },
                new() { Id = "bugs", Title = "Bugs" }
            ]);

        // Define test items: one feature, one bug
        List<ItemInfo> items =
        [
            new("1", "Add feature X", "https://example.com/1", "feature", 1),
            new("2", "Fix bug Y", "https://example.com/2", "bug", 2)
        ];

        // Act - Apply routing rules
        var sections = connector.ExposedApplyRules(items);

        // Assert - Two sections returned
        Assert.Equal(2, sections.Count);

        // Assert - Feature item routed to features section
        var featuresSection = sections.FirstOrDefault(s => s.SectionId == "features");
        Assert.True(featuresSection.SectionTitle != null, "Features section should be present");
        Assert.Single(featuresSection.Items);
        Assert.True(featuresSection.Items[0].Id == "1", "Feature item should be in features section");

        // Assert - Bug item routed to bugs section
        var bugsSection = sections.FirstOrDefault(s => s.SectionId == "bugs");
        Assert.True(bugsSection.SectionTitle != null, "Bugs section should be present");
        Assert.Single(bugsSection.Items);
        Assert.True(bugsSection.Items[0].Id == "2", "Bug item should be in bugs section");
    }

    /// <summary>
    ///     Test that FindVersionIndex finds the correct index when tags have different prefixes but the same semantic version.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindVersionIndex_DifferentPrefixSameVersion_ReturnsCorrectIndex()
    {
        // Arrange - Create version tags with different prefixes but same semantic version
        List<VersionTag> versions =
        [
            VersionTag.Create("v1.0.0"),
            VersionTag.Create("VER1.2.3"),
            VersionTag.Create("Release_1.2.3"),
            VersionTag.Create("v2.0.0")
        ];

        // Target version with a different prefix but the same semantic version as index 1 and 2
        var targetVersion = VersionTag.Create("v1.2.3");

        // Act - Find version index using the protected static method
        var foundIndex = TestableRepoConnector.ExposedFindVersionIndex(versions, targetVersion);

        // Assert - Should find the first matching semantic version (index 1)
        Assert.True(foundIndex == 1, "Should find the first tag with matching semantic version 1.2.3");
    }

    /// <summary>
    ///     Test that FindVersionIndex returns -1 when the target version is not in the list.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindVersionIndex_VersionNotInList_ReturnsMinusOne()
    {
        // Arrange - Create a version list that does not contain the target version
        List<VersionTag> versions =
        [
            VersionTag.Create("v1.0.0"),
            VersionTag.Create("v2.0.0"),
            VersionTag.Create("v3.0.0")
        ];

        // Target version is absent from the list
        var targetVersion = VersionTag.Create("v4.0.0");

        // Act - Attempt to find a version that is not present
        var foundIndex = TestableRepoConnector.ExposedFindVersionIndex(versions, targetVersion);

        // Assert - Should return -1 when version is not found
        Assert.True(foundIndex == -1, "Should return -1 when the target version is not in the list");
    }

    /// <summary>
    ///     Test that FindBaselineForPreRelease skips same-commit tags and returns the most recent
    ///     preceding tag that has a different commit hash.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForPreRelease_SameCommitSkipped_ReturnsPreviousDistinctCommit()
    {
        // Arrange - three preceding tags; first two share the target's commit hash, third differs
        const string targetHash = "aaaa";
        List<VersionCommitTag> precedingVersions =
        [
            new(VersionTag.Create("v1.0.0"), "bbbb"),       // different hash - oldest
            new(VersionTag.Create("v2.0.0-beta.1"), targetHash), // same hash
            new(VersionTag.Create("v2.0.0-beta.2"), targetHash)  // same hash - newest
        ];

        // Act - find baseline for a pre-release with the shared hash
        var baseline = RepoConnectorBase.FindBaselineForPreRelease(precedingVersions, targetHash);

        // Assert - returns the first tag with a different commit hash (v1.0.0), skipping same-hash entries
        Assert.True(baseline != null, "Baseline should not be null when a different-commit predecessor exists");
        Assert.True(
            baseline!.VersionTag.Tag == "v1.0.0",
            $"Expected baseline 'v1.0.0' but got '{baseline.VersionTag.Tag}'");
    }

    /// <summary>
    ///     Test that FindBaselineForPreRelease returns null when all preceding tags share
    ///     the same commit hash as the target.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForPreRelease_AllSameCommit_ReturnsNull()
    {
        // Arrange - all preceding tags share the same commit hash as the target
        const string targetHash = "aaaa";
        List<VersionCommitTag> precedingVersions =
        [
            new(VersionTag.Create("v1.0.0-rc.1"), targetHash),
            new(VersionTag.Create("v1.0.0-rc.2"), targetHash)
        ];

        // Act - find baseline when all predecessors share the target hash
        var baseline = RepoConnectorBase.FindBaselineForPreRelease(precedingVersions, targetHash);

        // Assert - returns null because no different-commit predecessor exists
        Assert.True(baseline == null, "Baseline should be null when all predecessors share the same commit hash");
    }

    /// <summary>
    ///     Test that FindBaselineForRelease skips pre-release tags and returns the most recent
    ///     preceding release tag.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForRelease_SkipsPreReleaseTags_ReturnsPreviousReleaseTag()
    {
        // Arrange - preceding tags include several pre-releases before the previous release
        List<VersionCommitTag> precedingVersions =
        [
            new(VersionTag.Create("v1.0.0"), "hash1"),         // release - oldest
            new(VersionTag.Create("v2.0.0-alpha.1"), "hash2"), // pre-release
            new(VersionTag.Create("v2.0.0-beta.1"), "hash3"),  // pre-release - newest
        ];

        // Act - find baseline for a release target (e.g., v2.0.0)
        var baseline = RepoConnectorBase.FindBaselineForRelease(precedingVersions);

        // Assert - returns v1.0.0, the most recent non-pre-release tag
        Assert.True(baseline != null, "Baseline should not be null when a preceding release tag exists");
        Assert.True(
            baseline!.VersionTag.Tag == "v1.0.0",
            $"Expected baseline 'v1.0.0' but got '{baseline.VersionTag.Tag}'");
    }

    /// <summary>
    ///     Test that FindBaselineForRelease returns null when only pre-release tags precede
    ///     the release target.
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForRelease_NoPreviousRelease_ReturnsNull()
    {
        // Arrange - only pre-release tags exist before the target release
        List<VersionCommitTag> precedingVersions =
        [
            new(VersionTag.Create("v1.0.0-alpha.1"), "hash1"),
            new(VersionTag.Create("v1.0.0-beta.1"), "hash2")
        ];

        // Act - find baseline when no release tag precedes the target
        var baseline = RepoConnectorBase.FindBaselineForRelease(precedingVersions);

        // Assert - returns null because no preceding release tag exists
        Assert.True(baseline == null, "Baseline should be null when only pre-release tags precede the target");
    }

    /// <summary>
    ///     Test that FindBaselineForRelease returns null when the version list is empty
    ///     (first release scenario).
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForRelease_NoPreviousVersion_ReturnsNull()
    {
        // Arrange - empty preceding version list (first release in repository)
        List<VersionCommitTag> precedingVersions = [];

        // Act - find baseline when no preceding versions exist
        var baseline = RepoConnectorBase.FindBaselineForRelease(precedingVersions);

        // Assert - returns null because no preceding versions exist
        Assert.True(baseline == null, "Baseline should be null when no preceding versions exist");
    }

    /// <summary>
    ///     Test that FindBaselineForPreRelease returns null when the version list is empty
    ///     (first pre-release scenario).
    /// </summary>
    [Fact]
    public void RepoConnectorBase_FindBaselineForPreRelease_NoPreviousVersion_ReturnsNull()
    {
        // Arrange - empty preceding version list (first tag in repository)
        List<VersionCommitTag> precedingVersions = [];

        // Act - find baseline when no preceding versions exist
        var baseline = RepoConnectorBase.FindBaselineForPreRelease(precedingVersions, "aaaa");

        // Assert - returns null because no preceding versions exist
        Assert.True(baseline == null, "Baseline should be null when no preceding versions exist");
    }
}
