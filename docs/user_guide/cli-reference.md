<!-- markdownlint-disable MD025 -->

# Command-Line Options

| Option | Description |
| :----- | :---------- |
| `-v`, `--version` | Display version information and exit |
| `-h`, `-?`, `--help` | Display help message and exit |
| `--silent` | Suppress console output |
| `--log <file>` | Write output to a log file |
| `--validate` | Run self-validation tests |
| `--lint` | Validate `.buildmark.yaml` and exit |
| `--results <file>` | Write validation results (TRX or JUnit) |
| `--build-version <ver>` | Specify the build version for report generation |
| `--report <file>` | Export build notes to a markdown file |
| `--depth <n>` | Set markdown heading depth (default: 1) |
| `--include-known-issues` | Include known issues (open bugs, plus closed bugs with matching affected-versions). |

## Display Options

### `--version`, `-v`

Display version information and exit.

```bash
buildmark --version
```

### `--help`, `-h`, `-?`

Display help message with all available options.

```bash
buildmark --help
```

## Output Control

### `--silent`

Suppress console output. Useful in automated scripts where only the exit code matters.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --silent
```

### `--log <file>`

Write all output to a log file in addition to console output.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --log analysis.log
```

## Build Version Options

### `--build-version <version>` (Required for report generation)

Specify the build version for which to generate the report. This should be a version tag in your Git repository
(e.g., `v1.2.3`, `1.2.3`).

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

BuildMark will automatically find the previous version tag and generate a report covering all changes between
the two versions.

## Report Generation

### `--report <file>`

Export build notes to a markdown file. The file will contain version information, changes, bug fixes, and
optionally known issues.

```bash
buildmark --build-version v1.2.3 --report build-notes.md
```

### `--depth <depth>`

Set the markdown header depth for the report and self-validation output. Default is 1. Use this when embedding
the report in larger documents.

```bash
# Use level 2 headers (##) instead of level 1 (#)
buildmark --build-version v1.2.3 --report build-notes.md --depth 2
```

### `--include-known-issues`

Include known issues in the generated report. Known issues are open bugs in the repository,
plus closed bugs whose `affected-versions` field intersects the current build version.

```bash
buildmark --build-version v1.2.3 --report build-notes.md --include-known-issues
```

## Self-Validation

Self-validation produces a report demonstrating that BuildMark is functioning correctly. This is useful in
regulated industries where tool validation evidence is required.

### Running Validation

To perform self-validation:

```bash
buildmark --validate
```

To save validation results to a file:

```bash
buildmark --validate --results results.trx
```

The results file format is determined by the file extension: `.trx` for TRX (MSTest) format,
or `.xml` for JUnit format.

### Validation Report

The validation report contains the tool version, machine name, operating system version,
.NET runtime version, timestamp, and test results.

Example validation report:

```text
# DEMA Consulting BuildMark Self-validation

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| BuildMark Version   | 1.0.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Ubuntu 22.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.0                                        |
| Time Stamp          | 2024-01-15 10:30:00 UTC                            |

✓ BuildMark_MarkdownReportGeneration - Passed
✓ BuildMark_GitIntegration - Passed
✓ BuildMark_IssueTracking - Passed
✓ BuildMark_KnownIssuesReporting - Passed
✓ BuildMark_RulesRouting - Passed

Total Tests: 5
Passed: 5
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`BuildMark_MarkdownReportGeneration`** - Markdown report is correctly generated from mock data.
- **`BuildMark_GitIntegration`** - Git repository connector reads version tags and commits.
- **`BuildMark_IssueTracking`** - GitHub issue and pull request tracking works correctly.
- **`BuildMark_KnownIssuesReporting`** - Known issues are correctly included when requested.
- **`BuildMark_RulesRouting`** - Rules-based item routing assigns items to the correct report sections.
