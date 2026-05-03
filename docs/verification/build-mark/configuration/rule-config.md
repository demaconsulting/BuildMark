# RuleConfig

## Verification Approach

`RuleConfig` is a data model class verified indirectly through `ItemRouterTests.cs`
and connector configuration tests. Tests that provide `RuleConfig` instances to
connectors or to `ItemRouter` exercise the routing logic and confirm that items
are routed to the correct sections.

## Dependencies

| Mock / Stub | Reason     |
| ----------- | ---------- |
| None        | Data class |

## Test Scenarios (Integration)

### ItemRouter_Route_MatchingRuleRoutesItemToConfiguredSection

**Scenario**: A `RuleConfig` with a matching condition routes an item to the
configured section.

**Expected**: Item appears in the correct section of the output.

**Requirement coverage**: `BuildMark-Configuration-RuleConfig`

### ItemRouter_Route_SuppressedRouteOmitsMatchingItem

**Scenario**: A `RuleConfig` with suppress set routes a matching item to be omitted.

**Expected**: Item does not appear in any section of the output.

**Requirement coverage**: `BuildMark-Configuration-RuleConfig`

### MockRepoConnector_Configure_StoresRulesAndSections

**Scenario**: `MockRepoConnector.Configure` is called with `RuleConfig` entries.

**Expected**: Rules are stored; `HasRules` returns `true`.

**Requirement coverage**: `BuildMark-Configuration-RuleConfig`

## Requirements Coverage

- **BuildMark-Configuration-RuleConfig**:
  ItemRouter_Route_MatchingRuleRoutesItemToConfiguredSection,
  ItemRouter_Route_SuppressedRouteOmitsMatchingItem,
  MockRepoConnector_Configure_StoresRulesAndSections
