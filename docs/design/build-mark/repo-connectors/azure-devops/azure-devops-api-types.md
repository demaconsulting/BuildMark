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
- Provide `System.Text.Json`-compatible record definitions so that
  `AzureDevOpsRestClient` can deserialize API responses without reflection
  workarounds or third-party JSON libraries

## Key Types

All record types are defined as C# `record` types with init-only properties using
conventional PascalCase property names. No `[JsonPropertyName]` attributes are
required for standard Azure DevOps fields because `AzureDevOpsRestClient` supplies a
`JsonSerializerOptions` instance with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`
and `NumberHandling = JsonNumberHandling.AllowReadingFromString`,
which maps those PascalCase property names to the Azure DevOps camelCase JSON field
names automatically and handles numeric fields that the API may return as quoted
strings. The sole exception is `AzureDevOpsWorkItem.Fields`, which is
deserialized as a `Dictionary<string, object?>` and preserves the dotted
field-reference-name keys (e.g. `System.WorkItemType`) verbatim.

### `AzureDevOpsRepository`

Repository metadata returned by the repository lookup endpoint.

Fields: `id`, `name`, `remoteUrl`

### `AzureDevOpsCommit`

Commit data returned by the commits endpoint.

Fields: `commitId`, `comment`

### `AzureDevOpsGitCommitRef`

Minimal Git commit reference containing only the commit SHA.

Fields: `commitId`

### `AzureDevOpsPullRequest`

Pull request data returned by the pull requests endpoint.

Fields: `pullRequestId`, `title`, `url`, `status`, `lastMergeCommit`,
`sourceRefName`, `description`

The `lastMergeCommit` field is an `AzureDevOpsGitCommitRef` object (or `null`)
representing the commit of the most recent pull request merge. The type exposes
a computed `MergeCommitId` property that returns `LastMergeCommit?.CommitId`,
providing a convenient nullable string accessor for the merge commit SHA.

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

### `AzureDevOpsWorkItemRef`

Work item id reference returned by WIQL queries and pull request work item links.

Fields: `id`, `url`

### `AzureDevOpsRef`

Git reference (tag or branch) returned by the Azure DevOps refs endpoint.

Fields: `name`, `objectId`, `peeledObjectId`

The `objectId` field contains the SHA of the object this reference points to
directly - for lightweight tags this is the commit SHA, and for annotated tags
this is the tag object SHA. The `peeledObjectId` field contains the commit SHA
for annotated tags (resolved through the tag object), or `null` for lightweight
tags. The type exposes a computed `CommitId` property that returns
`PeeledObjectId ?? ObjectId`, providing the resolved commit SHA regardless of
tag type.

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
