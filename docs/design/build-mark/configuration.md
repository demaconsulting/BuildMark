## Configuration

![Configuration Structure](ConfigurationView.svg)

### Overview

The Configuration subsystem is responsible for reading and parsing the optional `.buildmark.yaml`
file located in the repository root. It uses the YamlDotNet library's representation model to
parse raw YAML content, then manually walks the resulting node tree to produce a strongly-typed
`BuildMarkConfig` object. Any parse errors or validation warnings are captured as
`ConfigurationIssue` records and surfaced through a `ConfigurationLoadResult` that `Program`
consumes during startup.

When no `.buildmark.yaml` file is present, `BuildMarkConfigReader` returns a result with
`Config = null` and an empty issues list, and the rest of the system operates with
`BuildMarkConfig.CreateDefault()` defaults. When the file is present but malformed, the result
carries `Config = null` alongside a list of `ConfigurationIssue` records that describe each
problem with its file path, 1-based line number, severity, and description.

The subsystem contains the following units: `BuildMarkConfig`, `BuildMarkConfigReader`,
`ConfigurationLoadResult`, `ConfigurationIssue`, `ConnectorConfig`, `GitHubConnectorConfig`,
`AzureDevOpsConnectorConfig`, `ReportConfig`, `SectionConfig`, `RuleConfig`, and
`RuleMatchConfig`.

### Interfaces

**BuildMarkConfigReader API**: Public static entry point consumed by `Program` to load
configuration.

- *Type*: In-process .NET public API.
- *Role*: Provider — `Program` calls this to obtain configuration before any other work.
- *Contract*: `BuildMarkConfigReader.ReadAsync(string path)` accepts a repository root directory
  or a direct `.buildmark.yaml` file path and returns `Task<ConfigurationLoadResult>`. The result
  always contains a non-null `Issues` list. `Config` is non-null only when the file was found and
  parsed without errors.
- *Constraints*: Never throws; all error conditions are reported as `ConfigurationIssue` records
  in the returned result.

**YamlDotNet Integration**: Representation-model YAML parsing consumed internally by
`BuildMarkConfigReader`.

- *Type*: In-process .NET library (OTS).
- *Role*: Consumer — `BuildMarkConfigReader` uses YamlDotNet exclusively for YAML parsing.
- *Contract*: Uses `YamlStream.Load(TextReader)` to parse raw text into a node tree; accesses
  `YamlMappingNode`, `YamlSequenceNode`, and `YamlScalarNode` to walk the parsed structure.
  Catches `YamlException` and converts it to a `ConfigurationIssue`.
- *Constraints*: Input text must be UTF-8 encoded YAML. `YamlException` is the only expected
  exception; all other exceptions propagate to the caller.

### Design

`BuildMarkConfigReader.ReadAsync` is the subsystem's single entry point. The following steps
describe the flow from call to result:

1. `Program` calls `BuildMarkConfigReader.ReadAsync(path)` passing the repository root directory.
2. `BuildMarkConfigReader` resolves `path` to the `.buildmark.yaml` file via `ResolveFilePath`;
   if the path is a directory, `.buildmark.yaml` is appended.
3. If the resolved file does not exist, a `ConfigurationLoadResult` with `Config = null` and an
   empty issues list is returned immediately.
4. If the file exists, its content is read asynchronously and passed to `YamlStream.Load`. An
   empty or comment-only file returns a result with a default `BuildMarkConfig` and no issues.
5. On a `YamlException`, a `ConfigurationIssue` with `Severity = Error` is created from the
   exception's start position and message, and a result with `Config = null` is returned.
6. For a successful parse, `BuildMarkConfigReader` walks the root `YamlMappingNode`, dispatching
   each top-level key (`connector`, `report`, `sections`, `rules`) to a dedicated private parser.
   Each parser populates the corresponding data record (`ConnectorConfig`, `ReportConfig`,
   `SectionConfig`, `RuleConfig`, `RuleMatchConfig`). Invalid node types or unrecognized keys
   produce `Error`-severity `ConfigurationIssue` records.
7. If any `Error`-severity issues were collected, the result is returned with `Config = null`;
   otherwise `Config` carries the fully populated `BuildMarkConfig`.
8. `Program` calls `result.ReportTo(context)` to surface all issues to the user, then checks
   `result.HasErrors` before proceeding to connector selection and report generation.
