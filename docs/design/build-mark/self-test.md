## SelfTest Subsystem

![SelfTest Structure](SelfTestView.svg)

### Overview

The SelfTest subsystem provides a self-validation capability for BuildMark. When the user passes `--validate`,
the subsystem exercises the core functionality of the tool using a `MockRepoConnector` to avoid external API
calls. It prints a header table showing the BuildMark version, machine name, OS, .NET runtime, and current
timestamp, then prints a pass/fail summary and optionally writes a TRX or JUnit XML results file.

The subsystem contains one unit:

- **`Validation`** (`SelfTest/Validation.cs`) — runs self-tests and writes results to a file

The subsystem has no dependencies on the Cli subsystem beyond receiving a `Context` as its input parameter.

### Interfaces

**`Validation.Run`**: Entry point for self-validation, consumed by `Program`.

- *Type*: In-process .NET static method
- *Role*: Provider — the SelfTest subsystem exposes this method for `Program`.
- *Contract*: `Run(Context context) → void`; executes all self-tests using `MockRepoConnector` for
  deterministic data, prints a summary, and optionally writes TRX or JUnit XML results to
  `context.ResultsFile`.
- *Constraints*: Sets `context.ExitCode` to 1 if any test fails. If `--results` is specified with an
  unsupported extension, writes an error and returns without writing a file.

### Design

The SelfTest subsystem contains a single unit (`Validation`), so there is no inter-unit collaboration to
describe. `Validation.Run` executes each self-test method in sequence using a `MockRepoConnector` for
deterministic data, accumulates `DemaConsulting.TestResults.TestResult` records, and writes the results
file at the end of the run.

The five self-tests each exercise a distinct aspect of the tool:

| Test Name                            | What It Verifies                                                      |
|--------------------------------------|-----------------------------------------------------------------------|
| `BuildMark_MarkdownReportGeneration` | Markdown report is correctly generated from mock data                 |
| `BuildMark_GitIntegration`           | Git repository connector reads version tags and commits               |
| `BuildMark_IssueTracking`            | GitHub issue and pull request tracking works correctly                |
| `BuildMark_KnownIssuesReporting`     | Known issues are correctly included in the report when requested      |
| `BuildMark_RulesRouting`             | Rules-based item routing assigns items to the correct report sections |

Each test method creates its own `MockRepoConnector` instance, builds a `BuildInformation` record, and
validates the output against expected content. Tests are independent and share no mutable state.
