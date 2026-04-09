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

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
///     Routes report items into configured sections.
/// </summary>
public static class ItemRouter
{
    /// <summary>
    ///     Routes items into section buckets using the configured rules.
    /// </summary>
    /// <param name="items">The items to route.</param>
    /// <param name="rules">The routing rules.</param>
    /// <param name="sections">The configured sections.</param>
    /// <returns>A dictionary keyed by section id.</returns>
    public static Dictionary<string, List<ItemInfo>> Route(
        IReadOnlyList<ItemInfo> items,
        IReadOnlyList<RuleConfig> rules,
        IReadOnlyList<SectionConfig> sections)
    {
        // Initialize the output buckets using the configured section order.
        var routedItems = sections.ToDictionary(section => section.Id, _ => new List<ItemInfo>());
        var defaultSectionId = sections.FirstOrDefault()?.Id ?? "changes";

        // Route each item to the first matching destination.
        foreach (var item in items)
        {
            // Find the first rule that matches this item, defaulting to the first section.
            var matchingRule = rules.FirstOrDefault(rule => RuleMatches(rule, item));
            var destination = matchingRule?.Route ?? defaultSectionId;

            // Suppressed items are omitted from all sections.
            if (string.Equals(destination, "suppressed", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Retrieve or create the destination bucket for the target section.
            if (!routedItems.TryGetValue(destination, out var bucket))
            {
                bucket = [];
                routedItems[destination] = bucket;
            }

            bucket.Add(item);
        }

        return routedItems;
    }

    /// <summary>
    ///     Determines whether a routing rule matches an item.
    /// </summary>
    /// <param name="rule">The candidate rule.</param>
    /// <param name="item">The candidate item.</param>
    /// <returns>True when the rule matches.</returns>
    private static bool RuleMatches(RuleConfig rule, ItemInfo item)
    {
        // Treat rules with no match block as catch-all entries.
        if (rule.Match == null)
        {
            return true;
        }

        // Apply label matches against the normalized item type.
        if (rule.Match.Label.Count > 0 &&
            !rule.Match.Label.Any(label => string.Equals(label, item.Type, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        // Apply work-item-type matches against the same normalized type when no richer metadata is available.
        if (rule.Match.WorkItemType.Count > 0 &&
            !rule.Match.WorkItemType.Any(type => string.Equals(type, item.Type, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}
