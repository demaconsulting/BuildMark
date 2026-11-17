## YamlDotNet

### Verification Approach

YamlDotNet is an OTS YAML parsing library used only by `BuildMarkConfigReader` to parse the
`.buildmark.yaml` configuration file. It is verified through the Configuration unit and
subsystem tests in `DemaConsulting.BuildMark.Tests`, which write real YAML files to a temporary
directory and load them through `BuildMarkConfigReader.ReadAsync`. YamlDotNet is not mocked, so
each test exercises the library's parser, its node tree, and its source locations. The tests run
on every operating system and .NET version in the CI matrix.

### Evidence

The tests run as part of `dotnet test` in `build.ps1` and in the CI `build` job. Their TRX
results are consumed by ReqStream to satisfy the YamlDotNet OTS requirements.

### Test Scenarios

**YamlDotNet parses a well-formed document**: A configuration file with nested mappings
(`connector`, `github`), a block sequence (`sections`, `rules`), and a flow sequence
(`label: [feature]`) is loaded. Every value must reach the parsed configuration with no issues.
This scenario is tested by `BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration`
and `Configuration_ReadAsync_ValidFile_ReturnsConfiguration`.

**YamlDotNet rejects malformed YAML**: A configuration file indented with a tab is loaded.
YamlDotNet must throw a `YamlException` whose message identifies the tab, which BuildMark reports
as an error issue with no configuration. This scenario is tested by
`BuildMarkConfigReader_ReadAsync_MalformedFile_ReturnsErrorIssue`.

**YamlDotNet reports the line of a syntax error**: A configuration file whose third line holds
a mapping value inside a plain scalar (`title: Changes: Notes`) is loaded. The single error issue
must be reported on line 3, confirming that `YamlException.Start.Line` is correct and 1-based.
This scenario is tested by `BuildMarkConfigReader_ReadAsync_SyntaxError_ReportsErrorLine`.

**YamlDotNet reports node source lines**: A syntactically valid file with an unsupported key on
line 3 is loaded. The validation error must be reported on line 3, confirming that
`YamlNode.Start.Line` is correct and 1-based. This scenario is tested by
`Configuration_Issues_ValidationError_ReportsAccurateLine`.
