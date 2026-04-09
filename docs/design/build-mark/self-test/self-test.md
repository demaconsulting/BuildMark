# SelfTest Subsystem

## Overview

The SelfTest subsystem provides a self-validation capability for BuildMark. When the
user passes `--validate`, the subsystem exercises the core functionality of the tool
in the current environment, using a `MockRepoConnector` to avoid external API calls.

The subsystem has no dependencies on the Cli subsystem beyond receiving a `Context`
as its input parameter.

## Units

| Unit         | File                     | Responsibility                               |
|--------------|--------------------------|----------------------------------------------|
| `Validation` | `SelfTest/Validation.cs` | Runs self-tests and writes results to a file |

## Interfaces

`Validation` exposes one public method:

| Member         | Kind   | Description                                                |
|----------------|--------|------------------------------------------------------------|
| `Run(context)` | Method | Execute all self-tests and optionally write a results file |

## Interactions

| Unit / Subsystem    | Role                                                                        |
|---------------------|-----------------------------------------------------------------------------|
| `Context`           | Provides output methods and `ResultsFile` path                              |
| `MockRepoConnector` | Provides deterministic repository data for self-tests (RepoConnectors/Mock) |
| `BuildInformation`  | Generated during tests to verify report content                             |

## Validation Tests

`Validation.Run` executes the following self-tests in order:

| Test Name                                | What it verifies                                                       |
|------------------------------------------|------------------------------------------------------------------------|
| `BuildMark_MarkdownReportGeneration`     | Markdown report is correctly generated from mock data                  |
| `BuildMark_GitIntegration`               | Git repository connector reads version tags and commits                |
| `BuildMark_IssueTracking`                | GitHub issue and pull request tracking works correctly                 |
| `BuildMark_KnownIssuesReporting`         | Known issues are correctly included in the report when requested       |
| `BuildMark_RulesRouting`                 | Rules-based item routing assigns items to the correct report sections  |
