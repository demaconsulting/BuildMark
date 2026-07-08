#### AzureDevOpsApiTypes

![AzureDevOps Structure](../../../generated/AzureDevOpsView.svg)

##### Purpose

`AzureDevOpsApiTypes` is the collection of internal C# record types used by the AzureDevOps
subsystem to represent REST API request and response payloads. These types allow
`AzureDevOpsRestClient` to deserialize Azure DevOps API responses into strongly typed objects that
`AzureDevOpsRepoConnector` and `WorkItemMapper` can process safely and predictably.

All record types use conventional PascalCase property names. `AzureDevOpsRestClient` supplies a
`JsonSerializerOptions` instance with `PropertyNamingPolicy = JsonNamingPolicy.CamelCase` and
`NumberHandling = JsonNumberHandling.AllowReadingFromString`, which maps PascalCase names to the
Azure DevOps camelCase JSON field names automatically and handles numeric fields that the API may
return as quoted strings. The sole exception is `AzureDevOpsWorkItem.Fields`, which is
deserialized as a `Dictionary<string, object?>` and preserves the dotted field-reference-name keys
(e.g. `System.WorkItemType`) verbatim.

##### Data Model

**AzureDevOpsRepository.Id**: `string?` — Repository identifier used in subsequent API calls.

**AzureDevOpsRepository.Name**: `string?` — Repository display name.

**AzureDevOpsRepository.RemoteUrl**: `string?` — HTTPS clone URL for the repository.

**AzureDevOpsCommit.CommitId**: `string?` — SHA of the commit.

**AzureDevOpsCommit.Comment**: `string?` — Commit message summary.

**AzureDevOpsGitCommitRef.CommitId**: `string?` — SHA of the referenced commit; used as the merge
commit reference inside `AzureDevOpsPullRequest`.

**AzureDevOpsPullRequest.PullRequestId**: `int` — Pull request number.

**AzureDevOpsPullRequest.Title**: `string?` — Pull request title.

**AzureDevOpsPullRequest.Url**: `string?` — HTML URL of the pull request.

**AzureDevOpsPullRequest.Status**: `string?` — Pull request status (e.g. `active`, `completed`,
`abandoned`).

**AzureDevOpsPullRequest.LastMergeCommit**: `AzureDevOpsGitCommitRef?` — Reference to the most
recent merge commit; `null` for open pull requests. The computed property `MergeCommitId` returns
`LastMergeCommit?.CommitId`.

**AzureDevOpsPullRequest.SourceRefName**: `string?` — Full ref name of the source branch (e.g.
`refs/heads/my-feature`).

**AzureDevOpsPullRequest.Description**: `string?` — Pull request description body; passed to
`ItemControlsParser`.

**AzureDevOpsWorkItem.Id**: `int` — Work item identifier.

**AzureDevOpsWorkItem.Fields**: `Dictionary<string, object?>` — All work item fields keyed by
Azure DevOps field reference name (e.g. `System.Title`, `System.WorkItemType`,
`System.Description`, `Custom.Visibility`, `Custom.AffectedVersions`).

**AzureDevOpsWorkItemRef.Id**: `int` — Work item identifier; used in WIQL query results and
pull-request work-item links.

**AzureDevOpsWorkItemRef.Url**: `string?` — REST API URL for the work item.

**AzureDevOpsRef.Name**: `string?` — Full reference name (e.g. `refs/tags/v1.0.0`).

**AzureDevOpsRef.ObjectId**: `string?` — SHA the reference points to directly; for annotated tags
this is the tag object SHA, not the commit SHA.

**AzureDevOpsRef.PeeledObjectId**: `string?` — Resolved commit SHA for annotated tags; `null` for
lightweight tags. The computed property `CommitId` returns `PeeledObjectId ?? ObjectId`, providing
the commit SHA regardless of tag type.

**AzureDevOpsWorkItemQuery.WorkItems**: `List<AzureDevOpsWorkItemRef>?` — Work item id references
returned by a WIQL query.

**AzureDevOpsCollectionResponse\<T\>.Count**: `int` — Number of items in the current page.

**AzureDevOpsCollectionResponse\<T\>.Value**: `List<T>?` — Items in the current page.

**AzureDevOpsApiError.Message**: `string?` — Human-readable error description returned by the
Azure DevOps API on failure.

**AzureDevOpsApiError.TypeKey**: `string?` — Machine-readable error type identifier returned by
the Azure DevOps API on failure.

##### Key Methods

N/A — `AzureDevOpsApiTypes` is a collection of immutable C# record types used purely for JSON
deserialization. No methods beyond C#-generated record members are defined.

##### Error Handling

N/A — These are immutable data record types used purely for JSON deserialization. No methods detect
or propagate errors.

##### Dependencies

- **System.Text.Json** — runtime library used by `AzureDevOpsRestClient` to deserialize these
  records from HTTP response bodies.

##### Callers

- **AzureDevOpsRestClient** — uses these records as serialization and deserialization targets for
  REST API HTTP traffic.
- **AzureDevOpsRepoConnector** — consumes the deserialized record data returned by
  `AzureDevOpsRestClient`.
- **WorkItemMapper** — receives `AzureDevOpsWorkItem` records and reads their `Fields` dictionary.
