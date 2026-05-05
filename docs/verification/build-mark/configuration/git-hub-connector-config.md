### GitHubConnectorConfig

#### Verification Approach

`GitHubConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml`
files with a GitHub connector block and assert that `Owner`, `Repo`, and `BaseUrl` properties are
correctly parsed. No mocking is required.

#### Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

#### Test Scenarios

##### BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration

**Scenario**: A `.buildmark.yaml` with `github.owner`, `github.repo`, and `github.base-url` fields
is written; `BuildMarkConfigReader.ReadAsync` is called; `Config.Connector.GitHub` is inspected.

**Expected**: `Owner` equals `"example-owner"`; `Repo` equals `"hello-world"`; `BaseUrl` equals
`"https://api.github.com"`.

**Requirement coverage**: `BuildMark-GitHubConnectorConfig-Properties`.

#### Requirements Coverage

- **`BuildMark-GitHubConnectorConfig-Properties`**:
  - BuildMarkConfigReader_ReadAsync_ValidFile_ReturnsParsedConfiguration
