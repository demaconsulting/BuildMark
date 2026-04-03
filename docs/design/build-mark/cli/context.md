# Context

## Overview

`Context` is the sole unit in the Cli subsystem. It owns the lifecycle of
command-line argument parsing, console and log-file output, and exit-code tracking.
`Program` creates exactly one `Context` per invocation and passes it to all other
units that need to write output or read configuration.

`Context` implements `IDisposable`. Callers must dispose of the context after use
so that any open log file is properly flushed and closed.

## Data Model

### Parsed Flags

| Property             | Type   | Default | Source                   |
|----------------------|--------|---------|--------------------------|
| `Version`            | `bool` | `false` | `-v` / `--version`       |
| `Help`               | `bool` | `false` | `-?` / `-h` / `--help`   |
| `Silent`             | `bool` | `false` | `--silent`               |
| `Validate`           | `bool` | `false` | `--validate`             |
| `IncludeKnownIssues` | `bool` | `false` | `--include-known-issues` |

### Parsed Arguments

| Property           | Type                    | Default | Source                      |
|--------------------|-------------------------|---------|-----------------------------|
| `BuildVersion`     | `string?`               | `null`  | `--build-version <version>` |
| `ReportFile`       | `string?`               | `null`  | `--report <file>`           |
| `ReportDepth`      | `int`                   | `1`     | `--report-depth <depth>`    |
| `ResultsFile`      | `string?`               | `null`  | `--results <file>`          |
| `ConnectorFactory` | `Func<IRepoConnector>?` | `null`  | Injected via overload       |

### Derived State

| Property   | Type  | Description                                       |
|------------|-------|---------------------------------------------------|
| `ExitCode` | `int` | `0` if no errors have been written; `1` otherwise |

## Methods

### `Create(string[] args) → Context`

Public factory method. Constructs a new `Context` by forwarding to
`ArgumentParser`, then opens a log file if `--log` was specified.
Throws `ArgumentException` for invalid arguments; throws
`InvalidOperationException` if the log file cannot be opened.

### `Create(string[] args, Func<IRepoConnector>? connectorFactory) → Context`

Overload that additionally accepts a connector factory for dependency injection
during testing. The factory is stored on `ConnectorFactory` and used by
`Program.ProcessBuildNotes` instead of the default factory.

### `OpenLogFile(string logFile)`

Opens the specified file for writing in overwrite mode (truncating any existing
file) with `AutoFlush` enabled. If the directory does not exist, the method
throws `InvalidOperationException`.

### `WriteLine(string message)`

Writes `message` to standard output (unless `Silent` is set) and to the log file
(if open).

### `WriteError(string message)`

Writes `message` to standard error in red (unless `Silent` is set) and to the
log file (if open). Sets the internal error flag so that `ExitCode` returns `1`.

### `Dispose()`

Flushes and closes the log file stream. Safe to call multiple times.

## Inner Class: `ArgumentParser`

`ArgumentParser` is a private nested class used exclusively by `Create`. It
iterates over the argument array and classifies each token:

- Short flags (`-v`, `-h`, `-?`) and long flags (`--version`, `--help`, etc.)
  are mapped to boolean properties.
- Value arguments (`--build-version`, `--report`, `--report-depth`, `--results`,
  `--log`) expect the next token as their value and throw `ArgumentException` if
  no next token is present.
- `--report-depth` additionally validates that the value is a positive integer.
- Any unrecognized token causes `ArgumentException` to be thrown.
