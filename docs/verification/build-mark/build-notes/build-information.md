### BuildInformation

#### Verification Approach

`BuildInformation` is verified with dedicated unit tests in `BuildInformationTests.cs`. Tests
construct `BuildInformation` instances either directly or via `MockRepoConnector`, invoke
`ToMarkdown`, and assert on the rendered output. `MockRepoConnector` supplies deterministic
`BuildInformation` instances for rendering tests. `NSubstitute` is used in a small number of tests
to simulate connector error conditions that `MockRepoConnector` cannot reproduce (for example,
`InvalidOperationException` on missing tags). No further mocking is needed.

#### Test Environment

N/A - standard test environment. `BuildInformationTests.cs` runs within the standard `dotnet test`
host; no external services or environment setup beyond a `MockRepoConnector` instance are required.

#### Acceptance Criteria

- All tests in `BuildInformationTests.cs` pass with zero failures.
- Both success and error paths of `GetBuildInformationAsync` and `ToMarkdown` are covered.

#### Test Scenarios

**BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndNoTags**: Verifies that when the
connector throws `InvalidOperationException` with "No tags found", calling
`GetBuildInformationAsync()` propagates the exception.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndNoTags`.

**BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag**: Verifies
that when the connector throws with "does not match any tag", the exception propagates unchanged.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_ThrowsWhenNoVersionAndCommitDoesNotMatchTag`.

**BuildInformation_GetBuildInformationAsync_WorksWithExplicitVersion**: Verifies that calling with
`VersionTag.Create("v2.1.0")` returns a `BuildInformation` with `CurrentVersionTag.VersionTag.Tag`
equal to `"v2.1.0"` and `BaselineVersionTag.VersionTag.Tag` equal to `"v2.0.0"`.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_WorksWithExplicitVersion`.

**BuildInformation_GetBuildInformationAsync_WorksWhenCurrentCommitMatchesLatestTag**: Verifies that
when the current commit matches the latest tag `v2.0.0`, the returned baseline is `ver-1.1.0`.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_WorksWhenCurrentCommitMatchesLatestTag`.

**BuildInformation_GetBuildInformationAsync_PreReleaseUsesPreviousTag**: Verifies that a
pre-release version `v2.0.0-beta.1` uses the preceding release tag `v2.0.0` as its baseline.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_PreReleaseUsesPreviousTag`.

**BuildInformation_GetBuildInformationAsync_ReleaseSkipsPreReleases**: Verifies that a release
version `v2.0.0` skips pre-release tags and selects `ver-1.1.0` as the baseline.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_ReleaseSkipsPreReleases`.

**BuildInformation_GetBuildInformationAsync_CollectsIssuesCorrectly**: Verifies that for
`ver-1.1.0`, `Changes` contains 2 items (ids `"1"` and `"#13"`), `Bugs` is empty, and
`KnownIssues` contains 2 items (ids `"4"` and `"6"`).
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_CollectsIssuesCorrectly`.

**BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex**: Verifies that `Changes` items
for `ver-1.1.0` are sorted by `Index` in ascending order, with `Index` 10 preceding `Index` 13.
This scenario is tested by
`BuildInformation_GetBuildInformationAsync_OrdersChangesByIndex`.

**BuildInformation_GetBuildInformationAsync_SeparatesBugAndChangeIssues**: Verifies that for
`v2.0.0`, items labelled as bugs appear in `Bugs` (id `"2"`, title `"Fix bug in Y"`) and not in
`Changes`. This scenario is tested by
`BuildInformation_GetBuildInformationAsync_SeparatesBugAndChangeIssues`.

**BuildInformation_GetBuildInformationAsync_HandlesFirstReleaseCorrectly**: Verifies that the
first release `v1.0.0` returns `BaselineVersionTag` as null with `CurrentVersionTag.VersionTag.Tag`
equal to `"v1.0.0"`. This scenario is tested by
`BuildInformation_GetBuildInformationAsync_HandlesFirstReleaseCorrectly`.

**BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults**: Verifies that `ToMarkdown()`
on `v2.0.0` produces the expected section headings and version strings, and that `## Known Issues`
is absent by default. This scenario is tested by
`BuildInformation_ToMarkdown_GeneratesCorrectMarkdownWithDefaults`.

**BuildInformation_ToMarkdown_IncludesKnownIssuesWhenRequested**: Verifies that
`ToMarkdown(includeKnownIssues: true)` on `v2.0.0` includes `Known bug A` and `Known bug C` while
excluding `Known bug B`. This scenario is tested by
`BuildInformation_ToMarkdown_IncludesKnownIssuesWhenRequested`.

