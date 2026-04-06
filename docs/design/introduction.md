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

This document covers the detailed design of the following software units:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Context** — command-line argument parser and I/O owner (`Cli/Context.cs`)
- **Validation** — self-validation test runner (`SelfTest/Validation.cs`)
- **PathHelpers** — safe path combination utilities (`Utilities/PathHelpers.cs`)
- **ItemControlsParser** — buildmark block parser (`ItemControls/ItemControlsParser.cs`)
- **VersionInterval** — version interval model and parser (`ItemControls/VersionInterval.cs`)
- **GitHubRepoConnector** — GitHub GraphQL API integration (`RepoConnectors/GitHub/GitHubRepoConnector.cs`)

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
├── SelfTest (Subsystem)
│   └── Validation (Unit)
├── Utilities (Subsystem)
│   └── PathHelpers (Unit)
├── ItemControls (Subsystem)
│   ├── ItemControlsInfo (Unit)
│   ├── ItemControlsParser (Unit)
│   ├── VersionInterval (Unit)
│   └── VersionIntervalSet (Unit)
└── RepoConnectors (Subsystem)
    ├── RepoConnectorBase (Unit)
    ├── MockRepoConnector (Unit)
    ├── ProcessRunner (Unit)
    ├── RepoConnectorFactory (Unit)
    └── GitHubRepoConnector (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.BuildMark/
├── Program.cs                               — entry point and execution orchestrator
├── BuildInformation.cs                      — build information data model
├── ItemInfo.cs                              — item information data model
├── Version.cs                               — version information
├── VersionTag.cs                            — version tag representation
├── WebLink.cs                               — web link helper
├── Cli/
│   └── Context.cs                           — command-line argument parser and I/O owner
├── SelfTest/
│   └── Validation.cs                        — self-validation test runner
├── Utilities/
│   └── PathHelpers.cs                       — safe path combination utilities
├── ItemControls/
│   ├── ItemControlsInfo.cs                  — item controls data model
│   ├── ItemControlsParser.cs                — buildmark block parser
│   ├── VersionInterval.cs                   — single version interval model and parser
│   └── VersionIntervalSet.cs                — ordered set of version intervals
└── RepoConnectors/
    ├── IRepoConnector.cs                    — repository connector interface
    ├── RepoConnectorBase.cs                 — repository connector base class
    ├── RepoConnectorFactory.cs              — repository connector factory
    ├── MockRepoConnector.cs                 — mock repository connector for testing
    ├── ProcessRunner.cs                     — process runner for Git commands
    ├── GitHubRepoConnector.cs               — GitHub API integration
    └── GitHub/
        ├── GitHubGraphQLClient.cs           — GraphQL API client
        └── GitHubGraphQLTypes.cs            — GraphQL type definitions
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
