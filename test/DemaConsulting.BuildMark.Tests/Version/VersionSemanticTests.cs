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
///     Tests for the VersionSemantic class.
/// </summary>
public class VersionSemanticTests
{
    /// <summary>
    ///     Test that VersionSemantic creates instance with build metadata.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_WithBuildMetadata_ReturnsInstance()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3+build.123");

        // Assert
        Assert.NotNull(version);
        Assert.Equal("build.123", version.Metadata);
        Assert.Equal("1.2.3+build.123", version.FullVersion);
    }

    /// <summary>
    ///     Test that VersionSemantic creates instance without build metadata.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_WithoutBuildMetadata_ReturnsInstance()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3");

        // Assert
        Assert.NotNull(version);
        Assert.Null(version.Metadata);
        Assert.Equal("1.2.3", version.FullVersion);
    }

    /// <summary>
    ///     Test that VersionSemantic properties delegate to comparable correctly.
    /// </summary>
    [Fact]
    public void VersionSemantic_Properties_DelegateToComparable_Correctly()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3-alpha");

        // Assert
        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal("alpha", version.PreRelease);
        Assert.Equal("1.2.3-alpha", version.CompareVersion);
    }

    /// <summary>
    ///     Test that VersionSemantic formats completely with all components.
    /// </summary>
    [Fact]
    public void VersionSemantic_ToString_FormatsCompletely_WithAllComponents()
    {
        // Arrange
        var version = VersionSemantic.Create("1.2.3-alpha+build.123");

        // Act
        var result = version.FullVersion;

        // Assert
        Assert.Equal("1.2.3-alpha+build.123", result);
    }

    /// <summary>
    ///     Test that VersionSemantic PreRelease returns empty string for release versions.
    /// </summary>
    [Fact]
    public void VersionSemantic_PreRelease_ReturnsEmptyStringForRelease()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3");

        // Assert
        Assert.Equal("", version.PreRelease);
        Assert.False(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionSemantic parses valid semantic versions correctly.
    /// </summary>
    [Fact]
    public void VersionSemantic_Parse_ValidSemanticVersions_ParsesCorrectly()
    {
        // Arrange & Act
        var simple = VersionSemantic.Create("1.0.0");
        var preRelease = VersionSemantic.Create("1.0.0-alpha.1");
        var withMetadata = VersionSemantic.Create("1.0.0+20130313144700");
        var complex = VersionSemantic.Create("1.0.0-beta.2+exp.sha.5114f85");

        // Assert
        Assert.Equal("1.0.0", simple.FullVersion);
        Assert.Equal("1.0.0-alpha.1", preRelease.FullVersion);
        Assert.Equal("1.0.0+20130313144700", withMetadata.FullVersion);
        Assert.Equal("1.0.0-beta.2+exp.sha.5114f85", complex.FullVersion);
    }

    /// <summary>
    ///     Test that VersionSemantic parses simple version without metadata.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_SimpleVersion_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3");

        // Assert
        Assert.Equal(1, version.Major);
        Assert.Equal(2, version.Minor);
        Assert.Equal(3, version.Patch);
        Assert.Equal("1.2.3", version.Numbers);
        Assert.Equal("", version.PreRelease);
        Assert.Null(version.Metadata);
        Assert.Equal("1.2.3", version.CompareVersion);
        Assert.Equal("1.2.3", version.FullVersion);
        Assert.False(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionSemantic parses version with metadata.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_VersionWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3+build.5");

        // Assert
        Assert.Equal("1.2.3", version.Numbers);
        Assert.Equal("", version.PreRelease);
        Assert.Equal("build.5", version.Metadata);
        Assert.Equal("1.2.3", version.CompareVersion);
        Assert.Equal("1.2.3+build.5", version.FullVersion);
        Assert.False(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionSemantic parses pre-release with metadata.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("2.0.0-alpha.1+linux.x64");

        // Assert
        Assert.Equal("2.0.0", version.Numbers);
        Assert.Equal("alpha.1", version.PreRelease);
        Assert.Equal("linux.x64", version.Metadata);
        Assert.Equal("2.0.0-alpha.1", version.CompareVersion);
        Assert.Equal("2.0.0-alpha.1+linux.x64", version.FullVersion);
        Assert.True(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid version.
    /// </summary>
    [Fact]
    public void VersionSemantic_TryCreate_InvalidVersion_ReturnsNull()
    {
        // Act
        var version = VersionSemantic.TryCreate("not-a-version");

        // Assert
        Assert.Null(version);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid version.
    /// </summary>
    [Fact]
    public void VersionSemantic_Create_InvalidVersion_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(() => VersionSemantic.Create("not-a-version"));

        // Assert
        Assert.Contains("does not match semantic version format", exception.Message);
    }

    /// <summary>
    ///     Test that VersionSemantic correctly exposes comparable for comparison.
    /// </summary>
    [Fact]
    public void VersionSemantic_Comparable_AllowsComparison()
    {
        // Arrange
        var version1 = VersionSemantic.Create("1.2.3+build1");
        var version2 = VersionSemantic.Create("1.2.4+build2");

        // Act & Assert
        Assert.True(version1.Comparable < version2.Comparable);
    }
}


