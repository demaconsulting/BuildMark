# BuildMarkConfig

## Overview

`BuildMarkConfig` is the top-level configuration data model for BuildMark. It holds all
settings read from the `.buildmark.yaml` file, including connector configuration, report
settings, section definitions, and item routing rules.

When no `.buildmark.yaml` file is present, `Program` calls `BuildMarkConfig.CreateDefault()`
to obtain a default configuration with built-in section and rule definitions.

## Data Model

| Property    | Type                     | Description                                           |
|-------------|--------------------------|-------------------------------------------------------|
| `Connector` | `ConnectorConfig?`       | Optional connector configuration; `null` when absent  |
| `Report`    | `ReportConfig?`          | Optional report settings; `null` when absent          |
| `Sections`  | `IList<SectionConfig>`   | Ordered list of report section definitions            |
| `Rules`     | `IList<RuleConfig>`      | List of item routing rules                            |

## Interactions

| Unit / Subsystem         | Role                                                                             |
|--------------------------|----------------------------------------------------------------------------------|
| `BuildMarkConfigReader`  | Produces `BuildMarkConfig` instances by parsing `.buildmark.yaml`                |
| `Program`                | Reads `Connector`, `Report`, `Sections`, and `Rules` to drive build notes output |
| `RepoConnectorBase`      | Receives `Rules` and `Sections` via `Configure(rules, sections)`                 |
| `RepoConnectorFactory`   | Receives `Connector` to select the appropriate connector implementation          |
