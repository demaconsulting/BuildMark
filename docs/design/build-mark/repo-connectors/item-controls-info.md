### ItemControlsInfo

#### Purpose

`ItemControlsInfo` is an immutable data record used by the RepoConnectors subsystem to carry the
controls extracted from a `buildmark` fenced code block. It stores the optional visibility
override, type override, and affected-version interval set that connectors apply while constructing
`ItemInfo` records.

#### Data Model

**Visibility**: `string?` — Optional visibility override; accepted values are `"public"` and
`"internal"`. A value of `"internal"` causes the item to be excluded from all report sections;
`"public"` forces inclusion regardless of label-derived type.

**Type**: `string?` — Optional type override; accepted values are `"bug"` and `"feature"`. Replaces
the label- or work-item-type-derived category when present.

**AffectedVersions**: `VersionIntervalSet?` — Optional parsed interval set representing the version
range in which a known issue applies. When present, the item is included as a known issue if and
only if the target version falls within the set.

#### Key Methods

N/A — `ItemControlsInfo` is an immutable C# `record` type. No methods beyond those auto-generated
by the C# record feature are defined.

#### Error Handling

N/A — This is an immutable data record with no methods that detect or propagate errors.

#### Dependencies

- **VersionIntervalSet** — carries the parsed interval representation for the `AffectedVersions`
  field.

#### Callers

- **ItemControlsParser** — creates an `ItemControlsInfo` instance when a `buildmark` block
  contains one or more recognized keys.
- **GitHubRepoConnector** — consumes the parsed values to override item visibility, type, and
  affected-version metadata.
- **WorkItemMapper** — consumes the parsed values (merged with Azure DevOps custom fields) to
  override item visibility, type, and affected-version metadata for work items.
