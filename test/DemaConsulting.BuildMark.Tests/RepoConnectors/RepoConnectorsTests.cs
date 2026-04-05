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

namespace DemaConsulting.BuildMark.Tests;

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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

        // Assert: build information is complete and accurate
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Assert: v1.1.0 is selected as baseline and a changelog link is generated
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.IsNotNull(buildInfo.BaselineVersionTag, "Previous version should be identified");
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionInfo.FullVersion);
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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.1.0"));

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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v1.0.0"));

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
        var buildInfo = await connector.GetBuildInformationAsync(Version.Create("v2.0.0"));

        // Assert: baseline should be v1.1.0 (the last release), not v2.0.0-rc.1 (a pre-release)
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionInfo.FullVersion);
        Assert.IsNotNull(buildInfo.BaselineVersionTag, "Baseline version should be set");
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionInfo.FullVersion,
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

        // Assert: MockRepoConnector derives from the base class and satisfies the interface
        Assert.IsInstanceOfType<RepoConnectorBase>(connector);
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

        // Assert: GitHubRepoConnector derives from the base class and satisfies the interface
        Assert.IsInstanceOfType<RepoConnectorBase>(connector);
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
        var version = Version.Create("2.0.0");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert: current version tag matches the requested version
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual(version.Tag, buildInfo.CurrentVersionTag.VersionInfo.Tag);
    }

    /// <summary>
    ///     Test that MockRepoConnector returns a complete BuildInformation structure.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectors_MockConnector_GetBuildInformation_ReturnsCompleteInformation()
    {
        // Arrange: create connector
        var connector = new MockRepoConnector();
        var version = Version.Create("2.0.0");

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
}
