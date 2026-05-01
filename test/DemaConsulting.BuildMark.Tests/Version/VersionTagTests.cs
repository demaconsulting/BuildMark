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
///     Tests for the VersionTag class.
/// </summary>
public class VersionTagTests
{
    /// <summary>
    ///     Test that VersionTag creates instances from valid tags.
    /// </summary>
    [Fact]
    public void VersionTag_Create_ValidTag_ReturnsVersionTag()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.NotNull(versionTag);
        Assert.Equal("v1.2.3", versionTag.Tag);
        Assert.Equal("1.2.3", versionTag.FullVersion);
    }

    /// <summary>
    ///     Test that VersionTag parses standard tags correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_StandardTag_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("1.2.3");

        // Assert
        Assert.Equal("1.2.3", versionTag.Tag);
        Assert.Equal("1.2.3", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
    }

    /// <summary>
    ///     Test that VersionTag parses prefixed tags correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_PrefixedTag_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.Equal("v1.2.3", versionTag.Tag);
        Assert.Equal("1.2.3", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
    }

    /// <summary>
    ///     Test that VersionTag normalizes dot-separated pre-release to hyphen.
    /// </summary>
    [Fact]
    public void VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3.alpha.1");

        // Assert
        Assert.Equal("v1.2.3.alpha.1", versionTag.Tag);
        Assert.Equal("1.2.3-alpha.1", versionTag.FullVersion);
        Assert.Equal("alpha.1", versionTag.PreRelease);
    }

    /// <summary>
    ///     Test that VersionTag extracts version from complex tags correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_ComplexTag_ExtractsVersionCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Release_1.2.3.beta.5+build.123");

        // Assert
        Assert.Equal("Release_1.2.3.beta.5+build.123", versionTag.Tag);
        Assert.Equal("1.2.3-beta.5+build.123", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("beta.5", versionTag.PreRelease);
        Assert.Equal("build.123", versionTag.Metadata);
    }

    /// <summary>
    ///     Test that VersionTag properties expose original and parsed versions correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Properties_ExposeOriginalAndParsed_Correctly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3-alpha");

        // Assert
        Assert.Equal("v1.2.3-alpha", versionTag.Tag); // Original
        Assert.Equal("1.2.3-alpha", versionTag.FullVersion); // Parsed
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("alpha", versionTag.PreRelease);
    }

    /// <summary>
    ///     Test that VersionTag ToString returns original tag.
    /// </summary>
    [Fact]
    public void VersionTag_ToString_ReturnsOriginalTag()
    {
        // Arrange
        var originalTag = "v1.2.3-alpha+build.123";
        var versionTag = VersionTag.Create(originalTag);

        // Act
        var result = versionTag.ToString();

        // Assert
        Assert.Equal(originalTag, result);
    }

    /// <summary>
    ///     Test that VersionTag parses simple v-prefix version.
    /// </summary>
    [Fact]
    public void VersionTag_Create_SimpleVPrefix_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.Equal("v1.2.3", versionTag.Tag);
        Assert.Equal("1.2.3", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("", versionTag.PreRelease);
        Assert.Equal("1.2.3", versionTag.CompareVersion);
        Assert.Equal("", versionTag.Metadata);
        Assert.False(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses complex version with prefix, pre-release, and metadata.
    /// </summary>
    [Fact]
    public void VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Rel_1.2.3.rc.4+build.5");

        // Assert
        Assert.Equal("Rel_1.2.3.rc.4+build.5", versionTag.Tag);
        Assert.Equal("1.2.3-rc.4+build.5", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("rc.4", versionTag.PreRelease);
        Assert.Equal("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.Equal("build.5", versionTag.Metadata);
        Assert.True(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid tag.
    /// </summary>
    [Fact]
    public void VersionTag_TryCreate_InvalidTag_ReturnsNull()
    {
        // Act
        var versionTag = VersionTag.TryCreate("not-a-version");

        // Assert
        Assert.Null(versionTag);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid tag.
    /// </summary>
    [Fact]
    public void VersionTag_Create_InvalidTag_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(() => VersionTag.Create("not-a-version"));

        // Assert
        Assert.Contains("does not match version format", exception.Message);
    }

    /// <summary>
    ///     Test that VersionTag handles no prefix.
    /// </summary>
    [Fact]
    public void VersionTag_Create_NoPrefix_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("1.0.0");

        // Assert
        Assert.Equal("1.0.0", versionTag.Tag);
        Assert.Equal("1.0.0", versionTag.FullVersion);
        Assert.False(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag correctly parses hyphen-separated pre-release.
    /// </summary>
    [Fact]
    public void VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Rel_1.2.3-rc.4+build.5");

        // Assert
        Assert.Equal("Rel_1.2.3-rc.4+build.5", versionTag.Tag);
        Assert.Equal("1.2.3-rc.4+build.5", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("rc.4", versionTag.PreRelease);
        Assert.Equal("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.Equal("build.5", versionTag.Metadata);
        Assert.True(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag provides access to comparable version for sorting.
    /// </summary>
    [Fact]
    public void VersionTag_Semantic_AllowsComparison()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.2.3");
        var tag2 = VersionTag.Create("v1.11.2");

        // Act & Assert
        Assert.True(tag1.Semantic.Comparable < tag2.Semantic.Comparable);
    }

    /// <summary>
    ///     Test that VersionComparable equals works with different prefixes but same version.
    /// </summary>
    [Fact]
    public void VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.2.3");
        var tag2 = VersionTag.Create("VER1.2.3");
        var tag3 = VersionTag.Create("Release_1.2.3");
        var tag4 = VersionTag.Create("release/1.2.3");

        // Act & Assert
        Assert.Equal(tag1.Semantic.Comparable, tag2.Semantic.Comparable);
        Assert.Equal(tag1.Semantic.Comparable, tag3.Semantic.Comparable);
        Assert.Equal(tag1.Semantic.Comparable, tag4.Semantic.Comparable);
        Assert.Equal(tag2.Semantic.Comparable, tag3.Semantic.Comparable);
    }

    /// <summary>
    ///     Test that VersionTag GetVersionComparable works for semantic tag comparison.
    /// </summary>
    [Fact]
    public void VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.0.0-alpha");
        var tag2 = VersionTag.Create("v1.0.0-beta");
        var tag3 = VersionTag.Create("v1.0.0");

        // Act & Assert - Test semantic version comparison rules
        Assert.True(tag1.Semantic.Comparable < tag2.Semantic.Comparable, "alpha < beta");
        Assert.True(tag2.Semantic.Comparable < tag3.Semantic.Comparable, "beta < release");
        Assert.True(tag1.Semantic.Comparable < tag3.Semantic.Comparable, "alpha < release");
    }

    /// <summary>
    ///     Test that VersionTag parses tags with path-separator prefix correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("release/1.2.3");

        // Assert
        Assert.Equal("release/1.2.3", versionTag.Tag);
        Assert.Equal("1.2.3", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("", versionTag.PreRelease);
        Assert.False(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses tags with path-separator prefix and pre-release correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("release/1.2.3-rc.4");

        // Assert
        Assert.Equal("release/1.2.3-rc.4", versionTag.Tag);
        Assert.Equal("1.2.3-rc.4", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("rc.4", versionTag.PreRelease);
        Assert.Equal("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.True(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses tags with multi-level path-separator prefix correctly.
    /// </summary>
    [Fact]
    public void VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("builds/release/1.2.3-beta.1+build.99");

        // Assert
        Assert.Equal("builds/release/1.2.3-beta.1+build.99", versionTag.Tag);
        Assert.Equal("1.2.3-beta.1+build.99", versionTag.FullVersion);
        Assert.Equal("1.2.3", versionTag.Numbers);
        Assert.Equal("beta.1", versionTag.PreRelease);
        Assert.Equal("build.99", versionTag.Metadata);
        Assert.True(versionTag.IsPreRelease);
    }
}


