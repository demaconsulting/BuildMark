### ConfigurationLoadResult

#### Purpose

`ConfigurationLoadResult` is an immutable positional record that carries the output of
`BuildMarkConfigReader.ReadAsync`. It holds the parsed configuration (or `null` if parsing
failed or the file was absent) together with an ordered list of issues found during parsing.
`Program` calls `result.ReportTo(context)` immediately after reading configuration to surface
all issues to the user and set the process exit code when errors are present.

#### Data Model

**Config**: `BuildMarkConfig?` — Parsed configuration; `null` when parsing failed or the file
was absent.

**Issues**: `IReadOnlyList<ConfigurationIssue>` — Ordered list of issues encountered during
parsing; empty when none were found.

**HasErrors**: `bool` — Computed property; `true` when any entry in `Issues` has
`Severity == Error`.

#### Key Methods

**ReportTo(context)**: Writes all configuration issues to the supplied context and sets the
process exit code to `1` if any error-severity issue is present.

- *Parameters*: `Context context` — the CLI context that receives issue messages and holds the
  exit code.
- *Returns*: `void`.
- *Preconditions*: `context` must not be null.
- *Postconditions*: Each issue is written to `context` using `context.WriteError` for
  error-severity issues and `context.WriteLine` for warnings. Calling `context.WriteError`
  sets the context's internal error flag, causing `context.ExitCode` to return `1`. Does not
  throw exceptions.

Each issue is formatted as `{FilePath}:{Line}: {Severity}: {Description}` before being written
to the context.

#### Error Handling

N/A — `ConfigurationLoadResult` is an immutable record. `ReportTo` writes issues and may
indirectly set the process exit code to `1` via `context.WriteError` but does not throw
exceptions.

#### Dependencies

- **BuildMarkConfig** — held by `Config` when parsing succeeds.
- **ConfigurationIssue** — each entry in the `Issues` list.
- **Context** — consumed by `ReportTo` to write issue messages and set the exit code.

#### Callers

- **BuildMarkConfigReader** — creates `ConfigurationLoadResult` instances and returns them from
  `ReadAsync`.
- **Program** — calls `ReportTo(context)` and checks `HasErrors` before proceeding to connector
  selection and report generation.
