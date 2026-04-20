# ItemControlsParser and ItemControlsInfo

## Overview

`ItemControlsParser` is a static utility class that extracts a `buildmark`
fenced code block from an issue or pull request description and parses its
key-value fields into an `ItemControlsInfo` record. If no block is found, the
method returns `null` so that callers can apply default label-based rules.

`ItemControlsInfo` is the immutable data record produced by the parser. It holds
up to three optional fields: `Visibility`, `Type`, and `AffectedVersions`.

## Block Detection

The parser locates a `buildmark` block using the following rules:

1. Strip all HTML comment wrappers (`<!-- … -->`), which allows the block to be
   hidden from the GitHub rendered view.
2. Scan the resulting text for a fenced code block whose language identifier is
   exactly `buildmark` (case-insensitive).
3. The fence delimiter is three or more back-tick characters followed immediately
   by `buildmark`.
4. The closing fence is the same number of back-tick characters on a line by
   itself.
5. If no such block is found, `Parse` returns `null`.

## Key-Value Parsing

Each non-empty line inside the block is treated as a `key: value` pair:

- The key is the text before the first `:`, trimmed of whitespace.
- The value is the text after the first `:`, trimmed of whitespace.
- Lines that do not contain `:` are ignored.
- Unknown keys are silently ignored.

Recognized keys:

| Key                  | Accepted Values                  | Effect on `ItemControlsInfo` |
|----------------------|----------------------------------|------------------------------|
| `visibility`         | `public`, `internal`             | Sets `Visibility`            |
| `type`               | `bug`, `feature`                 | Sets `Type`                  |
| `affected-versions`  | Interval expression (see below)  | Sets `AffectedVersions`      |

Unrecognized values for a known key are silently ignored (the field remains
`null`).

## Data Model — ItemControlsInfo

```csharp
public record ItemControlsInfo(
    string? Visibility,
    string? Type,
    VersionIntervalSet? AffectedVersions);
```

| Property           | Type                  | Description                                   |
|--------------------|-----------------------|-----------------------------------------------|
| `Visibility`       | `string?`             | `"public"`, `"internal"`, or `null`           |
| `Type`             | `string?`             | `"bug"`, `"feature"`, or `null`               |
| `AffectedVersions` | `VersionIntervalSet?` | Parsed interval set, or `null` if not present |

## Methods

### `ItemControlsParser.Parse(string? description) → ItemControlsInfo?`

Entry point for the parser. Steps:

1. Return `null` if `description` is null or empty.
2. Strip HTML comment wrappers (`<!--` / `-->`) from the description while
   preserving the enclosed content.
3. Locate the `buildmark` code fence using the rules above.
4. Return `null` if no fence is found.
5. Split the block body into lines.
6. Parse each line as a key-value pair.
7. Build and return an `ItemControlsInfo` from the recognized keys, or `null` if no
   recognized keys were found.

## Interactions

| Unit / Subsystem      | Role                                                                            |
|-----------------------|---------------------------------------------------------------------------------|
| `GitHubRepoConnector` | Calls `Parse` on each issue and PR description body                             |
| `WorkItemMapper`      | Calls `Parse` on each work item description body (`AzureDevOpsRepoConnector`)   |
| `VersionIntervalSet`  | Created by the parser when `affected-versions` key is present                   |
| `ItemControlsInfo`    | The record returned by `Parse`                                                  |
