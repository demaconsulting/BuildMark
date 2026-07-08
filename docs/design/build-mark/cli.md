## Cli Subsystem

![Cli Structure](../generated/CliView.svg)

### Overview

The Cli subsystem provides the command-line interface layer for BuildMark. It parses arguments
supplied by the user, routes output to the console or a log file, and exposes the parsed state to
the rest of the system via the `Context` object.

The subsystem contains one unit:

- **`Context`** (`Cli/Context.cs`) — argument parsing, output routing, and exit-code tracking

The Cli subsystem has no dependencies on other BuildMark subsystems. All other subsystems receive a
`Context` from the caller rather than creating one themselves.

### Interfaces

**`Context.Create`**: Factory method for constructing a `Context` from command-line arguments.

- *Type*: In-process .NET static method
- *Role*: Provider — the Cli subsystem exposes this factory for `Program`.
- *Contract*: `Create(string[] args) → Context` and `Create(string[] args, Func<IRepoConnector>?) → Context`;
  parses the argument array, opens any specified log file, and returns a fully initialized `Context`.
- *Constraints*: Throws `ArgumentException` for invalid or unrecognized flags; throws
  `InvalidOperationException` if the log file cannot be opened.

**`Context` output and control methods**: Instance methods consumed by all subsystems for writing
output and tracking errors.

- *Type*: In-process .NET instance methods
- *Role*: Provider
- *Contract*: `WriteLine(message)` writes to console and log; `WriteError(message)` writes to standard
  error and sets `ExitCode` to 1; `Dispose()` flushes and closes the log file.
- *Constraints*: `Context` implements `IDisposable`; callers must dispose after use.

### Design

The Cli subsystem contains a single unit (`Context`), so there is no inter-unit collaboration to
describe. `Context.Create` parses the argument array using the private `ArgumentParser` nested class
and opens the optional log file. The resulting `Context` object is passed by `Program` to every other
subsystem that needs to write output, read flags, or set the exit code. No other subsystem creates a
`Context`; all output and exit-code management flows through the single instance created at startup.
