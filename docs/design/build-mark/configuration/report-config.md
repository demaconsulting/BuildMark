# ReportConfig

## Overview

`ReportConfig` is a configuration data model holding the optional report output settings
read from the `report:` section of `.buildmark.yaml`. All properties are nullable; when
absent, `Program` uses CLI argument values or built-in defaults.

## Data Model

| Property             | Type      | Description                                                                    |
|----------------------|-----------|--------------------------------------------------------------------------------|
| `File`               | `string?` | Optional output file path override; `null` uses the `--report` CLI argument    |
| `Depth`              | `int?`    | Optional heading depth for report sections; `null` defaults to 1               |
| `IncludeKnownIssues` | `bool?`   | Optional flag to include known issues; `null` defaults to `false`              |

## Interactions

| Unit / Subsystem         | Role                                                                              |
|--------------------------|-----------------------------------------------------------------------------------|
| `BuildMarkConfig`        | Holds `ReportConfig` in its `Report` property                                     |
| `BuildMarkConfigReader`  | Parses the `report:` YAML node and populates this record                          |
| `Program`                | Reads `File`, `Depth`, and `IncludeKnownIssues` as fallbacks to CLI arguments     |
