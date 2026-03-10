# Theory of Operations — BuildMark

In every software project, releases must be accompanied by build notes that describe the changes,
bug fixes, and known issues shipped in that version. Producing these notes manually is error-prone
and time-consuming: developers must comb through commit histories, cross-reference issue trackers, and
format the output consistently for every release.

**BuildMark** automates build notes generation. It queries the Git repository history and the issue
tracker to collect commits, pull requests, and issues that fall within a defined version range, then
renders a consistently formatted markdown report that can be published directly as release documentation.

## How It Works

### Version Tag Recognition

BuildMark identifies releases by their Git tags. A tag is recognized as a version tag if it matches
the pattern:

```text
[prefix] major.minor.patch [separator pre-release] [+build-metadata]
```

Where:

- `prefix` — optional non-numeric prefix such as `v`, `ver-`, or `release_`
- `major.minor.patch` — three-part numeric version (e.g., `1.2.3`)
- `separator` — `.` or `-` separating the core version from the pre-release identifier
- `pre-release` — optional pre-release label such as `alpha.1`, `beta.2`, or `rc.1`
- `build-metadata` — optional metadata after `+` such as `build.5` or `linux.x64`

Examples of recognized version tags:

| Tag | Recognized? |
| --- | ----------- |
| `1.2.3` | ✅ |
| `v1.2.3` | ✅ |
| `ver-1.2.3` | ✅ |
| `release_1.2.3` | ✅ |
| `2.0.0-beta.1` | ✅ |
| `v2.0.0-rc.1+linux` | ✅ |
| `feature-x` | ❌ |
| `latest` | ❌ |

Tags that do not match this pattern are ignored. Only matching tags are considered when determining
the current version and the baseline.

### Pulling Repository Information

BuildMark collects the data it needs in two layers: Git provides commit history and tag information,
and a platform connector enriches that data with issues, pull requests, and release metadata from the
issue tracker.

#### Connector Selection

At startup, BuildMark selects a platform connector based on the current execution environment:

1. **GitHub Actions environment** — if the `GITHUB_ACTIONS` or `GITHUB_WORKSPACE` environment
   variable is set, the GitHub connector is used.
2. **GitHub remote** — if the `origin` remote URL contains `github.com`, the GitHub connector is
   used.
3. **Default** — the GitHub connector is used as the default for unrecognized environments.

> **Note**: Azure DevOps support is planned for a future release.

#### GitHub

When the GitHub connector is selected, BuildMark proceeds as follows:

##### 1. Identify the repository

BuildMark runs `git remote get-url origin` to retrieve the remote URL, then parses the GitHub owner
and repository name from the URL. Both HTTPS and SSH remote formats are supported:

```text
https://github.com/owner/repo.git
git@github.com:owner/repo.git
```

BuildMark also runs `git rev-parse --abbrev-ref HEAD` to determine the current branch, and
`git rev-parse HEAD` to get the current commit hash.

##### 2. Authenticate with GitHub

BuildMark reads an authentication token from environment variables. It checks `GH_TOKEN` first,
then falls back to `GITHUB_TOKEN`. If no token is found, unauthenticated requests are made, which
are subject to lower API rate limits (60 requests per hour versus 5,000 with a token).

##### 3. Fetch repository data

BuildMark uses the GitHub GraphQL API to fetch all required data in parallel:

| Data | Purpose |
| ---- | ------- |
| Branch commits | Determines which commits are on the current branch and their order |
| Tags | Maps tag names to commit SHAs for version resolution |
| Releases | Provides ordered release history and release metadata |
| Pull requests | Associates commits with the pull requests that introduced them |
| Issues | Supplies issue titles and labels for change and bug classification |

Pagination is handled automatically; BuildMark fetches all pages until the complete dataset is
retrieved.

##### 4. Filter to the current branch

Because GitHub returns tags and releases from the entire repository, BuildMark filters them to only
those whose tagged commit appears in the current branch's commit list. This ensures that tags from
other branches do not affect version selection or change collection.

### Baseline Version Selection

Once the repository data is collected, BuildMark determines two version boundaries:

- **Current version** (`toVersion`) — the version specified by `--build-version`, or, if omitted,
  the most recent release whose tag commit matches the current `HEAD`.
- **Baseline version** (`fromVersion`) — the previous release, selected according to the rules below.

The selection rule differs depending on whether the current version is a release or a pre-release:

**Release versions** (no pre-release suffix, e.g., `1.2.3`):

BuildMark walks backward through the release history and picks the first version that is also a
release (not a pre-release). All pre-release tags between the two releases are skipped.

