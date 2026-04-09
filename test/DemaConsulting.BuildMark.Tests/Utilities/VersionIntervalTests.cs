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

using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.Tests.Utilities;

/// <summary>
///     Tests for VersionInterval.Parse method.
/// </summary>
[TestClass]
public class VersionIntervalTests
{
    /// <summary>
    ///     Test that Parse returns LowerInclusive=true for '[' opening bracket.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_InclusiveLower_IsInclusive()
    {
        // Arrange
        var text = "[1.0.0,2.0.0)";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("1.0.0", result.LowerBound);
        Assert.IsTrue(result.LowerInclusive);
    }

    /// <summary>
    ///     Test that Parse returns LowerInclusive=false for '(' opening bracket.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_ExclusiveLower_IsExclusive()
    {
        // Arrange
        var text = "(1.0.0,2.0.0)";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("1.0.0", result.LowerBound);
        Assert.IsFalse(result.LowerInclusive);
    }

    /// <summary>
    ///     Test that Parse returns UpperInclusive=true for ']' closing bracket.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_InclusiveUpper_IsInclusive()
    {
        // Arrange
        var text = "[1.0.0,2.0.0]";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("2.0.0", result.UpperBound);
        Assert.IsTrue(result.UpperInclusive);
    }

    /// <summary>
    ///     Test that Parse returns UpperInclusive=false for ')' closing bracket.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_ExclusiveUpper_IsExclusive()
    {
        // Arrange
        var text = "[1.0.0,2.0.0)";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("2.0.0", result.UpperBound);
        Assert.IsFalse(result.UpperInclusive);
    }

    /// <summary>
    ///     Test that Parse returns LowerBound=null for empty lower bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_UnboundedLower_HasNullLowerBound()
    {
        // Arrange
        var text = "(,1.0.1]";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNull(result.LowerBound);
        Assert.IsFalse(result.LowerInclusive);
        Assert.AreEqual("1.0.1", result.UpperBound);
        Assert.IsTrue(result.UpperInclusive);
    }

    /// <summary>
    ///     Test that Parse returns UpperBound=null for empty upper bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_UnboundedUpper_HasNullUpperBound()
    {
        // Arrange
        var text = "[3.0.0,)";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("3.0.0", result.LowerBound);
        Assert.IsTrue(result.LowerInclusive);
        Assert.IsNull(result.UpperBound);
        Assert.IsFalse(result.UpperInclusive);
    }

    /// <summary>
    ///     Test that Parse returns an interval with both bounds present.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_BothBoundsPresent_ReturnsInterval()
    {
        // Arrange
        var text = "[1.0.0,2.0.0]";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("1.0.0", result.LowerBound);
        Assert.AreEqual("2.0.0", result.UpperBound);
    }

    /// <summary>
    ///     Test that Parse returns null for invalid format (no brackets).
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_InvalidFormat_ReturnsNull()
    {
        // Arrange
        var text = "not-an-interval";

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns null for null input.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_NullInput_ReturnsNull()
    {
        // Arrange - null input

        // Act
        var result = VersionInterval.Parse(null);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns null for empty string.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Parse_EmptyString_ReturnsNull()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = VersionInterval.Parse(text);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Contains returns true when the candidate equals the inclusive lower bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringEqualToInclusiveLower_ReturnsTrue()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0,2.0.0)");

        // Act
        var result = interval!.Contains("1.0.0");

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains returns false when the candidate equals the exclusive lower bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringEqualToExclusiveLower_ReturnsFalse()
    {
        // Arrange
        var interval = VersionInterval.Parse("(1.0.0,2.0.0)");

        // Act
        var result = interval!.Contains("1.0.0");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Test that Contains returns true when the candidate equals the inclusive upper bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringEqualToInclusiveUpper_ReturnsTrue()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0,2.0.0]");

        // Act
        var result = interval!.Contains("2.0.0");

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains returns false when the candidate equals the exclusive upper bound.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringEqualToExclusiveUpper_ReturnsFalse()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0,2.0.0)");

        // Act
        var result = interval!.Contains("2.0.0");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Test that Contains returns true for a candidate inside an unbounded interval.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringInsideUnboundedInterval_ReturnsTrue()
    {
        // Arrange
        var interval = VersionInterval.Parse("(,1.0.1]");

        // Act
        var result = interval!.Contains("1.0.0");

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains returns false for a candidate outside the interval.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_StringOutsideInterval_ReturnsFalse()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0,2.0.0)");

        // Act
        var result = interval!.Contains("2.1.0");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Test that the VersionInfo overload delegates to the semantic version.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_Version_DelegatesToSemanticVersion()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0,2.0.0)");
        var version = VersionTag.Create("v1.5.0-beta.1");

        // Act
        var result = interval!.Contains(version);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains correctly handles pre-release version bounds.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_PreReleaseBounds_HandlesCorrectly()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.2.0-rc.1,1.2.0]");

        // Act & Assert
        Assert.IsTrue(interval!.Contains("1.2.0-rc.1"));  // Equal to inclusive lower bound
        Assert.IsTrue(interval!.Contains("1.2.0-rc.2"));  // Between bounds (rc.2 > rc.1)
        Assert.IsTrue(interval!.Contains("1.2.0"));       // Equal to inclusive upper bound
        Assert.IsFalse(interval!.Contains("1.2.0-alpha.1")); // Before lower bound (alpha < rc)
        Assert.IsFalse(interval!.Contains("1.2.1"));     // After upper bound
        Assert.IsFalse(interval!.Contains("1.1.9"));     // Before lower bound
    }

    /// <summary>
    ///     Test that Contains correctly handles pre-release to pre-release intervals.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_PreReleaseToPreRelease_HandlesCorrectly()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.2.0-alpha.1,1.2.0-rc.1)");

        // Act & Assert
        Assert.IsTrue(interval!.Contains("1.2.0-alpha.1"));  // Equal to inclusive lower bound
        Assert.IsTrue(interval!.Contains("1.2.0-beta.1"));   // Between bounds
        Assert.IsFalse(interval!.Contains("1.2.0-rc.1"));   // Equal to exclusive upper bound
        Assert.IsFalse(interval!.Contains("1.2.0"));        // After upper bound (release > pre-release)
    }

    /// <summary>
    ///     Test that Contains correctly orders pre-release versions lexicographically.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_PreReleaseOrdering_UsesLexicographicComparison()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.0.0-alpha.5,1.0.0-alpha.10]");

        // Act & Assert
        Assert.IsTrue(interval!.Contains("1.0.0-alpha.5"));   // Equal to lower bound
        Assert.IsTrue(interval!.Contains("1.0.0-alpha.6"));   // Between bounds
        Assert.IsTrue(interval!.Contains("1.0.0-alpha.10"));  // Equal to upper bound
        Assert.IsFalse(interval!.Contains("1.0.0-alpha.4"));  // Before lower bound
        Assert.IsFalse(interval!.Contains("1.0.0-alpha.11")); // After upper bound
    }

    /// <summary>
    ///     Test that VersionComparable overload works with pre-release versions.
    /// </summary>
    [TestMethod]
    public void VersionInterval_Contains_VersionComparable_HandlesPreRelease()
    {
        // Arrange
        var interval = VersionInterval.Parse("[1.2.0-rc.1,1.2.0]");
        var preReleaseVersion = VersionComparable.Create("1.2.0-rc.2");
        var releaseVersion = VersionComparable.Create("1.2.0");

        // Act & Assert
        Assert.IsTrue(interval!.Contains(preReleaseVersion));
        Assert.IsTrue(interval!.Contains(releaseVersion));
    }
}



