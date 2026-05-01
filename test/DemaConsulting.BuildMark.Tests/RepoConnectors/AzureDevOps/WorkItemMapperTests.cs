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

using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <inheritdoc/>
/// <inheritdoc/>
[TestClass]
public class WorkItemMapperTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-WorkItemMapper
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Bug work item type maps to a bug ItemInfo.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_BugType_ReturnsBugItem()
    {
        // Arrange
        var workItem = CreateWorkItem(100, "A bug", "Bug", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/100", 1);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("100", itemInfo.Id);
        Assert.AreEqual("A bug", itemInfo.Title);
        Assert.AreEqual("bug", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that User Story work item type maps to a feature ItemInfo.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_UserStoryType_ReturnsFeatureItem()
    {
        // Arrange
        var workItem = CreateWorkItem(101, "A user story", "User Story", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/101", 2);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("101", itemInfo.Id);
        Assert.AreEqual("A user story", itemInfo.Title);
        Assert.AreEqual("feature", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that Epic work item type maps to a feature ItemInfo.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_EpicType_ReturnsFeatureItem()
    {
        // Arrange
        var workItem = CreateWorkItem(103, "An epic", "Epic", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/103", 4);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("103", itemInfo.Id);
        Assert.AreEqual("An epic", itemInfo.Title);
        Assert.AreEqual("feature", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that Task work item type maps to an ItemInfo with the raw type name.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_TaskType_ReturnsTaskItem()
    {
        // Arrange
        var workItem = CreateWorkItem(102, "A task", "Task", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/102", 3);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("102", itemInfo.Id);
        Assert.AreEqual("A task", itemInfo.Title);
        Assert.AreEqual("Task", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that IsWorkItemResolved returns true for a resolved work item.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_IsWorkItemResolved_ResolvedState_ReturnsTrue()
    {
        // Arrange - test all resolved states
        var resolvedItem = CreateWorkItem(100, "Resolved item", "Bug", "Resolved");
        var closedItem = CreateWorkItem(101, "Closed item", "Bug", "Closed");
        var doneItem = CreateWorkItem(102, "Done item", "Bug", "Done");

        // Act & Assert
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(resolvedItem), "Resolved state should be resolved");
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(closedItem), "Closed state should be resolved");
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(doneItem), "Done state should be resolved");
    }

    /// <summary>
    ///     Verify that IsWorkItemResolved returns false for an active work item.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_IsWorkItemResolved_ActiveState_ReturnsFalse()
    {
        // Arrange
        var activeItem = CreateWorkItem(100, "Active item", "Bug", "Active");
        var newItem = CreateWorkItem(101, "New item", "Bug", "New");

        // Act & Assert
        Assert.IsFalse(WorkItemMapper.IsWorkItemResolved(activeItem), "Active state should not be resolved");
        Assert.IsFalse(WorkItemMapper.IsWorkItemResolved(newItem), "New state should not be resolved");
    }

    /// <summary>
    ///     Verify that GetWorkItemTypeForRuleMatching returns the raw work item type name.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_GetWorkItemTypeForRuleMatching_ReturnsWorkItemTypeName()
    {
        // Arrange
        var bugItem = CreateWorkItem(100, "Bug item", "Bug", "Active");
        var storyItem = CreateWorkItem(101, "Story item", "User Story", "Active");

        // Act
        var bugType = WorkItemMapper.GetWorkItemTypeForRuleMatching(bugItem);
        var storyType = WorkItemMapper.GetWorkItemTypeForRuleMatching(storyItem);

        // Assert
        Assert.AreEqual("Bug", bugType);
        Assert.AreEqual("User Story", storyType);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-CustomFields
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Custom.Visibility field returns mapped controls.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomVisibilityField_ReturnsMappedControls()
    {
        // Arrange - work item with Custom.Visibility field
        var workItem = CreateWorkItem(200, "Test item", "User Story", "Active",
            customVisibility: "internal");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert
        Assert.IsNotNull(controls);
        Assert.AreEqual("internal", controls.Visibility);
    }

    /// <summary>
    ///     Verify that Custom.AffectedVersions field returns mapped version set.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomAffectedVersionsField_ReturnsMappedVersionSet()
    {
        // Arrange - work item with Custom.AffectedVersions field
        var workItem = CreateWorkItem(200, "Test item", "Bug", "Active",
            customAffectedVersions: "(,1.0.1]");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert
        Assert.IsNotNull(controls);
        Assert.IsNotNull(controls.AffectedVersions);
        Assert.IsNotEmpty(controls.AffectedVersions.Intervals);
    }

    /// <summary>
    ///     Verify that custom fields take precedence over buildmark blocks.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomFieldsTakePrecedenceOverBuildmarkBlock()
    {
        // Arrange - work item with BOTH a buildmark block saying "public" AND a custom field saying "internal"
        var description = "Description\n```buildmark\nvisibility: public\n```";
        var workItem = CreateWorkItem(200, "Test item", "Bug", "Active",
            description: description,
            customVisibility: "internal");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert - custom field "internal" should take precedence over buildmark block "public"
        Assert.IsNotNull(controls);
        Assert.AreEqual("internal", controls.Visibility);
    }

    private static AzureDevOpsWorkItem CreateWorkItem(
        int id,
        string title,
        string workItemType,
        string state,
        string? description = null,
        string? customVisibility = null,
        string? customAffectedVersions = null)
    {
        var fields = new Dictionary<string, object?>
        {
            ["System.Title"] = title,
            ["System.WorkItemType"] = workItemType,
            ["System.State"] = state,
            ["System.Description"] = description
        };

        if (customVisibility != null)
        {
            fields["Custom.Visibility"] = customVisibility;
        }

        if (customAffectedVersions != null)
        {
            fields["Custom.AffectedVersions"] = customAffectedVersions;
        }

        return new AzureDevOpsWorkItem(id, fields);
    }
}
