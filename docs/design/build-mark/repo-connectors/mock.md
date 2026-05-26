### Mock

#### Overview

The Mock subsystem groups the in-memory connector used by the built-in `--validate` self-test. It
sits within the RepoConnectors subsystem and contains a single unit, `MockRepoConnector`.

`MockRepoConnector` lives in production code — not in the test project — because the `--validate`
flag must work in any deployment without requiring a separate test assembly or external tooling.

The subsystem contains the following unit:

- `MockRepoConnector` — in-memory implementation of `IRepoConnector`; returns a deterministic
  `BuildInformation` record from hard-coded dictionaries without making any network or filesystem
  calls.

#### Interfaces

**MockRepoConnector**: The in-memory `IRepoConnector` implementation used for self-validation.

- *Type*: In-process .NET public API.
- *Role*: Provider — exposed to the `Validation` unit in the SelfTest subsystem.
- *Contract*: Default constructor `MockRepoConnector()` requires no arguments; inherits
  `Configure(rules, sections)` from `RepoConnectorBase`; `GetBuildInformationAsync(VersionTag?
  version)` returns a deterministic `BuildInformation` from hard-coded in-memory data.
- *Constraints*: Does not call any external processes or network endpoints; throws
  `InvalidOperationException` when no version tags exist in the hard-coded data and no version
  argument is provided, or when the current commit hash does not match any tag and no version
  argument is provided.

#### Design

The Mock subsystem contains a single unit, so there is no inter-unit collaboration to describe.
`MockRepoConnector` overrides `GetBuildInformationAsync` entirely with an in-memory implementation
that mirrors the production connector logic but operates on hard-coded dictionaries instead of live
API responses:

1. Build the list of `VersionTag` entries from `_tagHashes`.
2. Determine the target version tag using `FindVersionIndex` (inherited from `RepoConnectorBase`)
   or the supplied `version` argument.
3. Determine the baseline version tag (highest tag below the target for full releases; most recent
   tag with a different commit hash for pre-releases) using `FindBaselineForRelease` or
   `FindBaselineForPreRelease` (both inherited from `RepoConnectorBase`).
4. Collect changes from `_pullRequestIssues` whose linked issues fall in the commit range between
   baseline and target.
5. Collect known issues from `_issueTitles`: if an issue has an entry in
   `_issueAffectedVersions`, include it only when `AffectedVersions.Contains(targetVersion)`;
   otherwise include it only if it appears in `_openIssues`.
6. If routing rules are configured via `Configure`, call `ApplyRules` (inherited from
   `RepoConnectorBase`) to produce `BuildInformation.RoutedSections`; otherwise use the legacy
   `Changes`, `Bugs`, and `KnownIssues` categorization.
7. Return the assembled `BuildInformation` record.
