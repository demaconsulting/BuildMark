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

using DemaConsulting.BuildMark.Version;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemaConsulting.BuildMark.Tests.Version;

/// <summary>
///     Version subsystem integration tests.
///     Tests the overall behavior of the Version subsystem components working together.
/// </summary>
[TestClass]
public class VersionTests
{
    /// <summary>
    ///     Test that VersionComparable can be created from valid versions.
    /// </summary>
    [TestMethod]
    public void VersionComparable_Create_ValidVersions_ReturnsVersionComparable()
    {
        // Arrange & Act
        var simple = VersionComparable.Create("1.2.3");
        var preRelease = VersionComparable.Create("2.0.0-alpha.1");
        var complex = VersionComparable.Create("10.5.99-beta.10");

        // Assert
        Assert.IsNotNull(simple);
        Assert.IsNotNull(preRelease);
        Assert.IsNotNull(complex);
        Assert.IsInstanceOfType<VersionComparable>(simple);
        Assert.IsInstanceOfType<VersionComparable>(preRelease);
        Assert.IsInstanceOfType<VersionComparable>(complex);
    }

    /// <summary>
    ///     Test that VersionSemantic can be created from valid semantic versions.
    /// </summary>
    [TestMethod]
    public void VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic()
    {
        // Arrange & Act
        var simple = VersionSemantic.Create("1.2.3");
        var withMetadata = VersionSemantic.Create("2.0.0+build.123");
        var complex = VersionSemantic.Create("1.0.0-alpha.beta.2+exp.sha.5114f85");

        // Assert
        Assert.IsNotNull(simple);
        Assert.IsNotNull(withMetadata);
        Assert.IsNotNull(complex);
        Assert.IsInstanceOfType<VersionSemantic>(simple);
        Assert.IsInstanceOfType<VersionSemantic>(withMetadata);
        Assert.IsInstanceOfType<VersionSemantic>(complex);
    }

    /// <summary>
    ///     Test that VersionTag can be created from valid tags.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_ValidTag_ReturnsVersionTag()
    {
        // Arrange & Act
        var simple = VersionTag.Create("1.2.3");
        var prefixed = VersionTag.Create("v2.0.0");
        var complex = VersionTag.Create("release-1.5.0-rc.1");

        // Assert
        Assert.IsNotNull(simple);
        Assert.IsNotNull(prefixed);
        Assert.IsNotNull(complex);
        Assert.IsInstanceOfType<VersionTag>(simple);
        Assert.IsInstanceOfType<VersionTag>(prefixed);
        Assert.IsInstanceOfType<VersionTag>(complex);
    }

    /// <summary>
    ///     Test that VersionInterval can be created from valid interval parameters.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Create_ValidInterval_ReturnsVersionInterval()
    {
        // Arrange & Act
        var inclusive = new VersionInterval("1.0.0", true, "2.0.0", true);
        var exclusive = new VersionInterval("1.0.0", false, "2.0.0", false);
        var mixed = new VersionInterval("1.0.0", true, "2.0.0", false);

        // Assert
        Assert.IsNotNull(inclusive);
        Assert.IsNotNull(exclusive);
        Assert.IsNotNull(mixed);
        Assert.IsInstanceOfType<VersionInterval>(inclusive);
        Assert.IsInstanceOfType<VersionInterval>(exclusive);
        Assert.IsInstanceOfType<VersionInterval>(mixed);
    }

    /// <summary>
    ///     Test that VersionCommitTag can be created from valid parameters.
    /// </summary>
    [TestMethod]
    public void VersionCommitTag_Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var versionTag = VersionTag.Create("v1.2.3");
        var commitHash = "abc123def456789";

        // Act
        var versionCommitTag = new VersionCommitTag(versionTag!, commitHash);

