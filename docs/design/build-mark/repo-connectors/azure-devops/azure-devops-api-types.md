# AzureDevOpsApiTypes

## Overview

`AzureDevOpsApiTypes` is the collection of internal record types used by the
Azure DevOps subsystem to represent REST API request and response payloads. These
types allow `AzureDevOpsRestClient` to deserialize Azure DevOps API responses into
strongly typed objects that `AzureDevOpsRepoConnector` and `WorkItemMapper` can
process safely and predictably.

## Responsibilities

- Represent repository, commit, pull request, and work item response objects
- Carry pagination metadata (`AzureDevOpsCollectionResponse<T>`)
- Preserve work item description (`System.Description`) and custom fields
  (`Custom.Visibility`, `Custom.AffectedVersions`) for item-controls parsing

## Key Types

### `AzureDevOpsRepository`

Repository metadata returned by the repository lookup endpoint.

Fields: `id`, `name`, `remoteUrl`

### `AzureDevOpsCommit`

Commit data returned by the commits endpoint.

Fields: `commitId`, `comment`

### `AzureDevOpsPullRequest`

Pull request data returned by the pull requests endpoint.

Fields: `pullRequestId`, `title`, `url`, `status`, `mergeCommitId`,
`sourceRefName`, `workItemRefs`, `description`

### `AzureDevOpsWorkItem`

Work item data returned by the work items endpoint with all fields expanded.

Fields: `id`, `fields` (dictionary)

Key dictionary entries:

| Field Key                     | Description                                       |
|-------------------------------|---------------------------------------------------|
| `System.Title`                | Work item title                                   |
| `System.WorkItemType`         | Work item type (e.g. `Bug`, `User Story`)         |
| `System.State`                | Work item state (e.g. `Active`, `Resolved`)       |
| `System.Description`          | Work item description body (HTML or plain text)   |
| `Custom.Visibility`           | Optional visibility override (`public`/`internal`)|
| `Custom.AffectedVersions`     | Optional affected version range expression        |

### `AzureDevOpsWorkItemQuery`

Result of a WIQL query, used to identify open work items matching a given filter.

Fields: `workItems` (list of id references)

### `AzureDevOpsCollectionResponse<T>`

Generic wrapper for paginated collection responses from the Azure DevOps REST API.

Fields: `count`, `value` (list of `T`)

## Interactions

- `AzureDevOpsRestClient` uses these records as serialization and deserialization
  targets for REST API HTTP traffic.
- `AzureDevOpsRepoConnector` and `WorkItemMapper` consume the deserialized data
  returned by `AzureDevOpsRestClient`.
