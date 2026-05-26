### RepoConnectorBase

#### Purpose

`RepoConnectorBase` is an abstract class that implements `IRepoConnector` and provides shared
utilities used by all concrete connector implementations. It stores routing rules and section
definitions supplied via `Configure`, exposes `ApplyRules` to route items through `ItemRouter`,
provides `FindVersionIndex`, `FindBaselineForPreRelease`, and `FindBaselineForRelease` for version
resolution, and exposes `RunCommandAsync` for delegating shell commands to `ProcessRunner`.

`IRepoConnector` defines the single public method contract: `GetBuildInformationAsync(VersionTag?
version)` returning `Task<BuildInformation>`. `RepoConnectorBase` declares this method `abstract`
so that each concrete connector supplies its own implementation while inheriting all shared
behavior.

#### Data Model

**_rules**: `IReadOnlyList<RuleConfig>` — Routing rules stored by `Configure`; defaults to an
empty list when `Configure` has not been called.

**_sections**: `IReadOnlyList<SectionConfig>` — Section definitions stored by `Configure`; defaults
to an empty list when `Configure` has not been called.

#### Key Methods

**Configure**: Stores routing rules and section definitions on the connector instance.

- *Parameters*: `IReadOnlyList<RuleConfig> rules` — routing rules to apply when distributing
  items; `IReadOnlyList<SectionConfig> sections` — section definitions that determine output
  structure.
- *Returns*: `void`.
- *Preconditions*: None; may be called at any point before `GetBuildInformationAsync`.
- *Postconditions*: `_rules` and `_sections` are updated; subsequent calls to `ApplyRules` use the
  newly stored values.

Called by `Program.ProcessBuildNotes` after the connector is created, passing `Rules` and
`Sections` from the loaded `.buildmark.yaml` configuration.

**HasRules**: Internal boolean property that returns `true` when at least one rule has been stored
via `Configure`.

- *Returns*: `bool` — `true` when `_rules.Count > 0`.

Used by concrete connectors in `GetBuildInformationAsync` to decide whether to call `ApplyRules`
or use legacy categorization.

**ApplyRules**: Routes the provided items using `ItemRouter.Route`, then assembles an ordered list
of `(SectionId, SectionTitle, Items)` tuples following the configured section order.

- *Parameters*: `IEnumerable<ItemInfo> allItems` — all items collected by the connector.
- *Returns*: `IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo>
  Items)>` — sections in configured order, with any extra rule-introduced sections appended.
- *Preconditions*: `Configure` should have been called with at least one rule.
- *Postconditions*: All items have been routed; the returned list preserves the configured section
  order.

Any items routed to section IDs not in the configured `sections` list are appended at the end
using the section ID as the display title.

**FindVersionIndex**: Protected static method that finds the index of a target version within a
list of `VersionTag` instances using semantic `VersionComparable` equality.

- *Parameters*: `List<VersionTag> versions` — the version list to search;
  `VersionTag targetVersion` — the version to find.
- *Returns*: `int` — zero-based index when found; `-1` when no semantically equivalent version
  exists.
- *Preconditions*: Neither argument may be null.
- *Postconditions*: Returns `-1` rather than throwing when no match is found.

Comparison uses `versions[i].Semantic.Comparable.Equals(targetVersion.Semantic.Comparable)`, so
tags with different prefixes but identical semantic versions are considered equal (e.g. `"v1.2.3"`
matches `"VER1.2.3"`).

**FindBaselineForPreRelease**: Internal static method that finds the most recent preceding version
with a different commit hash than the target, skipping entries that share the same commit
(which would produce an empty changelog).

- *Parameters*: `List<VersionCommitTag> precedingVersions` — version-commit tags that precede the
  target, ordered oldest first; `string targetCommitHash` — commit hash of the target version.
- *Returns*: `VersionCommitTag?` — the most recent preceding entry with a different commit hash,
  or `null` if none exists.

**FindBaselineForRelease**: Internal static method that finds the most recent preceding
non-pre-release version, skipping any pre-release entries.

- *Parameters*: `List<VersionCommitTag> precedingVersions` — version-commit tags that precede the
  target, ordered oldest first.
- *Returns*: `VersionCommitTag?` — the most recent preceding non-pre-release entry, or `null` if
  none exists.

**RunCommandAsync**: Protected virtual method that delegates shell commands to
`ProcessRunner.RunAsync`.

- *Parameters*: `string command` — the executable to run; `params string[] arguments` — arguments
  passed to the process.
- *Returns*: `Task<string>` — standard output of the command.
- *Preconditions*: The command must be available in the system PATH.
- *Postconditions*: Throws `InvalidOperationException` if the process exits with a non-zero code
  or produces no output.

Declared `virtual` so that test subclasses can override it without spawning real processes.

#### Error Handling

`RunCommandAsync` propagates `InvalidOperationException` from `ProcessRunner.RunAsync` when a
shell command fails. `FindVersionIndex` returns `-1` rather than throwing when no matching version
is found. `FindBaselineForPreRelease` and `FindBaselineForRelease` return `null` rather than
throwing when no suitable baseline exists. Error handling for `GetBuildInformationAsync` is
delegated to concrete implementations.

#### Dependencies

- **IRepoConnector** — the interface that `RepoConnectorBase` implements; defines
  `GetBuildInformationAsync`.
- **ProcessRunner** — used by `RunCommandAsync` to execute shell commands.
- **ItemRouter** — called by `ApplyRules` to route items into section buckets.
- **RuleConfig** — routing rules stored and applied by this unit.
- **SectionConfig** — section definitions stored and applied by this unit.
- **VersionTag** — used by `FindVersionIndex` for version list lookups.
- **VersionCommitTag** — used by `FindBaselineForPreRelease` and `FindBaselineForRelease` for
  baseline selection.
- **ItemInfo** — the item type passed to `ApplyRules`.

#### Callers

- **GitHubRepoConnector** — concrete subclass that calls `Configure`, `HasRules`, `ApplyRules`,
  `FindVersionIndex`, `FindBaselineForPreRelease`, `FindBaselineForRelease`, and `RunCommandAsync`.
- **AzureDevOpsRepoConnector** — concrete subclass with the same usage pattern.
- **MockRepoConnector** — concrete subclass that calls `Configure`, `HasRules`, `ApplyRules`,
  `FindVersionIndex`, `FindBaselineForPreRelease`, and `FindBaselineForRelease`.
- **Program** — calls `Configure` via the `IRepoConnector` reference before invoking
  `GetBuildInformationAsync`.
