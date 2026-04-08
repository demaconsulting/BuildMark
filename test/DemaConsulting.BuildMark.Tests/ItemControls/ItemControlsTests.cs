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
using DemaConsulting.BuildMark.Utilities;

/// <summary>
///     Subsystem-level tests for ItemControls parsing.
/// </summary>
[TestClass]
public class ItemControlsTests
{
    /// <summary>
    ///     Test that the subsystem returns "public" visibility when specified.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithVisibilityPublic_ReturnsPublicVisibility()
    {
        // Arrange
        var description = "Issue description.\n\n```buildmark\nvisibility: public\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
    }

    /// <summary>
    ///     Test that the subsystem returns "internal" visibility when specified.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithVisibilityInternal_ReturnsInternalVisibility()
    {
        // Arrange
        var description = "Issue description.\n\n```buildmark\nvisibility: internal\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("internal", result.Visibility);
    }

    /// <summary>
    ///     Test that the subsystem returns "bug" type when specified.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithTypeBug_ReturnsBugType()
    {
        // Arrange
        var description = "Bug description.\n\n```buildmark\ntype: bug\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("bug", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem returns "feature" type when specified.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithTypeFeature_ReturnsFeatureType()
    {
        // Arrange
        var description = "Feature description.\n\n```buildmark\ntype: feature\n```\n";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("feature", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem parses an affected-versions interval set correctly.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithAffectedVersions_ReturnsIntervalSet()
    {
        // Arrange - multi-range affected-versions field
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

    /// <summary>
    ///     Test that the subsystem recognizes a buildmark block hidden inside an HTML comment.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithHiddenBlock_ReturnsControls()
    {
        // Arrange - buildmark block wrapped in HTML comment to hide from GitHub rendered view
        var description = "Description.\n<!-- ```buildmark\ntype: feature\n``` -->";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert - HTML comment delimiters are stripped, exposing and parsing the block
        Assert.IsNotNull(result);
        Assert.AreEqual("feature", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem returns null when no buildmark block is present.
    /// </summary>
    [TestMethod]
    public void ItemControls_Parse_WithNoBlock_ReturnsNull()
    {
        // Arrange - description with no buildmark fenced block
        var description = "A plain description with no controls block.";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that a single version interval in affected-versions is returned correctly.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_SingleInterval_ReturnsInterval()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
    }

    /// <summary>
    ///     Test that multiple version intervals in affected-versions are returned correctly.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_MultipleIntervals_ReturnsIntervalSet()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,2.0.0),[3.0.0,4.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(2, result.AffectedVersions.Intervals);
    }

    /// <summary>
    ///     Test that LowerInclusive is true when '[' is used in affected-versions.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_InclusiveLowerBound_IsInclusive()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
        Assert.IsTrue(result.AffectedVersions.Intervals[0].LowerInclusive);
    }

    /// <summary>
    ///     Test that UpperInclusive is false when ')' is used in affected-versions.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_ExclusiveUpperBound_IsExclusive()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
        Assert.IsFalse(result.AffectedVersions.Intervals[0].UpperInclusive);
    }

    /// <summary>
    ///     Test that LowerBound is null when lower bound is empty in affected-versions.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_UnboundedLower_HasNullLowerBound()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: (,2.0.0)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
        Assert.IsNull(result.AffectedVersions.Intervals[0].LowerBound);
    }

    /// <summary>
    ///     Test that UpperBound is null when upper bound is empty in affected-versions.
    /// </summary>
    [TestMethod]
    public void ItemControls_VersionInterval_Parse_UnboundedUpper_HasNullUpperBound()
    {
        // Arrange
        var description = "```buildmark\naffected-versions: [1.0.0,)\n```";

        // Act
        var result = ItemControlsParser.Parse(description);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(1, result.AffectedVersions.Intervals);
        Assert.IsNull(result.AffectedVersions.Intervals[0].UpperBound);
    }
}
