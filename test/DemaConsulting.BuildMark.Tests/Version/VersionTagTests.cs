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
[TestClass]
public class VersionTagTests
{
    /// <summary>
    ///     Test that VersionTag parses simple v-prefix version.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_SimpleVPrefix_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.AreEqual("v1.2.3", versionTag.Tag);
        Assert.AreEqual("1.2.3", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("", versionTag.PreRelease);
        Assert.AreEqual("1.2.3", versionTag.CompareVersion);
        Assert.AreEqual("", versionTag.Metadata);
        Assert.IsFalse(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses complex version with prefix, pre-release, and metadata.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_ComplexVersionWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Rel_1.2.3.rc.4+build.5");

        // Assert
        Assert.AreEqual("Rel_1.2.3.rc.4+build.5", versionTag.Tag);
        Assert.AreEqual("1.2.3-rc.4+build.5", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("rc.4", versionTag.PreRelease);
        Assert.AreEqual("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.AreEqual("build.5", versionTag.Metadata);
        Assert.IsTrue(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid tag.
    /// </summary>
    [TestMethod]
    public void VersionTag_TryCreate_InvalidTag_ReturnsNull()
    {
        // Act
        var versionTag = VersionTag.TryCreate("not-a-version");

        // Assert
        Assert.IsNull(versionTag);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid tag.
    /// </summary>
    [TestMethod]
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
    [TestMethod]
    public void VersionTag_Create_NoPrefix_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("1.0.0");

        // Assert
        Assert.AreEqual("1.0.0", versionTag.Tag);
        Assert.AreEqual("1.0.0", versionTag.FullVersion);
        Assert.IsFalse(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag correctly parses hyphen-separated pre-release.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_HyphenPreReleaseWithMetadata_ParsesVersion()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Rel_1.2.3-rc.4+build.5");

        // Assert
        Assert.AreEqual("Rel_1.2.3-rc.4+build.5", versionTag.Tag);
        Assert.AreEqual("1.2.3-rc.4+build.5", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("rc.4", versionTag.PreRelease);
        Assert.AreEqual("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.AreEqual("build.5", versionTag.Metadata);
        Assert.IsTrue(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag provides access to comparable version for sorting.
    /// </summary>
    [TestMethod]
    public void VersionTag_Semantic_AllowsComparison()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.2.3");
        var tag2 = VersionTag.Create("v1.11.2");

        // Act & Assert
        Assert.IsTrue(tag1.Semantic.Comparable < tag2.Semantic.Comparable);
    }
}


