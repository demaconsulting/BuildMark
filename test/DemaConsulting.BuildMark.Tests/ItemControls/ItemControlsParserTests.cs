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

using DemaConsulting.BuildMark.RepoConnectors;

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
    public void ItemControlsParser_Parse_WithNullDescription_ReturnsNull()
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
    public void ItemControlsParser_Parse_WithEmptyDescription_ReturnsNull()
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
    public void ItemControlsParser_Parse_WithNoBlock_ReturnsNull()
    {
        // Arrange
        var description = "This is a regular description with no buildmark block.";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse returns "public" visibility when visibility is set to public.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithVisibilityPublic_ReturnsPublicVisibility()
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
    ///     Test that Parse returns "internal" visibility when visibility is set to internal.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithVisibilityInternal_ReturnsInternalVisibility()
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
    ///     Test that Parse returns "bug" type when type is set to bug.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithTypeBug_ReturnsBugType()
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
    ///     Test that Parse returns "feature" type when type is set to feature.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithTypeFeature_ReturnsFeatureType()
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
    public void ItemControlsParser_Parse_WithAffectedVersions_ReturnsIntervalSet()
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
    ///     Test that Parse recognizes a buildmark block hidden inside an HTML comment.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithHiddenBlock_ReturnsControls()
    {
        // Arrange - buildmark block wrapped in an HTML comment to hide from GitHub rendered view
        var description = "<!-- ```buildmark\nvisibility: public\n``` -->";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - HTML comment delimiters are stripped, exposing the block
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
    }

    /// <summary>
    ///     Test that Parse recognizes internal visibility from a block hidden inside an HTML comment.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithHiddenBlockVisibilityInternal_ReturnsInternalVisibility()
    {
        // Arrange - internal visibility block wrapped in HTML comment
        var description = "Issue description.\n<!-- ```buildmark\nvisibility: internal\n``` -->";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - hidden block is parsed and returns internal visibility
        Assert.IsNotNull(result);
        Assert.AreEqual("internal", result.Visibility);
    }

    /// <summary>
    ///     Test that Parse ignores unknown keys and returns null when no recognized keys are found.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithUnknownKey_IgnoresKey()
    {
        // Arrange - block with only unknown keys, which are silently ignored
        var description = "```buildmark\nunknown-key: some-value\nanother-key: other-value\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - no recognized keys, so null is returned
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse ignores an unrecognized visibility value and treats the field as absent.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithUnrecognizedVisibilityValue_IgnoresValue()
    {
        // Arrange - visibility value is not "public" or "internal"
        var description = "```buildmark\nvisibility: visible\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - unrecognized value is ignored; no valid fields → null result
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that Parse ignores an unrecognized type value and treats the field as absent.
    /// </summary>
    [TestMethod]
    public void ItemControlsParser_Parse_WithUnrecognizedTypeValue_IgnoresValue()
    {
        // Arrange - type value is not "bug" or "feature"
        var description = "```buildmark\ntype: enhancement\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - unrecognized value is ignored; no valid fields → null result
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
