### BuildInformation

#### Verification Approach

`BuildInformation` is verified with dedicated unit tests in `BuildInformationTests.cs`. Tests
construct `BuildInformation` instances either directly or via `MockRepoConnector`, invoke
`ToMarkdown`, and assert on the rendered output. `NSubstitute` is used in a small number of tests
to simulate connector error conditions; `MockRepoConnector` is used for the remaining tests. No
further mocking is needed.

#### Dependencies

| Mock / Stub | Reason |
| --- | --- |
| `MockRepoConnector` | Supplies deterministic `BuildInformation` instances for rendering tests. |
| `NSubstitute` (`IRepoConnector`) | Simulates error conditions that `MockRepoConnector` cannot reproduce. |

#### Test Scenarios

##### BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndNoTags

**Scenario**: A substitute connector is configured to throw `InvalidOperationException` with the
message "No tags found"; `GetBuildInformationAsync()` is called without a version argument.

**Expected**: `InvalidOperationException` is thrown; message contains "No tags found".

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag

**Scenario**: A substitute connector is configured to throw `InvalidOperationException` with the
message "does not match any tag"; `GetBuildInformationAsync()` is called.

**Expected**: `InvalidOperationException` is thrown; message contains "does not match any tag".

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_WorksWithExplicitVersion

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("v2.1.0"))` is called.

**Expected**: `CurrentVersionTag.VersionTag.Tag` equals `"v2.1.0"`; commit hash is
`"current123hash456"`; `BaselineVersionTag.VersionTag.Tag` equals `"v2.0.0"`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_WorksWhenCurrentCommitMatchesLatestTag

**Scenario**: A substitute connector returns a `BuildInformation` where `CurrentVersionTag` is
`v2.0.0`; `GetBuildInformationAsync()` is called.

**Expected**: Returned `BuildInformation.CurrentVersionTag.VersionTag.Tag` equals `"v2.0.0"`;
`BaselineVersionTag.VersionTag.Tag` equals `"ver-1.1.0"`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_PreReleaseUsesPreviousTag

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("v2.0.0-beta.1"))` is
called.

**Expected**: `CurrentVersionTag.VersionTag.Tag` equals `"v2.0.0-beta.1"`;
`BaselineVersionTag.VersionTag.Tag` equals `"v2.0.0"`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_ReleaseSkipsPreReleases

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"))` is called.

**Expected**: `CurrentVersionTag.VersionTag.Tag` equals `"v2.0.0"`;
`BaselineVersionTag.VersionTag.Tag` equals `"ver-1.1.0"` (pre-releases are skipped).

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_CollectsIssuesCorrectly

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("ver-1.1.0"))` is
called.

**Expected**: `Changes` contains 2 items (ids `"1"` and `"#13"`); `Bugs` is empty; `KnownIssues`
contains 2 items (ids `"4"` and `"6"`).

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("ver-1.1.0"))` is
called.

**Expected**: `Changes` are ordered by `Index`; the item with `Index` 10 precedes the item with
`Index` 13.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_SeparatesBugAndChangeIssues

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"))` is called.

**Expected**: `Changes` contains 1 item; `Bugs` contains 1 item (id `"2"`, title `"Fix bug in Y"`).

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_GetBuildInformationAsync_HandlesFirstReleaseCorrectly

**Scenario**: `MockRepoConnector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"))` is called.

**Expected**: `BaselineVersionTag` is null; `CurrentVersionTag.VersionTag.Tag` equals `"v1.0.0"`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults

**Scenario**: `ToMarkdown()` is called on `BuildInformation` for `v2.0.0` without any optional
arguments.

**Expected**: Markdown contains `# Build Report`, `## Version Information`, `## Changes`,
`## Bugs Fixed`, `v2.0.0`, and `ver-1.1.0`; `## Known Issues` is absent.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_IncludesKnownIssuesWhenRequested

**Scenario**: `ToMarkdown(includeKnownIssues: true)` is called on `BuildInformation` for `v2.0.0`.

**Expected**: Markdown contains `## Known Issues`, `Known bug A`, and `Known bug C`; `Known bug B`
is absent.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_RespectsCustomHeadingDepth

**Scenario**: `ToMarkdown(headingDepth: 3)` is called on `BuildInformation` for `v2.0.0`.

**Expected**: Top-level heading is `### Build Report`; sub-headings are `#### Version Information`,
`#### Changes`, and `#### Bugs Fixed`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_DisplaysNAForEmptyChanges

**Scenario**: A `BuildInformation` with an empty `Changes` list is rendered via `ToMarkdown()`.

**Expected**: The Changes section contains `- N/A`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_DisplaysNAForEmptyBugs

**Scenario**: A `BuildInformation` with an empty `Bugs` list is rendered via `ToMarkdown()`.

**Expected**: The Bugs Fixed section contains `- N/A`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_IncludesIssueLinks

