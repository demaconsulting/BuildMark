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
///     Tests for the VersionComparable class.
/// </summary>
[TestClass]
public class VersionComparableTests
{
    /// <summary>
    ///     Test that VersionComparable parses simple version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_Create_SimpleVersion_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionComparable.Create("1.2.3");

        // Assert
        Assert.AreEqual(1, version.Major);
        Assert.AreEqual(2, version.Minor);
        Assert.AreEqual(3, version.Patch);
        Assert.AreEqual("1.2.3", version.Numbers);
        Assert.IsNull(version.PreRelease);
        Assert.AreEqual("1.2.3", version.CompareVersion);
        Assert.IsFalse(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that VersionComparable parses pre-release version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_Create_PreReleaseVersion_ParsesVersion()
    {
        // Arrange & Act
        var version = VersionComparable.Create("2.0.0-alpha.1");

        // Assert
        Assert.AreEqual("2.0.0", version.Numbers);
        Assert.AreEqual("alpha.1", version.PreRelease);
        Assert.AreEqual("2.0.0-alpha.1", version.CompareVersion);
        Assert.IsTrue(version.IsPreRelease);
    }

    /// <summary>
    ///     Test that TryCreate returns null for invalid version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_TryCreate_InvalidVersion_ReturnsNull()
    {
        // Act
        var version = VersionComparable.TryCreate("not-a-version");

        // Assert
        Assert.IsNull(version);
    }

    /// <summary>
    ///     Test that Create throws ArgumentException for invalid version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_Create_InvalidVersion_ThrowsArgumentException()
    {
        // Act
        var exception = Assert.Throws<ArgumentException>(() => VersionComparable.Create("not-a-version"));

        // Assert
        Assert.Contains("does not match comparable version format", exception.Message);
    }

    /// <summary>
    ///     Test that CompareTo works correctly with identical versions.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_IdenticalVersions_ReturnsZero()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.2.3");
        var version2 = VersionComparable.Create("1.2.3");

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.AreEqual(0, result);
    }

