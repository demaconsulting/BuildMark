# Introduction

This document provides the verification design for BuildMark, a .NET command-line tool
that generates markdown build notes from Git repository metadata, including GitHub
and Azure DevOps repositories.

## Purpose

The purpose of this document is to describe how each software requirement for BuildMark
is verified. For each unit, subsystem, and OTS component it identifies the test class,
test methods, mock or stub dependencies, and the requirement identifiers that each test
satisfies. The document provides a traceable record of verification coverage that
supports formal code review, compliance audit, and ongoing maintenance.

## Scope

This document covers the verification design for the complete BuildMark system,
including all in-house subsystems and units and all Off-The-Shelf (OTS) components.

In-house software items verified in this document:

- **Program** — entry point and execution orchestrator
- **Cli** subsystem — `Context` unit (command-line argument parser and I/O owner)
- **BuildNotes** subsystem — `BuildInformation`, `ItemInfo`, and `WebLink` units
- **SelfTest** subsystem — `Validation` unit
- **Utilities** subsystem — `PathHelpers` and `ProcessRunner` units
- **Version** subsystem — `VersionComparable`, `VersionSemantic`, `VersionTag`,
  `VersionInterval`, `VersionIntervalSet`, and `VersionCommitTag` units
- **Configuration** subsystem — `BuildMarkConfig`, `BuildMarkConfigReader`,
  `ConfigurationLoadResult`, `ConfigurationIssue`, `ConnectorConfig`,
  `GitHubConnectorConfig`, `AzureDevOpsConnectorConfig`, `ReportConfig`,
  `SectionConfig`, `RuleConfig`, and `RuleMatchConfig` units
- **RepoConnectors** subsystem — `IRepoConnector`, `RepoConnectorBase`,
  `RepoConnectorFactory`, `ItemRouter`, `ItemControlsInfo`, and `ItemControlsParser`
  units, plus the following sub-subsystems:
  - **GitHub** sub-subsystem — `GitHubRepoConnector`, `GitHubGraphQLClient`,
    and `GitHubGraphQLTypes` units
  - **AzureDevOps** sub-subsystem — `AzureDevOpsRepoConnector`,
    `AzureDevOpsRestClient`, `AzureDevOpsApiTypes`, and `WorkItemMapper` units
  - **Mock** sub-subsystem — `MockRepoConnector` unit

OTS components verified in this document:

- **BuildMark** — build notes generation tool (self-referential)
- **FileAssert** — file content assertion tool
- **Pandoc** — document conversion tool
- **ReqStream** — requirements traceability tool
- **ReviewMark** — code review enforcement tool
- **SarifMark** — SARIF report generation tool
- **SonarMark** — SonarCloud report generation tool
- **VersionMark** — tool version capture tool
- **WeasyPrint** — HTML-to-PDF renderer
- **xUnit** — unit testing framework

The following topics are out of scope:

- External library internals not listed above
- Build pipeline configuration beyond the steps referenced as evidence
- Deployment and packaging

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
│   └── ProcessRunner (Unit)
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

## Companion Artifact Structure

Verification design documents are companion artifacts to requirements, design, source
code, and tests. The parallel tree below shows how each artifact type maps to the same
software structure:

```text
docs/requirements_doc/   — compiled requirements document (generated)
docs/reqstream/          — requirements source YAML files
docs/design/             — software design document source
docs/verification/       — this document (verification design source)
src/DemaConsulting.BuildMark/   — implementation source
test/DemaConsulting.BuildMark.Tests/   — test source
```

Each chapter in this verification document corresponds to a unit or subsystem chapter
in the design document. Requirement IDs referenced in the Requirements Coverage sections
match identifiers defined in the ReqStream YAML files under `docs/reqstream/`.

## References

- See the *BuildMark Software Design* document for implementation details of each unit.
- See the *BuildMark Requirements* document for the full requirements specification.
- [BuildMark Repository](https://github.com/demaconsulting/BuildMark)
