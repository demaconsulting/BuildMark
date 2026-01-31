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
///     Tests for the TagVersion class.
/// </summary>
[TestClass]
public class TagVersionTests
{
    /// <summary>
    ///     Test that TagVersion parses simple v-prefix version.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_ParsesSimpleVPrefix()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v1.2.3");

        // Assert
        Assert.AreEqual("v1.2.3", tagVersion.OriginalTag);
        Assert.AreEqual("1.2.3", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion parses ver-prefix version.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_ParsesVerPrefix()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("ver-2.0.0");

        // Assert
        Assert.AreEqual("ver-2.0.0", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion parses release_prefix version.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_ParsesReleaseUnderscorePrefix()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("release_3.1.4");

        // Assert
        Assert.AreEqual("release_3.1.4", tagVersion.OriginalTag);
        Assert.AreEqual("3.1.4", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion detects alpha pre-release.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DetectsAlphaPreRelease()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v2.0.0-alpha.1");

        // Assert
        Assert.AreEqual("v2.0.0-alpha.1", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0-alpha.1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion detects beta pre-release.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DetectsBetaPreRelease()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v2.0.0-beta.2");

        // Assert
        Assert.AreEqual("v2.0.0-beta.2", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0-beta.2", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion detects rc pre-release.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DetectsRcPreRelease()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v2.0.0-rc.1");

        // Assert
        Assert.AreEqual("v2.0.0-rc.1", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0-rc.1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion detects pre pre-release.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DetectsPrePreRelease()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v2.0.0-pre");

        // Assert
        Assert.AreEqual("v2.0.0-pre", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0-pre", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion detects dot-separated pre-release.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DetectsDotSeparatedPreRelease()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v2.0.0.beta.1");

        // Assert
        Assert.AreEqual("v2.0.0.beta.1", tagVersion.OriginalTag);
        Assert.AreEqual("2.0.0.beta.1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion handles no prefix.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_HandlesNoPrefix()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("1.0.0");

        // Assert
        Assert.AreEqual("1.0.0", tagVersion.OriginalTag);
        Assert.AreEqual("1.0.0", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion handles complex prefix.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_HandlesComplexPrefix()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("my-awesome-release_1.2.3-beta");

        // Assert
        Assert.AreEqual("my-awesome-release_1.2.3-beta", tagVersion.OriginalTag);
        Assert.AreEqual("1.2.3-beta", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion does not falsely detect rc in words like 'arch'.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_DoesNotFalselyDetectRcInArch()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v1.0.0.arch");

        // Assert
        Assert.AreEqual("v1.0.0.arch", tagVersion.OriginalTag);
        Assert.AreEqual("1.0.0.arch", tagVersion.FullVersion);
        Assert.IsFalse(tagVersion.IsPreRelease);
    }

    /// <summary>
    ///     Test that TagVersion correctly detects rc with number suffix.
    /// </summary>
    [TestMethod]
    public void TagVersion_Constructor_CorrectlyDetectsRcWithNumber()
    {
        // Arrange & Act
        var tagVersion = new TagVersion("v1.0.0-rc1");

        // Assert
        Assert.AreEqual("v1.0.0-rc1", tagVersion.OriginalTag);
        Assert.AreEqual("1.0.0-rc1", tagVersion.FullVersion);
        Assert.IsTrue(tagVersion.IsPreRelease);
    }
}
