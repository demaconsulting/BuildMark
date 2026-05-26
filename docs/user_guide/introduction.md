# Introduction

This guide describes how to install, configure, and use BuildMark.

## Purpose

The purpose of this guide is to explain how to install, configure, and operate BuildMark to generate
markdown build notes from Git repository history and connected issue-tracking systems.

BuildMark is a .NET command-line tool that analyzes commits, pull requests, and issues from GitHub
and Azure DevOps repositories to produce human-readable markdown build notes. It is designed for
integration into CI/CD pipelines and release documentation workflows.

Key capabilities include:

- **Git Integration** — analyzes repository history, tags, and branches
- **Issue Tracking** — extracts changes and bug fixes from GitHub and Azure DevOps
- **Configurable Routing** — routes items into custom report sections by label or work-item type
- **CI/CD Ready** — designed for automation in GitHub Actions and Azure Pipelines
- **Multi-Platform** — runs on Windows, Linux, and macOS with .NET 8, 9, and 10
- **Self-Validation** — built-in qualification tests for use in regulated environments

## Scope

This guide covers installation prerequisites and setup in *Installation*, first-run workflows and
common command examples in *Getting Started*, the `.buildmark.yaml` configuration file reference in
*Configuration*, command-line option reference in *Command-Line Options*, item visibility and
version targeting controls in *Item Visibility and Version Controls*, and CI/CD integration,
report format, version selection rules, best practices, and troubleshooting in *Common Use Cases*.
It is intended for developers and CI/CD engineers who want to automate build notes generation as
part of their release workflow. A .NET SDK (version 8.0, 9.0, or 10.0) is required.

The following topics are out of scope:

- Internal implementation details
- Contributing to BuildMark development

## References

- [BuildMark releases](https://github.com/demaconsulting/BuildMark/releases) — compiled user guide
  and documentation
- [.NET SDK](https://dotnet.microsoft.com/download)
- [Continuous Compliance](https://github.com/demaconsulting/ContinuousCompliance)
