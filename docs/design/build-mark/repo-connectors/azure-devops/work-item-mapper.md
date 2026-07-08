#### WorkItemMapper

![AzureDevOps Structure](../../../generated/AzureDevOpsView.svg)

##### Purpose

`WorkItemMapper` maps `AzureDevOpsWorkItem` records from the Azure DevOps REST API into `ItemInfo`
records for the `BuildInformation` model. It centralizes work item type normalization, state-based
filtering, and item-controls extraction from both `buildmark` code blocks in work item description
bodies and Azure DevOps custom fields (`Custom.Visibility`, `Custom.AffectedVersions`). Custom
fields take precedence over `buildmark` block values when both are present.

##### Data Model

N/A — `WorkItemMapper` is a static utility class with no instance state.

##### Key Methods

**MapWorkItemToItemInfo**: Maps a single `AzureDevOpsWorkItem` to an `ItemInfo` record, returning
`null` when the work item should be excluded.

- *Parameters*: `AzureDevOpsWorkItem workItem` — the work item to map.
- *Returns*: `ItemInfo?` — a populated record, or `null` when the work item is in a suppressed
  state or has `visibility: internal`.
- *Preconditions*: `workItem` must be non-null.
- *Postconditions*: Returns `null` for suppressed states (e.g. `Removed`) and for
  `visibility: internal`; never throws for absent dictionary keys.

Steps: (1) read `System.State`; return `null` immediately if the state is in the suppressed set
(`Removed`); (2) read `System.Title` and `System.WorkItemType`; (3) apply work item type mapping
(`Bug`/`Issue` → `"bug"`; `User Story`/`Feature`/`Epic` → `"feature"`; others preserved as-is);
(4) call `ExtractItemControls` to obtain overrides; (5) return `null` if controls specify
`visibility: internal`; (6) apply type override if controls specify one; (7) construct and return
the `ItemInfo` record.

**IsWorkItemResolved**: Checks whether a work item's state is one of the known resolved states.

- *Parameters*: `AzureDevOpsWorkItem workItem` — the work item to check.
- *Returns*: `bool` — `true` when `System.State` is `Resolved`, `Closed`, or `Done`; `false`
  otherwise.

Used by `AzureDevOpsRepoConnector` to filter resolved bugs from known-issues reporting when no
`AffectedVersions` is declared.

**GetWorkItemTypeForRuleMatching**: Returns the raw `System.WorkItemType` value from the work item
fields dictionary for use in routing rule matching.

- *Parameters*: `AzureDevOpsWorkItem workItem` — the work item to inspect.
- *Returns*: `string?` — the raw Azure DevOps work item type string (e.g. `Bug`, `User Story`).

Allows routing rules in `.buildmark.yaml` to match on Azure DevOps-native work item type names
rather than the normalized type.

**ExtractItemControls**: Merges item controls from `buildmark` blocks and Azure DevOps custom
fields into a single `ItemControlsInfo?` record.

- *Parameters*: `AzureDevOpsWorkItem workItem` — the work item whose controls to extract.
- *Returns*: `ItemControlsInfo?` — merged controls, or `null` when neither source provides any
  recognized values.

Steps: (1) call `ItemControlsParser.Parse(System.Description)`; (2) read `Custom.Visibility` and
`Custom.AffectedVersions` from the fields dictionary; (3) override the corresponding `buildmark`
block values with the custom field values when both are present; (4) return the merged
`ItemControlsInfo`, or `null` if no controls were found.

##### Error Handling

`MapWorkItemToItemInfo` returns `null` rather than throwing when a work item should be excluded
(suppressed state or `visibility: internal`). Missing or unexpected field values in the work item's
`Fields` dictionary are handled defensively; no exceptions are thrown for absent keys.

##### Dependencies

- **AzureDevOpsApiTypes** — provides the `AzureDevOpsWorkItem` input type.
- **ItemControlsParser** — called by `ExtractItemControls` to parse `buildmark` blocks from work
  item descriptions.
- **ItemControlsInfo** — the record type returned by `ItemControlsParser.Parse` and used within
  `ExtractItemControls`.
- **ItemInfo** — the output record type populated by `MapWorkItemToItemInfo`.
- **VersionIntervalSet** — held by `ItemControlsInfo.AffectedVersions` and stored on the returned
  `ItemInfo`.

##### Callers

- **AzureDevOpsRepoConnector** — calls `MapWorkItemToItemInfo`, `IsWorkItemResolved`, and
  `GetWorkItemTypeForRuleMatching` when processing REST API work item responses.
