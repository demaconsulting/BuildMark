### AzureDevOps Subsystem

#### Overview

The AzureDevOps subsystem groups the units responsible for querying the Azure DevOps
REST API. It sits within the RepoConnectors subsystem and provides the production
connector used when the repository host is Azure DevOps.

#### Units

- `AzureDevOpsRepoConnector` - `RepoConnectors/AzureDevOps/AzureDevOpsRepoConnector.cs` -
  implements `IRepoConnector` for Azure DevOps.
- `AzureDevOpsRestClient` - `RepoConnectors/AzureDevOps/AzureDevOpsRestClient.cs` -
  issues paginated REST API requests.
- `AzureDevOpsApiTypes` - `RepoConnectors/AzureDevOps/AzureDevOpsApiTypes.cs` -
  provides record types for REST API request and response data.
- `WorkItemMapper` - `RepoConnectors/AzureDevOps/WorkItemMapper.cs` -
  maps Azure DevOps work items to `ItemInfo` objects.

##### `AzureDevOpsRepoConnector`

The primary production connector. Resolves the repository URL, organization, and project
from the environment or configuration, creates an `AzureDevOpsRestClient`, fetches all
required data via REST APIs, applies item-controls overrides from buildmark blocks and
custom fields, calls `ItemRouter` to assign items to sections, and assembles the
`BuildInformation` record.

##### `AzureDevOpsRestClient`

Handles HTTPS communication with the Azure DevOps REST API endpoint. Supports paginated
requests and authenticates via a `Basic` or `Bearer` authorization header. Supports both
cloud (`dev.azure.com`) and on-premises Azure DevOps Server instances via configurable
organization URL.

##### `AzureDevOpsApiTypes`

Internal C# records that mirror the REST API response types returned by Azure DevOps.
Used as the deserialization target for responses from `AzureDevOpsRestClient`.

##### `WorkItemMapper`

Maps `AzureDevOpsWorkItem` records from the REST API into `ItemInfo` records for the
`BuildInformation` model. Extracts visibility, type, and affected-version controls from
both buildmark code blocks in the work item description and Azure DevOps custom fields
(`Custom.Visibility`, `Custom.AffectedVersions`). Custom fields take precedence over
buildmark blocks when both are present.

#### Interfaces

The AzureDevOps subsystem exposes `AzureDevOpsRepoConnector`, which implements
`IRepoConnector`. All other types in the subsystem are internal.

| Member | Kind | Description |
|---|---|---|
| `AzureDevOpsRepoConnector(config)` | Constructor | Create the connector with optional configuration overrides |
| `GetBuildInformationAsync(version)` | Method | Fetch complete build information from the Azure DevOps REST API |

#### Design

`AzureDevOpsRepoConnector` orchestrates the subsystem's data flow. It uses
`AzureDevOpsRestClient` for all HTTPS communication, `AzureDevOpsApiTypes`
records as serialization targets, and `WorkItemMapper` to transform raw work
item responses into `ItemInfo` records.

The connector calls `ItemControlsParser.Parse` on each work item description
body and also reads `Custom.Visibility` and `Custom.AffectedVersions` custom
fields directly. `WorkItemMapper` merges the two sources (custom fields take
precedence) and returns the final `ItemInfo` record or `null` when the item
should be excluded. When routing rules have been configured, the connector passes
all collected items to `ApplyRules` (inherited from `RepoConnectorBase`) to
populate `BuildInformation.RoutedSections`.

#### Interactions

| Unit / Subsystem              | Role                                                                      |
|-------------------------------|---------------------------------------------------------------------------|
| `IRepoConnector`              | Interface implemented by `AzureDevOpsRepoConnector`                       |
| `RepoConnectorBase`           | Base class for `AzureDevOpsRepoConnector`                                 |
| `ItemRouter`                  | Called by `AzureDevOpsRepoConnector` to assign items to sections          |
| `ProcessRunner`               | Used (via `RepoConnectorBase`) to run Git and az CLI commands             |
| `AzureDevOpsConnectorConfig`  | Supplies organization URL, project, and repository overrides              |
| `ItemControlsParser`          | Parses buildmark blocks from work item description bodies                 |
| `BuildInformation`            | The output record assembled and returned by `AzureDevOpsRepoConnector`    |
