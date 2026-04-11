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
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

/// <summary>
///     Maps Azure DevOps work items to BuildMark ItemInfo objects.
/// </summary>
internal static class WorkItemMapper
{
    /// <summary>
    ///     Work item types that map to the normalized "bug" type.
    /// </summary>
    private static readonly HashSet<string> BugTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Bug",
        "Issue"
    };

    /// <summary>
    ///     Work item types that map to the normalized "feature" type.
    /// </summary>
    private static readonly HashSet<string> FeatureTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "User Story",
        "Feature",
        "Epic"
    };

    /// <summary>
    ///     Work item states that are treated as resolved.
    /// </summary>
    private static readonly HashSet<string> ResolvedStates = new(StringComparer.OrdinalIgnoreCase)
    {
        "Resolved",
        "Closed",
        "Done"
    };

    /// <summary>
    ///     Maps a single Azure DevOps work item to an ItemInfo record.
    /// </summary>
    /// <param name="workItem">Azure DevOps work item.</param>
    /// <param name="workItemUrl">Web URL for the work item.</param>
    /// <param name="index">Index for sorting.</param>
    /// <returns>ItemInfo instance, or null if the item should be excluded.</returns>
    public static ItemInfo? MapWorkItemToItemInfo(AzureDevOpsWorkItem workItem, string workItemUrl, int index = 0)
    {
        // Read core fields from the work item
        var title = GetFieldValue(workItem, "System.Title") ?? string.Empty;
        var workItemType = GetFieldValue(workItem, "System.WorkItemType") ?? string.Empty;

        // Map the work item type to a normalized type
        var normalizedType = NormalizeType(workItemType);

        // Extract item controls from description and custom fields
        var controls = ExtractItemControls(workItem);

        // Exclude item if visibility is "internal"
        var forceInclude = controls?.Visibility == "public";
        if (!forceInclude && controls?.Visibility == "internal")
        {
            return null;
        }

        // Override type if item controls specify one
        normalizedType = ApplyTypeOverride(normalizedType, controls);

        // Create and return item info
        return new ItemInfo(
            workItem.Id.ToString(),
            title,
            workItemUrl,
            normalizedType,
            index,
            controls?.AffectedVersions);
    }

    /// <summary>
    ///     Checks whether a work item's state is one of the known resolved states.
    /// </summary>
    /// <param name="workItem">Azure DevOps work item.</param>
    /// <returns>True if the work item is resolved; false otherwise.</returns>
    public static bool IsWorkItemResolved(AzureDevOpsWorkItem workItem)
    {
        var state = GetFieldValue(workItem, "System.State") ?? string.Empty;
        return ResolvedStates.Contains(state);
    }

    /// <summary>
    ///     Returns the work item type string used for routing rule matching.
    /// </summary>
    /// <param name="workItem">Azure DevOps work item.</param>
    /// <returns>The raw System.WorkItemType value.</returns>
    public static string GetWorkItemTypeForRuleMatching(AzureDevOpsWorkItem workItem)
    {
        return GetFieldValue(workItem, "System.WorkItemType") ?? string.Empty;
    }

    /// <summary>
    ///     Combines item controls from both buildmark blocks and custom fields.
    /// </summary>
    /// <param name="workItem">Azure DevOps work item.</param>
    /// <returns>Merged ItemControlsInfo, or null if no controls were found.</returns>
    internal static ItemControlsInfo? ExtractItemControls(AzureDevOpsWorkItem workItem)
    {
        // Parse buildmark block from work item description
        var description = GetFieldValue(workItem, "System.Description");
        var blockControls = ItemControlsParser.Parse(description);

        // Read custom field values
        var customVisibility = GetFieldValue(workItem, "Custom.Visibility");
        var customAffectedVersions = GetFieldValue(workItem, "Custom.AffectedVersions");

        // If no controls from either source, return null
        if (blockControls == null && customVisibility == null && customAffectedVersions == null)
        {
            return null;
        }

        // Start with buildmark block values as defaults
        var visibility = blockControls?.Visibility;
        var type = blockControls?.Type;
        var affectedVersions = blockControls?.AffectedVersions;

        // Custom fields take precedence over buildmark blocks when present
        if (!string.IsNullOrEmpty(customVisibility))
        {
            visibility = customVisibility;
        }

        if (!string.IsNullOrEmpty(customAffectedVersions))
        {
            var parsed = VersionIntervalSet.Parse(customAffectedVersions);
            if (parsed.Intervals.Count > 0)
            {
                affectedVersions = parsed;
            }
        }

        // Return null if no recognized controls found after merging
        if (visibility == null && type == null && affectedVersions == null)
        {
            return null;
        }

        return new ItemControlsInfo(visibility, type, affectedVersions);
    }

    /// <summary>
    ///     Normalizes an Azure DevOps work item type to a BuildMark type string.
    /// </summary>
    /// <param name="workItemType">Azure DevOps work item type.</param>
    /// <returns>Normalized type string.</returns>
    private static string NormalizeType(string workItemType)
    {
        if (BugTypes.Contains(workItemType))
        {
            return "bug";
        }

        if (FeatureTypes.Contains(workItemType))
        {
            return "feature";
        }

        // Return the raw type name for unmapped types
        return workItemType;
    }

    /// <summary>
    ///     Applies type override from item controls if specified.
    /// </summary>
    /// <param name="type">Current normalized type.</param>
    /// <param name="controls">Parsed item controls (may be null).</param>
    /// <returns>Final type string.</returns>
    private static string ApplyTypeOverride(string type, ItemControlsInfo? controls)
    {
        if (controls?.Type == "bug")
        {
            return "bug";
        }

        if (controls?.Type == "feature")
        {
            return "feature";
        }

        return type;
    }

    /// <summary>
    ///     Gets a string field value from a work item's fields dictionary.
    ///     Handles values that may be stored as string, JsonElement, or other types.
    /// </summary>
    /// <param name="workItem">Azure DevOps work item.</param>
    /// <param name="fieldName">Field reference name.</param>
    /// <returns>Field value as string, or null if not present or empty.</returns>
    internal static string? GetFieldValue(AzureDevOpsWorkItem workItem, string fieldName)
    {
        if (!workItem.Fields.TryGetValue(fieldName, out var value) || value == null)
        {
            return null;
        }

        // Handle JsonElement values from deserialization
        if (value is System.Text.Json.JsonElement jsonElement)
        {
            return jsonElement.ValueKind == System.Text.Json.JsonValueKind.String
                ? jsonElement.GetString()
                : jsonElement.ToString();
        }

        // Handle direct string values
        var result = value.ToString();
        return string.IsNullOrEmpty(result) ? null : result;
    }
}
