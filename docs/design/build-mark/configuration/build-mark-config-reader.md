### BuildMarkConfigReader

![Configuration Structure](ConfigurationView.svg)

#### Purpose

`BuildMarkConfigReader` is a static utility class responsible for reading and deserializing
the optional `.buildmark.yaml` file from the repository root. It uses the YamlDotNet library's
representation model (`YamlStream`) to parse YAML content, then walks the resulting node tree
to produce a strongly-typed `BuildMarkConfig` object. The class always returns a
`ConfigurationLoadResult` and never throws; parse errors and validation warnings are captured
as `ConfigurationIssue` records within the result.

#### Data Model

N/A — `BuildMarkConfigReader` is a static utility class with no instance state.

#### Key Methods

**ReadAsync(path)**: Reads and deserializes the `.buildmark.yaml` file at the given path.

- *Parameters*: `string path` — a repository root directory or a direct `.buildmark.yaml` file
  path.
- *Returns*: `Task<ConfigurationLoadResult>` — always returns a result; never throws.
- *Preconditions*: `path` must be a valid file system path string (either an existing directory
  or a file path).
- *Postconditions*: If the file is absent, `Config = null` and `Issues` is empty. If parsing
  fails, `Config = null` and `Issues` contains one or more `Error`-severity records. If the file
  is valid, `Config` is a fully populated `BuildMarkConfig` and `Issues` contains any warnings
  collected during the walk.

Resolves `path` to the `.buildmark.yaml` file via `ResolveFilePath` (appending `.buildmark.yaml`
when `path` is a directory). Reads file content asynchronously, then passes it to
`YamlStream.Load`. An empty or comment-only file returns a result with a default
`BuildMarkConfig` and no issues. On a `YamlException`, the exception's start position and
message are captured as an `Error`-severity `ConfigurationIssue` and a `null`-config result is
returned. For a successful parse, `ParseDocument` dispatches each top-level YAML key to a
dedicated private parser that populates the corresponding data record; unrecognized keys and
invalid node types produce `Error`-severity issues. If any errors were collected, `Config` is
returned as `null`.

#### Error Handling

`ReadAsync` never throws. All `YamlException` parse errors are caught and converted to
`ConfigurationIssue` records with `Severity = Error`. Node-walk failures — including invalid
node types and unsupported configuration keys — also produce `Error`-severity issues. When any
error-severity issue is present, `Config` is `null` in the returned result.

#### Dependencies

- **YamlDotNet** — provides `YamlStream`, `YamlDocument`, `YamlMappingNode`,
  `YamlSequenceNode`, `YamlScalarNode`, and `YamlException` used for YAML parsing.
- **BuildMarkConfig** — produced and returned when parsing succeeds.
- **ConfigurationLoadResult** — always returned as the result of `ReadAsync`.
- **ConfigurationIssue** — created for each parse error or validation warning.
- **ConnectorConfig** — produced by the connector YAML block parser.
- **ReportConfig** — produced by the report YAML block parser.
- **SectionConfig** — produced by the sections YAML sequence parser.
- **RuleConfig** — produced by the rules YAML sequence parser.

#### Callers

- **Program** — calls `ReadAsync(Environment.CurrentDirectory)` via `LoadConfiguration()`.
