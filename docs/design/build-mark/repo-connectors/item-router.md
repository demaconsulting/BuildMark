# ItemRouter

## Overview

`ItemRouter` is a shared static utility in the RepoConnectors subsystem that routes
a list of `ItemInfo` objects into report sections. It applies a list of `RuleConfig`
entries to determine which section each item belongs to, avoiding duplication of
routing logic across multiple connector implementations.

All connectors (GitHub, future Azure DevOps, etc.) call `ItemRouter` rather than
each implementing their own routing.

## Methods

### `Route(items, rules, sections) → Dictionary<string, List<ItemInfo>>`

Takes a list of `ItemInfo` objects, a list of `RuleConfig` entries, and a list of
`SectionConfig` entries, and returns a dictionary mapping each section ID to the
items assigned to that section.

| Parameter  | Type                  | Description                                       |
|------------|-----------------------|---------------------------------------------------|
| `items`    | `List<ItemInfo>`      | Items to be distributed into sections             |
| `rules`    | `List<RuleConfig>`    | Routing rules that map item attributes to sections |
| `sections` | `List<SectionConfig>` | Ordered list of report sections                   |

Items are matched against `RuleConfig` entries in order; the first matching rule
wins. Items that do not match any rule are placed in a default section.

## Interactions

| Unit / Subsystem        | Role                                                              |
|-------------------------|-------------------------------------------------------------------|
| `ItemInfo`              | Input items to be routed (from BuildNotes subsystem)              |
| `RuleConfig`            | Routing rules from the Configuration subsystem                    |
| `SectionConfig`         | Section definitions from the Configuration subsystem             |
| `GitHubRepoConnector`   | Calls `ItemRouter.Route` to assign items to report sections       |
