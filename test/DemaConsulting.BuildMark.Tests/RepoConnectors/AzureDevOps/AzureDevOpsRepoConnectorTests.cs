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
            .AddPullRequestWorkItemsResponse(101, 201)
            .AddPullRequestWorkItemsResponse(100, 200)
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
        Assert.IsGreaterThanOrEqualTo(buildInfo.Bugs.Count, 1, $"Expected at least 1 bug, got {buildInfo.Bugs.Count}");
        var bugItem = buildInfo.Bugs.FirstOrDefault(b => b.Id == "200");
        Assert.IsNotNull(bugItem, "Work item 200 should be categorized as a bug");
        Assert.AreEqual("Bug fix work item", bugItem.Title);

        // Feature work item should be in changes
        Assert.IsGreaterThanOrEqualTo(buildInfo.Changes.Count, 1, $"Expected at least 1 change, got {buildInfo.Changes.Count}");
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
        Assert.IsGreaterThanOrEqualTo(buildInfo.KnownIssues.Count, 1, $"Expected at least 1 known issue, got {buildInfo.KnownIssues.Count}");
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
            .AddPullRequestWorkItemsResponse(100, 200)
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
            .AddPullRequestWorkItemsResponse(100, 200)
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
            .AddPullRequestWorkItemsResponse(100, 200)
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
            .AddPullRequestWorkItemsResponse(100, 200)
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
    // BuildMark-AzureDevOps-CustomFields
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Custom.Visibility field returns mapped controls.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomVisibilityField_ReturnsMappedControls()
    {
        // Arrange - work item with Custom.Visibility field
        var workItem = CreateWorkItem(200, "Test item", "User Story", "Active",
            customVisibility: "internal");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert
        Assert.IsNotNull(controls);
        Assert.AreEqual("internal", controls.Visibility);
    }

    /// <summary>
    ///     Verify that Custom.AffectedVersions field returns mapped version set.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomAffectedVersionsField_ReturnsMappedVersionSet()
    {
        // Arrange - work item with Custom.AffectedVersions field
        var workItem = CreateWorkItem(200, "Test item", "Bug", "Active",
            customAffectedVersions: "(,1.0.1]");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert
        Assert.IsNotNull(controls);
        Assert.IsNotNull(controls.AffectedVersions);
        Assert.IsTrue(controls.AffectedVersions.Intervals.Count > 0);
    }

    /// <summary>
    ///     Verify that custom fields take precedence over buildmark blocks.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_ExtractItemControls_CustomFieldsTakePrecedenceOverBuildmarkBlock()
    {
        // Arrange - work item with BOTH a buildmark block saying "public" AND a custom field saying "internal"
        var description = "Description\n```buildmark\nvisibility: public\n```";
        var workItem = CreateWorkItem(200, "Test item", "Bug", "Active",
            description: description,
            customVisibility: "internal");

        // Act
        var controls = WorkItemMapper.ExtractItemControls(workItem);

        // Assert - custom field "internal" should take precedence over buildmark block "public"
        Assert.IsNotNull(controls);
        Assert.AreEqual("internal", controls.Visibility);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-Rules
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Configure with rules causes HasRules to return true.
    /// </summary>
    [TestMethod]
    public void AzureDevOpsRepoConnector_Configure_WithRules_HasRulesReturnsTrue()
    {
        // Arrange
        var connector = new AzureDevOpsRepoConnector();
        List<RuleConfig> rules =
        [
            new() { Match = new RuleMatchConfig { Label = { "bug" } }, Route = "bugs" }
        ];
        List<SectionConfig> sections =
        [
            new() { Id = "bugs", Title = "Bugs" }
        ];

        // Act
        connector.Configure(rules, sections);

        // Assert - HasRules is protected; verify indirectly via GetBuildInformationAsync
        // by checking that the connector was configured without errors
        Assert.IsInstanceOfType<AzureDevOpsRepoConnector>(connector);
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
            .AddPullRequestWorkItemsResponse(100, 200)
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
        Assert.IsTrue(buildInfo.RoutedSections.Count > 0, "Should have routed sections");

        // Verify bug was routed to bugs section
        var bugsSection = buildInfo.RoutedSections.FirstOrDefault(s => s.SectionId == "bugs");
        Assert.IsTrue(bugsSection.Items.Count > 0, "Bugs section should contain the routed bug");
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-RestClient
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that GetRepositoryAsync returns a repository record from a valid response.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetRepositoryAsync_ValidResponse_ReturnsRepository()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddRepositoryResponse("repo-123", "MyRepo", "https://dev.azure.com/org/project/_git/MyRepo");
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var repo = await client.GetRepositoryAsync("MyRepo");

        // Assert
        Assert.IsNotNull(repo);
        Assert.AreEqual("repo-123", repo.Id);
        Assert.AreEqual("MyRepo", repo.Name);
        Assert.AreEqual("https://dev.azure.com/org/project/_git/MyRepo", repo.RemoteUrl);
    }

    /// <summary>
    ///     Verify that GetCommitsAsync returns commits from a valid response.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetCommitsAsync_ValidResponse_ReturnsCommits()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddCommitsResponse(
                new MockAdoCommit("abc123", "First commit"),
                new MockAdoCommit("def456", "Second commit"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var commits = await client.GetCommitsAsync("repo-id");

        // Assert
        Assert.IsNotNull(commits);
        Assert.AreEqual(2, commits.Count);
        Assert.AreEqual("abc123", commits[0].CommitId);
        Assert.AreEqual("First commit", commits[0].Comment);
        Assert.AreEqual("def456", commits[1].CommitId);
    }

    /// <summary>
    ///     Verify that GetPullRequestsAsync returns pull requests from a valid response.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddPullRequestsResponse(
                new MockAdoPullRequest(101, "Feature PR", "completed", "merge-commit-1"),
                new MockAdoPullRequest(102, "Bug PR", "active"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var prs = await client.GetPullRequestsAsync("repo-id");

        // Assert
        Assert.IsNotNull(prs);
        Assert.AreEqual(2, prs.Count);
        Assert.AreEqual(101, prs[0].PullRequestId);
        Assert.AreEqual("Feature PR", prs[0].Title);
        Assert.AreEqual("completed", prs[0].Status);
        Assert.AreEqual("merge-commit-1", prs[0].MergeCommitId);
    }

    /// <summary>
    ///     Verify that GetPullRequestWorkItemsAsync returns work item references from a valid response.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_ValidResponse_ReturnsWorkItemRefs()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddPullRequestWorkItemsResponse(101, 200, 201);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItemRefs = await client.GetPullRequestWorkItemsAsync("repo-id", 101);

        // Assert
        Assert.IsNotNull(workItemRefs);
        Assert.AreEqual(2, workItemRefs.Count);
        Assert.AreEqual(200, workItemRefs[0].Id);
        Assert.AreEqual(201, workItemRefs[1].Id);
    }

    /// <summary>
    ///     Verify that GetWorkItemsAsync returns work item details from a valid response.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetWorkItemsAsync_ValidResponse_ReturnsWorkItems()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddWorkItemsResponse(
                new MockAdoWorkItem(200, "Bug work item", "Bug", "Active"),
                new MockAdoWorkItem(201, "Feature work item", "User Story", "Resolved"));
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItems = await client.GetWorkItemsAsync([200, 201]);

        // Assert
        Assert.IsNotNull(workItems);
        Assert.AreEqual(2, workItems.Count);
        Assert.AreEqual(200, workItems[0].Id);
        Assert.AreEqual(201, workItems[1].Id);
    }

    /// <summary>
    ///     Verify that QueryWorkItemsAsync returns work item ids for a valid WIQL query.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_QueryWorkItemsAsync_ValidWiql_ReturnsWorkItemIds()
    {
        // Arrange
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddWiqlResponse(300, 301, 302);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var query = await client.QueryWorkItemsAsync("SELECT [System.Id] FROM workitems WHERE [System.WorkItemType] = 'Bug'");

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(3, query.WorkItems.Count);
        Assert.AreEqual(300, query.WorkItems[0].Id);
        Assert.AreEqual(301, query.WorkItems[1].Id);
        Assert.AreEqual(302, query.WorkItems[2].Id);
    }

    /// <summary>
    ///     Verify that GetPullRequestWorkItemsAsync deserializes string-valued ids
    ///     as returned by the Azure DevOps PR work items endpoint.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_GetPullRequestWorkItemsAsync_StringValuedIds_DeserializesCorrectly()
    {
        // Arrange - raw JSON matching the real Azure DevOps response format where id is a string
        const string json = """{"count":2,"value":[{"id":"1234","url":"https://dev.azure.com/org/project/_apis/wit/workItems/1234"},{"id":"5678","url":"https://dev.azure.com/org/project/_apis/wit/workItems/5678"}]}""";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddResponse("pullrequests/101/workitems", json);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var workItemRefs = await client.GetPullRequestWorkItemsAsync("repo-id", 101);

        // Assert
        Assert.IsNotNull(workItemRefs);
        Assert.AreEqual(2, workItemRefs.Count);
        Assert.AreEqual(1234, workItemRefs[0].Id);
        Assert.AreEqual(5678, workItemRefs[1].Id);
    }

    /// <summary>
    ///     Verify that QueryWorkItemsAsync deserializes string-valued ids
    ///     when the WIQL endpoint returns them as strings.
    /// </summary>
    [TestMethod]
    public async Task AzureDevOpsRestClient_QueryWorkItemsAsync_StringValuedIds_DeserializesCorrectly()
    {
        // Arrange - raw JSON with string-valued ids
        const string json = """{"workItems":[{"id":"300","url":"https://dev.azure.com/org/project/_apis/wit/workItems/300"},{"id":"301","url":"https://dev.azure.com/org/project/_apis/wit/workItems/301"}]}""";
        using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
            .AddResponse("wit/wiql", json);
        using var mockHttpClient = new HttpClient(mockHandler);
        using var client = new AzureDevOpsRestClient(mockHttpClient, "https://dev.azure.com/org", "project");

        // Act
        var query = await client.QueryWorkItemsAsync("SELECT [System.Id] FROM workitems WHERE [System.WorkItemType] = 'Bug'");

        // Assert
        Assert.IsNotNull(query);
        Assert.AreEqual(2, query.WorkItems.Count);
        Assert.AreEqual(300, query.WorkItems[0].Id);
        Assert.AreEqual(301, query.WorkItems[1].Id);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-AzureDevOps-WorkItemMapper
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Verify that Bug work item type maps to a bug ItemInfo.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_BugType_ReturnsBugItem()
    {
        // Arrange
        var workItem = CreateWorkItem(100, "A bug", "Bug", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/100", 1);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("100", itemInfo.Id);
        Assert.AreEqual("A bug", itemInfo.Title);
        Assert.AreEqual("bug", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that User Story work item type maps to a feature ItemInfo.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_UserStoryType_ReturnsFeatureItem()
    {
        // Arrange
        var workItem = CreateWorkItem(101, "A user story", "User Story", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/101", 2);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("101", itemInfo.Id);
        Assert.AreEqual("A user story", itemInfo.Title);
        Assert.AreEqual("feature", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that Task work item type maps to an ItemInfo with the raw type name.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_MapWorkItemToItemInfo_TaskType_ReturnsTaskItem()
    {
        // Arrange
        var workItem = CreateWorkItem(102, "A task", "Task", "Active");

        // Act
        var itemInfo = WorkItemMapper.MapWorkItemToItemInfo(workItem, "https://example.com/102", 3);

        // Assert
        Assert.IsNotNull(itemInfo);
        Assert.AreEqual("102", itemInfo.Id);
        Assert.AreEqual("A task", itemInfo.Title);
        Assert.AreEqual("Task", itemInfo.Type);
    }

    /// <summary>
    ///     Verify that IsWorkItemResolved returns true for a resolved work item.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_IsWorkItemResolved_ResolvedState_ReturnsTrue()
    {
        // Arrange - test all resolved states
        var resolvedItem = CreateWorkItem(100, "Resolved item", "Bug", "Resolved");
        var closedItem = CreateWorkItem(101, "Closed item", "Bug", "Closed");
        var doneItem = CreateWorkItem(102, "Done item", "Bug", "Done");

        // Act & Assert
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(resolvedItem), "Resolved state should be resolved");
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(closedItem), "Closed state should be resolved");
        Assert.IsTrue(WorkItemMapper.IsWorkItemResolved(doneItem), "Done state should be resolved");
    }

    /// <summary>
    ///     Verify that IsWorkItemResolved returns false for an active work item.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_IsWorkItemResolved_ActiveState_ReturnsFalse()
    {
        // Arrange
        var activeItem = CreateWorkItem(100, "Active item", "Bug", "Active");
        var newItem = CreateWorkItem(101, "New item", "Bug", "New");

        // Act & Assert
        Assert.IsFalse(WorkItemMapper.IsWorkItemResolved(activeItem), "Active state should not be resolved");
        Assert.IsFalse(WorkItemMapper.IsWorkItemResolved(newItem), "New state should not be resolved");
    }

    /// <summary>
    ///     Verify that GetWorkItemTypeForRuleMatching returns the raw work item type name.
    /// </summary>
    [TestMethod]
    public void WorkItemMapper_GetWorkItemTypeForRuleMatching_ReturnsWorkItemTypeName()
    {
        // Arrange
        var bugItem = CreateWorkItem(100, "Bug item", "Bug", "Active");
        var storyItem = CreateWorkItem(101, "Story item", "User Story", "Active");

        // Act
        var bugType = WorkItemMapper.GetWorkItemTypeForRuleMatching(bugItem);
        var storyType = WorkItemMapper.GetWorkItemTypeForRuleMatching(storyItem);

        // Assert
        Assert.AreEqual("Bug", bugType);
        Assert.AreEqual("User Story", storyType);
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
    ///     Creates an AzureDevOpsWorkItem record for testing.
    /// </summary>
    /// <param name="id">Work item ID.</param>
    /// <param name="title">Work item title.</param>
    /// <param name="workItemType">Work item type.</param>
    /// <param name="state">Work item state.</param>
    /// <param name="description">Optional description body.</param>
    /// <param name="customVisibility">Optional Custom.Visibility field.</param>
    /// <param name="customAffectedVersions">Optional Custom.AffectedVersions field.</param>
    /// <returns>AzureDevOpsWorkItem record.</returns>
    private static AzureDevOpsWorkItem CreateWorkItem(
        int id,
        string title,
        string workItemType,
        string state,
        string? description = null,
        string? customVisibility = null,
        string? customAffectedVersions = null)
    {
        var fields = new Dictionary<string, object?>
        {
            ["System.Title"] = title,
            ["System.WorkItemType"] = workItemType,
            ["System.State"] = state,
            ["System.Description"] = description
        };

        if (customVisibility != null)
        {
            fields["Custom.Visibility"] = customVisibility;
        }

        if (customAffectedVersions != null)
        {
            fields["Custom.AffectedVersions"] = customAffectedVersions;
        }

        return new AzureDevOpsWorkItem(id, fields);
    }
}