        // Assert
        Assert.IsNotNull(versionCommitTag);
        Assert.IsInstanceOfType<VersionCommitTag>(versionCommitTag);
        Assert.AreEqual(versionTag, versionCommitTag.VersionTag);
        Assert.AreEqual(commitHash, versionCommitTag.CommitHash);
    }

    /// <summary>
    ///     Test that the Version subsystem can create and use all version types correctly.
    ///     This validates the subsystem-level requirement for version processing capabilities.
    /// </summary>
    [TestMethod]
    public void Version_Subsystem_CreateAllVersionTypes_WorksCorrectly()
    {
        // Arrange - Create instances of all version types
        var versionComparable = VersionComparable.Create("1.2.3-alpha.1");
        var versionSemantic = VersionSemantic.Create("2.0.0-beta.2+build.1");
        var versionTag = VersionTag.Create("v3.1.0");
        var versionInterval = new VersionInterval("1.0.0", true, "2.0.0", false);
        var versionCommitTag = new VersionCommitTag(versionTag!, "abc123def456");

        // Assert - All types are created successfully
        Assert.IsNotNull(versionComparable);
        Assert.IsNotNull(versionSemantic);
        Assert.IsNotNull(versionTag);
        Assert.IsNotNull(versionInterval);
        Assert.IsNotNull(versionCommitTag);

        // Assert - Version properties are accessible and correct
        Assert.AreEqual("1.2.3-alpha.1", versionComparable.CompareVersion);
        Assert.AreEqual("2.0.0-beta.2+build.1", versionSemantic.FullVersion);
        Assert.AreEqual("v3.1.0", versionTag.Tag);
        Assert.AreEqual("1.0.0", versionInterval.LowerBound!);
        Assert.AreEqual("abc123def456", versionCommitTag.CommitHash);
    }

    /// <summary>
    ///     Test that version comparison operations work correctly across different version types.
    ///     This validates semantic versioning compliance requirement.
    /// </summary>
    [TestMethod]
    public void Version_Subsystem_SemanticVersioningCompliance_WorksCorrectly()
    {
        // Arrange - Create versions that test SemVer compliance
        var version1 = VersionComparable.Create("1.0.0-alpha");
        var version2 = VersionComparable.Create("1.0.0-alpha.1");
        var version3 = VersionComparable.Create("1.0.0-alpha.beta");
        var version4 = VersionComparable.Create("1.0.0-beta");
        var version5 = VersionComparable.Create("1.0.0-beta.2");
        var version6 = VersionComparable.Create("1.0.0");

        // Assert - SemVer precedence is maintained
        Assert.IsLessThan(0, version1!.CompareTo(version2), "1.0.0-alpha < 1.0.0-alpha.1");
        Assert.IsLessThan(0, version2!.CompareTo(version3), "1.0.0-alpha.1 < 1.0.0-alpha.beta");
        Assert.IsLessThan(0, version3!.CompareTo(version4), "1.0.0-alpha.beta < 1.0.0-beta");
        Assert.IsLessThan(0, version4!.CompareTo(version5), "1.0.0-beta < 1.0.0-beta.2");
        Assert.IsLessThan(0, version5!.CompareTo(version6), "1.0.0-beta.2 < 1.0.0");
    }

    /// <summary>
    ///     Test that version tags can extract comparable versions for repository operations.
    ///     This validates tag processing integration for BuildNotes functionality.
    /// </summary>
    [TestMethod]
    public void Version_Subsystem_TagToComparableIntegration_WorksCorrectly()
    {
        // Arrange - Create version tags with various formats
        var tag1 = VersionTag.Create("v1.2.3");
        var tag2 = VersionTag.Create("VER2.0.0-rc.1");
        var tag3 = VersionTag.Create("release-1.5.0");

        // Act - Get comparable versions
        var comparable1 = tag1!.Semantic.Comparable;
        var comparable2 = tag2!.Semantic.Comparable;
        var comparable3 = tag3!.Semantic.Comparable;

        // Assert - Comparable versions are extracted correctly
        Assert.IsNotNull(comparable1);
        Assert.IsNotNull(comparable2);
        Assert.IsNotNull(comparable3);

        Assert.AreEqual("1.2.3", comparable1.CompareVersion);
        Assert.AreEqual("2.0.0-rc.1", comparable2.CompareVersion);
        Assert.AreEqual("1.5.0", comparable3.CompareVersion);

        // Assert - Version ordering works as expected
        Assert.IsLessThan(0, comparable1.CompareTo(comparable3)); // 1.2.3 < 1.5.0
        Assert.IsLessThan(0, comparable3.CompareTo(comparable2)); // 1.5.0 < 2.0.0-rc.1
    }
}
