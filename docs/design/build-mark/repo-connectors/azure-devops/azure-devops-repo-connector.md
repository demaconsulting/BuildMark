#### AzureDevOpsRepoConnector

![AzureDevOps Structure](../../../generated/AzureDevOpsView.svg)

##### Purpose

`AzureDevOpsRepoConnector` is the production connector in the AzureDevOps subsystem. It implements
`RepoConnectorBase` and queries the Azure DevOps REST API via `AzureDevOpsRestClient` to fetch
commits, tags, pull requests, and work items needed to construct a `BuildInformation` record.

The unit reads the repository URL and current commit hash from Git, resolves the Azure DevOps
authentication token from environment variables or the `az` CLI, and applies item-controls
overrides from `buildmark` blocks and Azure DevOps custom fields before assembling the result.

##### Data Model

**_config**: `AzureDevOpsConnectorConfig?` — Optional configuration supplying organization URL,
project, repository, area path, and token variable overrides. Received from `RepoConnectorFactory`
at construction time.

Authentication is resolved in two modes. In custom variable mode (when `_config.TokenVariable` is
set), the named environment variable is read exclusively; missing or empty values throw
`InvalidOperationException`. In default mode, the following sources are tried in order: (1)
`AZURE_DEVOPS_PAT` as a Basic PAT credential; (2) `AZURE_DEVOPS_TOKEN` as a Basic PAT credential;
(3) `AZURE_DEVOPS_EXT_PAT` as a Basic PAT credential; (4) `SYSTEM_ACCESSTOKEN` as a Bearer
credential (set automatically by Azure Pipelines); (5) output of `az account get-access-token
--resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv` as a Bearer
credential. If no token is found, `InvalidOperationException` is thrown.

Work item types are normalized before storage in `ItemInfo`: `Bug` and `Issue` map to `"bug"`;
`User Story`, `Feature`, and `Epic` map to `"feature"`; all other types are preserved as-is.
Custom fields `Custom.Visibility` and `Custom.AffectedVersions` take precedence over equivalent
values extracted from `buildmark` blocks when both are present.

##### Key Methods

**ParseAzureDevOpsUrl**: Internal utility that parses a git remote URL into organization URL,
project name, and repository name.

- *Parameters*: `string url` — the git remote URL to parse.
- *Returns*: `(string organizationUrl, string project, string repository)`.
- *Preconditions*: `url` must be non-null.
- *Postconditions*: Returns a valid tuple when the URL matches a supported format.

Supported formats: `https://dev.azure.com/{org}/{project}/_git/{repo}`;
`https://{org}.visualstudio.com/{project}/_git/{repo}`; `https://{server}/{org}/{project}/_git/
{repo}` (on-premises); `git@ssh.dev.azure.com:v3/{org}/{project}/{repo}` (SSH). Throws
`ArgumentException` for unsupported formats.

**GetBuildInformationAsync**: Main entry point; fetches all data required to assemble a
`BuildInformation` record.

- *Parameters*: `VersionTag? version` — optional target version; when omitted, the highest
  available tag is used as the target.
- *Returns*: `Task<BuildInformation>` — fully populated build information record.
- *Preconditions*: A resolvable Azure DevOps token must be available in the environment.
- *Postconditions*: Returns a `BuildInformation` record; throws `InvalidOperationException` or
  `ArgumentException` on failure.

Steps: (1) get repository URL, branch, and current commit hash from Git; (2) determine organization
URL, project, and repository name from config or by parsing the remote URL; (3) resolve the
authentication token; (4) create `AzureDevOpsRestClient`; (5) fetch tags via
`GET /git/repositories/{id}/refs?filter=tags&peelTags=true` (the `peelTags=true` parameter
resolves annotated tags to their commit SHA); (6) fetch commits; (7) fetch all pull requests; (8)
determine target and baseline version tags; (9) collect pull-request changes in the commit range,
calling `WorkItemMapper` for linked work items; (10) collect known issues from a WIQL query scoped
to the configured area path (defaulting to the project name when not configured); (11) if routing
rules are configured, call `ApplyRules` to populate `BuildInformation.RoutedSections`; otherwise
use legacy `Changes`, `Bugs`, and `KnownIssues` lists; (12) return the assembled
`BuildInformation`.

For each candidate known issue: if `AffectedVersions` is declared, the bug is included if and only
if `AffectedVersions.Contains(targetVersion)` is true (regardless of resolved state, covering LTS
back-port gaps); if no `AffectedVersions` is declared, only unresolved bugs are included.

##### Error Handling

`GetBuildInformationAsync` throws `InvalidOperationException` when no authentication token can be
resolved or when a git command fails. `ParseAzureDevOpsUrl` propagates `ArgumentException` when the
URL format is not supported. These exceptions propagate to `Program.ProcessBuildNotes`, which
catches them, writes an error message via `context.WriteError`, and returns early without
generating a report.

##### Dependencies

- **RepoConnectorBase** — base class providing `Configure`, `HasRules`, `ApplyRules`,
  `FindVersionIndex`, `FindBaselineForPreRelease`, `FindBaselineForRelease`, and `RunCommandAsync`.
- **AzureDevOpsRestClient** — executes REST API requests against the Azure DevOps API.
- **AzureDevOpsConnectorConfig** — supplies organization URL, project, repository, area path, and
  token variable overrides.
- **WorkItemMapper** — maps `AzureDevOpsWorkItem` records to `ItemInfo` records.
- **ItemControlsParser** — parses `buildmark` blocks from pull request description bodies.
- **ProcessRunner** — used via `RunCommandAsync` to run Git and `az` CLI commands.
- **BuildInformation** — the output record assembled and returned by this unit.
- **ItemInfo** — the normalized item representation stored in `BuildInformation`.

##### Callers

- **RepoConnectorFactory** — creates `AzureDevOpsRepoConnector` when the environment indicates
  Azure DevOps.
