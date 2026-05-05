### RuleMatchConfig

#### Verification Approach

`RuleMatchConfig` is a data model class verified indirectly through `ItemRouterTests.cs`.
Tests that exercise label-based and type-based matching use `RuleMatchConfig` instances
within `RuleConfig` to control item routing. Case-insensitive matching tests confirm
the match comparison behavior.

#### Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

#### Test Scenarios (Integration)

##### ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem

**Scenario**: A `RuleMatchConfig` specifying a work item type is used; item with
matching type is routed.

**Expected**: Item appears in the correct section.

**Requirement coverage**: `BuildMark-Configuration-RuleMatchConfig`

##### ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem

**Scenario**: A `RuleMatchConfig` specifying a label is used; item with matching
label (case-insensitive) is routed.

**Expected**: Item appears in the correct section regardless of label case.

**Requirement coverage**: `BuildMark-Configuration-RuleMatchConfig`

##### ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem

**Scenario**: A `RuleMatchConfig` on a suppressed rule matches an item case-insensitively.

**Expected**: Item is omitted from all sections.

**Requirement coverage**: `BuildMark-Configuration-RuleMatchConfig`

#### Requirements Coverage

- **BuildMark-Configuration-RuleMatchConfig**:
  ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem,
  ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem,
  ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem
