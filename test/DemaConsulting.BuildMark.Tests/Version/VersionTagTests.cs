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
    ///     Test that VersionTag creates instances from valid tags.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_ValidTag_ReturnsVersionTag()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.IsNotNull(versionTag);
        Assert.AreEqual("v1.2.3", versionTag.Tag);
        Assert.AreEqual("1.2.3", versionTag.FullVersion);
    }

    /// <summary>
    ///     Test that VersionTag parses standard tags correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_StandardTag_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("1.2.3");

        // Assert
        Assert.AreEqual("1.2.3", versionTag.Tag);
        Assert.AreEqual("1.2.3", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
    }

    /// <summary>
    ///     Test that VersionTag parses prefixed tags correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_PrefixedTag_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3");

        // Assert
        Assert.AreEqual("v1.2.3", versionTag.Tag);
        Assert.AreEqual("1.2.3", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
    }

    /// <summary>
    ///     Test that VersionTag normalizes dot-separated pre-release to hyphen.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_DotSeparatedPreRelease_NormalizesToHyphen()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3.alpha.1");

        // Assert
        Assert.AreEqual("v1.2.3.alpha.1", versionTag.Tag);
        Assert.AreEqual("1.2.3-alpha.1", versionTag.FullVersion);
        Assert.AreEqual("alpha.1", versionTag.PreRelease);
    }

    /// <summary>
    ///     Test that VersionTag extracts version from complex tags correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_ComplexTag_ExtractsVersionCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("Release_1.2.3.beta.5+build.123");

        // Assert
        Assert.AreEqual("Release_1.2.3.beta.5+build.123", versionTag.Tag);
        Assert.AreEqual("1.2.3-beta.5+build.123", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("beta.5", versionTag.PreRelease);
        Assert.AreEqual("build.123", versionTag.Metadata);
    }

    /// <summary>
    ///     Test that VersionTag properties expose original and parsed versions correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Properties_ExposeOriginalAndParsed_Correctly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("v1.2.3-alpha");

        // Assert
        Assert.AreEqual("v1.2.3-alpha", versionTag.Tag); // Original
        Assert.AreEqual("1.2.3-alpha", versionTag.FullVersion); // Parsed
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("alpha", versionTag.PreRelease);
    }

    /// <summary>
    ///     Test that VersionTag ToString returns original tag.
    /// </summary>
    [TestMethod]
    public void VersionTag_ToString_ReturnsOriginalTag()
    {
        // Arrange
        var originalTag = "v1.2.3-alpha+build.123";
        var versionTag = VersionTag.Create(originalTag);

        // Act
        var result = versionTag.Tag;

        // Assert
        Assert.AreEqual(originalTag, result);
    }

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

    /// <summary>
    ///     Test that VersionComparable equals works with different prefixes but same version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_Equals_DifferentPrefixesSameVersion_ReturnsTrue()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.2.3");
        var tag2 = VersionTag.Create("VER1.2.3");
        var tag3 = VersionTag.Create("Release_1.2.3");
        var tag4 = VersionTag.Create("release/1.2.3");

        // Act & Assert
        Assert.AreEqual(tag1.Semantic.Comparable, tag2.Semantic.Comparable);
        Assert.AreEqual(tag1.Semantic.Comparable, tag3.Semantic.Comparable);
        Assert.AreEqual(tag1.Semantic.Comparable, tag4.Semantic.Comparable);
        Assert.AreEqual(tag2.Semantic.Comparable, tag3.Semantic.Comparable);
    }

    /// <summary>
    ///     Test that VersionTag GetVersionComparable works for semantic tag comparison.
    /// </summary>
    [TestMethod]
    public void VersionTag_GetVersionComparable_SemanticTags_ReturnsCorrectComparison()
    {
        // Arrange
        var tag1 = VersionTag.Create("v1.0.0-alpha");
        var tag2 = VersionTag.Create("v1.0.0-beta");
        var tag3 = VersionTag.Create("v1.0.0");

        // Act & Assert - Test semantic version comparison rules
        Assert.IsTrue(tag1.Semantic.Comparable < tag2.Semantic.Comparable, "alpha < beta");
        Assert.IsTrue(tag2.Semantic.Comparable < tag3.Semantic.Comparable, "beta < release");
        Assert.IsTrue(tag1.Semantic.Comparable < tag3.Semantic.Comparable, "alpha < release");
    }

    /// <summary>
    ///     Test that VersionTag parses tags with path-separator prefix correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_PathSeparatorPrefix_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("release/1.2.3");

        // Assert
        Assert.AreEqual("release/1.2.3", versionTag.Tag);
        Assert.AreEqual("1.2.3", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("", versionTag.PreRelease);
        Assert.IsFalse(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses tags with path-separator prefix and pre-release correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_PathSeparatorPrefixWithPreRelease_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("release/1.2.3-rc.4");

        // Assert
        Assert.AreEqual("release/1.2.3-rc.4", versionTag.Tag);
        Assert.AreEqual("1.2.3-rc.4", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("rc.4", versionTag.PreRelease);
        Assert.AreEqual("1.2.3-rc.4", versionTag.CompareVersion);
        Assert.IsTrue(versionTag.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionTag parses tags with multi-level path-separator prefix correctly.
    /// </summary>
    [TestMethod]
    public void VersionTag_Create_MultiLevelPathPrefix_ParsesCorrectly()
    {
        // Arrange & Act
        var versionTag = VersionTag.Create("builds/release/1.2.3-beta.1+build.99");

        // Assert
        Assert.AreEqual("builds/release/1.2.3-beta.1+build.99", versionTag.Tag);
        Assert.AreEqual("1.2.3-beta.1+build.99", versionTag.FullVersion);
        Assert.AreEqual("1.2.3", versionTag.Numbers);
        Assert.AreEqual("beta.1", versionTag.PreRelease);
        Assert.AreEqual("build.99", versionTag.Metadata);
        Assert.IsTrue(versionTag.IsPreRelease);
    }
}