**BuildInformation_ToMarkdown_RespectsCustomHeadingDepth**: Verifies that
`ToMarkdown(headingDepth: 3)` uses `### Build Report` as the top heading and
`#### Version Information`, `#### Changes`, `#### Bugs Fixed` as sub-headings.
This scenario is tested by `BuildInformation_ToMarkdown_RespectsCustomHeadingDepth`.

**BuildInformation_ToMarkdown_DisplaysNAForEmptyChanges**: Verifies that a `BuildInformation`
with an empty `Changes` list renders `- N/A` in the Changes section.
This scenario is tested by `BuildInformation_ToMarkdown_DisplaysNAForEmptyChanges`.

**BuildInformation_ToMarkdown_DisplaysNAForEmptyBugs**: Verifies that a `BuildInformation` with
an empty `Bugs` list renders `- N/A` in the Bugs Fixed section.
This scenario is tested by `BuildInformation_ToMarkdown_DisplaysNAForEmptyBugs`.

**BuildInformation_ToMarkdown_IncludesIssueLinks**: Verifies that items with GitHub URLs are
rendered as formatted links such as `- [3](https://github.com/example/repo/issues/3)`.
This scenario is tested by `BuildInformation_ToMarkdown_IncludesIssueLinks`.

**BuildInformation_ToMarkdown_HandlesFirstReleaseWithNA**: Verifies that `v1.0.0` (no baseline)
renders `| **Previous Version** | N/A |` and `| **Previous Commit Hash** | N/A |` in the version
information table. This scenario is tested by
`BuildInformation_ToMarkdown_HandlesFirstReleaseWithNA`.

**BuildInformation_ToMarkdown_IncludesFullChangelogWhenLinkPresent**: Verifies that when a
changelog link is present, rendering produces a `## Full Changelog` section with the comparison
URL containing `ver-1.1.0...v2.0.0`.
This scenario is tested by `BuildInformation_ToMarkdown_IncludesFullChangelogWhenLinkPresent`.

**BuildInformation_ToMarkdown_ExcludesFullChangelogWhenNoBaseline**: Verifies that `v1.0.0` (no
baseline) does not produce a `## Full Changelog` section.
This scenario is tested by `BuildInformation_ToMarkdown_ExcludesFullChangelogWhenNoBaseline`.

**BuildInformation_ToMarkdown_UsesBulletLists**: Verifies that Changes, Bugs Fixed, and Known
Issues sections each use `- [` bullet format with no markdown table notation.
This scenario is tested by `BuildInformation_ToMarkdown_UsesBulletLists`.

**BuildInformation_ToMarkdown_WithRoutedSections_RendersCustomSections**: Verifies that when
`RoutedSections` is populated with custom sections (e.g., Features, Bugs), `ToMarkdown()` renders
those headings instead of the default `## Changes` and `## Bugs Fixed`.
This scenario is tested by
`BuildInformation_ToMarkdown_WithRoutedSections_RendersCustomSections`.

**BuildInformation_ToMarkdown_WithoutRoutedSections_RendersDefaultSections**: Verifies that when
`RoutedSections` is null, the default `## Changes` and `## Bugs Fixed` headings are rendered.
This scenario is tested by
`BuildInformation_ToMarkdown_WithoutRoutedSections_RendersDefaultSections`.

**BuildInformation_ToMarkdown_WithRoutedSectionsAndKnownIssues_RendersKnownIssuesSection**: Verifies
that when both `RoutedSections` and `KnownIssues` are populated, `ToMarkdown(includeKnownIssues: true)`
renders the routed section headings and also appends `## Known Issues`.
This scenario is tested by
`BuildInformation_ToMarkdown_WithRoutedSectionsAndKnownIssues_RendersKnownIssuesSection`.

**BuildInformation_ToMarkdown_WithRoutedSectionsAndKnownIssuesFlagFalse_DoesNotRenderKnownIssuesSection**:
Verifies that `ToMarkdown(includeKnownIssues: false)` suppresses the Known Issues section even
when `RoutedSections` and `KnownIssues` are populated.
This scenario is tested by
`BuildInformation_ToMarkdown_WithRoutedSectionsAndKnownIssuesFlagFalse_DoesNotRenderKnownIssuesSection`.

**VersionCommitTag_Constructor_StoresVersionAndHash**: Verifies that constructing a
`VersionCommitTag` stores the supplied `VersionTag` and commit hash string in the corresponding
properties. This scenario is tested by `VersionCommitTag_Constructor_StoresVersionAndHash`.
