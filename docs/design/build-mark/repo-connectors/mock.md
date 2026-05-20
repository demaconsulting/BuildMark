### Mock Subsystem

#### Overview

The Mock subsystem groups the in-memory connector used by the built-in
`--validate` self-test. It sits within the RepoConnectors subsystem.

`MockRepoConnector` lives in production code - not in the test project - because
the `--validate` flag must work in any deployment without requiring a separate test
assembly or external tooling.

#### Units

| Unit                 | File                                        | Responsibility                               |
|----------------------|---------------------------------------------|----------------------------------------------|
| `MockRepoConnector`  | `RepoConnectors/Mock/MockRepoConnector.cs`  | In-memory connector for self-validation      |

#### Interfaces

The Mock subsystem exposes `MockRepoConnector`, which implements `IRepoConnector`.

| Member | Kind | Description |
|---|---|---|
| `MockRepoConnector()` | Constructor | Create the connector with hard-coded in-memory data |
| `GetBuildInformationAsync(version)` | Method | Return a deterministic `BuildInformation` record from in-memory data |

#### Design

The Mock subsystem contains a single unit, so there is no inter-unit
collaboration to describe. `MockRepoConnector` overrides
`GetBuildInformationAsync` entirely with an in-memory implementation that
mirrors the production connector logic but operates on hard-coded dictionaries
instead of live API responses. The `RunCommandAsync` method inherited from
`RepoConnectorBase` is not called, as the mock does not execute any shell
commands. When routing rules have been configured via `Configure`, the connector
calls `ApplyRules` (inherited from `RepoConnectorBase`) to populate
`BuildInformation.RoutedSections`.

#### Interactions

| Unit / Subsystem    | Role                                                              |
|---------------------|-------------------------------------------------------------------|
| `IRepoConnector`    | Interface implemented by `MockRepoConnector`                      |
| `RepoConnectorBase` | Base class providing `FindVersionIndex` and command delegation    |
| `Validation`        | Instantiates `MockRepoConnector` directly for self-tests          |
