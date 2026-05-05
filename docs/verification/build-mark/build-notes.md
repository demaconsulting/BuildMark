## BuildNotes

### Verification Approach

`BuildNotes` is the subsystem encompassing `BuildInformation`, `ItemInfo`, and `WebLink`. It is
verified with dedicated subsystem tests in `BuildNotesTests.cs`. The subsystem uses
`MockRepoConnector` to supply deterministic `BuildInformation` instances; no other mocking or test
doubles are required.

### Dependencies

| Mock / Stub         | Reason                                                               |
| ------------------- | -------------------------------------------------------------------- |
| `MockRepoConnector` | Provides deterministic `BuildInformation` for subsystem-level tests. |

### Test Scenarios

#### BuildNotes_ReportModel_GeneratesCorrectMarkdown

**Scenario**: `MockRepoConnector.GetBuildInformationAsync` is called with version `v2.0.0`; the
resulting `BuildInformation` is rendered via `ToMarkdown()`.

**Expected**: Markdown contains `# Build Report`, `## Version Information`, `## Changes`,
`## Bugs Fixed`, `v2.0.0`, and `ver-1.1.0`.

**Requirement coverage**: `BuildMark-BuildNotes-ReportModel`.

#### BuildNotes_ReportModel_IncludesKnownIssues

**Scenario**: `ToMarkdown(includeKnownIssues: true)` is called on a `BuildInformation` for `v2.0.0`
that has known issues.

**Expected**: Markdown contains `## Known Issues`, `Known bug A`, and `Known bug C`; `Known bug B`
(which has `affected-versions [5.0.0,)`) does not appear.

**Requirement coverage**: `BuildMark-BuildNotes-ReportModel`.

#### BuildNotes_ReportModel_IncludesFullChangelog

**Scenario**: `ToMarkdown()` is called on a `BuildInformation` for `v2.0.0` that has a changelog
link.

**Expected**: Markdown contains `## Full Changelog` and the comparison link text
`ver-1.1.0...v2.0.0`.

**Requirement coverage**: `BuildMark-BuildNotes-ReportModel`.

### Requirements Coverage

- **`BuildMark-BuildNotes-ReportModel`**:
  - BuildNotes_ReportModel_GeneratesCorrectMarkdown
  - BuildNotes_ReportModel_IncludesKnownIssues
  - BuildNotes_ReportModel_IncludesFullChangelog
