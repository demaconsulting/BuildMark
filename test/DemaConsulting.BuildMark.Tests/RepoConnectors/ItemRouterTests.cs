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

using DemaConsulting.BuildMark.BuildNotes;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors;

/// <summary>
///     Tests for the ItemRouter class.
/// </summary>
public class ItemRouterTests
{
    /// <summary>
    ///     Test that matching items are routed to the first configured section.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_MatchingRuleRoutesItemToConfiguredSection()
    {
        // Arrange
        List<ItemInfo> items =
        [
            new("1", "Feature", "https://example.com/1", "feature", 1),
            new("2", "Bug", "https://example.com/2", "bug", 2)
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" },
            new() { Id = "bugs", Title = "Bugs" }
        ];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "bug" } },
                Route = "bugs"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert
        Assert.Single(routedItems["changes"]);
        Assert.Equal("1", routedItems["changes"][0].Id);
        Assert.Single(routedItems["bugs"]);
        Assert.Equal("2", routedItems["bugs"][0].Id);
    }

    /// <summary>
    ///     Test that suppressed routes exclude matching items.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_SuppressedRouteOmitsMatchingItem()
    {
        // Arrange
        List<ItemInfo> items = [new("1", "Internal", "https://example.com/1", "internal", 1)];
        List<SectionConfig> sections = [new() { Id = "changes", Title = "Changes" }];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "internal" } },
                Route = "suppressed"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert
        Assert.Empty(routedItems["changes"]);
    }

    /// <summary>
    ///     Test that a rule with null Match block acts as a catch-all and matches all items.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_WithNullMatchBlock_MatchesAllItems()
    {
        // Arrange
        List<ItemInfo> items =
        [
            new("1", "Feature", "https://example.com/1", "feature", 1),
            new("2", "Bug", "https://example.com/2", "bug", 2)
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" },
            new() { Id = "other", Title = "Other" }
        ];
        List<RuleConfig> rules =
        [
            new() { Match = null, Route = "other" }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - catch-all routes all items to "other", leaving "changes" empty
        Assert.Empty(routedItems["changes"]);
        Assert.Equal(2, routedItems["other"].Count);
    }

    /// <summary>
    ///     Test that WorkItemType matching routes only items with a matching type.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem()
    {
        // Arrange
        List<ItemInfo> items =
        [
            new("1", "Feature", "https://example.com/1", "feature", 1),
            new("2", "Bug", "https://example.com/2", "bug", 2)
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" },
            new() { Id = "bugs", Title = "Bugs" }
        ];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { WorkItemType = { "bug" } },
                Route = "bugs"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - only the bug item is routed to "bugs"; feature falls through to default
        Assert.Single(routedItems["changes"]);
        Assert.Equal("1", routedItems["changes"][0].Id);
        Assert.Single(routedItems["bugs"]);
        Assert.Equal("2", routedItems["bugs"][0].Id);
    }

    /// <summary>
    ///     Test that items with no matching rule are routed to the default (first) section.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_WithNoMatchingRule_RoutesToDefaultSection()
    {
        // Arrange
        List<ItemInfo> items = [new("1", "Unknown", "https://example.com/1", "unknown", 1)];
        List<SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" },
            new() { Id = "bugs", Title = "Bugs" }
        ];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "bug" } },
                Route = "bugs"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - unmatched item lands in the first section ("changes")
        Assert.Single(routedItems["changes"]);
        Assert.Equal("1", routedItems["changes"][0].Id);
        Assert.Empty(routedItems["bugs"]);
    }

    /// <summary>
    ///     Test that items routed to a section not in the initial list create a new bucket entry.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_ItemNotInConfiguredSections_CreatesNewSection()
    {
        // Arrange
        List<ItemInfo> items = [new("1", "Feature", "https://example.com/1", "feature", 1)];
        List<SectionConfig> sections = [new() { Id = "changes", Title = "Changes" }];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "feature" } },
                Route = "new-section"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - item is routed to the new section, which is created dynamically
        Assert.Empty(routedItems["changes"]);
        Assert.True(routedItems.TryGetValue("new-section", out var newSection));
        Assert.Single(newSection);
        Assert.Equal("1", newSection[0].Id);
    }

    /// <summary>
    ///     Test that label matching is case-insensitive.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem()
    {
        // Arrange - item type is "Bug" (capitalized) while rule label is "bug" (lowercase)
        List<ItemInfo> items =
        [
            new("1", "Bug Item", "https://example.com/1", "Bug", 1)
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" },
            new() { Id = "bugs", Title = "Bugs" }
        ];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "bug" } },
                Route = "bugs"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - "Bug" type matches "bug" label rule due to case-insensitive comparison
        Assert.Empty(routedItems["changes"]);
        Assert.Single(routedItems["bugs"]);
        Assert.Equal("1", routedItems["bugs"][0].Id);
    }

    /// <summary>
    ///     Test that the suppressed route value is matched case-insensitively.
    /// </summary>
    [Fact]
    public void ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem()
    {
        // Arrange - route value is "SUPPRESSED" (uppercase) to verify case-insensitive comparison
        List<ItemInfo> items = [new("1", "Internal", "https://example.com/1", "internal", 1)];
        List<SectionConfig> sections = [new() { Id = "changes", Title = "Changes" }];
        List<RuleConfig> rules =
        [
            new()
            {
                Match = new RuleMatchConfig { Label = { "internal" } },
                Route = "SUPPRESSED"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert - item is omitted even when the route value uses uppercase "SUPPRESSED"
        Assert.Empty(routedItems["changes"]);
    }
}
