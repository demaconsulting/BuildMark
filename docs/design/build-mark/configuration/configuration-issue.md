# ConfigurationIssue

## Overview

`ConfigurationIssue` is an immutable record representing a single problem found while reading
or validating the `.buildmark.yaml` file. Each issue carries a file path, line number,
severity, and human-readable description.

## Data Model

| Property      | Type                        | Description                                        |
|---------------|-----------------------------|----------------------------------------------------|
| `FilePath`    | `string`                    | Path to the file containing the issue              |
| `Line`        | `int`                       | Line number (1-based) of the issue                 |
| `Severity`    | `ConfigurationIssueSeverity`| `Warning` or `Error`                               |
| `Description` | `string`                    | Human-readable description of the issue            |

`ConfigurationIssueSeverity` is a public enum:

| Value     | Description                                              |
|-----------|----------------------------------------------------------|
| `Warning` | Non-fatal issue; tool continues and exit code is 0       |
| `Error`   | Fatal issue; tool reports all errors, exits with code 1  |

## Interactions

| Unit / Subsystem          | Role                                                           |
|---------------------------|----------------------------------------------------------------|
| `BuildMarkConfigReader`   | Creates `ConfigurationIssue` records for each problem found    |
| `ConfigurationLoadResult` | Holds the ordered list of `ConfigurationIssue` records         |
| `Program`                 | Reads issues via `ConfigurationLoadResult.ReportTo(context)`   |
