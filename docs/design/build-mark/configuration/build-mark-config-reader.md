# BuildMarkConfigReader

## Overview

`BuildMarkConfigReader` is a static utility class responsible for reading and deserializing
the optional `.buildmark.yaml` file from the repository root. It uses the YamlDotNet library's
representation model (`YamlStream`) to parse YAML content, then walks the resulting node tree
to produce a strongly-typed `BuildMarkConfig` object.

The method always returns a `ConfigurationLoadResult` and never throws. Parse errors and
validation warnings are captured as `ConfigurationIssue` records within the result.

## Interface

| Member            | Kind          | Description                                                               |
|-------------------|---------------|---------------------------------------------------------------------------|
| `ReadAsync(path)` | Static method | Reads and deserializes `.buildmark.yaml`; always returns a load result    |

### `ReadAsync(string path) → Task<ConfigurationLoadResult>`

Looks for a `.buildmark.yaml` file at the supplied path (normally the repository root):

- If the file is absent, returns a result with `Config = null` and an empty issues list.
- If the file is present but contains YAML errors or invalid values, returns a result with
  `Config = null` and one or more `ConfigurationIssue` records describing each problem.
- If the file is valid, returns a result with a fully populated `BuildMarkConfig` and an empty
  issues list.

## Interactions

| Unit / Subsystem          | Role                                                                       |
|---------------------------|----------------------------------------------------------------------------|
| `BuildMarkConfig`         | Produced by `ReadAsync` when parsing succeeds                              |
| `ConfigurationLoadResult` | Always returned by `ReadAsync`, carries config and any issues              |
| `ConfigurationIssue`      | Created for each parse error or validation warning encountered             |
| `Program`                 | Calls `ReadAsync(Environment.CurrentDirectory)` via `LoadConfiguration()`  |
