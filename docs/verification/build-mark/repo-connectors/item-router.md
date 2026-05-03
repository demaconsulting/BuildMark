# ItemRouter

## Verification Approach

`ItemRouter` is tested through `ItemRouterTests.cs`, which contains 8 unit tests.
The tests cover matching rules (with and without a match block), suppression rules,
type-based matching, label-based matching (case-insensitive), routing to new sections,
and default section fallback.

## Dependencies

| Mock / Stub      | Reason                                                       |
| ---------------- | ------------------------------------------------------------ |
| `ItemInfo` stubs | Constructed with specific labels and types for routing tests |
| `SectionConfig`  | Provided as test input to define available sections          |
| `RuleConfig`     | Provided as test input to define routing rules               |

## Test Scenarios

### ItemRouter_Route_MatchingRuleRoutesItemToConfiguredSection

**Scenario**: An item matching a configured rule's label is routed.

**Expected**: Item appears in the section specified by the rule.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_SuppressedRouteOmitsMatchingItem

**Scenario**: An item matching a suppressed rule is processed.

**Expected**: Item is not placed in any section.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_WithNullMatchBlock_MatchesAllItems

**Scenario**: A rule with a `null` match block is configured; all items are tested.

**Expected**: All items match the rule (null match = match all).

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem

**Scenario**: A rule matching a specific work item type is applied.

**Expected**: Items of the matching type are routed to the configured section.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_WithNoMatchingRule_RoutesToDefaultSection

**Scenario**: An item that does not match any rule is processed.

**Expected**: Item is placed in the default section.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_ItemNotInConfiguredSections_CreatesNewSection

**Scenario**: A rule routes an item to a section name not in `SectionConfig`.

**Expected**: A new section is created dynamically for the item.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem

**Scenario**: Rule matches a label `"bug"` and item has label `"Bug"`.

**Expected**: Case-insensitive match routes the item correctly.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

### ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem

**Scenario**: Suppressed rule matches a label case-insensitively.

**Expected**: Item is omitted from all sections.

**Requirement coverage**: `BuildMark-RepoConnectors-ItemRouter`

## Requirements Coverage

- **BuildMark-RepoConnectors-ItemRouter**: All 8 tests in `ItemRouterTests.cs`
