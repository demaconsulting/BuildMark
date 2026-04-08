# Introduction

This document provides the detailed design for BuildMark, a .NET command-line tool
that generates markdown build notes from GitHub repository metadata.

## Purpose

The purpose of this document is to describe the internal design of each software unit
that comprises BuildMark. It captures data models, algorithms, key methods, and
inter-unit interactions at a level of detail sufficient for formal code review,
compliance verification, and future maintenance. The document does not restate
requirements; it explains how they are realized.

## Scope

This document covers the detailed design of the BuildMark system, including all
subsystems and units identified in the software structure below. Each unit has
its own detailed design chapter within this document.

The following topics are out of scope:

- External library internals
- Build pipeline configuration
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
│   ├── VersionTag (Unit)
│   └── WebLink (Unit)
├── SelfTest (Subsystem)
│   └── Validation (Unit)
├── Utilities (Subsystem)
│   ├── PathHelpers (Unit)
│   ├── ProcessRunner (Unit)
│   ├── VersionInfo (Unit)
│   ├── VersionInterval (Unit)
│   └── VersionIntervalSet (Unit)
├── Configuration (Subsystem)
│   ├── BuildMarkConfig (Unit)
│   ├── BuildMarkConfigReader (Unit)
│   ├── ConfigurationLoadResult (Unit)
│   ├── ConfigurationIssue (Unit)
│   ├── ConnectorConfig (Unit)
│   ├── GitHubConnectorConfig (Unit)
│   ├── AzureDevOpsConnectorConfig (Unit)
│   ├── SectionConfig (Unit)
│   └── RuleConfig (Unit)
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
    └── Mock (Subsystem)
        └── MockRepoConnector (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.BuildMark/
├── Program.cs                               — entry point and execution orchestrator
├── BuildNotes/
│   ├── BuildInformation.cs                  — build information data model
│   ├── ItemInfo.cs                          — item information data model
│   ├── VersionTag.cs                        — version tag representation
│   └── WebLink.cs                           — web link helper
├── Cli/
│   └── Context.cs                           — command-line argument parser and I/O owner
├── SelfTest/
│   └── Validation.cs                        — self-validation test runner
├── Utilities/
│   ├── PathHelpers.cs                       — safe path combination utilities
│   ├── ProcessRunner.cs                     — process runner for Git commands
│   ├── VersionInfo.cs                       — semantic version parser and normalized model
│   ├── VersionInterval.cs                   — single version interval model and parser
│   └── VersionIntervalSet.cs                — ordered set of version intervals
├── Configuration/
│   ├── BuildMarkConfig.cs                   — top-level configuration data model
│   ├── BuildMarkConfigReader.cs             — reads and deserializes .buildmark.yaml
│   ├── ConfigurationLoadResult.cs           — holds config and any load issues
│   ├── ConfigurationIssue.cs                — single issue with location and severity
│   ├── ConnectorConfig.cs                   — connector envelope data model
│   ├── GitHubConnectorConfig.cs             — GitHub connector settings data model
│   ├── AzureDevOpsConnectorConfig.cs        — Azure DevOps connector settings (future)
│   ├── SectionConfig.cs                     — report section definition data model
│   └── RuleConfig.cs                        — routing rule data model
└── RepoConnectors/
    ├── IRepoConnector.cs                    — repository connector interface
    ├── RepoConnectorBase.cs                 — repository connector base class
    ├── RepoConnectorFactory.cs              — repository connector factory
    ├── ItemRouter.cs                        — shared item routing logic
    ├── ItemControlsInfo.cs                  — item controls data model
    ├── ItemControlsParser.cs                — buildmark block parser
    ├── GitHub/
    │   ├── GitHubRepoConnector.cs           — GitHub API integration
    │   ├── GitHubGraphQLClient.cs           — GraphQL API client
    │   └── GitHubGraphQLTypes.cs            — GraphQL type definitions
    └── Mock/
        └── MockRepoConnector.cs             — mock repository connector for self-test
```

The test project mirrors the same layout under `test/DemaConsulting.BuildMark.Tests/`.

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## References

- [BuildMark User Guide][user-guide]
- [BuildMark Repository][repo]

[user-guide]: ../guide/guide.md
[repo]: https://github.com/demaconsulting/BuildMark
