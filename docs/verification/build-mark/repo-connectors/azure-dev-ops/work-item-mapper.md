#### WorkItemMapper

##### Verification Approach

`WorkItemMapper` is tested through `WorkItemMapperTests.cs`, which contains 10 unit
tests. The tests verify mapping of Azure DevOps work items to the BuildMark model -
classification of features and bugs, title and description extraction, change link
generation, and handling of known issue identification based on work item type and
state.

##### Dependencies

| Mock / Stub          | Reason                                                      |
| -------------------- | ----------------------------------------------------------- |
| `WorkItem` test data | Constructed in-line with specific types, states, and fields |

##### Test Scenarios

###### WorkItemMapper_MapToItemInfo_Bug_ReturnsBugType

**Scenario**: A work item with type `"Bug"` is mapped.

**Expected**: `ItemInfo.Type` is bug.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_Feature_ReturnsFeatureType

**Scenario**: A work item with type `"Feature"` is mapped.

**Expected**: `ItemInfo.Type` is feature.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_UserStory_ReturnsFeatureType

**Scenario**: A work item with type `"User Story"` is mapped.

**Expected**: `ItemInfo.Type` is feature.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_Task_ReturnsOtherType

**Scenario**: A work item with type `"Task"` is mapped.

**Expected**: `ItemInfo.Type` is other (or a non-bug, non-feature classification).

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_ExtractsTitle

**Scenario**: A work item has a title.

**Expected**: Mapped `ItemInfo.Title` matches the work item title.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_ExtractsDescription

**Scenario**: A work item has a description.

**Expected**: Mapped `ItemInfo.Description` matches the work item description.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_MapToItemInfo_GeneratesWebLink

**Scenario**: A work item has an ID and a valid organization URL.

**Expected**: `ItemInfo.WebLink` contains the Azure DevOps work item URL.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_IsKnownIssue_OpenBug_ReturnsTrue

**Scenario**: Work item is an open bug.

**Expected**: `IsKnownIssue` returns `true`.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_IsKnownIssue_ClosedBug_ReturnsFalse

**Scenario**: Work item is a closed bug.

**Expected**: `IsKnownIssue` returns `false`.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

###### WorkItemMapper_IsKnownIssue_OpenFeature_ReturnsFalse

**Scenario**: Work item is an open feature (not a bug).

**Expected**: `IsKnownIssue` returns `false`.

**Requirement coverage**: `BuildMark-RepoConnectors-WorkItemMapper`

##### Requirements Coverage

- **BuildMark-RepoConnectors-WorkItemMapper**: All 10 tests in `WorkItemMapperTests.cs`
