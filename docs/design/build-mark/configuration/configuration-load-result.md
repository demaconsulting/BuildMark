### ConfigurationLoadResult

#### Purpose

`ConfigurationLoadResult` is an immutable record that carries the output of
`BuildMarkConfigReader.ReadAsync`. It holds the parsed configuration (or `null` if parsing
failed) together with an ordered list of issues found during parsing.

`Program` calls `result.ReportTo(context)` immediately after reading the configuration to
surface any issues to the user and set the exit code when errors are present.

#### Data Model

| Member              | Kind     | Description                                              |
|---------------------|----------|----------------------------------------------------------|
| `Config`            | Property | Parsed `BuildMarkConfig`; `null` if parsing failed       |
| `Issues`            | Property | Ordered list of `ConfigurationIssue` objects             |
| `HasErrors`         | Property | `true` when any issue has `Severity` of `Error`          |
| `ReportTo(context)` | Method   | Writes all issues to `Context`; sets exit code on errors |

##### ReportTo Overview

Iterates `Issues` and writes each one to the context output. If any issue has severity
`Error`, sets `context.ExitCode` to 1.

#### Key Methods

##### `ReportTo(Context context)`

Iterates `Issues` and writes each one to the context output. Issues with `Error` severity
are written via `context.WriteError`, which sets the exit code to `1`. Non-error issues are
written via `context.WriteLine`. Does not throw exceptions.

#### Error Handling

N/A — `ConfigurationLoadResult` is an immutable record. `ReportTo(context)` writes issues
to the context output and may set the exit code to `1`, but does not throw exceptions.

#### Interactions

| Unit / Subsystem        | Role                                                                    |
|-------------------------|-------------------------------------------------------------------------|
| `BuildMarkConfigReader` | Produces `ConfigurationLoadResult` from `ReadAsync`                     |
| `BuildMarkConfig`       | Held by the `Config` property when parsing succeeds                     |
| `ConfigurationIssue`    | Each issue in the `Issues` list                                         |
| `Program`               | Calls `ReportTo(context)` and checks `HasErrors` before proceeding      |
