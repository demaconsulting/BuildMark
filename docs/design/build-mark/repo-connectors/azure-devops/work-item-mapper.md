#### WorkItemMapper

##### Purpose

`WorkItemMapper` maps `AzureDevOpsWorkItem` records from the Azure DevOps REST API
into `ItemInfo` records for the `BuildInformation` model. It centralizes work item
type normalization, state-based filtering, and item-controls extraction from both
buildmark code blocks in work item description bodies and Azure DevOps custom fields.

##### Data Model

###### Work Item Type Mapping

Azure DevOps work item types are mapped to normalized types for the `ItemInfo` model:

| Azure DevOps Work Item Types        | Normalized Type |
|-------------------------------------|-----------------|
| `Bug`, `Issue`                      | `"bug"`         |
| `User Story`, `Feature`, `Epic`     | `"feature"`     |
| `Task`, `Test Case`, etc.           | work item type  |

###### Work Item State Filtering

When identifying known issues, only work items in an unresolved state are included.
The following state values are treated as resolved and excluded from known-issues
reporting:

- `Resolved`
- `Closed`
- `Done`

The following state values are **suppressed** entirely from all sections of build
notes (both changes and known issues). `MapWorkItemToItemInfo` returns `null` for
these states before any other processing:

- `Removed`

All other state values (e.g. `Active`, `New`, `In Progress`) are treated as
unresolved and included in known-issues reporting.

###### Item Controls Extraction

Item controls are extracted from two sources and merged:

1. **Buildmark blocks** - `ItemControlsParser.Parse(description)` is called on the
   `System.Description` field of the work item. The resulting `ItemControlsInfo`
   provides `Visibility`, `Type`, and `AffectedVersions` overrides from embedded
   YAML blocks in the description body.
2. **Custom fields** - The `Custom.Visibility` and `Custom.AffectedVersions` fields
   in the work item's fields dictionary are read directly.

**Precedence**: custom fields take priority over buildmark blocks when both are
present for the same control. If a custom field value is non-null, it supersedes
the corresponding value from the buildmark block.

##### Key Methods

###### `MapWorkItemToItemInfo(workItem)`

Maps a single `AzureDevOpsWorkItem` to an `ItemInfo` record.

Steps:

1. Read `System.State` from the work item's fields dictionary. If the state is in the
   suppressed-states set (e.g. `Removed`), return `null` immediately.
2. Read `System.Title` and `System.WorkItemType` from the work item's fields dictionary.
3. Apply work item type mapping to determine the normalized type.
4. Call `ExtractItemControls(workItem)` to obtain any item controls overrides.
5. If item controls specify a visibility of `internal`, return `null` to signal that
   the item should be excluded. Items with `visibility: public` (or no visibility
   override) are included normally; there is no separate "force include" logic.
6. If item controls specify a type override, apply it to the normalized type.
7. Construct and return the `ItemInfo` record with the title, url, type, and
   affected versions.

###### `IsWorkItemResolved(workItem)`

Checks whether a work item's state is one of the known resolved states
(`Resolved`, `Closed`, `Done`).

Returns `true` if the work item is resolved; `false` otherwise. Used by
`AzureDevOpsRepoConnector` to filter out resolved work items when collecting
known issues.

###### `GetWorkItemTypeForRuleMatching(workItem)`

Returns the work item type string used for routing rule matching. This is the raw
`System.WorkItemType` value from the work item's fields dictionary, allowing routing
rules in `.buildmark.yaml` to match on Azure DevOps-native work item type names.

###### `ExtractItemControls(workItem)`

Combines item controls from both buildmark blocks and custom fields into a single
`ItemControlsInfo?` record.

Steps:

1. Call `ItemControlsParser.Parse(description)` on `System.Description`.
2. Read `Custom.Visibility` and `Custom.AffectedVersions` from the fields dictionary.
3. If custom fields are present, override the corresponding values from the buildmark
   block result.
4. Return the merged `ItemControlsInfo`, or `null` if no controls were found.

##### Error Handling

`MapWorkItemToItemInfo` returns `null` rather than throwing when a work item should be
excluded (suppressed state or `visibility: internal`). Missing or unexpected field values
are handled defensively; no exceptions are thrown for absent dictionary keys.

##### Interactions

- `AzureDevOpsRepoConnector` calls `WorkItemMapper` to convert REST API work item
  records into `ItemInfo` records.
- `ItemControlsParser` is called by `WorkItemMapper` to parse buildmark blocks
  from work item description bodies.
- `AzureDevOpsApiTypes` provides the `AzureDevOpsWorkItem` type that `WorkItemMapper`
  receives as input.
- `BuildInformation` consumes the `ItemInfo` records produced by `WorkItemMapper`.
