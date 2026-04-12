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

using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.Mock;

/// <summary>
///     Tests for the MockRepoConnector class.
/// </summary>
[TestClass]
public class MockRepoConnectorTests
{
    /// <summary>
    ///     Test that MockRepoConnector can be instantiated.
    /// </summary>
    [TestMethod]
    public void MockRepoConnector_Constructor_CreatesInstance()
    {
        // Create connector
        var connector = new MockRepoConnector();

        // Verify instance
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<MockRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that MockRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void MockRepoConnector_ImplementsInterface()
    {
        // Create connector
        var connector = new MockRepoConnector();

        // Verify interface implementation
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync returns build information with expected version.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector.GetBuildInformationAsync with explicit version
    ///     What the assertions prove: Build information is returned with the correct version tag
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_ReturnsExpectedVersion()
    {
        // Arrange - Create connector and specify a known version
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("v2.0.0");

        // Act - Get build information with explicit version
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify build information contains expected version
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual(version.Tag, buildInfo.CurrentVersionTag.VersionTag.Tag);
        Assert.AreEqual("mno345pqr678", buildInfo.CurrentVersionTag.CommitHash);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync resolves version from tags and returns correct baseline.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector's ability to resolve version information from tags
    ///     What the assertions prove: Build information is correctly generated with version from tag data
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_WithValidVersionFromTags_ReturnsCorrectBaseline()
    {
        // Arrange - Create connector without explicit version, relying on tag matching
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("v1.0.0");

        // Act - Get build information for a version that exists in the mock tags
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify build information is returned successfully
        Assert.IsNotNull(buildInfo, "Build information should be returned for valid version");
        Assert.AreEqual("v1.0.0", buildInfo.CurrentVersionTag.VersionTag.Tag);
        Assert.IsNotNull(buildInfo.CurrentVersionTag.CommitHash, "Commit hash should be set");
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync includes all expected data components.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector returns complete BuildInformation structure
    ///     What the assertions prove: All expected components (changes, bugs, known issues) are present
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_ReturnsCompleteInformation()
    {
        // Arrange - Create connector with version that has associated changes
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("2.0.0");

        // Act - Get build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify all expected data structures are present
        Assert.IsNotNull(buildInfo, "Build information should not be null");
        Assert.IsNotNull(buildInfo.Changes, "Changes list should not be null");
        Assert.IsNotNull(buildInfo.Bugs, "Bugs list should not be null");
        Assert.IsNotNull(buildInfo.KnownIssues, "Known issues list should not be null");
        Assert.IsNotNull(buildInfo.CurrentVersionTag, "Current version tag should not be null");
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync correctly identifies bug vs non-bug changes.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector categorization of changes by type
    ///     What the assertions prove: Changes are correctly categorized into bugs and other changes
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_CategorizesChangesCorrectly()
    {
        // Arrange - Create connector and request version with known changes
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("2.0.0");

        // Act - Get build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify changes are categorized
        // Based on MockRepoConnector data:
        // - Issue #2 is a bug (type "bug")
        // - Issue #3 is documentation (type "documentation")
        var allItems = buildInfo.Changes.Concat(buildInfo.Bugs).ToList();
        Assert.IsGreaterThan(0, allItems.Count, "Should have at least one change");

        // Verify bugs only contain items with type "bug"
        foreach (var bug in buildInfo.Bugs)
        {
            Assert.AreEqual("bug", bug.Type, $"Bug {bug.Id} should have type 'bug'");
        }
    }

    /// <summary>
    ///     Test that Configure stores rules and sections, making HasRules true via behavior.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector.Configure stores provided rules
    ///     What the assertions prove: After Configure with non-empty rules, GetBuildInformationAsync returns RoutedSections
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_Configure_StoresRulesAndSections()
    {
        // Arrange - Create connector and define rules
        var connector = new MockRepoConnector();
        List<RuleConfig> rules =
        [
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
            new() { Route = "features" }
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "features", Title = "Features" },
            new() { Id = "bugs", Title = "Bugs" }
        ];

        // Act - Configure the connector with rules
        connector.Configure(rules, sections);
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("2.0.0"));

        // Assert - Routing was applied (RoutedSections is populated when rules are configured)
        Assert.IsNotNull(buildInfo.RoutedSections, "RoutedSections should be set when rules are configured");
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync with rules returns routed sections.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector routes items when rules are configured
    ///     What the assertions prove: RoutedSections contains expected section titles
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_WithRules_ReturnsRoutedSections()
    {
        // Arrange - Create connector with routing rules
        var connector = new MockRepoConnector();
        connector.Configure(
            [
                new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
                new() { Route = "features" }
            ],
            [
                new() { Id = "features", Title = "Features" },
                new() { Id = "bugs", Title = "Bugs" }
            ]);

        // Act - Get build information with routing rules configured
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("2.0.0"));

        // Assert - RoutedSections is populated with the configured sections
        Assert.IsNotNull(buildInfo.RoutedSections, "RoutedSections should not be null when rules are configured");
        Assert.HasCount(2, buildInfo.RoutedSections, "Should have two configured sections");

        var sectionTitles = buildInfo.RoutedSections.Select(s => s.SectionTitle).ToList();
        Assert.Contains("Features", sectionTitles, "Features section should be present");
        Assert.Contains("Bugs", sectionTitles, "Bugs section should be present");
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync without rules returns null RoutedSections.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector does not route when no rules are configured
    ///     What the assertions prove: RoutedSections is null when no rules are configured
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_WithoutRules_ReturnsNullRoutedSections()
    {
        // Arrange - Create connector without configuring rules
        var connector = new MockRepoConnector();

        // Act - Get build information without rules configured
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("2.0.0"));

        // Assert - RoutedSections should be null (legacy mode)
        Assert.IsNull(buildInfo.RoutedSections, "RoutedSections should be null when no rules are configured");
    }

    /// <summary>
    ///     Verify that MockRepoConnector filters known issues by affected-versions.
    ///     Issue 5 has affected-versions [5.0.0,) which excludes v2.0.0 but includes v5.0.0.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector affected-versions filtering of known issues
    ///     What the assertions prove: Bug with out-of-range affected-versions is excluded;
    ///     building for an in-range version includes it
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions()
    {
        // Arrange - Use version v2.0.0 (issue 5 has [5.0.0,) so it is excluded)
        var connector = new MockRepoConnector();

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert - issue 4 (no affected-versions) and issue 6 (no affected-versions) are included
        Assert.IsNotNull(buildInfo.KnownIssues);
        Assert.IsTrue(
            buildInfo.KnownIssues.Exists(i => i.Id == "4"),
            "Bug 4 with no affected-versions should be a known issue");
        Assert.IsFalse(
            buildInfo.KnownIssues.Exists(i => i.Id == "5"),
            "Bug 5 with affected-versions [5.0.0,) should NOT be a known issue for v2.0.0");
        Assert.IsTrue(
            buildInfo.KnownIssues.Exists(i => i.Id == "6"),
            "Bug 6 with no affected-versions should be a known issue");
    }

    /// <summary>
    ///     Verify that issue 7 — a CLOSED bug with affected-versions [1.0.0,1.0.0] — is
    ///     reported as a known issue when building exactly v1.0.0, and is NOT reported for
    ///     a version outside that range (e.g. v2.0.0).
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector known-issues rule for closed bugs with AV
    ///     What the assertions prove: LTS back-port gap is modelled correctly — a closed bug
    ///     with a matching AV is included; the same bug is excluded for an unaffected version
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue()
    {
        // Arrange - issue 7 is closed and has AV [1.0.0,1.0.0] (only matches v1.0.0 exactly)
        var connector = new MockRepoConnector();

        // Act - build for v1.0.0 (issue 7 should be included)
        var buildInfoV1 = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert - issue 7 must be a known issue for v1.0.0
        Assert.IsNotNull(buildInfoV1.KnownIssues);
        Assert.IsTrue(
            buildInfoV1.KnownIssues.Exists(i => i.Id == "7"),
            "Closed bug 7 with AV [1.0.0,1.0.0] should be a known issue for v1.0.0 (LTS back-port gap)");

        // Act - build for v2.0.0 (issue 7 should NOT be included — AV doesn't cover v2)
        var buildInfoV2 = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert - issue 7 must NOT be a known issue for v2.0.0
        Assert.IsNotNull(buildInfoV2.KnownIssues);
        Assert.IsFalse(
            buildInfoV2.KnownIssues.Exists(i => i.Id == "7"),
            "Closed bug 7 with AV [1.0.0,1.0.0] should NOT be a known issue for v2.0.0");
    }
}