    /// <summary>
    ///     Test that CompareTo handles numeric comparison correctly (e.g., 1.2.3 &lt; 1.11.2).
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_NumericComparison_CorrectOrdering()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.2.3");
        var version2 = VersionComparable.Create("1.11.2");

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.IsTrue(result < 0, "1.2.3 should be less than 1.11.2");
    }

    /// <summary>
    ///     Test that CompareTo treats non-pre-release as greater than pre-release with same version.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_ReleaseGreaterThanPreRelease_CorrectOrdering()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.2.3");
        var version2 = VersionComparable.Create("1.2.3-beta.1");

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.IsTrue(result > 0, "1.2.3 should be greater than 1.2.3-beta.1");
    }

    /// <summary>
    ///     Test that CompareTo orders pre-releases lexicographically.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_PreReleaseLexicographic_CorrectOrdering()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.2.3-alpha.1");
        var version2 = VersionComparable.Create("1.2.3-beta.1");

        // Act
        var result = version1.CompareTo(version2);

        // Assert
        Assert.IsTrue(result < 0, "alpha should be less than beta lexicographically");
    }

    /// <summary>
    ///     Test that less-than operator works correctly.
    /// </summary>
    [TestMethod]
    public void VersionComparable_LessThanOperator_FirstVersionLess_ReturnsTrue()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.2.3");
        var version2 = VersionComparable.Create("1.11.2");

        // Act & Assert
        Assert.IsTrue(version1 < version2);
        Assert.IsFalse(version2 < version1);
    }

    /// <summary>
    ///     Test that greater-than operator works correctly.
    /// </summary>
    [TestMethod]
    public void VersionComparable_GreaterThanOperator_FirstVersionGreater_ReturnsTrue()
    {
        // Arrange
        var version1 = VersionComparable.Create("1.11.2");
        var version2 = VersionComparable.Create("1.2.3");

        // Act & Assert
        Assert.IsTrue(version1 > version2);
        Assert.IsFalse(version2 > version1);
    }

    /// <summary>
    ///     Test that numeric pre-release identifiers are compared numerically, not lexicographically.
    ///     This tests the core SemVer requirement that "1.0.0-alpha.5" &lt; "1.0.0-alpha.10".
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_PreReleaseNumeric_ComparesNumerically()
    {
        // Arrange - This was the original failing case
        var version5 = VersionComparable.Create("1.0.0-alpha.5");
        var version10 = VersionComparable.Create("1.0.0-alpha.10");

        // Act
        var result = version5!.CompareTo(version10);

        // Assert
        Assert.IsTrue(result < 0, "1.0.0-alpha.5 should be less than 1.0.0-alpha.10 (numeric comparison)");
    }

    /// <summary>
    ///     Test comprehensive SemVer pre-release comparison rules.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_PreReleaseSemVerRules_CorrectOrdering()
    {
        // Test cases from SemVer specification
        var versions = new[]
        {
            "1.0.0-alpha",
            "1.0.0-alpha.1",
            "1.0.0-alpha.beta",
            "1.0.0-beta",
            "1.0.0-beta.2",
            "1.0.0-beta.11",
            "1.0.0-rc.1"
        }.Select(v => VersionComparable.Create(v)).ToArray();

        // Test that they are in ascending order
        for (var i = 0; i < versions.Length - 1; i++)
        {
            var current = versions[i];
            var next = versions[i + 1];
            var result = current!.CompareTo(next);

            Assert.IsTrue(result < 0, $"{current.CompareVersion} should be less than {next!.CompareVersion}");
        }
    }

    /// <summary>
    ///     Test that numeric identifiers are always less than non-numeric identifiers.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_NumericVsNonNumeric_NumericIsLess()
    {
        // Arrange
        var numericVersion = VersionComparable.Create("1.0.0-1");
        var nonNumericVersion = VersionComparable.Create("1.0.0-alpha");

        // Act
        var result = numericVersion!.CompareTo(nonNumericVersion);

        // Assert
        Assert.IsTrue(result < 0, "1.0.0-1 should be less than 1.0.0-alpha (numeric < non-numeric)");
    }

    /// <summary>
    ///     Test that shorter pre-release identifiers are considered less when all compared parts are equal.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_ShorterPreRelease_IsLess()
    {
        // Arrange
        var shorterVersion = VersionComparable.Create("1.0.0-alpha");
        var longerVersion = VersionComparable.Create("1.0.0-alpha.1");

        // Act
        var result = shorterVersion!.CompareTo(longerVersion);

        // Assert
        Assert.IsTrue(result < 0, "1.0.0-alpha should be less than 1.0.0-alpha.1 (shorter is less)");
    }

    /// <summary>
    ///     Test complex multi-segment numeric and non-numeric pre-release comparison.
    /// </summary>
    [TestMethod]
    public void VersionComparable_CompareTo_ComplexPreRelease_CorrectOrdering()
    {
        // Arrange - Test various complex scenarios
        var test1 = VersionComparable.Create("1.0.0-alpha.1.2");
        var test2 = VersionComparable.Create("1.0.0-alpha.1.10");
        var test3 = VersionComparable.Create("1.0.0-alpha.2.1");
        var test4 = VersionComparable.Create("1.0.0-alpha.beta.1");

        // Act & Assert
        Assert.IsTrue(test1!.CompareTo(test2) < 0, "alpha.1.2 < alpha.1.10 (numeric segment comparison)");
        Assert.IsTrue(test2!.CompareTo(test3) < 0, "alpha.1.10 < alpha.2.1 (first numeric difference wins)");
        Assert.IsTrue(test3!.CompareTo(test4) < 0, "alpha.2.1 < alpha.beta.1 (numeric < non-numeric)");
    }
}


