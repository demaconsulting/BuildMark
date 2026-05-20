#### WorkItemMapper

##### Verification Approach

`WorkItemMapper` is tested through `WorkItemMapperTests.cs`, which contains 11 unit
tests. The tests verify mapping of Azure DevOps work items to the BuildMark model -
classification of features and bugs, type normalization, suppression of Removed work
items, resolved-state identification, rule-matching type retrieval, and custom field
extraction for visibility and affected-versions controls.

##### Dependencies

| Mock / Stub          | Reason                                                      |
| -------------------- | ----------------------------------------------------------- |
| `WorkItem` test data | Constructed in-line with specific types, states, and fields |

##### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

#### Test Scenarios

##### WorkItemMapper_MapWorkItemToItemInfo_BugType_ReturnsBugItem

**Scenario**: A work item with type `"Bug"` is mapped.

**Expected**: `ItemInfo.Type` is `"bug"`.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_MapWorkItemToItemInfo_UserStoryType_ReturnsFeatureItem

**Scenario**: A work item with type `"User Story"` is mapped.

**Expected**: `ItemInfo.Type` is `"feature"`.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_MapWorkItemToItemInfo_EpicType_ReturnsFeatureItem

**Scenario**: A work item with type `"Epic"` is mapped.

**Expected**: `ItemInfo.Type` is `"feature"`.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_MapWorkItemToItemInfo_TaskType_ReturnsTaskItem

**Scenario**: A work item with type `"Task"` is mapped.

**Expected**: `ItemInfo.Type` is the raw type name (`"Task"`), not a normalized type.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_IsWorkItemResolved_ResolvedState_ReturnsTrue

**Scenario**: Work items with states `Resolved`, `Closed`, and `Done` are checked.

**Expected**: `IsWorkItemResolved` returns `true` for each.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_IsWorkItemResolved_ActiveState_ReturnsFalse

**Scenario**: Work items with states `Active` and `New` are checked.

**Expected**: `IsWorkItemResolved` returns `false` for each.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_GetWorkItemTypeForRuleMatching_ReturnsWorkItemTypeName

**Scenario**: Work items with types `"Bug"` and `"User Story"` are queried for rule matching.

**Expected**: The raw `System.WorkItemType` value is returned unchanged.

**Requirement coverage**: `BuildMark-AzureDevOps-WorkItemMapper`

###### WorkItemMapper_MapWorkItemToItemInfo_RemovedState_ReturnsNull

**Scenario**: Work items with state `Removed` are mapped (one bug, one feature).

**Expected**: `MapWorkItemToItemInfo` returns `null` for both, suppressing them from
all sections of build notes.

**Requirement coverage**: `BuildMark-AzureDevOps-SuppressRemovedWorkItems`

###### WorkItemMapper_ExtractItemControls_CustomVisibilityField_ReturnsMappedControls

**Scenario**: A work item has a `Custom.Visibility` field set to `"internal"`.

**Expected**: `ExtractItemControls` returns controls with `Visibility` set to `"internal"`.

**Requirement coverage**: `BuildMark-AzureDevOps-CustomFields`

###### WorkItemMapper_ExtractItemControls_CustomAffectedVersionsField_ReturnsMappedVersionSet

**Scenario**: A work item has a `Custom.AffectedVersions` field set to a version range.

**Expected**: `ExtractItemControls` returns controls with a non-empty `AffectedVersions`
interval set.

**Requirement coverage**: `BuildMark-AzureDevOps-CustomFields`

###### WorkItemMapper_ExtractItemControls_CustomFieldsTakePrecedenceOverBuildmarkBlock

**Scenario**: A work item has both a buildmark block (visibility `"public"`) and a
`Custom.Visibility` field (visibility `"internal"`).

**Expected**: The custom field value (`"internal"`) takes precedence over the buildmark
block value.

**Requirement coverage**: `BuildMark-AzureDevOps-CustomFields`

##### Requirements Coverage

- **BuildMark-AzureDevOps-WorkItemMapper**: `WorkItemMapper_MapWorkItemToItemInfo_BugType_ReturnsBugItem`,
  `WorkItemMapper_MapWorkItemToItemInfo_UserStoryType_ReturnsFeatureItem`,
  `WorkItemMapper_MapWorkItemToItemInfo_EpicType_ReturnsFeatureItem`,
  `WorkItemMapper_MapWorkItemToItemInfo_TaskType_ReturnsTaskItem`,
  `WorkItemMapper_IsWorkItemResolved_ResolvedState_ReturnsTrue`,
  `WorkItemMapper_IsWorkItemResolved_ActiveState_ReturnsFalse`,
  `WorkItemMapper_GetWorkItemTypeForRuleMatching_ReturnsWorkItemTypeName` in `WorkItemMapperTests.cs`
- **BuildMark-AzureDevOps-SuppressRemovedWorkItems**:
  `WorkItemMapper_MapWorkItemToItemInfo_RemovedState_ReturnsNull` in `WorkItemMapperTests.cs`
- **BuildMark-AzureDevOps-CustomFields**:
  `WorkItemMapper_ExtractItemControls_CustomVisibilityField_ReturnsMappedControls`,
  `WorkItemMapper_ExtractItemControls_CustomAffectedVersionsField_ReturnsMappedVersionSet`,
  `WorkItemMapper_ExtractItemControls_CustomFieldsTakePrecedenceOverBuildmarkBlock` in `WorkItemMapperTests.cs`
