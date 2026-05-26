## BuildNotes

### Verification Approach

The BuildNotes subsystem is verified with dedicated subsystem tests in `BuildNotesTests.cs`. The
subsystem tests call `MockRepoConnector.GetBuildInformationAsync` with specific version tags to
obtain deterministic `BuildInformation` instances, then invoke `ToMarkdown` and assert on the
rendered output. `MockRepoConnector` provides all repository data; no other mocking or test doubles
are required. Unit-level tests for the individual units (`BuildInformation`, `ItemInfo`, `WebLink`)
are in `BuildInformationTests.cs`.

### Test Environment

N/A - standard test environment. `BuildNotesTests.cs` runs within the standard `dotnet test` host;
no external services, live network, or special configuration are required.

### Acceptance Criteria

- All tests in `BuildNotesTests.cs` pass with zero failures.
- All `BuildMark-BuildNotes-*` requirements have at least one test in the subsystem test file.

### Test Scenarios

**BuildNotes_ReportModel_GeneratesCorrectMarkdown**: Verifies that `BuildInformation` obtained from
`MockRepoConnector` for version `v2.0.0` renders markdown containing the expected section headings
(`# Build Report`, `## Version Information`, `## Changes`, `## Bugs Fixed`) and version strings
(`v2.0.0`, `ver-1.1.0`). This scenario is tested by
`BuildNotes_ReportModel_GeneratesCorrectMarkdown`.

**BuildNotes_ReportModel_IncludesKnownIssues**: Verifies that calling `ToMarkdown(includeKnownIssues: true)`
on a `BuildInformation` for `v2.0.0` produces a `## Known Issues` section containing `Known bug A`
and `Known bug C` but excluding `Known bug B` (whose affected-versions interval does not include
`v2.0.0`). This scenario is tested by `BuildNotes_ReportModel_IncludesKnownIssues`.

**BuildNotes_ReportModel_IncludesFullChangelog**: Verifies that rendering `BuildInformation` for
`v2.0.0` produces a `## Full Changelog` section containing the comparison link text
`ver-1.1.0...v2.0.0`. This scenario is tested by `BuildNotes_ReportModel_IncludesFullChangelog`.
