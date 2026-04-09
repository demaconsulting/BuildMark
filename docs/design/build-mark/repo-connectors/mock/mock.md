# Mock Subsystem

## Overview

The Mock subsystem groups the in-memory connector used by the built-in
`--validate` self-test. It sits within the RepoConnectors subsystem.

`MockRepoConnector` lives in production code — not in the test project — because
the `--validate` flag must work in any deployment without requiring a separate test
assembly or external tooling.

## Units

| Unit                 | File                                        | Responsibility                               |
|----------------------|---------------------------------------------|----------------------------------------------|
| `MockRepoConnector`  | `RepoConnectors/Mock/MockRepoConnector.cs`  | In-memory connector for self-validation      |

## Interactions

| Unit / Subsystem    | Role                                                              |
|---------------------|-------------------------------------------------------------------|
| `IRepoConnector`    | Interface implemented by `MockRepoConnector`                      |
| `RepoConnectorBase` | Base class providing `FindVersionIndex` and command delegation    |
| `Validation`        | Instantiates `MockRepoConnector` directly for self-tests          |
