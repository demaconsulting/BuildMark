# ItemRouter

## Overview

`ItemRouter` is a shared static utility in the RepoConnectors subsystem that routes
a list of `ItemInfo` objects into report sections. It applies a list of `RuleConfig`
entries to determine which section each item belongs to, avoiding duplication of
routing logic across multiple connector implementations.

All connectors (GitHub, Azure DevOps, Mock) call `ItemRouter` rather than
each implementing their own routing.

## Methods

### `Route(items, rules, sections) → Dictionary<string, List<ItemInfo>>`

Takes a list of `ItemInfo` objects, a list of `RuleConfig` entries, and a list of
`SectionConfig` entries, and returns a dictionary mapping each section ID to the
items assigned to that section.

- `items` (`IReadOnlyList<ItemInfo>`) - items to be distributed into sections
- `rules` (`IReadOnlyList<RuleConfig>`) - routing rules that map item attributes to
  sections
- `sections` (`IReadOnlyList<SectionConfig>`) - ordered list of report sections

#### Algorithm

Rules are evaluated in order; the first matching rule wins. Items that do not match
any rule are placed in the **default section**, which is the first entry in the
`sections` list, or `"changes"` if the list is empty.

A route value of `"suppressed"` (case-insensitive) causes the item to be omitted
entirely from all sections.

Sections not present in the configured `sections` list are created dynamically
when a rule routes an item to an unknown section ID. This allows rules to introduce
ad-hoc sections without requiring them to be pre-declared.

#### Rule matching

- A `null` `Match` block is a **catch-all** - the rule matches every item.
- A non-null `Match` block may specify `Label` and/or `WorkItemType` filter lists.
  Both lists are matched case-insensitively against the item's `Type` field.
  All non-empty filter lists must match for the rule to apply.

#### Error Handling

No explicit error handling is performed. Callers are responsible for passing valid, non-null
arguments. Duplicate section IDs in the `sections` list will cause an `ArgumentException` from
the internal dictionary initialization. Null inputs will result in a `NullReferenceException`
propagating to the caller.

## Interactions

- `ItemInfo` provides the input items to be routed from the BuildNotes
  subsystem.
- `RuleConfig` provides routing rules from the Configuration subsystem.
- `SectionConfig` provides section definitions from the Configuration subsystem.
- `RepoConnectorBase.ApplyRules` calls `ItemRouter.Route` to assign items to report sections.
- `GitHubRepoConnector`, `AzureDevOpsRepoConnector`, and `MockRepoConnector` call `ApplyRules` when rules are configured.
