### RuleMatchConfig

#### Verification Approach

`RuleMatchConfig` is a data model unit with no external dependencies. It is verified indirectly
through `ItemRouterTests.cs`, which places `RuleMatchConfig` instances inside `RuleConfig` objects
and exercises label-based and work-item-type-based matching via `ItemRouter`. No mocking is
required; the real `ItemRouter` logic is used.

#### Test Environment

N/A - standard test environment.

#### Acceptance Criteria

- All `ItemRouterTests.cs` tests that exercise `RuleMatchConfig`-driven routing pass with zero
  failures.

#### Test Scenarios

**ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem**: A `RuleMatchConfig` specifying a
work-item type is placed in a `RuleConfig` and an item with that type is routed via `ItemRouter`.
The item must appear in the correct section, confirming that work-item-type matching is evaluated
correctly. This scenario is tested by `ItemRouter_Route_WithWorkItemTypeMatch_RoutesMatchingItem`.

**ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem**: A `RuleMatchConfig` specifying a
label is placed in a `RuleConfig` and an item carrying that label (in a different case) is routed
via `ItemRouter`. The item must appear in the correct section regardless of label case, confirming
that label matching is case-insensitive. This scenario is tested by
`ItemRouter_Route_WithCaseInsensitiveLabelMatch_RoutesItem`.

**ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem**: A `RuleMatchConfig` on
a suppressed rule matches an item case-insensitively via `ItemRouter`. The item must be omitted
from all sections, confirming that suppression works together with case-insensitive label matching.
This scenario is tested by
`ItemRouter_Route_WithCaseInsensitiveSuppressedRoute_OmitsMatchingItem`.
