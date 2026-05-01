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

namespace DemaConsulting.BuildMark.Tests.Version;

/// <summary>
///     Version subsystem integration tests.
///     Tests the overall behavior of the Version subsystem components working together.
/// </summary>
public class VersionTests
{
    /// <summary>
    ///     Test that VersionComparable can be created from valid versions.
    /// </summary>
    [Fact]
    public void VersionComparable_Create_ValidVersions_ReturnsVersionComparable()
    {
        // Arrange & Act
        var simple = VersionComparable.Create("1.2.3");
        var preRelease = VersionComparable.Create("2.0.0-alpha.1");
        var complex = VersionComparable.Create("10.5.99-beta.10");

        // Assert
        Assert.NotNull(simple);
        Assert.NotNull(preRelease);
        Assert.NotNull(complex);
        Assert.IsAssignableFrom<VersionComparable>(simple);
        Assert.IsAssignableFrom<VersionComparable>(preRelease);
        Assert.IsAssignableFrom<VersionComparable>(complex);
    }

    /// <summary>
    ///     Test that VersionSemantic can be created from valid semantic versions.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_ValidSemanticVersion_ReturnsVersionSemantic()
    {
        // Arrange & Act
        var simple = VersionSemantic.Create("1.2.3");
        var withMetadata = VersionSemantic.Create("2.0.0+build.123");
        var complex = VersionSemantic.Create("1.0.0-alpha.beta.2+exp.sha.5114f85");

        // Assert
        Assert.NotNull(simple);
        Assert.NotNull(withMetadata);
        Assert.NotNull(complex);
        Assert.IsAssignableFrom<VersionSemantic>(simple);
        Assert.IsAssignableFrom<VersionSemantic>(withMetadata);
        Assert.IsAssignableFrom<VersionSemantic>(complex);
    }

    /// <summary>
    ///     Test that VersionTag can be created from valid tags.
    /// </summary>
    [Fact]
    public void VersionTag_Create_ValidTag_ReturnsVersionTag()
    {
        // Arrange & Act
        var simple = VersionTag.Create("1.2.3");
        var prefixed = VersionTag.Create("v2.0.0");
        var complex = VersionTag.Create("release-1.5.0-rc.1");

        // Assert
        Assert.NotNull(simple);
        Assert.NotNull(prefixed);
        Assert.NotNull(complex);
        Assert.IsAssignableFrom<VersionTag>(simple);
        Assert.IsAssignableFrom<VersionTag>(prefixed);
        Assert.IsAssignableFrom<VersionTag>(complex);
    }

    /// <summary>
    ///     Test that VersionInterval can be created from valid interval parameters.
    /// </summary>
    [Fact]
    public void VersionInterval_Create_ValidInterval_ReturnsVersionInterval()
    {
        // Arrange & Act
        var inclusive = new VersionInterval("1.0.0", true, "2.0.0", true);
        var exclusive = new VersionInterval("1.0.0", false, "2.0.0", false);
        var mixed = new VersionInterval("1.0.0", true, "2.0.0", false);

        // Assert
        Assert.NotNull(inclusive);
        Assert.NotNull(exclusive);
        Assert.NotNull(mixed);
        Assert.IsAssignableFrom<VersionInterval>(inclusive);
        Assert.IsAssignableFrom<VersionInterval>(exclusive);
        Assert.IsAssignableFrom<VersionInterval>(mixed);
    }

    /// <summary>
    ///     Test that VersionCommitTag can be created from valid parameters.
    /// </summary>
    [Fact]
    public void VersionCommitTag_Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var versionTag = VersionTag.Create("v1.2.3");
        var commitHash = "abc123def456789";

        // Act
        var versionCommitTag = new VersionCommitTag(versionTag!, commitHash);

        // Assert
        Assert.NotNull(versionCommitTag);
        Assert.IsAssignableFrom<VersionCommitTag>(versionCommitTag);
        Assert.Equal(versionTag, versionCommitTag.VersionTag);
        Assert.Equal(commitHash, versionCommitTag.CommitHash);
    }

    /// <summary>
    ///     Test that the Version subsystem can create and use all version types correctly.
    ///     This validates the subsystem-level requirement for version processing capabilities.
    /// </summary>
    [Fact]
    public void Version_Subsystem_CreateAllVersionTypes_WorksCorrectly()
    {
        // Arrange - Create instances of all version types
        var versionComparable = VersionComparable.Create("1.2.3-alpha.1");
        var versionSemantic = VersionSemantic.Create("2.0.0-beta.2+build.1");
        var versionTag = VersionTag.Create("v3.1.0");
        var versionInterval = new VersionInterval("1.0.0", true, "2.0.0", false);
        var versionCommitTag = new VersionCommitTag(versionTag!, "abc123def456");

        // Assert - All types are created successfully
        Assert.NotNull(versionComparable);
        Assert.NotNull(versionSemantic);
        Assert.NotNull(versionTag);
        Assert.NotNull(versionInterval);
        Assert.NotNull(versionCommitTag);

        // Assert - Version properties are accessible and correct
        Assert.Equal("1.2.3-alpha.1", versionComparable.CompareVersion);
        Assert.Equal("2.0.0-beta.2+build.1", versionSemantic.FullVersion);
        Assert.Equal("v3.1.0", versionTag.Tag);
        Assert.Equal("1.0.0", versionInterval.LowerBound!);
        Assert.Equal("abc123def456", versionCommitTag.CommitHash);
    }

    /// <summary>
    ///     Test that version comparison operations work correctly across different version types.
    ///     This validates semantic versioning compliance requirement.
    /// </summary>
    [Fact]
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
        Assert.True(version1!.CompareTo(version2) < 0, "1.0.0-alpha < 1.0.0-alpha.1");
        Assert.True(version2!.CompareTo(version3) < 0, "1.0.0-alpha.1 < 1.0.0-alpha.beta");
        Assert.True(version3!.CompareTo(version4) < 0, "1.0.0-alpha.beta < 1.0.0-beta");
        Assert.True(version4!.CompareTo(version5) < 0, "1.0.0-beta < 1.0.0-beta.2");
        Assert.True(version5!.CompareTo(version6) < 0, "1.0.0-beta.2 < 1.0.0");
    }

    /// <summary>
    ///     Test that version tags can extract comparable versions for repository operations.
    ///     This validates tag processing integration for BuildNotes functionality.
    /// </summary>
    [Fact]
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
        Assert.NotNull(comparable1);
        Assert.NotNull(comparable2);
        Assert.NotNull(comparable3);

        Assert.Equal("1.2.3", comparable1.CompareVersion);
        Assert.Equal("2.0.0-rc.1", comparable2.CompareVersion);
        Assert.Equal("1.5.0", comparable3.CompareVersion);

        // Assert - Version ordering works as expected
        Assert.True(comparable1.CompareTo(comparable3) < 0); // 1.2.3 < 1.5.0
        Assert.True(comparable3.CompareTo(comparable2) < 0); // 1.5.0 < 2.0.0-rc.1
    }
}
