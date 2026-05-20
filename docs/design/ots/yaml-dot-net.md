## YamlDotNet Integration Design

### Why YamlDotNet Was Chosen

YamlDotNet is the established YAML parsing library for .NET. It provides a
representation model (`YamlStream`, `YamlMappingNode`, `YamlSequenceNode`,
`YamlScalarNode`) that allows walking the YAML node tree without requiring a
pre-defined schema. This is important for BuildMark because the configuration
file is optional and partially structured: the reader must tolerate absent
sections gracefully and convert malformed content into `ConfigurationIssue`
records rather than throwing unhandled exceptions.

### APIs Used

BuildMark uses the YamlDotNet **representation model** exclusively:

| Type                | Usage                                                            |
|---------------------|------------------------------------------------------------------|
| `YamlStream`        | Top-level container; parsed from the raw file text               |
| `YamlMappingNode`   | Key-value mapping; used for all object nodes in the config       |
| `YamlSequenceNode`  | Ordered list; used for `sections` and `rules` arrays             |
| `YamlScalarNode`    | Leaf node value; represents a string, boolean, or number value   |

The serializer/deserializer (`YamlDotNet.Serialization`) is **not** used
because it requires a fixed schema and throws on unknown keys, which would
prevent forward-compatible configuration files.

### Integration Pattern

`BuildMarkConfigReader.ReadAsync` reads the file text and passes it to
`YamlStream.Load(reader)`. If parsing succeeds, the first document's root
node is cast to `YamlMappingNode` and walked recursively. Each expected key
is looked up by name; absent keys produce `null` and are handled with
`ConfigurationIssue` creation when they are required, or silently ignored
when optional.

The node walk is deliberately defensive:

- Every cast is guarded; an unexpected node type generates a `ConfigurationIssue`
  with `Severity.Warning` so the tool can continue with partial configuration.
- Line numbers are extracted from `YamlNode.Start.Line` and included in every
  issue for precise user feedback.

### Version Constraints

BuildMark targets the version of YamlDotNet specified in the project file
(`src/DemaConsulting.BuildMark/DemaConsulting.BuildMark.csproj`). No version
below 13.x is supported because earlier versions used a different namespace
layout for the representation model types.