**Scenario**: `ToMarkdown()` is called on `BuildInformation` for `v2.0.0` which has items with
GitHub URLs.

**Expected**: Markdown contains formatted links such as
`- [3](https://github.com/example/repo/issues/3)` and
`- [2](https://github.com/example/repo/issues/2)`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_HandlesFirstReleaseWithNA

**Scenario**: `ToMarkdown()` is called on `BuildInformation` for `v1.0.0` (no baseline).

**Expected**: Version Information section contains `| **Previous Version** | N/A |` and
`| **Previous Commit Hash** | N/A |`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_IncludesFullChangelogWhenLinkPresent

**Scenario**: `ToMarkdown()` is called on `BuildInformation` for `v2.0.0` which has a changelog
link.

**Expected**: Markdown contains `## Full Changelog`, the text `See the full changelog at`, and the
URL containing `ver-1.1.0...v2.0.0`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_ExcludesFullChangelogWhenNoBaseline

**Scenario**: `ToMarkdown()` is called on `BuildInformation` for `v1.0.0` (no baseline version).

**Expected**: Markdown does not contain `## Full Changelog`.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### BuildInformation_ToMarkdown_UsesBulletLists

**Scenario**: `ToMarkdown(includeKnownIssues: true)` is called on `BuildInformation` for `v2.0.0`.

**Expected**: Changes, Bugs Fixed, and Known Issues sections each use `- [` bullet format; no
markdown table notation (`| :-: | :---------- |`) is present in those sections.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### VersionCommitTag_Constructor_StoresVersionAndHash

**Scenario**: A `VersionCommitTag` is constructed with a `VersionTag` and a commit hash string.

**Expected**: `VersionTag` property equals the supplied tag; `CommitHash` equals the supplied hash
string.

**Requirement coverage**: `BuildMark-BuildInformation-Markdown`.

##### WebLink_Constructor_StoresTextAndUrl

**Scenario**: A `WebLink` is constructed with display text and a target URL.

**Expected**: `LinkText` property equals the supplied text; `TargetUrl` property equals the
supplied URL.

**Requirement coverage**: `BuildMark-WebLink-Record`.

##### BuildInformation_ToMarkdown_WithRoutedSections_RendersCustomSections

**Scenario**: A `BuildInformation` is constructed with a populated `RoutedSections` list containing
two custom sections (Features, Bugs); `ToMarkdown()` is called.

**Expected**: Markdown contains `## Features` and `## Bugs` headings with their respective items;
`## Changes` and `## Bugs Fixed` are absent.

**Requirement coverage**: `BuildMark-BuildInformation-RoutedSections`.

##### BuildInformation_ToMarkdown_WithoutRoutedSections_RendersDefaultSections

**Scenario**: A `BuildInformation` is constructed with `RoutedSections` left as null;
`ToMarkdown()` is called.

**Expected**: Markdown contains the default `## Changes` and `## Bugs Fixed` headings.

**Requirement coverage**: `BuildMark-BuildInformation-RoutedSections`.

#### Requirements Coverage

- **`BuildMark-BuildInformation-Markdown`**:
  - BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndNoTags
  - BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag
  - BuildInformation_GetBuildInformationAsync_WorksWithExplicitVersion
  - BuildInformation_GetBuildInformationAsync_WorksWhenCurrentCommitMatchesLatestTag
  - BuildInformation_GetBuildInformationAsync_PreReleaseUsesPreviousTag
  - BuildInformation_GetBuildInformationAsync_ReleaseSkipsPreReleases
  - BuildInformation_GetBuildInformationAsync_CollectsIssuesCorrectly
  - BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex
  - BuildInformation_GetBuildInformationAsync_SeparatesBugAndChangeIssues
  - BuildInformation_GetBuildInformationAsync_HandlesFirstReleaseCorrectly
  - BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults
  - BuildInformation_ToMarkdown_IncludesKnownIssuesWhenRequested
  - BuildInformation_ToMarkdown_RespectsCustomHeadingDepth
  - BuildInformation_ToMarkdown_DisplaysNAForEmptyChanges
  - BuildInformation_ToMarkdown_DisplaysNAForEmptyBugs
  - BuildInformation_ToMarkdown_IncludesIssueLinks
  - BuildInformation_ToMarkdown_HandlesFirstReleaseWithNA
  - BuildInformation_ToMarkdown_IncludesFullChangelogWhenLinkPresent
  - BuildInformation_ToMarkdown_ExcludesFullChangelogWhenNoBaseline
  - BuildInformation_ToMarkdown_UsesBulletLists
  - VersionCommitTag_Constructor_StoresVersionAndHash
- **`BuildMark-WebLink-Record`**:
  - WebLink_Constructor_StoresTextAndUrl
- **`BuildMark-BuildInformation-RoutedSections`**:
  - BuildInformation_ToMarkdown_WithRoutedSections_RendersCustomSections
  - BuildInformation_ToMarkdown_WithoutRoutedSections_RendersDefaultSections
