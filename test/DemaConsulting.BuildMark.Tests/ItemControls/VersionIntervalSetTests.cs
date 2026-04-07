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

using DemaConsulting.BuildMark.ItemControls;

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
}
