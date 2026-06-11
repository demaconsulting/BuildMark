# Introduction

This document provides the detailed design for BuildMark — a .NET command-line tool that
generates markdown build notes from Git repository metadata, including GitHub and Azure DevOps
repositories. It covers local software items (systems, subsystems, and units) and the OTS
software items they consume.

## Purpose

The purpose of this document is to define the design for each software item in BuildMark —
full architectural and detailed design for local items (systems, subsystems, and units), and
integration and usage design for OTS software items. A reviewer should be able to understand
how each item satisfies its requirements without reading source code. The document does not
restate requirements; it explains how they are realized.

## Scope

This document covers the following software items:

Local items:

- **BuildMark**: system, subsystem, and unit design for all local components.

OTS items:

- **YamlDotNet**: integration and usage design.

The following topics are out of scope:

- External library internals
- Build pipeline configuration
- Deployment and packaging
- Test projects

## Software Structure

The following tree shows how the BuildMark software items are organized across the
system, subsystem, and unit levels:

```text
BuildMark (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── BuildNotes (Subsystem)
│   ├── BuildInformation (Unit)
│   ├── ItemInfo (Unit)
│   └── WebLink (Unit)
├── SelfTest (Subsystem)
│   └── Validation (Unit)
├── Utilities (Subsystem)
│   ├── PathHelpers (Unit)
│   ├── ProcessRunner (Unit)
│   └── TemporaryDirectory (Unit)
├── Version (Subsystem)
│   ├── VersionComparable (Unit)
│   ├── VersionSemantic (Unit)
│   ├── VersionTag (Unit)
│   ├── VersionInterval (Unit)
│   ├── VersionIntervalSet (Unit)
│   └── VersionCommitTag (Unit)
├── Configuration (Subsystem)
│   ├── BuildMarkConfig (Unit)
│   ├── BuildMarkConfigReader (Unit)
│   ├── ConfigurationLoadResult (Unit)
│   ├── ConfigurationIssue (Unit)
│   ├── ConnectorConfig (Unit)
│   ├── GitHubConnectorConfig (Unit)
│   ├── AzureDevOpsConnectorConfig (Unit)
│   ├── ReportConfig (Unit)
│   ├── SectionConfig (Unit)
│   ├── RuleConfig (Unit)
│   └── RuleMatchConfig (Unit)
└── RepoConnectors (Subsystem)
    ├── IRepoConnector (Unit)
    ├── RepoConnectorBase (Unit)
    ├── RepoConnectorFactory (Unit)
    ├── ItemRouter (Unit)
    ├── ItemControlsInfo (Unit)
    ├── ItemControlsParser (Unit)
    ├── GitHub (Subsystem)
    │   ├── GitHubRepoConnector (Unit)
    │   ├── GitHubGraphQLClient (Unit)
    │   └── GitHubGraphQLTypes (Unit)
    ├── AzureDevOps (Subsystem)
    │   ├── AzureDevOpsRepoConnector (Unit)
    │   ├── AzureDevOpsRestClient (Unit)
    │   ├── AzureDevOpsApiTypes (Unit)
    │   └── WorkItemMapper (Unit)
    └── Mock (Subsystem)
        └── MockRepoConnector (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.BuildMark/
├── Program.cs                               - entry point and execution orchestrator
├── BuildNotes/
│   ├── BuildInformation.cs                  - build information data model
│   ├── ItemInfo.cs                          - item information data model
│   └── WebLink.cs                           - web link helper
├── Cli/
│   └── Context.cs                           - command-line argument parser and I/O owner
├── SelfTest/
│   └── Validation.cs                        - self-validation test runner
├── Utilities/
│   ├── PathHelpers.cs                       - safe path combination utilities
│   ├── ProcessRunner.cs                     - process runner for Git commands
│   └── TemporaryDirectory.cs                - temporary directory lifecycle management
├── Version/
│   ├── VersionComparable.cs                 - core integer-based version comparison
│   ├── VersionSemantic.cs                   - semantic version with build metadata
│   ├── VersionTag.cs                        - repository tag parsing and normalization
│   ├── VersionInterval.cs                   - single version interval model and parser
│   ├── VersionIntervalSet.cs                - ordered set of version intervals
│   └── VersionCommitTag.cs                  - version commit tag representation
├── Configuration/
│   ├── BuildMarkConfig.cs                   - top-level configuration data model
│   ├── BuildMarkConfigReader.cs             - reads and parses .buildmark.yaml using YamlDotNet
│   ├── ConfigurationLoadResult.cs           - holds config and any load issues
│   ├── ConfigurationIssue.cs                - single issue with location and severity
│   ├── ConnectorConfig.cs                   - connector envelope data model
│   ├── GitHubConnectorConfig.cs             - GitHub connector settings data model
│   ├── AzureDevOpsConnectorConfig.cs        - Azure DevOps connector settings data model
│   ├── ReportConfig.cs                      - report output settings data model
│   ├── SectionConfig.cs                     - report section definition data model
│   └── RuleConfig.cs                        - routing rule and rule-match condition data models
└── RepoConnectors/
    ├── IRepoConnector.cs                    - repository connector interface
    ├── RepoConnectorBase.cs                 - repository connector base class
    ├── RepoConnectorFactory.cs              - repository connector factory
    ├── ItemRouter.cs                        - shared item routing logic
    ├── ItemControlsInfo.cs                  - item controls data model
    ├── ItemControlsParser.cs                - buildmark block parser
    ├── GitHub/
    │   ├── GitHubRepoConnector.cs           - GitHub API integration
    │   ├── GitHubGraphQLClient.cs           - GraphQL API client
    │   └── GitHubGraphQLTypes.cs            - GraphQL type definitions
    ├── AzureDevOps/
    │   ├── AzureDevOpsRepoConnector.cs      - Azure DevOps API integration
    │   ├── AzureDevOpsRestClient.cs         - REST API client
    │   ├── AzureDevOpsApiTypes.cs           - REST API type definitions
    │   └── WorkItemMapper.cs                - work item to ItemInfo mapper
    └── Mock/
        └── MockRepoConnector.cs             - mock repository connector for self-test
```

The test project mirrors the same layout under `test/DemaConsulting.BuildMark.Tests/`.

## Companion Artifact Structure

Each local software item has corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/build-mark.yaml`, `docs/reqstream/build-mark/.../{item}.yaml`
- Design: `docs/design/build-mark.md`, `docs/design/build-mark/.../{item}.md`
- Verification: `docs/verification/build-mark.md`, `docs/verification/build-mark/.../{item}.md`
- Source: `src/DemaConsulting.BuildMark/.../{Item}.cs`
- Tests: `test/DemaConsulting.BuildMark.Tests/.../{Item}Tests.cs`

OTS items have integration and usage design documentation parallel to system folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- [BuildMark releases](https://github.com/demaconsulting/BuildMark/releases) — compiled user
  guide and documentation
