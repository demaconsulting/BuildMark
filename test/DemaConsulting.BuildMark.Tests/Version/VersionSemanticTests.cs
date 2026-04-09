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
[TestClass]
public class VersionSemanticTests
{
    /// <summary>
    ///     Test that VersionSemantic parses simple version without metadata.
    /// </summary>
    [TestMethod]
    public void VersionSemantic_Create_SimpleVersion_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3");

        // Assert
        Assert.AreEqual(1, version.Major);
        Assert.AreEqual(2, version.Minor);
        Assert.AreEqual(3, version.Patch);
        Assert.AreEqual("1.2.3", version.Numbers);
        Assert.AreEqual("", version.PreRelease);
        Assert.IsNull(version.Metadata);
        Assert.AreEqual("1.2.3", version.CompareVersion);
        Assert.AreEqual("1.2.3", version.FullVersion);
        Assert.IsFalse(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionSemantic parses version with metadata.
    /// </summary>
    [TestMethod]
    public void VersionSemantic_Create_VersionWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("1.2.3+build.5");

        // Assert
        Assert.AreEqual("1.2.3", version.Numbers);
        Assert.AreEqual("", version.PreRelease);
        Assert.AreEqual("build.5", version.Metadata);
        Assert.AreEqual("1.2.3", version.CompareVersion);
        Assert.AreEqual("1.2.3+build.5", version.FullVersion);
        Assert.IsFalse(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionSemantic parses pre-release with metadata.
    /// </summary>
    [TestMethod]
    public void VersionSemantic_Create_PreReleaseWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionSemantic.Create("2.0.0-alpha.1+linux.x64");

        // Assert
        Assert.AreEqual("2.0.0", version.Numbers);
        Assert.AreEqual("alpha.1", version.PreRelease);
        Assert.AreEqual("linux.x64", version.Metadata);
        Assert.AreEqual("2.0.0-alpha.1", version.CompareVersion);
        Assert.AreEqual("2.0.0-alpha.1+linux.x64", version.FullVersion);
        Assert.IsTrue(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid version.
    /// </summary>
    [TestMethod]
    public void VersionSemantic_TryCreate_InvalidVersion_ReturnsNull()
    {
        // Act
        var version = VersionSemantic.TryCreate("not-a-version");

        // Assert
        Assert.IsNull(version);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid version.
    /// </summary>
    [TestMethod]
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
    [TestMethod]
    public void VersionSemantic_Comparable_AllowsComparison()
    {
        // Arrange
        var version1 = VersionSemantic.Create("1.2.3+build1");
        var version2 = VersionSemantic.Create("1.2.4+build2");

        // Act & Assert
        Assert.IsTrue(version1.Comparable < version2.Comparable);
    }
}


