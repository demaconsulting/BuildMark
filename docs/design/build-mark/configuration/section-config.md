# SectionConfig

## Overview

`SectionConfig` is a configuration data model representing a single report section definition
read from the `sections:` list in `.buildmark.yaml`. Each section has a unique identifier
used by routing rules and a display title used as the markdown heading.

## Data Model

| Property | Type     | Description                          |
|----------|----------|--------------------------------------|
| `Id`     | `string` | Unique identifier for the section    |
| `Title`  | `string` | Display title for the report section |

## Interactions

| Unit / Subsystem        | Role                                                                    |
|-------------------------|-------------------------------------------------------------------------|
| `BuildMarkConfig`       | Holds the ordered list of `SectionConfig` objects in `Sections`         |
| `BuildMarkConfigReader` | Parses the `sections:` YAML list and creates `SectionConfig` records    |
| `RepoConnectorBase`     | Receives the list via `Configure(rules, sections)` for output ordering  |
| `ItemRouter`            | Uses section IDs to map routed items to display sections                |
