### ItemRouter

#### Purpose

`ItemRouter` is a static utility class that routes a list of `ItemInfo` objects into report
sections by applying a list of `RuleConfig` entries in order. It centralizes routing logic so that
all connectors — GitHub, Azure DevOps, and Mock — share a single implementation rather than each
maintaining their own.

Rules are evaluated in declaration order; the first matching rule wins. Items that do not match any
rule are placed in the default section, which is the first entry in the `sections` list, or
`"changes"` if the list is empty. A route value of `"suppressed"` (case-insensitive) causes an
item to be omitted from all sections. Sections not declared in `sections` are created dynamically
when a rule routes an item to an unknown section ID.

#### Data Model

N/A — `ItemRouter` is a static utility class with no instance state.

#### Key Methods

**Route**: Routes a list of `ItemInfo` objects into section buckets using the configured rules and
returns a dictionary keyed by section ID.

- *Parameters*: `IReadOnlyList<ItemInfo> items` — the items to distribute; `IReadOnlyList<RuleConfig>
  rules` — routing rules evaluated in order; `IReadOnlyList<SectionConfig> sections` — the ordered
  set of configured report sections.
- *Returns*: `Dictionary<string, List<ItemInfo>>` — maps each section ID to the items assigned to
  it; pre-populated for all configured section IDs, with dynamic entries for any rule-introduced
  section IDs not in `sections`.
- *Preconditions*: All arguments must be non-null; duplicate IDs in `sections` will cause an
  `ArgumentException` during dictionary initialization.
- *Postconditions*: Every item in `items` has been routed to exactly one section or suppressed; the
  returned dictionary contains all configured section IDs plus any dynamically created ones.

Rule matching: a `null` `Match` block is a catch-all that matches every item. A non-null `Match`
block may specify `Label` and/or `WorkItemType` filter lists; both are matched
case-insensitively against the item's `Type` field, and all non-empty filter lists must match for
the rule to apply.

#### Error Handling

No explicit error handling is performed. Callers are responsible for passing valid, non-null
arguments. Duplicate section IDs in `sections` result in an `ArgumentException` from the internal
dictionary initialization. Null inputs result in a `NullReferenceException` propagating to the
caller.

#### Dependencies

- **ItemInfo** — provides the input items to be routed; defined in the BuildNotes subsystem.
- **RuleConfig** — provides routing rules; defined in the Configuration subsystem.
- **SectionConfig** — provides section definitions; defined in the Configuration subsystem.

#### Callers

- **RepoConnectorBase** — calls `ItemRouter.Route` from within `ApplyRules` to distribute items
  into sections before assembling the ordered section list.
