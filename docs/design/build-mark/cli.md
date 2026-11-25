## Cli Subsystem

![Cli Structure](CliView.svg)

### Overview

The Cli subsystem provides the command-line interface layer for BuildMark. It parses arguments
supplied by the user, routes output to the console or a log file, and exposes the parsed state to
the rest of the system via the `Context` object. It also answers the built-in `help` command.

The subsystem contains two units:

- **`Context`** (`Cli/Context.cs`) — argument parsing, output routing, and exit-code tracking
- **`HelpCommand`** (`Cli/HelpCommand.cs`) — the built-in `help` command, answered from embedded
  help cards

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

**`HelpCommand.TryRun`**: Answers the `help` command from the built-in help cards.

- *Type*: In-process .NET instance method (on the shared `HelpCommand.Default` instance)
- *Role*: Provider
- *Contract*: Returns `false`, writing nothing, when the first argument is not `help`. Otherwise
  writes the answer to the console, returns `true`, and sets the exit code (see *Help Command* in
  the system design).
- *Constraints*: Does not throw for user input; does not use `Context`.

**`HelpCommand.Footer`**: The sentence that points a reader of the usage text to the help command.

- *Type*: In-process .NET instance property
- *Role*: Provider
- *Contract*: Returns the one-line pointer that HelpCards supplies.
- *Constraints*: Reading it never builds the help catalog.

### Design

`Context` and `HelpCommand` do not call each other. `Program.Main` offers the arguments to
`HelpCommand.TryRun` first; when the help command answers, the invocation ends with its exit code
and no `Context` is created. `HelpCommand` writes its answers to the console itself. It is the one
exception to `Context` owning all output, because the `help` command is answered before a `Context`
exists.

For every other invocation, `Context.Create` parses the argument array using the private
`ArgumentParser` nested class
and opens the optional log file. The resulting `Context` object is passed by `Program` to every other
subsystem that needs to write output, read flags, or set the exit code. No other subsystem creates a
`Context`; all output and exit-code management flows through the single instance created at startup.
