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

using System.Net;
using System.Text;
using System.Text.Json;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <summary>
///     Unit tests for the AzureDevOps subsystem.
/// </summary>
[TestClass]
public class AzureDevOpsRepoConnectorTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-ConnectorConfig
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that the constructor stores AzureDevOpsConnectorConfig overrides.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_Constructor_WithConfig_StoresConfigurationOverrides()
    {
        // Arrange
        var config = new AzureDevOpsConnectorConfig
        {
            OrganizationUrl = "https://dev.azure.com/myorg",
            Project = "myproject",
            Repository = "myrepo"
        };

        // Act
        var connector = new AzureDevOpsRepoConnector(config);

        // Assert
        Assert.IsNotNull(connector.ConfigurationOverrides);
        Assert.AreEqual("https://dev.azure.com/myorg", connector.ConfigurationOverrides.OrganizationUrl);
        Assert.AreEqual("myproject", connector.ConfigurationOverrides.Project);
        Assert.AreEqual("myrepo", connector.ConfigurationOverrides.Repository);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-BuildInformation
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that GetBuildInformationAsync returns valid build information from mocked data.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_WithMockedData_ReturnsValidBuildInformation()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
            .AddCommitsResponse(new MockAdoCommit("abc123"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "abc123");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("abc123", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.Changes);
        Assert.IsNotNull(buildInfo.Bugs);
        Assert.IsNotNull(buildInfo.KnownIssues);
    }

    /// <summary>
    ///     Verify that GetBuildInformationAsync selects the correct previous version with multiple versions.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_WithMultipleVersions_SelectsCorrectPreviousVersion()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v2.0.0", "commit3"),
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit3");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.1.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit2", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Verify that GetBuildInformationAsync gathers changes from pull requests correctly.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_WithPullRequests_GathersChangesCorrectly()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit3"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(101, "Add new feature", "completed", "commit3"),
                new MockAdoPullRequest(100, "Fix critical bug", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 101, 201)
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(201, "New feature work item", "User Story"),
                new MockAdoWorkItem(200, "Bug fix work item", "Bug"))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit3");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("1.1.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);

        // Bug work item should be in bugs
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.Bugs.Count, $"Expected at least 1 bug, got {buildInfo.Bugs.Count}");
        var bugItem = buildInfo.Bugs.FirstOrDefault(b => b.Id == "200");
        Assert.IsNotNull(bugItem, "Work item 200 should be categorized as a bug");
        Assert.AreEqual("Bug fix work item", bugItem.Title);

        // Feature work item should be in changes
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.Changes.Count, $"Expected at least 1 change, got {buildInfo.Changes.Count}");
        var featureItem = buildInfo.Changes.FirstOrDefault(c => c.Id == "201");
        Assert.IsNotNull(featureItem, "Work item 201 should be categorized as a change");
        Assert.AreEqual("New feature work item", featureItem.Title);
    }

    /// <summary>
    ///     Verify that GetBuildInformationAsync identifies open work items as known issues.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_WithOpenWorkItems_IdentifiesKnownIssues()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse(301) // Open bug 301 from WIQL
            .AddWorkItemsResponse(
                new MockAdoWorkItem(301, "Known open bug", "Bug", "Active"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit1");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.IsGreaterThanOrEqualTo(1, buildInfo.KnownIssues.Count, $"Expected at least 1 known issue, got {buildInfo.KnownIssues.Count}");
        var knownIssue = buildInfo.KnownIssues.FirstOrDefault(i => i.Id == "301");
        Assert.IsNotNull(knownIssue, "Work item 301 should be a known issue");
        Assert.AreEqual("Known open bug", knownIssue.Title);
    }

    /// <summary>
    ///     Verify that release baseline selection skips all pre-release versions.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_ReleaseVersion_SkipsAllPreReleases()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit4"),
                new MockAdoTag("v1.1.0-rc.1", "commit3"),
                new MockAdoTag("v1.1.0-beta.1", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit4"),
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit4");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert - Should skip pre-releases and use v1.0.0 as baseline
        Assert.IsNotNull(buildInfo);
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.0.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit1", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Verify that annotated tags (with peeledObjectId) resolve to the correct commit.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_AnnotatedTags_ResolvesToPeeledCommit()
    {
        // Arrange - Annotated tags have objectId pointing to the tag object, peeledObjectId to the commit
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v2.0.0", "tag-object-3", "commit3"),
                new MockAdoTag("v1.0.0", "tag-object-1", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit3"),
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit3");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert - Tags should resolve via peeledObjectId, not the tag object SHA
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit3", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.0.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit1", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Verify that mixed annotated and lightweight tags both resolve correctly.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_MixedTagTypes_ResolvesCorrectly()
    {
        // Arrange - Mix of annotated (with peeledObjectId) and lightweight (without) tags
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v2.0.0", "tag-object-2", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Assert - Both tag types should resolve to correct commits
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual("2.0.0", buildInfo.CurrentVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit2", buildInfo.CurrentVersionTag.CommitHash);
        Assert.IsNotNull(buildInfo.BaselineVersionTag);
        Assert.AreEqual("1.0.0", buildInfo.BaselineVersionTag.VersionTag.FullVersion);
        Assert.AreEqual("commit1", buildInfo.BaselineVersionTag.CommitHash);
    }

    /// <summary>
    ///     Verify that AzureDevOpsRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ImplementsInterface_ReturnsTrue()
    {
        // Arrange
        var connector = new AzureDevOpsRepoConnector();

        // Assert
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-ItemControls
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that visibility:internal in a buildmark block excludes the item from the report.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_VisibilityInternal_ExcludesItem()
    {
        // Arrange - work item with visibility:internal in description
        var description = "Some description\n```buildmark\nvisibility: internal\n```";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Internal fix", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Internal work item", "Bug", "Active", description))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert - the internal item should not appear in bugs or changes
        Assert.IsNotNull(buildInfo);
        Assert.DoesNotContain(b => b.Id == "200", buildInfo.Bugs, "Internal item should be excluded from bugs");
        Assert.DoesNotContain(c => c.Id == "200", buildInfo.Changes, "Internal item should be excluded from changes");
    }

    /// <summary>
    ///     Verify that visibility:public in a buildmark block includes the item in the report.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_VisibilityPublic_IncludesItem()
    {
        // Arrange - work item with visibility:public in description
        var description = "Some description\n```buildmark\nvisibility: public\n```";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Public feature", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Public work item", "User Story", "Active", description))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert - the public item should appear in changes
        Assert.IsNotNull(buildInfo);
        Assert.Contains(
            c => c.Id == "200",
            buildInfo.Changes,
            "Public item should be included in changes");
    }

    /// <summary>
    ///     Verify that type:bug override classifies the item as a bug.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_TypeBugOverride_ClassifiesAsBug()
    {
        // Arrange - User Story with type:bug override in description
        var description = "Some description\n```buildmark\ntype: bug\n```";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Feature that is actually a bug", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Reclassified bug", "User Story", "Active", description))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert - User Story should be classified as bug due to override
        Assert.IsNotNull(buildInfo);
        Assert.Contains(
            b => b.Id == "200",
            buildInfo.Bugs,
            "Item with type:bug override should appear in bugs");
    }

    /// <summary>
    ///     Verify that type:feature override classifies the item as a feature.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_TypeFeatureOverride_ClassifiesAsFeature()
    {
        // Arrange - Bug with type:feature override in description
        var description = "Some description\n```buildmark\ntype: feature\n```";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Bug that is actually a feature", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Reclassified feature", "Bug", "Active", description))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert - Bug should be classified as feature due to override
        Assert.IsNotNull(buildInfo);
        Assert.Contains(
            c => c.Id == "200",
            buildInfo.Changes,
            "Item with type:feature override should appear in changes");
        Assert.DoesNotContain(
            b => b.Id == "200",
            buildInfo.Bugs,
            "Item with type:feature override should NOT appear in bugs");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-Rules
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Configure with rules causes HasRules to return true.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_Configure_WithRules_HasRulesReturnsTrue()
    {
        // Arrange - configure a connector with rules and verify via GetBuildInformationAsync
        // that RoutedSections is populated (HasRules is protected, verified indirectly)
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit1");
        connector.Configure(
            [new() { Route = "all" }],
            [new() { Id = "all", Title = "All Items" }]);

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.0.0"));

        // Assert - RoutedSections is only populated when HasRules is true
        Assert.IsNotNull(buildInfo.RoutedSections, "RoutedSections should be populated when rules are configured (HasRules == true)");
    }

    /// <summary>
    ///     Verify that configured rules populate routed sections.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_WithConfiguredRules_PopulatesRoutedSections()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(
                new MockAdoTag("v1.1.0", "commit2"),
                new MockAdoTag("v1.0.0", "commit1"))
            .AddCommitsResponse(
                new MockAdoCommit("commit2"),
                new MockAdoCommit("commit1"))
            .AddPullRequestsResponse(
                new MockAdoPullRequest(100, "Bug fix PR", "completed", "commit2"))
            .AddPullRequestWorkItemsResponse("repo", 100, 200)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Bug to route", "Bug", "Active"))
            .AddWiqlResponse();

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit2");

        // Configure routing rules
        connector.Configure(
            [
                new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" },
                new() { Route = "features" }
            ],
            [
                new() { Id = "features", Title = "Features" },
                new() { Id = "bugs", Title = "Bugs" }
            ]);

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.1.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.IsNotNull(buildInfo.RoutedSections);
        Assert.IsNotEmpty(buildInfo.RoutedSections, "Should have routed sections");

        // Verify bug was routed to bugs section
        var bugsSection = buildInfo.RoutedSections.FirstOrDefault(s => s.SectionId == "bugs");
        Assert.AreEqual("bugs", bugsSection.SectionId, "Should contain a bugs routed section");
        Assert.IsNotEmpty(bugsSection.Items, "Bugs section should contain the routed bug");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-UrlParsing
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that a dev.azure.com HTTPS URL is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_DevAzureComHttps_ReturnsCorrectComponents()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://dev.azure.com/myorg/myproject/_git/myrepo");

        // Assert
        Assert.AreEqual("https://dev.azure.com/myorg", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that a dev.azure.com HTTPS URL with .git suffix is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_DevAzureComWithGitSuffix_StripsGitSuffix()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://dev.azure.com/myorg/myproject/_git/myrepo.git");

        // Assert
        Assert.AreEqual("https://dev.azure.com/myorg", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that a visualstudio.com HTTPS URL is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_VisualStudioComHttps_ReturnsCorrectComponents()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://myorg.visualstudio.com/myproject/_git/myrepo");

        // Assert
        Assert.AreEqual("https://myorg.visualstudio.com", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that an SSH URL is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_SshUrl_ReturnsCorrectComponents()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "git@ssh.dev.azure.com:v3/myorg/myproject/myrepo");

        // Assert
        Assert.AreEqual("https://dev.azure.com/myorg", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that an on-premises Azure DevOps Server URL is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_OnPremServer_ReturnsCorrectComponents()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://devops.mycompany.com/DefaultCollection/myproject/_git/myrepo");

        // Assert
        Assert.AreEqual("https://devops.mycompany.com/DefaultCollection", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that an on-premises Azure DevOps Server URL with a custom port is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_OnPremWithPort_ReturnsCorrectComponents()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://devops.internal.net:8080/tfs/DefaultCollection/myproject/_git/myrepo");

        // Assert
        Assert.AreEqual("https://devops.internal.net:8080/tfs/DefaultCollection", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that an on-premises Azure DevOps Server URL with .git suffix is parsed correctly.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_OnPremWithGitSuffix_StripsGitSuffix()
    {
        // Act
        var (orgUrl, project, repo) = AzureDevOpsRepoConnector.ParseAzureDevOpsUrl(
            "https://devops.mycompany.com/DefaultCollection/myproject/_git/myrepo.git");

        // Assert
        Assert.AreEqual("https://devops.mycompany.com/DefaultCollection", orgUrl);
        Assert.AreEqual("myproject", project);
        Assert.AreEqual("myrepo", repo);
    }

    /// <summary>
    ///     Verify that an unsupported URL format throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_ParseAzureDevOpsUrl_UnsupportedFormat_ThrowsArgumentException()
    {
        // Act / Assert
        Assert.ThrowsExactly<ArgumentException>(() =>
            AzureDevOpsRepoConnector.ParseAzureDevOpsUrl("https://example.com/not-a-valid-url"));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helper Methods
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Creates a mock connector with standard command responses configured.
    /// </summary>
    /// <param name="mockHttpClient">Mock HTTP client.</param>
    /// <param name="currentCommitHash">Current commit hash to return from git rev-parse HEAD.</param>
    /// <returns>Configured MockableAzureDevOpsRepoConnector.</returns>
    private static MockableAzureDevOpsRepoConnector CreateMockConnector(
        HttpClient mockHttpClient,
        string currentCommitHash)
    {
        var connector = new MockableAzureDevOpsRepoConnector(mockHttpClient);
        connector.SetCommandResponse("git remote get-url origin", "https://dev.azure.com/org/project/_git/repo");
        connector.SetCommandResponse("git rev-parse HEAD", currentCommitHash);
        connector.SetCommandResponse("az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv", "mock-token");
        return connector;
    }

    /// <summary>
    ///     Verify that known issues are filtered by affected-versions (via Custom.AffectedVersions)
    ///     when the field is present on a work item. Bugs whose affected-versions do not contain
    ///     the build version are excluded; bugs with matching ranges or no field are included.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_KnownIssues_FilteredByAffectedVersions()
    {
        // Arrange - three open bugs via WIQL:
        //   401: Custom.AffectedVersions = [1.0.0,2.0.0) => includes v1.5.0
        //   402: Custom.AffectedVersions = [3.0.0,) => excludes v1.5.0
        //   403: no Custom.AffectedVersions => always included when open
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.5.0", "commit1"))
            .AddCommitsResponse(new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse(401, 402, 403)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(401, "Bug affecting v1.x", "Bug", "Active",
                    CustomAffectedVersions: "[1.0.0,2.0.0)"),
                new MockAdoWorkItem(402, "Bug affecting v3+", "Bug", "Active",
                    CustomAffectedVersions: "[3.0.0,)"),
                new MockAdoWorkItem(403, "Bug with no versions", "Bug", "Active"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit1");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.5.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.IsNotNull(buildInfo.KnownIssues);

        // Bug 401 should be included (v1.5.0 is in [1.0.0,2.0.0))
        Assert.IsTrue(
            buildInfo.KnownIssues.Exists(i => i.Id == "401"),
            "Bug 401 with Custom.AffectedVersions [1.0.0,2.0.0) should be a known issue for v1.5.0");

        // Bug 402 should be excluded (v1.5.0 is NOT in [3.0.0,))
        Assert.IsFalse(
            buildInfo.KnownIssues.Exists(i => i.Id == "402"),
            "Bug 402 with Custom.AffectedVersions [3.0.0,) should NOT be a known issue for v1.5.0");

        // Bug 403 should be included (no affected-versions, fallback to open status)
        Assert.IsTrue(
            buildInfo.KnownIssues.Exists(i => i.Id == "403"),
            "Bug 403 with no Custom.AffectedVersions should be a known issue (open status fallback)");
    }

    /// <summary>
    ///     Verify that a RESOLVED/CLOSED bug with a Custom.AffectedVersions range that contains
    ///     the build version is reported as a known issue (LTS back-port gap scenario).
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRepoConnector_GetBuildInformationAsync_ClosedBugWithMatchingAffectedVersions_IsKnownIssue()
    {
        // Arrange - three resolved bugs:
        //   404: Resolved, AV [1.0.0,2.0.0) - fixed in v2, LTS v1.5 branch never got the fix
        //   405: Resolved, AV [3.0.0,) - does NOT affect v1.5.0
        //   406: Resolved, no AV - resolved bug with no AV is NOT a known issue (status fallback)
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddTagsResponse(new MockAdoTag("v1.5.0", "commit1"))
            .AddCommitsResponse(new MockAdoCommit("commit1"))
            .AddPullRequestsResponse()
            .AddWiqlResponse(404, 405, 406)
            .AddWorkItemsResponse(
                new MockAdoWorkItem(404, "Closed bug affecting v1.x", "Bug", "Resolved",
                    CustomAffectedVersions: "[1.0.0,2.0.0)"),
                new MockAdoWorkItem(405, "Closed bug affecting v3+", "Bug", "Resolved",
                    CustomAffectedVersions: "[3.0.0,)"),
                new MockAdoWorkItem(406, "Closed bug with no AV", "Bug", "Resolved"));

        using var mockHttpClient = new HttpClient(mockHandler);
        var connector = CreateMockConnector(mockHttpClient, "commit1");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v1.5.0"));

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.IsNotNull(buildInfo.KnownIssues);

        // Bug 404 is Resolved but AV [1.0.0,2.0.0) contains v1.5.0 → IS a known issue
        Assert.IsTrue(
            buildInfo.KnownIssues.Exists(i => i.Id == "404"),
            "Resolved bug 404 with AV [1.0.0,2.0.0) should be a known issue for v1.5.0 (LTS back-port gap)");

        // Bug 405 is Resolved and AV [3.0.0,) does NOT contain v1.5.0 → NOT a known issue
        Assert.IsFalse(
            buildInfo.KnownIssues.Exists(i => i.Id == "405"),
            "Resolved bug 405 with AV [3.0.0,) should NOT be a known issue for v1.5.0");

        // Bug 406 is Resolved with no AV → NOT a known issue (resolved/unresolved fallback)
        Assert.IsFalse(
            buildInfo.KnownIssues.Exists(i => i.Id == "406"),
            "Resolved bug 406 with no AV should NOT be a known issue (resolved, no AV)");
    }
}
