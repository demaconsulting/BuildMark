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
///     Subsystem-level tests for ItemControls parsing.
/// </summary>
[TestClass]
public class ItemControlsTests
{
    /// <summary>
    ///     Test that visibility and type can both be parsed from a buildmark block.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_VisibilityAndTypeInBlock_ReturnsBothFields()
    {
        // Arrange
        var description = "Issue description.\n\n```buildmark\nvisibility: public\ntype: feature\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
        Assert.AreEqual("feature", result.Type);
        Assert.IsNull(result.AffectedVersions);
    }

    /// <summary>
    ///     Test that a buildmark block with multi-range affected-versions parses all intervals correctly.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_HiddenBlockWithAffectedVersions_ReturnsIntervals()
    {
        // Arrange - block not in an HTML comment, but with multi-range affected-versions
        var description = "Feature description.\n\n```buildmark\naffected-versions: (,1.0.1],[1.1.0,1.2.0)\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(2, result.AffectedVersions.Intervals);
        Assert.IsNull(result.AffectedVersions.Intervals[0].LowerBound);
        Assert.AreEqual("1.0.1", result.AffectedVersions.Intervals[0].UpperBound);
        Assert.AreEqual("1.1.0", result.AffectedVersions.Intervals[1].LowerBound);
        Assert.AreEqual("1.2.0", result.AffectedVersions.Intervals[1].UpperBound);
    }
}
