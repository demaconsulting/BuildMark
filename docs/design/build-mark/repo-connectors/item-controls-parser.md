### ItemControlsParser

![RepoConnectors Structure](../../generated/RepoConnectorsView.svg)

#### Purpose

`ItemControlsParser` is a static utility class that extracts a `buildmark` fenced code block from
an issue, pull request, or work item description and parses its key-value fields into an
`ItemControlsInfo` record. If no block is found, or no recognized keys are present, the method
returns `null` so callers can apply default label- or type-based rules.

The parser first strips HTML comment delimiters (`<!-- ... -->`) from the description, which allows
the `buildmark` block to be hidden from the GitHub rendered view while remaining detectable. It
then scans for a fenced code block whose language identifier is exactly `buildmark`
(case-insensitive), with a fence of three or more back-tick characters. Inside the block, each
non-empty line is treated as a `key: value` pair; the key is compared case-insensitively and the
value is compared case-sensitively. Unknown keys and unrecognized values are silently ignored.

Recognized keys and accepted values:

| Key                 | Accepted values      | Effect                                    |
|---------------------|----------------------|-------------------------------------------|
| `visibility`        | `public`, `internal` | Sets `ItemControlsInfo.Visibility`        |
| `type`              | `bug`, `feature`     | Sets `ItemControlsInfo.Type`              |
| `affected-versions` | Interval expression  | Sets `ItemControlsInfo.AffectedVersions`  |

#### Data Model

N/A — `ItemControlsParser` is a static utility class with no instance state.

#### Key Methods

**Parse**: Parses item controls from a description string and returns an `ItemControlsInfo` record
or `null`.

- *Parameters*: `string? description` — the issue, pull request, or work item description text to
  parse.
- *Returns*: `ItemControlsInfo?` — a populated record when at least one recognized key is found;
  `null` when the description is empty, no `buildmark` block is present, or no recognized keys are
  found.
- *Preconditions*: None; null or empty input is handled gracefully.
- *Postconditions*: The returned record contains only recognized and valid values; all invalid
  input is silently discarded.

The algorithm: (1) return `null` if `description` is null or empty; (2) strip HTML comment
delimiters using a compiled regex; (3) scan lines for an opening fence of three or more back-ticks
followed by `buildmark` (case-insensitive); (4) return `null` if no such fence is found; (5) read
lines until the matching closing fence (same back-tick count on a line by itself); (6) parse each
line as `key: value` by splitting on the first colon, normalizing the key to lowercase; (7) build
and return an `ItemControlsInfo` from recognized keys, or `null` if none were recognized.

#### Error Handling

`Parse` returns `null` rather than throwing for any of: null or empty input, a missing `buildmark`
code block, unrecognized key-value pairs, or unrecognized values for known keys. No exceptions
propagate to callers; all invalid or unrecognized content is silently discarded.

#### Dependencies

- **ItemControlsInfo** — the record type that `Parse` instantiates and returns.
- **VersionIntervalSet** — constructed by the parser when the `affected-versions` key is present
  and its value parses to a non-empty interval set.

#### Callers

- **GitHubRepoConnector** — calls `Parse` on the `body` of each pull request and issue.
- **WorkItemMapper** — calls `Parse` on the `System.Description` field of each Azure DevOps work
  item.
