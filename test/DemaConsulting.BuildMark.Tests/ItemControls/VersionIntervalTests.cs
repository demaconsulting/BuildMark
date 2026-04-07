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
}
