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

The software structure is modeled in SysML2 under `docs/sysml2/` and rendered to the
diagram below by SysML2Tools as part of the build pipeline. AI agents should query the
SysML2 model directly (see the `sysml2tools-query` skill) rather than parsing this
diagram or the prose below.

![Software Structure](generated/SoftwareStructureView.svg)

## Folder Layout

- **src/** - source files and projects
  - **DemaConsulting.BuildMark/** - BuildMark system source
    - **Cli/** - Cli subsystem
    - **BuildNotes/** - BuildNotes subsystem
    - **SelfTest/** - SelfTest subsystem
    - **Utilities/** - Utilities subsystem
    - **Version/** - Version subsystem
    - **Configuration/** - Configuration subsystem
    - **RepoConnectors/** - RepoConnectors subsystem
      - **GitHub/** - GitHub subsystem
      - **AzureDevOps/** - AzureDevOps subsystem
      - **Mock/** - Mock subsystem

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
