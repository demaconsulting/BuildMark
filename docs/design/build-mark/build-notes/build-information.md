### BuildInformation

![BuildNotes Structure](../../generated/BuildNotesView.svg)

#### Purpose

`BuildInformation` is the top-level data record in the BuildNotes subsystem that holds all
data needed to produce one markdown build-notes report. It is assembled by repository
connectors and passed to `Program`, which calls `ToMarkdown` to render the final file.

#### Data Model

**BaselineVersionTag**: `VersionCommitTag?` — the previous version tag, which is the lower
boundary of the reported range; `null` when reporting from the beginning of history.

**CurrentVersionTag**: `VersionCommitTag` — the version tag being reported.

**Changes**: `List<ItemInfo>` — feature and other non-bug items in this build.

**Bugs**: `List<ItemInfo>` — bug-fix items in this build.

**KnownIssues**: `List<ItemInfo>` — open issues not yet fixed.

**CompleteChangelogLink**: `WebLink?` — optional link to the full changelog on the host;
`null` when not available.

**RoutedSections**: `IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>?` —
optional ordered list of custom report sections populated by `RepoConnectorBase.ApplyRules`
when routing rules are configured; `null` when no rules are active. This is an init-only
property set after record construction.

#### Key Methods

**ToMarkdown**: Renders the build information as a markdown string.

- *Parameters*: `int headingDepth` — root markdown heading depth (default `1`);
  `bool includeKnownIssues` — flag to include the Known Issues section (default `false`).
- *Returns*: `string` — fully formatted markdown report.
- *Preconditions*: All required record fields are populated by a connector.
- *Postconditions*: Returns a valid UTF-8 markdown string; record state is not mutated.

When `RoutedSections` is non-null and non-empty, renders each section from the
`RoutedSections` list instead of the legacy `Changes` and `Bugs` lists. Known issues are
always excluded from routing; when `includeKnownIssues` is `true`, a Known Issues section is
appended after the routed sections regardless of which rendering mode is active. When
`RoutedSections` is `null` or empty, falls back to the legacy Changes and Bugs Fixed sections.

The rendered output includes: (1) Version Information table; (2) custom routed sections or
legacy Changes/Bugs Fixed sections; (3) optional Known Issues section; (4) optional Full
Changelog link.

#### Error Handling

N/A — `BuildInformation` is an immutable data record. `ToMarkdown` renders content from
already-validated data and does not throw under normal operation.

#### Dependencies

- **VersionCommitTag** — carries version and commit hash for baseline and current entries
  (Version subsystem)
- **ItemInfo** — each item in `Changes`, `Bugs`, `KnownIssues`, and `RoutedSections`
- **WebLink** — optional complete-changelog hyperlink

#### Callers

- **RepoConnectors** — connectors assemble and return a `BuildInformation` record
- **Program** — calls `ToMarkdown` to produce the final report file
- **Validation** — creates `BuildInformation` records during self-tests to verify rendering
