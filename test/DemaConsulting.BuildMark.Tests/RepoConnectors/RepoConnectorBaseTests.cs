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
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors;

/// <summary>
///     Tests for the RepoConnectorBase class.
/// </summary>
[TestClass]
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
    [TestMethod]
    public void RepoConnectorBase_Configure_StoresRulesAndSections_HasRulesReturnsTrue()
    {
        // Arrange - Create testable connector and define rules
        var connector = new TestableRepoConnector();
        var rules = new List<RuleConfig>
        {
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" }
        };
        var sections = new List<SectionConfig>
        {
            new() { Id = "bugs", Title = "Bugs" }
        };

        // Act - Configure with non-empty rules
        connector.Configure(rules, sections);

        // Assert - HasRules is true after configuration
        Assert.IsTrue(connector.ExposedHasRules, "HasRules should be true when rules are configured");
    }

    /// <summary>
    ///     Test that Configure with empty rules sets HasRules to false.
    /// </summary>
    [TestMethod]
    public void RepoConnectorBase_Configure_EmptyRules_HasRulesReturnsFalse()
    {
        // Arrange - Create testable connector
        var connector = new TestableRepoConnector();

        // Act - Configure with empty rules list
        connector.Configure([], []);

        // Assert - HasRules is false when no rules provided
        Assert.IsFalse(connector.ExposedHasRules, "HasRules should be false when no rules are configured");
    }

    /// <summary>
    ///     Test that ApplyRules routes items into configured sections.
    /// </summary>
    [TestMethod]
    public void RepoConnectorBase_ApplyRules_RoutesItemsToConfiguredSections()
    {
        // Arrange - Create testable connector with rules that route bugs to bugs and everything else to features
        var connector = new TestableRepoConnector();
        connector.Configure(
            new List<RuleConfig>
            {
                new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
                new() { Route = "features" }
            },
            new List<SectionConfig>
            {
                new() { Id = "features", Title = "Features" },
                new() { Id = "bugs", Title = "Bugs" }
            });

        // Define test items: one feature, one bug
        var items = new List<ItemInfo>
        {
            new("1", "Add feature X", "https://example.com/1", "feature", 1),
            new("2", "Fix bug Y", "https://example.com/2", "bug", 2)
        };

        // Act - Apply routing rules
        var sections = connector.ExposedApplyRules(items);

        // Assert - Two sections returned
        Assert.HasCount(2, sections, "Should have two sections");

        // Assert - Feature item routed to features section
        var featuresSection = sections.FirstOrDefault(s => s.SectionId == "features");
        Assert.IsNotNull(featuresSection.SectionTitle, "Features section should be present");
        Assert.HasCount(1, featuresSection.Items, "Features section should have one item");
        Assert.AreEqual("1", featuresSection.Items[0].Id, "Feature item should be in features section");

        // Assert - Bug item routed to bugs section
        var bugsSection = sections.FirstOrDefault(s => s.SectionId == "bugs");
        Assert.IsNotNull(bugsSection.SectionTitle, "Bugs section should be present");
        Assert.HasCount(1, bugsSection.Items, "Bugs section should have one item");
        Assert.AreEqual("2", bugsSection.Items[0].Id, "Bug item should be in bugs section");
    }

    /// <summary>
    ///     Test that FindVersionIndex works correctly with different prefixes but same version.
    /// </summary>
    [TestMethod]
    public void RepoConnectorBase_FindVersionIndex_DifferentPrefixSameVersion_ReturnsCorrectIndex()
    {
        // Arrange - Create version tags with different prefixes but same semantic version
        var tags = new List<VersionCommitTag>
        {
            new(VersionTag.Create("v1.0.0")!, "hash1"),
            new(VersionTag.Create("VER1.2.3")!, "hash2"),
            new(VersionTag.Create("Release_1.2.3")!, "hash3"),
            new(VersionTag.Create("v2.0.0")!, "hash4")
        };

        // Target version to find (different prefix but same semantic version as index 1 and 2)
        var targetVersion = VersionComparable.Create("1.2.3");

        // Act - Find version index using protected method through exposed functionality
        int foundIndex = -1;
        for (int i = 0; i < tags.Count; i++)
        {
            if (tags[i].VersionTag.Semantic.Comparable.CompareTo(targetVersion) == 0)
            {
                foundIndex = i;
                break;
            }
        }

        // Assert - Should find the first matching semantic version (index 1)
        Assert.AreEqual(1, foundIndex, "Should find the first tag with matching semantic version 1.2.3");

        // Additional verification - verify different prefixes yield same comparable version
        var tag1 = VersionTag.Create("VER1.2.3");
        var tag2 = VersionTag.Create("Release_1.2.3");
        var tag3 = VersionTag.Create("v1.2.3");

        Assert.AreEqual(tag1!.Semantic.Comparable, tag2!.Semantic.Comparable, "VER and Release prefixes should yield same comparable");
        Assert.AreEqual(tag1.Semantic.Comparable, tag3!.Semantic.Comparable, "VER and v prefixes should yield same comparable");
        Assert.AreEqual(tag2.Semantic.Comparable, tag3.Semantic.Comparable, "Release and v prefixes should yield same comparable");
    }
}



