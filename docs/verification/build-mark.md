# BuildMark

## Verification Approach

BuildMark is verified at two levels.

System-level integration testing is provided by `IntegrationTests.cs`, which runs the BuildMark
executable end-to-end via `Runner.Run()` (`dotnet <dll>`). These tests invoke the full compiled
binary, exercising the complete pipeline from CLI argument parsing through build notes generation,
and validate exit codes and console output without any in-process mocking.

Unit-level testing of the system entry point is provided by `ProgramTests.cs`, which calls
`Program.Run()` directly with a controlled `Context` object. These tests validate individual flags
and error conditions with fast, isolated, in-process invocations. The connector factory is injected
via a context override to avoid live API calls where needed.

The `RepoConnectorsTests.cs` file exercises the full data pipeline, from connector factory creation
through `GetBuildInformationAsync`, using mock data to cover GitHub, Azure DevOps, and Mock
connector paths.

Self-test (`--validate`) is covered by `BuildMark_ValidateFlag_RunsSelfValidation` in
`IntegrationTests.cs`. The CI pipeline additionally runs the full build notes generation chain
with live GitHub metadata to confirm end-to-end operation.

Mock objects used at the system boundary: `MockRepoConnector` provides deterministic repository
data without live API calls; `MockHttpMessageHandler` intercepts HTTP communication used by
GraphQL/REST client tests; and context output capture replaces `Console.Out` with `StringWriter`
for assertion.

## Test Environment

Tests run via `dotnet test` on the CI matrix (Windows, Ubuntu, macOS) against .NET 8, 9, and 10.
No external services are required for unit and integration tests; all HTTP communication is
intercepted by `MockHttpMessageHandler`. A live GitHub Actions environment is used for the
end-to-end CI validation of the report generation pipeline.

## Acceptance Criteria

- All automated tests in `ProgramTests.cs` and `RepoConnectorsTests.cs` pass with zero failures.
- The CI pipeline executes the end-to-end build notes generation step without error.
- No unresolved anomalies of Error severity remain.

## Test Scenarios

**BuildMark_VersionFlag_OutputsVersion**: Verifies that the BuildMark executable invoked with
`--version` exits with code 0 and writes a non-empty version string to standard output.
This scenario is tested by `BuildMark_VersionFlag_OutputsVersion`.

**BuildMark_HelpFlag_OutputsUsageInformation**: Verifies that invoking BuildMark with `--help`
exits with code 0 and writes usage information including available options to standard output.
This scenario is tested by `BuildMark_HelpFlag_OutputsUsageInformation`.

**BuildMark_SilentFlag_SuppressesOutput**: Verifies that invoking BuildMark with `--silent --help`
suppresses the banner but still exits with code 0, confirming the silent flag suppresses non-error
output. This scenario is tested by `BuildMark_SilentFlag_SuppressesOutput`.

**BuildMark_InvalidArgument_ShowsError**: Verifies that an unrecognized argument causes BuildMark
to write an error message containing "Unsupported argument" and exit with code 1.
This scenario is tested by `BuildMark_InvalidArgument_ShowsError`.

**BuildMark_ValidateFlag_RunsSelfValidation**: Verifies that invoking BuildMark with `--validate`
writes self-validation output and exits with code 0, confirming the self-test subsystem is
reachable from the CLI. This scenario is tested by `BuildMark_ValidateFlag_RunsSelfValidation`.

**BuildMark_LogParameter_IsAccepted**: Verifies that `--log test.log --help` is accepted without
an "Unsupported argument" error and exits with code 0.
This scenario is tested by `BuildMark_LogParameter_IsAccepted`.

**BuildMark_ReportParameter_IsAccepted**: Verifies that `--report output.md --help` is accepted
without error and exits with code 0.
This scenario is tested by `BuildMark_ReportParameter_IsAccepted`.

**BuildMark_DepthParameter_IsAccepted**: Verifies that `--depth 2 --help` is accepted without
error and exits with code 0.
This scenario is tested by `BuildMark_DepthParameter_IsAccepted`.

