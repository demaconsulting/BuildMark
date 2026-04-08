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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the Version class.
/// </summary>
[TestClass]
public class TagInfoTests
{
    /// <summary>
    ///     Test that Version parses simple v-prefix version.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesSimpleVPrefix()
    {
        // Parse a simple version tag with v prefix
        var tagVersion = Version.Create("v1.2.3");

        // Verify tag and version are extracted correctly
        Assert.AreEqual("v1.2.3", tagVersion.Tag);
        Assert.AreEqual("1.2.3", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version parses ver-prefix version.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesVerPrefix()
    {
        // Arrange & Act
        var tagVersion = Version.Create("ver-2.0.0");

        // Assert
        Assert.AreEqual("ver-2.0.0", tagVersion.Tag);
        Assert.AreEqual("2.0.0", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version parses release_prefix version.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesReleaseUnderscorePrefix()
    {
        // Arrange & Act
        var tagVersion = Version.Create("release_3.1.4");

        // Assert
        Assert.AreEqual("release_3.1.4", tagVersion.Tag);
        Assert.AreEqual("3.1.4", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version detects alpha pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_DetectsAlphaPreRelease()
    {
        // Parse a version tag with alpha pre-release
        var tagVersion = Version.Create("v2.0.0-alpha.1");

        // Verify pre-release is detected correctly
        Assert.AreEqual("v2.0.0-alpha.1", tagVersion.Tag);
        Assert.AreEqual("2.0.0-alpha.1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version detects beta pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_DetectsBetaPreRelease()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v2.0.0-beta.2");

        // Assert
        Assert.AreEqual("v2.0.0-beta.2", tagVersion.Tag);
        Assert.AreEqual("2.0.0-beta.2", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version detects rc pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_DetectsRcPreRelease()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v2.0.0-rc.1");

        // Assert
        Assert.AreEqual("v2.0.0-rc.1", tagVersion.Tag);
        Assert.AreEqual("2.0.0-rc.1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version detects pre pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_DetectsPrePreRelease()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v2.0.0-pre");

        // Assert
        Assert.AreEqual("v2.0.0-pre", tagVersion.Tag);
        Assert.AreEqual("2.0.0-pre", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version handles no prefix.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_HandlesNoPrefix()
    {
        // Arrange & Act
        var tagVersion = Version.Create("1.0.0");

        // Assert
        Assert.AreEqual("1.0.0", tagVersion.Tag);
        Assert.AreEqual("1.0.0", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version handles complex prefix.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_HandlesComplexPrefix()
    {
        // Arrange & Act
        var tagVersion = Version.Create("my-awesome-release_1.2.3-beta");

        // Assert
        Assert.AreEqual("my-awesome-release_1.2.3-beta", tagVersion.Tag);
        Assert.AreEqual("1.2.3-beta", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version correctly parses build metadata with plus separator.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesBuildMetadata()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v1.0.0+arch");

        // Assert
        Assert.AreEqual("v1.0.0+arch", tagVersion.Tag);
        Assert.AreEqual("1.0.0+arch", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version accepts dot-separated pre-release and treats it as pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_AcceptsDotSeparatedAsPreRelease()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v1.0.0.arch");

        // Assert
        Assert.AreEqual("v1.0.0.arch", tagVersion.Tag);
        Assert.AreEqual("1.0.0.arch", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version correctly parses complex version with prefix, pre-release, and metadata.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesComplexVersionWithMetadata()
    {
        // Arrange & Act
        var tagVersion = Version.Create("Rel_1.2.3.rc.4+build.5");

        // Assert
        Assert.AreEqual("Rel_1.2.3.rc.4+build.5", tagVersion.Tag);
        Assert.AreEqual("1.2.3.rc.4+build.5", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version correctly parses complex version with hyphen separator for pre-release.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesComplexVersionWithHyphenPreRelease()
    {
        // Arrange & Act
        var tagVersion = Version.Create("Rel_1.2.3-rc.4+build.5");

        // Assert
        Assert.AreEqual("Rel_1.2.3-rc.4+build.5", tagVersion.Tag);
        Assert.AreEqual("1.2.3-rc.4+build.5", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version correctly detects rc with number suffix.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_CorrectlyDetectsRcWithNumber()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v1.0.0-rc1");

        // Assert
        Assert.AreEqual("v1.0.0-rc1", tagVersion.Tag);
        Assert.AreEqual("1.0.0-rc1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that Version correctly parses pre-release with build metadata.
    /// </summary>
    [TestMethod]
    public void TagInfo_Constructor_ParsesPreReleaseWithBuildMetadata()
    {
        // Arrange & Act
        var tagVersion = Version.Create("v2.0.0-beta.1+linux.x64");

        // Assert
        Assert.AreEqual("v2.0.0-beta.1+linux.x64", tagVersion.Tag);
        Assert.AreEqual("2.0.0-beta.1+linux.x64", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid tag.
    /// </summary>
    [TestMethod]
    public void Version_TryCreate_ReturnsNullForInvalidTag()
    {
        // Attempt to parse an invalid tag
        var tagVersion = Version.TryCreate("not-a-version");

        // Verify null is returned for invalid format
        Assert.IsNull(tagVersion);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid tag.
    /// </summary>
    [TestMethod]
    public void Version_Create_ThrowsForInvalidTag()
    {
        try
        {
            // Attempt to create version from invalid tag
            _ = Version.Create("not-a-version");

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception for invalid tag format
        }
    }
}
