## YamlDotNet Integration Design

### Purpose

YamlDotNet is the established YAML parsing library for .NET. BuildMark uses it to parse the
optional `.buildmark.yaml` configuration file. It was chosen because its representation model
lets BuildMark walk the YAML node tree without a fixed schema and exposes the source line of
every node and parse error. BuildMark needs both: unknown keys and wrong node types must be
reported as `ConfigurationIssue` records with accurate line numbers rather than as unhandled
exceptions.

### Features Used

BuildMark uses the YamlDotNet **representation model** (`YamlDotNet.RepresentationModel`) and the
`YamlException` type (`YamlDotNet.Core`):

| Feature | Usage in BuildMark |
| --- | --- |
| `YamlStream` | Loads the file text and exposes the parsed documents |
| `YamlDocument.RootNode` | Root of the first document; must be a mapping |
| `YamlMappingNode` | Object nodes: the root, `connector`, `report`, sections, rules, and `match` |
| `YamlSequenceNode` | `sections` and `rules` lists, and multi-value `match` entries |
| `YamlScalarNode` | Leaf values such as keys, identifiers, titles, and routes |
| `YamlNode.Start.Line` | 1-based line number for each validation issue |
| `YamlException.Start.Line` | 1-based line number for syntax errors |

The serializer (`YamlDotNet.Serialization`) is not used: it maps YAML onto fixed types and does
not let BuildMark report every unknown key or wrong node type with its own line number.

### Integration Pattern

YamlDotNet is used only inside `BuildMarkConfigReader.ReadAsync`. No YamlDotNet type appears in
any public signature, so callers see only BuildMark configuration types.

1. If the configuration file does not exist, YamlDotNet is not invoked and an empty result is
   returned.
2. The file text is read and passed through a `StringReader` to `YamlStream.Load`. A
   `YamlException` is caught and converted into an error `ConfigurationIssue` at
   `YamlException.Start.Line`.
3. A file with no documents (empty or comment-only) produces a default `BuildMarkConfig`.
4. The first document's root node must be a `YamlMappingNode`. Structural nodes are then
   checked for their expected type before they are read: the `connector`, `github`,
   `azure-devops`, and `report` blocks and each section, rule, and `match` entry must be
   mappings, and `sections` and `rules` must be sequences. A wrong structural node type or an
   unsupported key produces an error `ConfigurationIssue` at `YamlNode.Start.Line`; parsing
   continues so that all issues are reported together.
5. Scalar fields must be `YamlScalarNode` values. A mapping or sequence in a scalar field, such
   as `connector.type`, the GitHub and Azure DevOps connector settings, `sections[].id`,
   `sections[].title`, `rules[].route`, or an entry of a `match` `label` or `work-item-type`
   list, produces an error `ConfigurationIssue` at the value's `YamlNode.Start.Line`. Fields
   with their own value checks report a wrong node type through those checks: `area-path`
   (must be a scalar), `token-variable` (must be a non-empty string), and the `report` `file`,
   `depth`, and `include-known-issues` values.
6. If any error issue was recorded the result carries a `null` configuration; otherwise it
   carries the parsed `BuildMarkConfig`.

No initialization or configuration of YamlDotNet is required. `YamlStream` holds no unmanaged
resources; the `StringReader` is disposed by a `using` declaration.
