### GitHubConnectorConfig

#### Verification Approach

`GitHubConnectorConfig` is verified through `ConfigurationTests.cs`. Tests write `.buildmark.yaml`
files with a GitHub connector block and assert that `Owner`, `Repo`, and `BaseUrl` properties are
correctly parsed. No mocking is required.

#### Dependencies

| Mock / Stub | Reason                                                               |
| ----------- | -------------------------------------------------------------------- |
| File system | Tests create temporary `.buildmark.yaml` files in `Path.GetTempPath` |

#### Test Environment

Standard dotnet test host; no external dependencies or environment setup required.

#### Acceptance Criteria

All tests in the test class pass with no errors or warnings.

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
