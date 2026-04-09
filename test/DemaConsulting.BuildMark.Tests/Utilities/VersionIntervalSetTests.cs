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
///     Tests for VersionIntervalSet.Parse method.
/// </summary>
[TestClass]
public class VersionIntervalSetTests
{
    /// <summary>
    ///     Test that Parse returns one interval for a single interval token.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_SingleInterval_ReturnsOneInterval()
    {
        // Arrange
        var text = "[1.0.0,2.0.0)";

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(1, result.Intervals);
        Assert.AreEqual("1.0.0", result.Intervals[0].LowerBound);
        Assert.AreEqual("2.0.0", result.Intervals[0].UpperBound);
    }

    /// <summary>
    ///     Test that Parse returns two intervals for two interval tokens separated by comma.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_TwoIntervals_ReturnsTwoIntervals()
    {
        // Arrange
        var text = "(,1.0.1],[1.1.0,1.2.0)";

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result.Intervals);
        Assert.IsNull(result.Intervals[0].LowerBound);
        Assert.AreEqual("1.0.1", result.Intervals[0].UpperBound);
        Assert.IsTrue(result.Intervals[0].UpperInclusive);
        Assert.AreEqual("1.1.0", result.Intervals[1].LowerBound);
        Assert.AreEqual("1.2.0", result.Intervals[1].UpperBound);
        Assert.IsFalse(result.Intervals[1].UpperInclusive);
    }

    /// <summary>
    ///     Test that a comma inside an interval is treated as a bound separator, not an interval separator.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_IntervalsWithInternalComma_ParsedCorrectly()
    {
        // Arrange - two intervals each containing an internal comma between bounds
        var text = "[1.0.0,2.0.0),[3.0.0,4.0.0)";

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert - the internal commas must not split the intervals; two intervals expected
        Assert.IsNotNull(result);
        Assert.HasCount(2, result.Intervals);
        Assert.AreEqual("1.0.0", result.Intervals[0].LowerBound);
        Assert.AreEqual("2.0.0", result.Intervals[0].UpperBound);
        Assert.AreEqual("3.0.0", result.Intervals[1].LowerBound);
        Assert.AreEqual("4.0.0", result.Intervals[1].UpperBound);
    }

    /// <summary>
    ///     Test that Parse returns empty set for empty string.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_EmptyString_ReturnsEmptySet()
    {
        // Arrange
        var text = string.Empty;

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(0, result.Intervals);
    }

    /// <summary>
    ///     Test that an invalid interval token (e.g., no comma) is silently discarded.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_InvalidToken_DiscardedSilently()
    {
        // Arrange - second token has brackets but no comma, making it invalid
        var text = "[1.0.0,2.0.0),[no-comma]";

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert - invalid token is discarded; only the valid interval remains
        Assert.IsNotNull(result);
        Assert.HasCount(1, result.Intervals);
        Assert.AreEqual("1.0.0", result.Intervals[0].LowerBound);
        Assert.AreEqual("2.0.0", result.Intervals[0].UpperBound);
    }

    /// <summary>
    ///     Test that Contains returns true when the candidate is inside the first interval.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_StringInsideFirstInterval_ReturnsTrue()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse("[1.0.0,2.0.0),[3.0.0,4.0.0)");

        // Act
        var result = intervalSet.Contains("1.5.0");

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains returns true when the candidate is inside a later interval.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_StringInsideLaterInterval_ReturnsTrue()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse("[1.0.0,2.0.0),[3.0.0,4.0.0)");

        // Act
        var result = intervalSet.Contains("3.5.0");

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains returns false when the candidate is outside all intervals.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_StringOutsideAllIntervals_ReturnsFalse()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse("[1.0.0,2.0.0),[3.0.0,4.0.0)");

        // Act
        var result = intervalSet.Contains("2.5.0");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Test that Contains returns false for an empty interval set.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_EmptySet_ReturnsFalse()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse(string.Empty);

        // Act
        var result = intervalSet.Contains("1.0.0");

        // Assert
        Assert.IsFalse(result);
    }

    /// <summary>
    ///     Test that the VersionInfo overload delegates to the semantic version.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_Version_DelegatesToSemanticVersion()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse("[1.0.0,2.0.0),[3.0.0,4.0.0)");
        var version = VersionTag.Create("v3.1.0-rc.1");

        // Act
        var result = intervalSet.Contains(version);

        // Assert
        Assert.IsTrue(result);
    }

    /// <summary>
    ///     Test that Contains works correctly with pre-release versions across multiple intervals.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_PreReleaseVersions_HandlesCorrectly()
    {
        // Arrange - intervals that include pre-release boundaries
        var intervalSet = VersionIntervalSet.Parse("[1.0.0-rc.1,1.0.0],[2.0.0-alpha.1,2.0.0-beta.5)");

        // Act & Assert - first interval tests
        Assert.IsTrue(intervalSet.Contains("1.0.0-rc.1"));   // Lower bound of first interval
        Assert.IsTrue(intervalSet.Contains("1.0.0-rc.2"));   // Within first interval
        Assert.IsTrue(intervalSet.Contains("1.0.0"));        // Upper bound of first interval
        
        // Act & Assert - second interval tests  
        Assert.IsTrue(intervalSet.Contains("2.0.0-alpha.1")); // Lower bound of second interval
        Assert.IsTrue(intervalSet.Contains("2.0.0-beta.1"));  // Within second interval
        Assert.IsFalse(intervalSet.Contains("2.0.0-beta.5")); // Exclusive upper bound
        Assert.IsFalse(intervalSet.Contains("2.0.0"));       // Outside second interval (release > pre-release)
        
        // Act & Assert - outside all intervals
        Assert.IsFalse(intervalSet.Contains("1.0.0-alpha.1")); // Before first interval
        Assert.IsFalse(intervalSet.Contains("1.5.0"));        // Between intervals
        Assert.IsFalse(intervalSet.Contains("2.1.0"));        // After all intervals
    }

    /// <summary>
    ///     Test that Contains works with VersionComparable overload for pre-release versions.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Contains_VersionComparable_HandlesPreRelease()
    {
        // Arrange
        var intervalSet = VersionIntervalSet.Parse("[1.2.0-rc.1,1.2.0],[2.0.0,3.0.0)");
        var preReleaseInFirst = VersionComparable.Create("1.2.0-rc.2");
        var releaseInFirst = VersionComparable.Create("1.2.0");
        var releaseInSecond = VersionComparable.Create("2.5.0");
        var versionOutside = VersionComparable.Create("1.1.0");

        // Act & Assert
        Assert.IsTrue(intervalSet.Contains(preReleaseInFirst));
        Assert.IsTrue(intervalSet.Contains(releaseInFirst));
        Assert.IsTrue(intervalSet.Contains(releaseInSecond));
        Assert.IsFalse(intervalSet.Contains(versionOutside));
    }

    /// <summary>
    ///     Test parsing intervals with pre-release bounds in the text.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_PreReleaseBounds_ParsesCorrectly()
    {
        // Arrange
        var text = "[1.0.0-alpha.1,1.0.0-rc.1),[2.0.0-beta.1,2.0.0]";

        // Act
        var result = VersionIntervalSet.Parse(text);

        // Assert
        Assert.IsNotNull(result);
        Assert.HasCount(2, result.Intervals);
        
        // First interval
        Assert.AreEqual("1.0.0-alpha.1", result.Intervals[0].LowerBound);
        Assert.AreEqual("1.0.0-rc.1", result.Intervals[0].UpperBound);
        Assert.IsTrue(result.Intervals[0].LowerInclusive);
        Assert.IsFalse(result.Intervals[0].UpperInclusive);
        
        // Second interval  
        Assert.AreEqual("2.0.0-beta.1", result.Intervals[1].LowerBound);
        Assert.AreEqual("2.0.0", result.Intervals[1].UpperBound);
        Assert.IsTrue(result.Intervals[1].LowerInclusive);
        Assert.IsTrue(result.Intervals[1].UpperInclusive);
    }
}



