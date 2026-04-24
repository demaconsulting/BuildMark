// Copyright (c) DEMA Consulting
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System.Runtime.InteropServices;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.Tests.RepoConnectors.GitHub;
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors;

/// <summary>
///     Subsystem tests for the RepoConnectors subsystem.
/// </summary>
[TestClass]
public class RepoConnectorsTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-GitHubConnector
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the GitHub connector implements the IRepoConnector interface.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_GitHubConnector_ImplementsInterface_ReturnsTrue()
    {
        // Arrange: create a GitHubRepoConnector instance
        var connector = new GitHubRepoConnector();

        // Assert: it satisfies the public IRepoConnector interface
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the GitHub connector returns valid build information from mocked API data.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_GitHubConnector_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange: set up a mocked GraphQL handler with a single release and commit
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("abc123def456")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(new MockTag("v1.0.0", "abc123def456"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "abc123def456");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: build information is complete and accurate
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("abc123def456", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsNotNull(buildInfo.Bugs);
        Assert.IsNotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that the GitHub connector selects the correct previous version as baseline.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_GitHubConnector_GetBuildInformation_WithMultipleVersions_SelectsCorrectBaseline()
    {
        // Arrange: set up three release tags so the connector can pick v1.1.0 as baseline for v2.0.0
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v2.0.0", "2024-03-01T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v2.0.0", "commit3"),
                new MockTag("v1.1.0", "commit2"),
                new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act: retrieve build information for v2.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: v1.1.0 is selected as baseline and a changelog link is generated
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.IsNotNull(buildInfo.BaselineVersionTag, "Previous version should be identified");
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.IsNotNull(buildInfo.CompleteChangelogLink, "Changelog link should be generated");
        Assert.Contains("v1.1.0...v2.0.0", buildInfo.CompleteChangelogLink.TargetUrl);
    }

    /// <summary>
    ///     Test that the GitHub connector correctly categorizes pull requests into changes and bugs.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_GitHubConnector_GetBuildInformation_WithPullRequests_GathersChanges()
    {
        // Arrange: two PRs – one labelled "feature", one labelled "bug"
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse(
                new MockPullRequest(
                    Number: 101,
                    Title: "Add new feature",
                    Url: "https://github.com/test/repo/pull/101",
                    Merged: true,
                    MergeCommitSha: "commit3",
                    HeadRefOid: "feature-branch",
                    Labels: ["feature"]),
                new MockPullRequest(
                    Number: 100,
                    Title: "Fix critical bug",
                    Url: "https://github.com/test/repo/pull/100",
                    Merged: true,
                    MergeCommitSha: "commit2",
                    HeadRefOid: "bugfix-branch",
                    Labels: ["bug"]))
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v1.1.0", "commit3"),
                new MockTag("v1.0.0", "commit1"))
            .AddResponse(
                "closingIssuesReferences",
                @"{""data"":{""repository"":{""pullRequest"":{""closingIssuesReferences"":{""nodes"":[],""pageInfo"":{""hasNextPage"":false,""endCursor"":null}}}}}}");

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit3");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert: feature PR is in Changes, bug PR is in Bugs
        Assert.IsNotNull(buildInfo);
        var featurePR = buildInfo.Changes.FirstOrDefault(c => c.Index == 101);
        Assert.IsNotNull(featurePR, "Feature PR should be in Changes");

        var bugPR = buildInfo.Bugs.FirstOrDefault(b => b.Index == 100);
        Assert.IsNotNull(bugPR, "Bug PR should be in Bugs");
    }

    /// <summary>
    ///     Test that the GitHub connector correctly identifies open issues as known issues.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_GitHubConnector_GetBuildInformation_WithOpenIssues_IdentifiesKnownIssues()
    {
        // Arrange: one open issue that is not resolved in this release
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit1")
            .AddReleasesResponse(new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse(
                new MockIssue(
                    Number: 201,
                    Title: "Known bug in feature X",
                    Url: "https://github.com/test/repo/issues/201",
                    State: "OPEN",
                    Labels: ["bug"]))
            .AddTagsResponse(new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit1");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: open issue surfaces as a known issue
        Assert.IsNotNull(buildInfo);
        Assert.IsGreaterThan(0, buildInfo.KnownIssues.Count, "Should have at least one known issue");
        var knownIssue = buildInfo.KnownIssues.FirstOrDefault(i => i.Index == 201);
        Assert.IsNotNull(knownIssue, "Open issue 201 should appear in KnownIssues");
        Assert.AreEqual("Known bug in feature X", knownIssue.Title);
    }

    /// <summary>
    ///     Test that the GitHub connector skips pre-releases when building the version baseline.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_GitHubConnector_GetBuildInformation_ReleaseVersion_SkipsPreReleases()
    {
        // Arrange: mix of release and pre-release tags; the connector must skip pre-releases
        // when selecting the baseline for a release version.
        using var mockHandler = new MockGitHubGraphQLHttpMessageHandler()
            .AddCommitsResponse("commit4", "commit3", "commit2", "commit1")
            .AddReleasesResponse(
                new MockRelease("v2.0.0", "2024-04-01T00:00:00Z"),
                new MockRelease("v2.0.0-rc.1", "2024-03-15T00:00:00Z"),
                new MockRelease("v1.1.0", "2024-02-01T00:00:00Z"),
                new MockRelease("v1.0.0", "2024-01-01T00:00:00Z"))
            .AddPullRequestsResponse()
            .AddIssuesResponse()
            .AddTagsResponse(
                new MockTag("v2.0.0", "commit4"),
                new MockTag("v2.0.0-rc.1", "commit3"),
                new MockTag("v1.1.0", "commit2"),
                new MockTag("v1.0.0", "commit1"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = new MockableGitHubRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://github.com/test/repo.git");
        connector.SetCommandResponse("git rev-parse --abbrev-ref HEAD", "main");
        connector.SetCommandResponse("git rev-parse HEAD", "commit4");
        connector.SetCommandResponse("gh auth token", "test-token");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: baseline should be v1.1.0 (the last release), not v2.0.0-rc.1 (a pre-release)
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.IsNotNull(buildInfo.BaselineVersionTag, "Baseline version should be set");
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion,
            "Release version should skip pre-releases when selecting baseline");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-ConnectorBase
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that MockRepoConnector satisfies the shared IRepoConnector interface.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ConnectorBase_MockConnector_ImplementsInterface()
    {
        // Arrange: create a MockRepoConnector
        var connector = new MockRepoConnector();

        // Assert: MockRepoConnector derives from the base class and satisfies the shared interface
        Assert.IsInstanceOfType<RepoConnectorBase>(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
        Assert.IsInstanceOfType<MockRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GitHubRepoConnector satisfies the shared IRepoConnector interface.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ConnectorBase_GitHubConnector_ImplementsInterface()
    {
        // Arrange: create a GitHubRepoConnector
        var connector = new GitHubRepoConnector();

        // Assert: GitHubRepoConnector derives from the base class and satisfies the shared interface
        Assert.IsInstanceOfType<RepoConnectorBase>(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-MockConnector
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that MockRepoConnector can be constructed.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_MockConnector_Constructor_CreatesInstance()
    {
        // Act: create a MockRepoConnector
        var connector = new MockRepoConnector();

        // Assert: instance is created and is of the expected type
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<MockRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that MockRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_MockConnector_ImplementsInterface_ReturnsTrue()
    {
        // Act: create a MockRepoConnector
        var connector = new MockRepoConnector();

        // Assert: it implements both the base class and the public interface
        Assert.IsInstanceOfType<RepoConnectorBase>(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that MockRepoConnector returns build information with the specified version.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_MockConnector_GetBuildInformation_ReturnsExpectedVersion()
    {
        // Arrange: create connector and request a known version
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("2.0.0");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert: current version tag matches the requested version
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("v2.0.0", buildInfo.CurrentVersionTag.VersionTag.Tag); // Actual repository tag
    }

    /// <summary>
    ///     Test that MockRepoConnector returns a complete BuildInformation structure.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation()
    {
        // Arrange: create connector
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("2.0.0");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert: all required collections are present
        Assert.IsNotNull(buildInfo, "BuildInformation should not be null");
        Assert.IsNotNull(buildInfo.Changes, "Changes list should not be null");
        Assert.IsNotNull(buildInfo.Bugs, "Bugs list should not be null");
        Assert.IsNotNull(buildInfo.KnownIssues, "KnownIssues list should not be null");
        Assert.IsNotNull(buildInfo.CurrentVersionTag, "CurrentVersionTag should not be null");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-ProcessRunner
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that TryRunAsync returns output when the command succeeds.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput()
    {
        // Arrange: choose a portable echo command
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "echo";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c echo test" : "test";

        // Act: run the command
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert: output is returned and contains the expected text
        Assert.IsNotNull(result, "TryRunAsync should return output for a successful command");
        Assert.IsTrue(result.Contains("test", StringComparison.OrdinalIgnoreCase),
            "Output should contain the echoed text");
    }

    /// <summary>
    ///     Test that TryRunAsync returns null when the command does not exist.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull()
    {
        // Arrange: a command that definitely does not exist
        var result = await ProcessRunner.TryRunAsync("nonexistent_command_12345678", "");

        // Assert: null is returned
        Assert.IsNull(result, "TryRunAsync should return null for a non-existent command");
    }

    /// <summary>
    ///     Test that TryRunAsync returns null when the command exits with a non-zero code.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull()
    {
        // Arrange: a command that exits with code 1
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c exit 1" : "-c 'exit 1'";

        // Act
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert: null is returned for a failed command
        Assert.IsNull(result, "TryRunAsync should return null when the command exits with a non-zero code");
    }

    /// <summary>
    ///     Test that RunAsync returns output when the command succeeds.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput()
    {
        // Arrange: a portable echo command
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "echo";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c echo test123" : "test123";

        // Act
        var result = await ProcessRunner.RunAsync(command, arguments);

        // Assert: output contains the expected text
        Assert.IsNotNull(result, "RunAsync should return output for a successful command");
        Assert.IsTrue(result.Contains("test123", StringComparison.OrdinalIgnoreCase),
            "Output should contain the echoed text");
    }

    /// <summary>
    ///     Test that RunAsync throws InvalidOperationException when the command fails.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_ProcessRunner_RunAsync_WithFailingCommand_ThrowsException()
    {
        // Arrange: a command that exits with code 1
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c exit 1" : "-c 'exit 1'";

        // Act & Assert: InvalidOperationException is thrown with a useful message
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await ProcessRunner.RunAsync(command, arguments));
        Assert.Contains("failed with exit code", exception.Message);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-Factory
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the factory creates a non-null connector instance.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_Factory_Create_ReturnsConnector()
    {
        // Act: create a connector via the factory
        var connector = RepoConnectorFactory.Create();

        // Assert: a valid IRepoConnector instance is returned
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the factory returns a GitHubRepoConnector for this repository.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_Factory_Create_ReturnsGitHubConnectorForThisRepo()
    {
        // Act: create a connector via the factory
        var connector = RepoConnectorFactory.Create();

        // Assert: the factory selects the GitHub connector for this GitHub-hosted repository
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-ItemControls
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the subsystem parses "public" visibility from a buildmark block.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_VisibilityPublic_ReturnsPublicVisibility()
    {
        // Arrange: description with public visibility
        var description = "Issue description.\n\n```buildmark\nvisibility: public\n```\n";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: visibility is "public"
        Assert.IsNotNull(result);
        Assert.AreEqual("public", result.Visibility);
    }

    /// <summary>
    ///     Test that the subsystem parses "internal" visibility from a buildmark block.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_VisibilityInternal_ReturnsInternalVisibility()
    {
        // Arrange: description with internal visibility
        var description = "Issue description.\n\n```buildmark\nvisibility: internal\n```\n";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: visibility is "internal"
        Assert.IsNotNull(result);
        Assert.AreEqual("internal", result.Visibility);
    }

    /// <summary>
    ///     Test that the subsystem parses "bug" type from a buildmark block.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_TypeBug_ReturnsBugType()
    {
        // Arrange: description with bug type
        var description = "Bug description.\n\n```buildmark\ntype: bug\n```\n";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: type is "bug"
        Assert.IsNotNull(result);
        Assert.AreEqual("bug", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem parses "feature" type from a buildmark block.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_TypeFeature_ReturnsFeatureType()
    {
        // Arrange: description with feature type
        var description = "Feature description.\n\n```buildmark\ntype: feature\n```\n";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: type is "feature"
        Assert.IsNotNull(result);
        Assert.AreEqual("feature", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem parses affected-versions from a buildmark block.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_AffectedVersions_ReturnsIntervalSet()
    {
        // Arrange: description with affected-versions field
        var description = "Description.\n\n```buildmark\naffected-versions: (,1.0.1],[1.1.0,1.2.0)\n```\n";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: affected versions interval set is parsed correctly
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.AffectedVersions);
        Assert.HasCount(2, result.AffectedVersions.Intervals);
    }

    /// <summary>
    ///     Test that the subsystem recognizes a buildmark block hidden in an HTML comment.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_HiddenBlock_ReturnsControls()
    {
        // Arrange: buildmark block wrapped in HTML comment delimiters
        var description = "Description.\n<!-- ```buildmark\ntype: feature\n``` -->";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: controls are extracted despite the HTML comment wrapping
        Assert.IsNotNull(result);
        Assert.AreEqual("feature", result.Type);
    }

    /// <summary>
    ///     Test that the subsystem returns null when no buildmark block is present.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemControls_NoBlock_ReturnsNull()
    {
        // Arrange: description with no buildmark block
        var description = "A plain description with no controls block.";

        // Act: parse the description
        var result = ItemControlsParser.Parse(description);

        // Assert: null is returned
        Assert.IsNull(result);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-ItemRouter
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the subsystem routes items to matching sections based on rules.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemRouter_MatchingRule_RoutesToSection()
    {
        // Arrange: define sections and rules, then create items to route
        List<BuildMark.Configuration.SectionConfig> sections =
        [
            new() { Id = "features", Title = "Features" },
            new() { Id = "bugs", Title = "Bugs Fixed" }
        ];

        List<BuildMark.Configuration.RuleConfig> rules =
        [
            new() { Match = new BuildMark.Configuration.RuleMatchConfig { Label = { "feature" } }, Route = "features" },
            new() { Match = new BuildMark.Configuration.RuleMatchConfig { Label = { "bug" } }, Route = "bugs" }
        ];

        List<BuildMark.BuildNotes.ItemInfo> items =
        [
            new("1", "Add feature X", "https://example.com/1", "feature", 1),
            new("2", "Fix bug Y", "https://example.com/2", "bug", 2)
        ];

        // Act: route the items
        var routed = ItemRouter.Route(items, rules, sections);

        // Assert: each item is routed to its matching section
        Assert.HasCount(1, routed["features"]);
        Assert.AreEqual("1", routed["features"][0].Id);
        Assert.HasCount(1, routed["bugs"]);
        Assert.AreEqual("2", routed["bugs"][0].Id);
    }

    /// <summary>
    ///     Test that the subsystem suppresses items when the route is "suppressed".
    /// </summary>
    [TestMethod]
    public void RepoConnectors_ItemRouter_SuppressedRoute_OmitsItem()
    {
        // Arrange: define a section and a suppression rule
        List<BuildMark.Configuration.SectionConfig> sections =
        [
            new() { Id = "changes", Title = "Changes" }
        ];

        List<BuildMark.Configuration.RuleConfig> rules =
        [
            new() { Match = new BuildMark.Configuration.RuleMatchConfig { Label = { "documentation" } }, Route = "suppressed" }
        ];

        List<BuildMark.BuildNotes.ItemInfo> items =
        [
            new("3", "Update docs", "https://example.com/3", "documentation", 3)
        ];

        // Act: route the items
        var routed = ItemRouter.Route(items, rules, sections);

        // Assert: the item is suppressed and does not appear in any section
        Assert.IsEmpty(routed["changes"]);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-RepoConnectors-AzureDevOps
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Azure DevOps connector implements the IRepoConnector interface.
    /// </summary>
    [TestMethod]
    public void RepoConnectors_AzureDevOps_ImplementsInterface_ReturnsTrue()
    {
        // Arrange: create an AzureDevOpsRepoConnector instance
        var connector = new AzureDevOpsRepoConnector();

        // Assert: it satisfies the public IRepoConnector interface
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the Azure DevOps connector returns valid build information from mocked API data.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_AzureDevOps_GetBuildInformation_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange: set up a mocked REST handler with a single tag and commit
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
            .AddCommitsResponse(new MockAdoCommit("abc123"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "abc123");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: build information is complete and accurate
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("abc123", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsNotNull(buildInfo.Bugs);
        Assert.IsNotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Test that the Azure DevOps connector gathers changes from pull requests.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_AzureDevOps_GetBuildInformation_WithPullRequests_GathersChanges()
    {
        // Arrange: set up two versions with a PR merged between them
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Add feature", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Feature work item", "User Story"))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "commit2");

        // Act: retrieve build information for v1.1.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert: changes include the work item from the PR
        Assert.IsNotNull(buildInfo);
        Assert.IsNotEmpty(buildInfo.Changes, "Changes should include items from merged PRs");
    }

    /// <summary>
    ///     Test that the Azure DevOps connector identifies open work items as known issues.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_AzureDevOps_GetBuildInformation_WithOpenWorkItems_IdentifiesKnownIssues()
    {
        // Arrange: set up a version with an open bug from WIQL query
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
            .AddCommitsResponse(new MockAdoCommit("abc123"))
            .AddPullRequestsResponse()
            .AddWiqlResponse(500)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(500, "Open bug", "Bug", "Active"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "abc123");

        // Act: retrieve build information for v1.0.0
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert: known issues include the open bug
        Assert.IsNotNull(buildInfo);
        Assert.IsNotEmpty(buildInfo.KnownIssues, "KnownIssues should include open bug work items");
    }

    /// <summary>
    ///     Test that the Azure DevOps connector skips pre-release tags for release versions.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_AzureDevOps_GetBuildInformation_ReleaseVersion_SkipsPreReleases()
    {
        // Arrange: set up a release version with a pre-release between it and the previous release
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v2.0.0", "commit3"),
                new MockAdoTag("v2.0.0-rc.1", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockAdoConnector(mockHttpClient, "commit3");

        // Act: retrieve build information for v2.0.0 (a release version)
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert: baseline should be v1.0.0, skipping the pre-release v2.0.0-rc.1
        Assert.IsNotNull(buildInfo);
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.0.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
    }

    /// <summary>
    ///     Creates a mock Azure DevOps connector with pre-configured git command responses.
    /// </summary>
    /// <param name="mockHttpClient">Mock HTTP client for REST API.</param>
    /// <param name="currentCommitHash">Current commit hash to return from git rev-parse HEAD.</param>
    /// <returns>Configured MockableAzureDevOpsRepoConnector.</returns>
    private static MockableAzureDevOpsRepoConnector CreateMockAdoConnector(
        HttpClient mockHttpClient,
        string currentCommitHash)
    {
        var connector = new MockableAzureDevOpsRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin",
            "https://dev.azure.com/org/project/_git/repo");
        connector.SetCommandResponse("git rev-parse HEAD", currentCommitHash);
        connector.SetCommandResponse(
            "az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv",
            "mock-token");
        return connector;
    }
}



