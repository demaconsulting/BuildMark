# Introduction

This document describes how each software item in BuildMark is verified.

## Purpose

The purpose of this document is to describe how each software requirement for BuildMark is verified.
For each unit, subsystem, and OTS component it identifies the test class, test methods, mock or stub
dependencies, and the requirement identifiers that each test satisfies. The document provides a
traceable record of verification coverage that supports formal code review, compliance audit, and
ongoing maintenance.

## Scope

This document covers the verification design for the complete BuildMark system, including all
in-house subsystems and units and all Off-The-Shelf (OTS) components.

Local items verified in this document:

- **BuildMark** system: all subsystems and units listed below.
  - **Program** unit — entry point and execution orchestrator.
  - **Cli** subsystem — `Context` unit (command-line argument parser and I/O owner).
  - **BuildNotes** subsystem — `BuildInformation`, `ItemInfo`, and `WebLink` units.
  - **SelfTest** subsystem — `Validation` unit.
  - **Utilities** subsystem — `PathHelpers` and `ProcessRunner` units.
  - **Version** subsystem — `VersionComparable`, `VersionSemantic`, `VersionTag`, `VersionInterval`,
    `VersionIntervalSet`, and `VersionCommitTag` units.
  - **Configuration** subsystem — `BuildMarkConfig`, `BuildMarkConfigReader`,
    `ConfigurationLoadResult`, `ConfigurationIssue`, `ConnectorConfig`, `GitHubConnectorConfig`,
    `AzureDevOpsConnectorConfig`, `ReportConfig`, `SectionConfig`, `RuleConfig`, and
    `RuleMatchConfig` units.
  - **RepoConnectors** subsystem — `IRepoConnector`, `RepoConnectorBase`, `RepoConnectorFactory`,
    `ItemRouter`, `ItemControlsInfo`, and `ItemControlsParser` units, plus the **GitHub**,
    **AzureDevOps**, and **Mock** sub-subsystems.

OTS items verified in this document:

- **BuildMark** — build notes generation tool (self-referential).
- **FileAssert** — file content assertion tool.
- **Pandoc** — document conversion tool.
- **ReqStream** — requirements traceability tool.
- **ReviewMark** — code review enforcement tool.
- **SarifMark** — SARIF report generation tool.
- **SonarMark** — SonarCloud report generation tool.
- **VersionMark** — tool version capture tool.
- **WeasyPrint** — HTML-to-PDF renderer.
- **xUnit** — unit testing framework.

The following topics are out of scope:

- External library internals not listed above.
- Build pipeline configuration beyond the steps referenced as evidence.
- Deployment and packaging.

## Companion Artifact Structure

Local items have parallel artifacts in:

- Requirements: `docs/reqstream/build-mark.yaml`,
  `docs/reqstream/build-mark[/{subsystem-name}...]/{item}.yaml`
- Design: `docs/design/build-mark.md`, `docs/design/build-mark[/{subsystem-name}...]/{item}.md`
- Verification: `docs/verification/build-mark.md`,
  `docs/verification/build-mark[/{subsystem-name}...]/{item}.md`
- Source: `src/DemaConsulting.BuildMark[/{SubsystemName}...]/{Item}.cs`
- Tests: `test/DemaConsulting.BuildMark.Tests[/{SubsystemName}...]/{Item}Tests.cs`

OTS items have integration/usage design documentation parallel to system folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Shared Packages have integration/usage design documentation parallel to system and OTS folders:

- Requirements: `docs/reqstream/shared/{package-name}.yaml`
- Design: `docs/design/shared/{package-name}.md`
- Verification: `docs/verification/shared/{package-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- [BuildMark releases](https://github.com/demaconsulting/BuildMark/releases) —
  compiled design, requirements, and compliance documents.
