#### MockRepoConnector

##### Purpose

`MockRepoConnector` is an in-memory implementation of `IRepoConnector` used for self-validation
via the `--validate` flag and for unit testing. It returns a fixed, deterministic dataset without
making any network or filesystem calls.

`MockRepoConnector` lives in production code — not in the test project — because the `--validate`
flag must work in any deployment without requiring a separate test assembly or external tooling.

##### Data Model

**_issueTitles**: `Dictionary<string, string>` — Maps issue ID to issue title for the hard-coded
in-memory dataset.

**_issueTypes**: `Dictionary<string, string>` — Maps issue ID to normalized type (e.g. `"bug"`,
`"feature"`, `"documentation"`).

**_pullRequestIssues**: `Dictionary<string, List<string>>` — Maps pull request ID to the list of
linked issue IDs.

**_tagHashes**: `Dictionary<string, string>` — Maps tag name to commit hash; provides the version
history for the mock repository.

**_openIssues**: `List<string>` — IDs of issues that remain open; used when no `AffectedVersions`
is declared to determine known-issue eligibility.

**_issueAffectedVersions**: `Dictionary<string, VersionIntervalSet>` — Maps issue ID to its
declared affected-version interval set; overrides open/closed status for known-issue determination
when present.

##### Key Methods

**GetBuildInformationAsync**: Resolves tag history and version information from internal
dictionaries, collects changes and known issues, and returns a fully populated `BuildInformation`
record.

- *Parameters*: `VersionTag? version` — optional target version; when omitted, the tag matching
  the hard-coded current commit hash is used.
- *Returns*: `Task<BuildInformation>` — deterministic build information record.
- *Preconditions*: None for the happy path; the internal dataset covers a representative set of
  versions, issues, and pull requests.
- *Postconditions*: Returns a non-null `BuildInformation`; throws `InvalidOperationException` when
  version resolution fails.

Steps: (1) build the `VersionTag` list from `_tagHashes`; (2) determine the target version using
`FindVersionIndex` (inherited from `RepoConnectorBase`) or the supplied `version` argument; (3)
determine the baseline version using `FindBaselineForRelease` or `FindBaselineForPreRelease`
(inherited); (4) collect changes from `_pullRequestIssues` entries whose linked issues fall in the
commit range between baseline and target; (5) collect known issues: for each issue in
`_issueTitles` of type `"bug"`, include it when `_issueAffectedVersions` contains the target
version or (if no `AffectedVersions` entry exists) when the issue appears in `_openIssues`; (6) if
routing rules are configured via `Configure`, call `ApplyRules` (inherited) to populate
`BuildInformation.RoutedSections`; otherwise use the legacy `Changes`, `Bugs`, and `KnownIssues`
categorization; (7) return the assembled `BuildInformation`.

##### Error Handling

`GetBuildInformationAsync` throws `InvalidOperationException` in two scenarios: (1) when no
version tags exist in `_tagHashes` and no version argument is provided; (2) when the hard-coded
current commit hash does not match any tag and no version argument is provided. These conditions
mirror the equivalent error paths in the production connectors, making the mock suitable for
testing error-handling code paths as well as the happy path.

##### Dependencies

- **RepoConnectorBase** — base class providing `Configure`, `HasRules`, `ApplyRules`,
  `FindVersionIndex`, `FindBaselineForPreRelease`, and `FindBaselineForRelease`.
- **VersionIntervalSet** — held in `_issueAffectedVersions`; used for known-issue determination.
- **BuildInformation** — the output record assembled and returned by this unit.
- **ItemInfo** — the normalized item representation stored in `BuildInformation`.

##### Callers

- **Validation** — instantiates `MockRepoConnector` directly and calls `Configure` and
  `GetBuildInformationAsync` as part of the `--validate` self-test.
