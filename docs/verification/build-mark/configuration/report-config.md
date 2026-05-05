### ReportConfig

#### Verification Approach

`ReportConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml` files
with a `report:` block and assert that `File`, `Depth`, and `IncludeKnownIssues` properties are
correctly parsed or that invalid values produce error issues. No mocking is required.

#### Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

#### Test Scenarios

##### BuildMarkConfigReader_ReadAsync_ValidReportSection_ReturnsParsedReportConfig

**Scenario**: A `.buildmark.yaml` with `report.file`, `report.depth: 2`, and
`report.include-known-issues: true` is written; `BuildMarkConfigReader.ReadAsync` is called;
`Config.Report` is inspected.

**Expected**: `Config.Report.File` equals `"build-notes.md"`; `Config.Report.Depth` equals 2;
`Config.Report.IncludeKnownIssues` is true.

**Requirement coverage**: `BuildMark-ReportConfig-Properties`.

##### BuildMarkConfigReader_ReadAsync_InvalidReportDepth_ReturnsErrorIssue

**Scenario**: A `.buildmark.yaml` with `report.depth: -1` is written;
`BuildMarkConfigReader.ReadAsync` is called.

**Expected**: `Config` is null; `HasErrors` is true; issue description contains
`"positive integer"`.

**Requirement coverage**: `BuildMark-ReportConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-ReportConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidReportSection_ReturnsParsedReportConfig
  - BuildMarkConfigReader_ReadAsync_InvalidReportDepth_ReturnsErrorIssue
