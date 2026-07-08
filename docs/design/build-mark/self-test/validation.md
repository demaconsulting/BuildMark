### Validation

![SelfTest Structure](../../generated/SelfTestView.svg)

#### Purpose

`Validation` is the sole unit in the SelfTest subsystem. It runs a fixed set of self-tests that exercise
the core functionality of BuildMark without requiring network access or external tools beyond Git. Results
are written to the console and, optionally, to a TRX or JUnit XML results file.

The unit is invoked by `Program.Run` when the `--validate` flag is set.

#### Data Model

N/A — `Validation` is a static class with no instance state. All data is local to the `Run` method and
its helpers. Test results are accumulated in a `DemaConsulting.TestResults.TestResults` collection that
is written to file at the end of the run.

#### Key Methods

**`Run`**: Entry point for self-validation.

- *Parameters*: `Context context` — provides output methods, `ResultsFile` path, and the exit code sink.
- *Returns*: `void`
- *Preconditions*: `context` is non-null.
- *Postconditions*: All self-tests have run; summary printed; results file written if `context.ResultsFile`
  is set; `context.ExitCode` is 1 if any test failed.

Prints a header showing the BuildMark version, machine name, OS, .NET runtime, and current timestamp.
Executes each self-test method in sequence. Prints a summary of passed and failed tests. If
`context.ResultsFile` is set, writes results in TRX (`.trx`) or JUnit XML (`.xml`) format. Sets
`context.ExitCode` to 1 if any test fails.

**`RunMarkdownReportGeneration`** (private): Creates a `MockRepoConnector` with representative data, calls
`Program.Run` with `--build-version 2.0.0` and `--report`, and verifies the output file contains expected
version and content markers (`# Build Report`, `## Version Information`, `v2.0.0`, `mno345pqr678`).

**`RunGitIntegration`** (private): Uses `MockRepoConnector` to verify that version, commit hash, and
previous version information appear in the console log (`Build Version: v2.0.0`,
`Commit Hash: mno345pqr678`, `Previous Version: ver-1.1.0`).

**`RunIssueTracking`** (private): Uses `MockRepoConnector` to verify that `Changes:` and `Bugs Fixed:`
lines appear in the console log.

**`RunKnownIssuesReporting`** (private): Uses `MockRepoConnector` with `--include-known-issues` to verify
that `Known Issues: 2` appears in the log and `## Known Issues` appears in the report file.

**`RunRulesRouting`** (private): Creates a `MockRepoConnector` configured with routing rules (`bug` label
→ `bugs` section; all others → `features` section). Verifies that the report file contains `## Features`
and `## Bugs` section headings.

#### Error Handling

If `--results` is provided with an unsupported file extension (not `.trx` or `.xml`), `Validation.Run`
writes an error message via `context.WriteError` and returns without writing a file. No exception is
propagated to the caller. Individual test exceptions are caught within the shared test-execution helper,
marked as failures, and reported via `context.WriteError`.

#### Dependencies

- **`Context`** — provides output methods, `ResultsFile` path, and the exit code sink (Cli subsystem)
- **`MockRepoConnector`** — supplies deterministic repository data for all tests
  (RepoConnectors/Mock subsystem)
- **`BuildInformation`** — returned by the mock connector; validated against expected content
  (BuildNotes subsystem)
- **`Program`** — `Run` is called within each test to exercise the full program execution path
- **`TemporaryDirectory`** — provides temporary directory management for test artifact isolation
  (Utilities subsystem)
- **`DemaConsulting.TestResults`** — provides `TestResults`, `TestResult`, `TrxSerializer`, and
  `JUnitSerializer` for results file output
- **`RuleConfig`, `RuleMatchConfig`, `SectionConfig`** — used in `RunRulesRouting` to configure routing
  rules (Configuration subsystem)

#### Callers

- **`Program`** — calls `Validation.Run(context)` when the `--validate` flag is set