**BuildMark_BuildVersionParameter_IsAccepted**: Verifies that `--build-version 1.0.0 --help` is
accepted without error and exits with code 0.
This scenario is tested by `BuildMark_BuildVersionParameter_IsAccepted`.

**BuildMark_ResultsParameter_IsAccepted**: Verifies that `--results results.trx --help` is accepted
without error and exits with code 0.
This scenario is tested by `BuildMark_ResultsParameter_IsAccepted`.

**BuildMark_LintFlag_IsAccepted**: Verifies that `--lint` is accepted without error and exits with
code 0. This scenario is tested by `BuildMark_LintFlag_IsAccepted`.

**Program_Version_ReturnsValidVersion**: Verifies that `Program.Version` returns a non-null,
non-empty version string in semver format, confirming version metadata is embedded in the assembly.
This scenario is tested by `Program_Version_ReturnsValidVersion`.

**Program_Run_VersionFlag_OutputsVersionToConsole**: Verifies that calling `Program.Run` with a
context having `Version = true` writes the version string to context output and exits with code 0.
This scenario is tested by `Program_Run_VersionFlag_OutputsVersionToConsole`.

**Program_Run_HelpFlag_OutputsHelpMessage**: Verifies that calling `Program.Run` with `Help = true`
writes help text to context output and exits with code 0. This scenario is tested by
`Program_Run_HelpFlag_OutputsHelpMessage`.

**Program_Run_QuestionMarkFlag_OutputsHelpMessage**: Verifies that the `?` short flag produces the
same help output as `--help`. This scenario is tested by
`Program_Run_QuestionMarkFlag_OutputsHelpMessage`.

**Program_Run_LongHelpFlag_OutputsHelpMessage**: Verifies that the `--help` long flag produces
standard help output and exits with code 0. This scenario is tested by
`Program_Run_LongHelpFlag_OutputsHelpMessage`.

**Program_Run_ValidateFlag_OutputsValidationMessage**: Verifies that calling `Program.Run` with
`Validate = true` writes validation output and exits with code 0.
This scenario is tested by `Program_Run_ValidateFlag_OutputsValidationMessage`.

**Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues**: Verifies that
`Program.Run` with report and `IncludeKnownIssues = true` flags generates a report file and exits
with code 0. This scenario is tested by
`Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues`.

**Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero**: Verifies that running with
`Lint = true` when no configuration file is present leaves the exit code at 0, confirming that
lint mode is not an error without a config file.
This scenario is tested by `Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero`.

**Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode**: Verifies that an invalid build
version string causes an error message on stderr and exits with code 1.
This scenario is tested by `Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode`.

**Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode**: Verifies
that when the connector factory throws `InvalidOperationException`, an error message is written
to stderr and the exit code is 1.
This scenario is tested by
`Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode`.

**GitHub_EnterpriseSupport_HostAgnosticUrlParsing**: Verifies that `GitHubRepoConnector` correctly
parses owner and repository name from SSH and HTTPS remote URLs using any hostname — covering
github.com (`GitHubRepoConnector_ParseGitHubUrl_GitHubCom_SSH_ReturnsOwnerAndRepo`,
`GitHubRepoConnector_ParseGitHubUrl_GitHubCom_HTTPS_ReturnsOwnerAndRepo`), GitHub Enterprise
Cloud (`GitHubRepoConnector_ParseGitHubUrl_GHECloud_HTTPS_ReturnsOwnerAndRepo`), and GitHub
Enterprise Server on-premises (`GitHubRepoConnector_ParseGitHubUrl_GHEServer_HTTPS_ReturnsOwnerAndRepo`,
`GitHubRepoConnector_ParseGitHubUrl_GHEServer_SSH_ReturnsOwnerAndRepo`). Additionally,
`GitHubRepoConnector_GetBuildInformationAsync_GHERemote_ChangelogUrlUsesGHEHost` verifies that the
generated changelog URL uses the GHE hostname rather than a hardcoded `github.com` value, confirming
end-to-end enterprise server support. This collection of scenarios provides system-level coverage for
`BuildMark-GitHub-EnterpriseSupport`.
