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
    ///     Test that Parse returns single interval for single interval text.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_SingleInterval_ReturnsSingleInterval()
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
    ///     Test that Parse returns all intervals for multiple interval text.
    /// </summary>
    [TestMethod]
    public void VersionIntervalSet_Parse_MultipleIntervals_ReturnsAllIntervals()
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
}
