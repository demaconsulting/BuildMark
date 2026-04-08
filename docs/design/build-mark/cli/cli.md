# Cli Subsystem

## Overview

The Cli subsystem provides the command-line interface layer for BuildMark. It
parses arguments supplied by the user, routes output to the console or a log file,
and exposes the parsed state to the rest of the system via the `Context` object.

The subsystem has no dependencies on other BuildMark subsystems. All other
subsystems receive a `Context` from the caller rather than creating one themselves.

## Units

| Unit      | File             | Responsibility                              |
|-----------|------------------|---------------------------------------------|
| `Context` | `Cli/Context.cs` | Argument parsing, output routing, exit code |

## Interfaces

`Context` exposes the following outward-facing interface consumed by `Program`:

| Member                | Kind     | Description                                        |
|-----------------------|----------|----------------------------------------------------|
| `Create(args)`        | Method   | Factory: parse arguments and return a `Context`    |
| `Version`             | Property | Set when `--version` / `-v` flag is present        |
| `Help`                | Property | Set when `--help` / `-h` / `-?` flag is present    |
| `Silent`              | Property | Set when `--silent` flag is present                |
| `Validate`            | Property | Set when `--validate` flag is present              |
| `Lint`                | Property | Set when `--lint` flag is present                  |
| `BuildVersion`        | Property | Value of `--build-version` argument                |
| `ReportFile`          | Property | Value of `--report` argument                       |
| `ReportDepth`         | Property | Value of `--report-depth` argument (default: 1)    |
| `IncludeKnownIssues`  | Property | Set when `--include-known-issues` flag is present  |
| `ResultsFile`         | Property | Value of `--results` argument                      |
| `ConnectorFactory`    | Property | Optional factory for dependency injection in tests |
| `ExitCode`            | Property | Returns 0 unless `WriteError` has been called      |
| `WriteLine(message)`  | Method   | Writes a line to console (if not silent) and log   |
| `WriteError(message)` | Method   | Writes an error line and sets exit code to 1       |
| `Dispose()`           | Method   | Closes the log file if one was opened              |

## Interactions

The Cli subsystem has no dependencies on other BuildMark subsystems. `Program`
creates a `Context` and passes it to other subsystems as needed.