**Pre-release versions** (with pre-release suffix, e.g., `1.2.3-rc.1`):

BuildMark walks backward through the release history and picks the first version whose commit hash
differs from the current version's commit hash. This handles re-tagging scenarios where multiple
pre-release tags point to the same commit — selecting such a tag would produce an empty changelog,
so BuildMark skips it.

If no suitable baseline is found (e.g., this is the first release), the baseline is `null` and the
report covers the full repository history from the beginning.

### Change Collection

BuildMark identifies all commits that fall within the range `(fromHash, toHash]` — that is, commits
reachable from `toHash` but not from `fromHash`.

For each commit in the range, BuildMark looks up the corresponding pull request using the commit's
SHA. For each matched pull request, it inspects the pull request labels to classify the change:

| Label keyword | Classification |
| ------------- | -------------- |
| `bug`, `defect` | Bug fix |
| `feature`, `enhancement` | Feature change |
| `documentation` | Documentation change |
| `performance` | Performance change |
| `security` | Security change |

Changes that are classified as bugs are placed in the **Bugs Fixed** list. All other changes go into
the **Changes** list.

**Known issues** are collected from all open issues that carry a bug-related label and were not
already resolved in this build.

### Report Generation

BuildMark renders the collected data as a markdown document with the following sections:

1. **Build Report** — the report title
2. **Version Information** — a table showing the current version, its commit hash, and the
   baseline version and commit hash (or `N/A` if this is the first release)
3. **Changes** — a list of non-bug pull requests and issues, each linked to its GitHub URL
4. **Bugs Fixed** — a list of bug-classified issues and pull requests, each linked to its GitHub URL
5. **Known Issues** — open bug issues not resolved in this build (only when `--include-known-issues`
   is specified)
6. **Full Changelog** — a link to the GitHub compare view between the baseline and current version
   tags (omitted if no baseline exists or the platform does not support compare links)

The `--report-depth` option shifts all heading levels by the specified amount. A depth of `1`
(default) uses `#` for the report title and `##` for section headings. A depth of `2` uses `##` for
the title and `###` for sections, making it straightforward to embed the report inside a larger
document.

## Output

### Build Report

The generated markdown report follows this structure:

```markdown
# Build Report

## Version Information

| Field | Value |
| ----- | ----- |
| **Version** | 1.2.3 |
| **Commit Hash** | abc123def456 |
| **Previous Version** | 1.2.0 |
| **Previous Commit Hash** | 789012abc345 |

## Changes

- [#42](https://github.com/owner/repo/pull/42) - Add new feature X
- [#43](https://github.com/owner/repo/pull/43) - Improve performance of Y

## Bugs Fixed

- [#40](https://github.com/owner/repo/issues/40) - Fix crash when Z is null
- [#41](https://github.com/owner/repo/issues/41) - Correct validation logic

## Known Issues

- [#50](https://github.com/owner/repo/issues/50) - Performance degradation on large datasets

## Full Changelog

See the full changelog at [v1.2.0...v1.2.3](https://github.com/owner/repo/compare/v1.2.0...v1.2.3).
```

If no changes or bugs were found in the range, the corresponding section contains `- N/A` rather
than being omitted, so the document structure remains predictable for downstream consumers.

## CI/CD Integration

BuildMark is typically invoked in the release stage of a CI/CD pipeline, after the build and tests
have passed. The GitHub token is supplied via an environment variable so that no credentials are
stored in source control:

```yaml
- name: Generate Build Notes
  env:
    GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
  run: |
    buildmark \
      --build-version ${{ inputs.version }} \
      --report docs/buildnotes/build-notes.md
```

The generated report can then be uploaded as a build artifact, committed back to the repository, or
fed into a documentation pipeline.

## Self-Validation

BuildMark includes a built-in self-validation mode that proves the tool is functioning correctly
without requiring access to an external repository or issue tracker. This is particularly important
in regulated environments where tool qualification evidence is required.

Running `buildmark --validate` executes a suite of tests against mock data and prints a summary:

```text
✓ BuildMark_MarkdownReportGeneration - Passed
✓ BuildMark_GitIntegration - Passed
✓ BuildMark_IssueTracking - Passed
✓ BuildMark_KnownIssuesReporting - Passed

Total Tests: 4
Passed: 4
Failed: 0
```

Results can be saved in TRX (MSTest) or JUnit XML format for integration with CI/CD test reporting:

```bash
buildmark --validate --results validation-results.trx
```

If any test fails, BuildMark exits with a non-zero exit code, blocking downstream pipeline stages
until the issue is resolved.
