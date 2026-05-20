### BuildInformation

#### Purpose

`BuildInformation` is a record in the BuildNotes subsystem that holds all data
needed to produce one markdown build-notes report. It is assembled by connectors
and passed to `Program`, which calls `ToMarkdown` to render the final file.

#### Data Model

```csharp
public record BuildInformation(
    VersionCommitTag? BaselineVersionTag,
    VersionCommitTag CurrentVersionTag,
    List<ItemInfo> Changes,
    List<ItemInfo> Bugs,
    List<ItemInfo> KnownIssues,
    WebLink? CompleteChangelogLink);
```

- `BaselineVersionTag` (`VersionCommitTag?`) - the previous version tag, which is the
  lower boundary of the reported range
- `CurrentVersionTag` (`VersionCommitTag`) - the version tag being reported
- `Changes` (`List<ItemInfo>`) - feature and other non-bug items in this build
- `Bugs` (`List<ItemInfo>`) - bug-fix items in this build
- `KnownIssues` (`List<ItemInfo>`) - open issues not yet fixed
- `CompleteChangelogLink` (`WebLink?`) - optional link to the full changelog on
  the host
- `RoutedSections` (`IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>?`) -
  optional ordered list of custom report sections populated by `RepoConnectorBase.ApplyRules`
  when routing rules are configured; `null` when no rules are active

#### Key Methods

##### `ToMarkdown(headingDepth, includeKnownIssues) → string`

Renders the build information as a markdown string. The `headingDepth` parameter
controls the top-level heading depth (default `1`), allowing the report to be
embedded at any level in a larger document. The `includeKnownIssues` flag controls
whether the Known Issues section is emitted.

When `RoutedSections` is non-null and non-empty, `ToMarkdown` renders each section
from the `RoutedSections` list (using `AppendRoutedSections`) instead of the legacy
`Changes` and `Bugs` lists. Known issues are **always excluded from routing** because
they are not linked to any commit in the build range; when `includeKnownIssues` is
`true`, a `## Known Issues` section is appended after the routed sections regardless
of which rendering mode is active. When `RoutedSections` is `null` or empty,
`ToMarkdown` falls back to the legacy sections.

The rendered output contains the following sections:

1. **Version Information** - baseline and current version tags with commit hashes.
2. **Custom sections from `RoutedSections`** *(when rules are configured)* - one
   sub-heading per section with the section title and its items.
   **OR** the following legacy sections *(when no rules are configured)*:
   - **Changes** - list of `ItemInfo` records from `Changes`.
   - **Bugs Fixed** - list of `ItemInfo` records from `Bugs`.
3. **Known Issues** *(optional)* - list of `ItemInfo` records from `KnownIssues`,
   emitted only when `includeKnownIssues` is `true`. Appears after routed sections
   in routed mode and after legacy sections in legacy mode.
4. **Full Changelog** *(optional)* - hyperlink from `CompleteChangelogLink`, emitted
   only when the link is non-null.

#### Error Handling

N/A — `BuildInformation` is an immutable data record. `ToMarkdown` renders content from
already-validated data and does not throw under normal operation.

#### Interactions

| Unit / Subsystem   | Role                                                              |
| :----------------- | :---------------------------------------------------------------- |
| `VersionCommitTag` | Carries version and commit hash for baseline and current entries  |
| `ItemInfo`         | Each item in `Changes`, `Bugs`, and `KnownIssues`                 |
| `WebLink`          | Optional complete-changelog hyperlink                             |
| `RepoConnectors`   | Connectors assemble and return a `BuildInformation` record        |
| `Program`          | Calls `ToMarkdown` to produce the final report file               |
