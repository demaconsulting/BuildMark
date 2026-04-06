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
///     Unit tests for ItemControlsParser.Parse method.
/// </summary>
[TestClass]
public class ItemControlsParserTests
{
    /// <summary>
    ///     Test that Parse returns null for null description.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_NullDescription_ReturnsNull()
    {
        // Arrange - null input

        // Act
        var result = ItemControlsParser.Parse(null);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns null for empty description.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_EmptyDescription_ReturnsNull()
    {
        // Arrange
        var description = string.Empty;

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns null when there is no buildmark block.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_NoBuildmarkBlock_ReturnsNull()
    {
        // Arrange
        var description = "This is a regular description with no buildmark block.";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns correct visibility for public.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_VisibilityPublic_ReturnsCorrectVisibility()
    {
        // Arrange
        var description = "Some description\n\n```buildmark\nvisibility: public\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
    }

    /// <summary>
    ///     Test that Parse returns correct visibility for internal.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_VisibilityInternal_ReturnsCorrectVisibility()
    {
        // Arrange
        var description = "Some description\n\n```buildmark\nvisibility: internal\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("internal", result.Visibility);
    }

    /// <summary>
    ///     Test that Parse returns correct type for bug.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_TypeBug_ReturnsCorrectType()
    {
        // Arrange
        var description = "```buildmark\ntype: bug\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("bug", result.Type);
    }

    /// <summary>
    ///     Test that Parse returns correct type for feature.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_TypeFeature_ReturnsCorrectType()
    {
        // Arrange
        var description = "```buildmark\ntype: feature\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("feature", result.Type);
    }

    /// <summary>
    ///     Test that Parse returns correct interval set for affected-versions.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_AffectedVersions_ReturnsCorrectIntervalSet()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
        Assert.AreEqual("1.0.0", result.AffectedVersions.Intervals[0].LowerBound);
        Assert.AreEqual("2.0.0", result.AffectedVersions.Intervals[0].UpperBound);
    }

    /// <summary>
    ///     Test that a buildmark block hidden in an HTML comment is not parsed.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_HiddenInHtmlComment_ReturnsParsedInfo()
    {
        // Arrange - buildmark block hidden inside an HTML comment
        var description = "<!-- ```buildmark\nvisibility: public\n``` -->";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - HTML comment is stripped, so block is not found
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns null when block contains only unknown keys.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_UnknownKey_ReturnsNull()
    {
        // Arrange - block with only unknown keys
        var description = "```buildmark\nunknown-key: some-value\nanother-key: other-value\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns complete info when all fields are present.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_AllFields_ReturnsCompleteInfo()
    {
        // Arrange
        var description = "```buildmark\nvisibility: public\ntype: bug\naffected-versions: [1.0.0,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
        Assert.AreEqual("bug", result.Type);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
    }
}
