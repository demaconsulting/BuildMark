### Context

![Cli Structure](../../generated/CliView.svg)

#### Purpose

`Context` is the sole unit in the Cli subsystem. It owns the lifecycle of command-line argument
parsing, console and log-file output, and exit-code tracking. `Program` creates exactly one
`Context` per invocation and passes it to all other units that need to write output or read
configuration flags.

`Context` implements `IDisposable`. Callers must dispose of the context after use so that any open
log file is properly flushed and closed.

#### Data Model

**`Version`**: `bool` — `true` when `-v` or `--version` is present; default `false`.

**`Help`**: `bool` — `true` when `-?`, `-h`, or `--help` is present; default `false`.

**`Silent`**: `bool` — `true` when `--silent` is present; default `false`.

**`Validate`**: `bool` — `true` when `--validate` is present; default `false`.

**`Lint`**: `bool` — `true` when `--lint` is present; default `false`.

**`IncludeKnownIssues`**: `bool` — `true` when `--include-known-issues` is present; default `false`.

**`BuildVersion`**: `string?` — value of `--build-version <version>`; `null` when not supplied.

**`ReportFile`**: `string?` — value of `--report <file>`; `null` when not supplied.

**`Depth`**: `int?` — value of `--depth <depth>` (also accepted as `--report-depth`); must be a
positive integer in the range 1–6; `null` when not supplied.

**`ResultsFile`**: `string?` — value of `--result <file>` or `--results <file>`; `null` when not
supplied.

**`ConnectorFactory`**: `Func<IRepoConnector>?` — optional factory injected via the testing overload
of `Create`; `null` in production.

**`ExitCode`**: `int` — returns 0 if no errors have been written; returns 1 after the first call to
`WriteError`.

#### Key Methods

**`Create(string[] args) → Context`**: Public factory method; constructs a new `Context` by
forwarding to `ArgumentParser`, then opens a log file if `--log` was specified.

- *Parameters*: `string[] args` — command-line arguments.
- *Returns*: `Context` — fully initialized context.
- *Preconditions*: None.
- *Postconditions*: All flags and arguments are parsed; log file is open if `--log` was specified.
- *Throws*: `ArgumentException` for invalid or unrecognized arguments; `InvalidOperationException`
  if the log file cannot be opened.

**`Create(string[] args, Func<IRepoConnector>? connectorFactory) → Context`**: Overload that
additionally stores a connector factory for dependency injection during testing.

- *Parameters*: `string[] args` — command-line arguments; `Func<IRepoConnector>? connectorFactory`
  — optional factory stored on `ConnectorFactory`.
- *Returns*: `Context` — fully initialized context.
- *Preconditions*: None.
- *Postconditions*: Same as the single-argument overload; `ConnectorFactory` is set when non-null.

**`WriteLine(string message)`**: Writes `message` to standard output (unless `Silent` is set) and to
the log file (if open).

- *Parameters*: `string message` — line to write.
- *Returns*: void.
- *Preconditions*: None.
- *Postconditions*: Message written to all enabled output channels.

**`WriteError(string message)`**: Writes `message` to standard error in red (unless `Silent` is set),
to the log file (if open), and sets the internal error flag so that `ExitCode` returns 1.

- *Parameters*: `string message` — error message to write.
- *Returns*: void.
- *Preconditions*: None.
- *Postconditions*: `ExitCode` returns 1 for this and all subsequent calls.

**`Dispose()`**: Flushes and closes the log file stream.

- *Parameters*: None.
- *Returns*: void.
- *Preconditions*: None.
- *Postconditions*: Log file stream is closed and released. Safe to call multiple times.

**`ArgumentParser` (private nested class)**: Used exclusively by `Create`. Iterates over the
argument array and classifies each token.

- Short flags (`-v`, `-h`, `-?`) and long flags (`--version`, `--help`, etc.) are mapped to boolean
  properties.
- Value arguments (`--build-version`, `--report`, `--depth`, `--results`, `--log`) consume the next
  token as their value; `ArgumentException` is thrown if no next token is present.
- `--depth` additionally validates that the value is a positive integer in the range 1–6.
- `--report-depth` is accepted as an undocumented alias for `--depth`.
- Any unrecognized token causes `ArgumentException` to be thrown.

#### Error Handling

`Create(string[] args)` throws `ArgumentException` when an unrecognized flag or missing value
argument is encountered, and `InvalidOperationException` if the log file path cannot be opened.
`WriteError` sets the internal error flag so that `ExitCode` returns 1 but does not throw.
`Dispose` is safe to call multiple times.

#### Dependencies

- **`IRepoConnector`** — the interface type used for the `ConnectorFactory` delegate (RepoConnectors
  subsystem)

#### Callers

- **`Program`** — creates exactly one `Context` per invocation via `Context.Create(args)` and passes
  it to all other subsystems
- **`Validation`** — receives `Context` as input parameter; writes output and reads `ResultsFile`
- **`BuildMarkConfigReader`** — receives `Context` to report configuration issues
- **`RepoConnectors`** — connector subsystems receive `Context` for logging and error reporting
