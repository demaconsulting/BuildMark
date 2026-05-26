#### GitHubRepoConnector

##### Purpose

`GitHubRepoConnector` is the production connector in the GitHub subsystem. It implements
`RepoConnectorBase` and queries the GitHub GraphQL API via `GitHubGraphQLClient` to fetch issues,
pull requests, version tags, and commits needed to construct a `BuildInformation` record.

The unit reads the repository URL and current commit hash from Git, resolves the GitHub token from
environment variables or the `gh` CLI, and applies item-controls overrides from `buildmark` blocks
embedded in issue and pull request description bodies before assembling the result.

##### Data Model

**_config**: `GitHubConnectorConfig?` — Optional configuration supplying owner, repository name,
GraphQL base URL, and token variable overrides. Received from `RepoConnectorFactory` at
construction time.

Authentication is resolved in two modes. In custom variable mode (when `_config.TokenVariable` is
set), the named environment variable is read exclusively; missing or empty values throw
`InvalidOperationException`. In default mode, the following sources are tried in order: (1)
`GH_TOKEN` environment variable; (2) `GITHUB_TOKEN` environment variable; (3) output of `gh auth
token`. If no token is found, `InvalidOperationException` is thrown.

GitHub labels are mapped to normalized `ItemInfo.Type` values: `bug` and `defect` map to `"bug"`;
`feature` and `enhancement` map to `"feature"`; `dependencies`, `renovate`, and `dependabot` map
to `"dependencies"`; `internal` and `chore` map to `"internal"`; `documentation`, `performance`,
and `security` are preserved as the label name; unlabeled items default to `"other"`.

##### Key Methods

**GetBuildInformationAsync**: Main entry point; fetches all data required to assemble a
`BuildInformation` record.

- *Parameters*: `VersionTag? version` — optional target version; when omitted, the most recent
  GitHub Release whose tag matches the current commit hash is used.
- *Returns*: `Task<BuildInformation>` — fully populated build information record.
- *Preconditions*: A resolvable GitHub token must be available in the environment.
- *Postconditions*: Returns a `BuildInformation` record; throws `InvalidOperationException` on
  authentication or version resolution failure.

Steps: (1) get repository URL, branch, and current commit hash from Git via `RunCommandAsync`
(inherited from `RepoConnectorBase`); (2) determine owner and repository name from `_config` or
by parsing the remote URL; (3) resolve the GitHub authentication token; (4) create a
`GitHubGraphQLClient` with the resolved token, using `_config.BaseUrl` as the GraphQL endpoint
when set (supports GitHub Enterprise Server); (5) fetch tags, releases, commits, pull requests
(with `body`), and issues (with `body`) via GraphQL; (6) determine the target version — if a
`version` argument is supplied, use it directly; otherwise use the most recent release whose tag
matches the current commit hash (throws `InvalidOperationException` when no match is found); (7)
determine the baseline version using `FindBaselineForRelease` or `FindBaselineForPreRelease`
(both inherited from `RepoConnectorBase`); (8) collect changes from pull requests merged in the
commit range, calling `ItemControlsParser.Parse` on each `body` and applying overrides; (9)
collect known issues from all issues (`states: [OPEN, CLOSED]`), applying item-controls overrides;
for each candidate bug, include it when `AffectedVersions.Contains(targetVersion)` if declared,
or only when the issue is open otherwise (covers LTS back-port gaps for closed bugs with declared
affected versions); (10) if routing rules are configured, call `ApplyRules` to populate
`BuildInformation.RoutedSections`; otherwise use legacy `Changes`, `Bugs`, and `KnownIssues`
lists; (11) generate the changelog URL; (12) return the assembled `BuildInformation`.

##### Error Handling

`GetBuildInformationAsync` throws `InvalidOperationException` when no GitHub token can be resolved,
when no release matches the current commit hash and no version is specified explicitly, or when a
git command fails. These exceptions propagate to `Program.ProcessBuildNotes`, which catches them,
writes an error message via `context.WriteError`, and returns early without generating a report.

##### Dependencies

- **RepoConnectorBase** — base class providing `Configure`, `HasRules`, `ApplyRules`,
  `FindVersionIndex`, `FindBaselineForPreRelease`, `FindBaselineForRelease`, and `RunCommandAsync`.
- **GitHubGraphQLClient** — executes GraphQL queries against the GitHub API.
- **GitHubConnectorConfig** — supplies owner, repository, base URL, and token variable overrides.
- **ItemControlsParser** — parses `buildmark` blocks from issue and pull request description
  bodies.
- **ProcessRunner** — used via `RunCommandAsync` to run Git commands.
- **BuildInformation** — the output record assembled and returned by this unit.
- **ItemInfo** — the normalized item representation stored in `BuildInformation`.

##### Callers

- **RepoConnectorFactory** — creates `GitHubRepoConnector` when the environment indicates GitHub
  or as the default fallback.
