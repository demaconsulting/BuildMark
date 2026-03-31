# Validation

## Overview

`Validation` is the sole unit in the SelfTest subsystem. It runs a fixed set of
self-tests that exercise the core functionality of BuildMark without requiring
network access or external tools beyond Git. Results are written to the console and,
optionally, to a TRX or JUnit XML results file.

The unit is invoked by `Program.Run` when the `--validate` flag is set.

## Data Model

`Validation` has no persistent state. All data is local to the `Run` method and
its helpers. Test results are accumulated in a list of `TestResult` records that
are written to a file at the end of the run.

### `TemporaryDirectory` Helper

A private nested class that creates a temporary directory on construction and
deletes it (with all contents) on disposal. Used to isolate test artifacts.

## Methods

### `Run(Context context)`

Entry point for self-validation. It:

1. Prints a header showing the OS, .NET runtime, and current timestamp.
2. Executes each self-test method in sequence.
3. Prints a summary of passed and failed tests.
4. If `context.ResultsFile` is set, writes the results in TRX or JUnit XML format
   (determined by the file extension `.trx` or `.xml`).
5. Writes any unsupported extension as an error.
6. Sets `context.ExitCode` to `1` if any test fails.

### `RunMarkdownReportGeneration`

Creates a `MockRepoConnector` with representative data, generates a
`BuildInformation` record, calls `ToMarkdown`, writes the output to a temporary
file, and verifies the file contains expected version and content markers.

### `RunGitIntegration`

Runs the tool against the local Git repository to verify that version, commit
hash, and previous version can be extracted correctly. Validates the output
against expected patterns.

### `RunIssueTracking`

Uses `MockRepoConnector` to verify that issues and pull requests are categorized
and collected correctly into the `Changes` and `Bugs` lists of `BuildInformation`.

### `RunKnownIssuesReporting`

Uses `MockRepoConnector` to verify that known issues are included in the markdown
report when the `--include-known-issues` flag is set.

## Interactions

| Unit / Subsystem    | Role                                                                                 |
|---------------------|--------------------------------------------------------------------------------------|
| `Context`           | Provides output methods, `ResultsFile`, and exit code sink                           |
| `MockRepoConnector` | Supplies deterministic data for all tests                                            |
| `BuildInformation`  | Target of the tests; validated against expected content                              |
| `PathHelpers`       | Used directly (e.g., `SafePathCombine`) to build temp, log, and report file paths   |
