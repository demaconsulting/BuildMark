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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the ItemRouter class.
/// </summary>
[TestClass]
public class ItemRouterTests
{
    /// <summary>
    ///     Test that matching items are routed to the first configured section.
    /// </summary>
    [TestMethod]
    public void ItemRouter_Route_MatchingRuleRoutesItemToConfiguredSection()
    {
        // Arrange
        List<ItemInfo> items =
        [
            new ItemInfo("1", "Feature", "https://example.com/1", "feature", 1),
            new ItemInfo("2", "Bug", "https://example.com/2", "bug", 2)
        ];
        List<SectionConfig> sections =
        [
            new SectionConfig { Id = "changes", Title = "Changes" },
            new SectionConfig { Id = "bugs", Title = "Bugs" }
        ];
        List<RuleConfig> rules =
        [
            new RuleConfig
            {
                Match = new RuleMatchConfig { Label = { "bug" } },
                Route = "bugs"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert
        Assert.HasCount(1, routedItems["changes"]);
        Assert.AreEqual("1", routedItems["changes"][0].Id);
        Assert.HasCount(1, routedItems["bugs"]);
        Assert.AreEqual("2", routedItems["bugs"][0].Id);
    }

    /// <summary>
    ///     Test that suppressed routes exclude matching items.
    /// </summary>
    [TestMethod]
    public void ItemRouter_Route_SuppressedRouteOmitsMatchingItem()
    {
        // Arrange
        List<ItemInfo> items = [new ItemInfo("1", "Internal", "https://example.com/1", "internal", 1)];
        List<SectionConfig> sections = [new SectionConfig { Id = "changes", Title = "Changes" }];
        List<RuleConfig> rules =
        [
            new RuleConfig
            {
                Match = new RuleMatchConfig { Label = { "internal" } },
                Route = "suppressed"
            }
        ];

        // Act
        var routedItems = ItemRouter.Route(items, rules, sections);

        // Assert
        Assert.IsEmpty(routedItems["changes"]);
    }
}
