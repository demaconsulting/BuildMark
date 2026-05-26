### AzureDevOps

#### Overview

The AzureDevOps subsystem groups the units responsible for querying the Azure DevOps REST API. It
sits within the RepoConnectors subsystem and provides the production connector used when the
repository host is Azure DevOps Services or Azure DevOps Server.

The subsystem contains the following units:

- `AzureDevOpsRepoConnector` — implements `IRepoConnector` for Azure DevOps; orchestrates
  authentication, REST API calls, and assembly of the `BuildInformation` record.
- `AzureDevOpsRestClient` — issues paginated HTTP requests to the Azure DevOps REST API and
  deserializes responses into typed records.
- `AzureDevOpsApiTypes` — C# record definitions that mirror Azure DevOps REST API request and
  response payloads; used as deserialization targets by `AzureDevOpsRestClient`.
- `WorkItemMapper` — maps `AzureDevOpsWorkItem` records into `ItemInfo` records, merging
  `buildmark` block overrides with Azure DevOps custom fields.

#### Interfaces

**AzureDevOpsRepoConnector**: The production `IRepoConnector` implementation for Azure DevOps
repositories. All other types in the subsystem are internal.

- *Type*: In-process .NET public API.
- *Role*: Provider — exposed to `RepoConnectorFactory` and callers of `IRepoConnector`.
- *Contract*: Constructor `AzureDevOpsRepoConnector(AzureDevOpsConnectorConfig?)` accepts optional
  configuration overrides; `GetBuildInformationAsync(VersionTag? version)` fetches complete build
  information from the Azure DevOps REST API and returns a `BuildInformation` record.
- *Constraints*: Requires a valid Azure DevOps authentication token resolvable from environment
  variables or the `az` CLI; throws `InvalidOperationException` when no token is found or when the
  remote URL cannot be parsed.

#### Design

`AzureDevOpsRepoConnector` orchestrates the subsystem's data flow:

1. Read the git remote URL and current commit hash via `RunCommandAsync` (inherited from
   `RepoConnectorBase`).
2. Determine the organization URL, project, and repository name from `AzureDevOpsConnectorConfig`
   or by parsing the remote URL.
3. Resolve an Azure DevOps authentication token (PAT or Entra ID Bearer token).
4. Create an `AzureDevOpsRestClient` and fetch tags, commits, pull requests, and work items via
   the REST API, using `AzureDevOpsApiTypes` records as deserialization targets.
5. Call `WorkItemMapper.MapWorkItemToItemInfo` for each work item, which internally calls
   `ItemControlsParser.Parse` on each description and merges the result with `Custom.Visibility`
   and `Custom.AffectedVersions` custom fields (custom fields take precedence).
6. Collect changes and known issues; if routing rules are configured, call `ApplyRules` (inherited
   from `RepoConnectorBase`) to distribute all items into the configured sections and populate
   `BuildInformation.RoutedSections`. If no rules are configured, items remain in the legacy
   `Changes`, `Bugs`, and `KnownIssues` lists.
7. Return the assembled `BuildInformation` record.
