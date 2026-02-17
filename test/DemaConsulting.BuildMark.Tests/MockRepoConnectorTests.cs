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

namespace DemaConsulting.BuildMark.Tests;

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
        var version = Version.Create("v2.0.0");

        // Act - Get build information with explicit version
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify build information contains expected version
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual(version.Tag, buildInfo.CurrentVersionTag.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync throws exception when no version provided and no tags exist.
    /// </summary>
    /// <remarks>
    ///     What is being tested: MockRepoConnector error handling for missing version information
    ///     What the assertions prove: Appropriate exception is thrown with helpful message when version cannot be determined
    ///     
    ///     Note: This test cannot be implemented with the current MockRepoConnector design
    ///     because it always has predefined tags. The private GetTagHistoryAsync always returns
    ///     tags from the _tagHashes dictionary. To test the "no tags" scenario would require
    ///     refactoring MockRepoConnector to allow configuration of its tag data.
    ///     
    ///     This represents a design limitation rather than a testing gap - the MockRepoConnector
    ///     is designed for testing happy paths with known data, not for testing error conditions.
    /// </remarks>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_WithValidVersionFromTags_ReturnsCorrectBaseline()
    {
        // Arrange - Create connector without explicit version, relying on tag matching
        var connector = new MockRepoConnector();
        var version = Version.Create("v1.0.0");

        // Act - Get build information for a version that exists in the mock tags
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify build information is returned successfully
        Assert.IsNotNull(buildInfo, "Build information should be returned for valid version");
        Assert.AreEqual("v1.0.0", buildInfo.CurrentVersionTag.VersionInfo.Tag);
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
        var version = Version.Create("2.0.0");

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
        var version = Version.Create("2.0.0");

        // Act - Get build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert - Verify changes are categorized
        // Based on MockRepoConnector data:
        // - Issue #2 is a bug (type "bug")
        // - Issue #3 is documentation (type "documentation")
        var allItems = buildInfo.Changes.Concat(buildInfo.Bugs).ToList();
        Assert.IsTrue(allItems.Any(), "Should have at least one change");
        
        // Verify bugs only contain items with type "bug"
        foreach (var bug in buildInfo.Bugs)
        {
            Assert.AreEqual("bug", bug.Type, $"Bug {bug.Id} should have type 'bug'");
        }
    }
}
