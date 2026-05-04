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

using DemaConsulting.BuildMark.BuildNotes;
using DemaConsulting.BuildMark.Cli;
using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.Utilities;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Integration tests that run the BuildMark application through dotnet.
/// </summary>
public class IntegrationTests
{
    private readonly string _dllPath;

    /// <summary>
    ///     Initialize test by locating the BuildMark DLL.
    /// </summary>
    public IntegrationTests()
    {
        // The DLL should be in the same directory as the test assembly
        // because the test project references the main project
        var baseDir = AppContext.BaseDirectory;
        _dllPath = PathHelpers.SafePathCombine(baseDir, "DemaConsulting.BuildMark.dll");

        Assert.True(File.Exists(_dllPath), $"Could not find BuildMark DLL at {_dllPath}");
    }

    /// <summary>
    ///     Test that version flag outputs version information.
    /// </summary>
    [Fact]
    public void BuildMark_VersionFlag_OutputsVersion()
    {
        // Run the application with --version flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify version is output
        Assert.False(string.IsNullOrWhiteSpace(output));
        Assert.DoesNotContain("Error", output);
    }

    /// <summary>
    ///     Test that help flag outputs usage information.
    /// </summary>
    [Fact]
    public void BuildMark_HelpFlag_OutputsUsageInformation()
    {
        // Run the application with --help flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify usage information
        Assert.Contains("Usage: buildmark", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--version", output);
        Assert.Contains("--help", output);
    }

    /// <summary>
    ///     Test that silent flag suppresses output.
    /// </summary>
    [Fact]
    public void BuildMark_SilentFlag_SuppressesOutput()
    {
        // Run the application with --silent and --help flags
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--silent",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify no banner in output
        Assert.DoesNotContain("BuildMark version", output);
    }

    /// <summary>
    ///     Test that invalid argument shows error.
    /// </summary>
    [Fact]
    public void BuildMark_InvalidArgument_ShowsError()
    {
        // Run the application with invalid argument
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--invalid-argument");

        // Verify error exit code
        Assert.Equal(1, exitCode);

        // Verify error message
        Assert.Contains("Error:", output);
        Assert.Contains("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that the tool handles an invalid report file path gracefully.
    /// </summary>
    [Fact]
    public void BuildMark_InvalidReportPath_ShowsError()
    {
        // Arrange: construct a path whose parent directory does not exist
        var invalidPath = Path.Combine(
            Path.GetTempPath(),
            Guid.NewGuid().ToString(),
            "nonexistent",
            "output.md");

        using var context = Context.Create(
            ["--build-version", "1.0.0", "--report", invalidPath],
            () => new MockRepoConnector());

        // Capture Console.Error to verify the error message
        using var errorOutput = new StringWriter();
        var originalError = Console.Error;
        try
        {
            Console.SetError(errorOutput);

            // Act: run the program
            Program.Run(context);
        }
        finally
        {
            Console.SetError(originalError);
        }

        // Assert: tool reports an error message and exits with error code
        Assert.Equal(1, context.ExitCode);
        Assert.Contains("Error:", errorOutput.ToString());
    }

    /// <summary>
    ///     Test that validate flag runs self-validation.
    /// </summary>
    [Fact]
    public void BuildMark_ValidateFlag_RunsSelfValidation()
    {
        // Run the application with --validate flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify validation runs
        Assert.False(string.IsNullOrWhiteSpace(output));
    }

    /// <summary>
    ///     Test that log parameter is accepted.
    /// </summary>
    [Fact]
    public void BuildMark_LogParameter_IsAccepted()
    {
        // Run the application with log parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--log", "test.log",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that report parameter is accepted.
    /// </summary>
    [Fact]
    public void BuildMark_ReportParameter_IsAccepted()
    {
        // Run the application with report parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--report", "output.md",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that depth parameter is accepted.
    /// </summary>
    [Fact]
    public void BuildMark_DepthParameter_IsAccepted()
    {
        // Run the application with depth parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--depth", "2",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that build-version parameter is accepted.
    /// </summary>
    [Fact]
    public void BuildMark_BuildVersionParameter_IsAccepted()
    {
        // Run the application with build-version parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--build-version", "1.0.0",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that results parameter is accepted.
    /// </summary>
    [Fact]
    public void BuildMark_ResultsParameter_IsAccepted()
    {
        // Run the application with results parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--results", "results.trx",
            "--help");

        // Verify success
        Assert.Equal(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that the report generates a markdown file with version information.
    /// </summary>
    [Fact]
    public void BuildMark_Report_GeneratesMarkdownWithVersionInformation()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report file contains markdown title and version information
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("# Build Report", content);
            Assert.Contains("## Version Information", content);
            Assert.Contains("2.0.0", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the report contains changes and bug fixes with hyperlinks.
    /// </summary>
    [Fact]
    public void BuildMark_Report_ContainsChangesAndBugFixesWithHyperlinks()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report contains changes and bug fixes sections with linked items
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("## Changes", content);
            Assert.Contains("## Bugs Fixed", content);
            Assert.Contains("](", content); // markdown hyperlink syntax [text](url)
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the report shows the version range from the previous release.
    /// </summary>
    [Fact]
    public void BuildMark_Report_ShowsVersionRangeFromPreviousRelease()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report identifies the previous version as the baseline of the version range
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("Previous Version", content);
            Assert.Contains("ver-1.1.0", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the report includes known issues when the flag is set.
    /// </summary>
    [Fact]
    public void BuildMark_Report_IncludesKnownIssues_WhenFlagIsSet()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector and include-known-issues flag
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--include-known-issues", "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report includes a known issues section
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("## Known Issues", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that report-depth 2 uses level-two headings in the report.
    /// </summary>
    [Fact]
    public void BuildMark_Report_DepthTwo_UsesLevelTwoHeadings()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector and report depth 2
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--depth", "2", "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report uses level-two heading for the title and level-three for sections
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("## Build Report", content);
            Assert.Contains("### Version Information", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the --lint flag is accepted and validates configuration without error.
    /// </summary>
    [Fact]
    public void BuildMark_LintFlag_IsAccepted()
    {
        // Act: run the application with --lint flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--lint");

        // Assert: tool completes successfully
        Assert.Equal(0, exitCode);
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that the tool consumes the .buildmark.yaml configuration file during report generation.
    /// </summary>
    [Fact]
    public void BuildMark_Report_ConsumesConfigurationFileDuringGeneration()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector (configuration loading executes as part of report generation)
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program which loads configuration before generating the report
            Program.Run(context);

            // Assert: tool succeeds and produces a report (configuration was loaded without error)
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.False(string.IsNullOrWhiteSpace(content));
            Assert.Contains("# Build Report", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the tool uses the configured repository connector to fetch build data.
    /// </summary>
    [Fact]
    public void BuildMark_Report_UsesConnectorForBuildData()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector injected via connector factory
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report contains data sourced from the mock connector
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("Update documentation", content);
            Assert.Contains("Fix bug in Y", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the report contains section definitions matching expected structure.
    /// </summary>
    [Fact]
    public void BuildMark_Report_ContainsSectionDefinitions()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report contains the expected section headings
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("## Version Information", content);
            Assert.Contains("## Changes", content);
            Assert.Contains("## Bugs Fixed", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that items are routed to the correct report sections by type.
    /// </summary>
    [Fact]
    public void BuildMark_Report_RoutesItemsToCorrectSections()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector (which provides items of different types)
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: bug items appear in Bugs Fixed section; non-bug items appear in Changes section
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            var changesStart = content.IndexOf("## Changes", StringComparison.Ordinal);
            var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
            Assert.True(changesStart >= 0, "Report must contain Changes section");
            Assert.True(bugsStart >= 0, "Report must contain Bugs Fixed section");

            // Bug-typed item "Fix bug in Y" should be in Bugs Fixed section
            var bugsSection = content[bugsStart..];
            Assert.Contains("Fix bug in Y", bugsSection);

            // Non-bug item "Update documentation" should be in Changes section
            var changesSection = content[changesStart..bugsStart];
            Assert.Contains("Update documentation", changesSection);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the tool recognizes a buildmark code block in item descriptions.
    /// </summary>
    [Fact]
    public void BuildMark_Report_RecognizesBuildmarkCodeBlock()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with buildmark type override was reclassified (proves block was recognized)
        Assert.Contains("Reclassified as bug", content);
    }

    /// <summary>
    ///     Test that the tool supports a visibility field in the buildmark block.
    /// </summary>
    [Fact]
    public void BuildMark_Report_VisibilityFieldControlsInclusion()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: public-visibility item is included; internal-visibility item is excluded
        Assert.Contains("Public feature X", content);
        Assert.DoesNotContain("Internal refactoring", content);
    }

    /// <summary>
    ///     Test that the tool includes an item when visibility is set to public.
    /// </summary>
    [Fact]
    public void BuildMark_Report_PublicVisibility_IncludesItem()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with visibility: public is present in the report
        Assert.Contains("Public feature X", content);
    }

    /// <summary>
    ///     Test that the tool excludes an item when visibility is set to internal.
    /// </summary>
    [Fact]
    public void BuildMark_Report_InternalVisibility_ExcludesItem()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with visibility: internal does not appear in the report
        Assert.DoesNotContain("Internal refactoring", content);
    }

    /// <summary>
    ///     Test that the tool supports a type field in the buildmark block to override classification.
    /// </summary>
    [Fact]
    public void BuildMark_Report_TypeFieldOverridesClassification()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item originally typed as feature was reclassified to bug (appears in Bugs Fixed)
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.True(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Reclassified as bug", bugsSection);

        // Assert: item originally typed as bug was reclassified to feature (appears in Changes)
        var changesStart = content.IndexOf("## Changes", StringComparison.Ordinal);
        var changesSection = content[changesStart..bugsStart];
        Assert.Contains("Reclassified as feature", changesSection);
    }

    /// <summary>
    ///     Test that the tool classifies an item as a bug fix when type is set to bug.
    /// </summary>
    [Fact]
    public void BuildMark_Report_TypeBug_PlacesItemInBugsFixed()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with type: bug override appears in the Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.True(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Reclassified as bug", bugsSection);
    }

    /// <summary>
    ///     Test that the tool classifies an item as a feature when type is set to feature.
    /// </summary>
    [Fact]
    public void BuildMark_Report_TypeFeature_PlacesItemInChanges()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with type: feature override appears in the Changes section
        var changesStart = content.IndexOf("## Changes", StringComparison.Ordinal);
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.True(changesStart >= 0, "Report must contain Changes section");
        Assert.True(bugsStart > changesStart, "Bugs Fixed section must follow Changes section");
        var changesSection = content[changesStart..bugsStart];
        Assert.Contains("Reclassified as feature", changesSection);
    }

    /// <summary>
    ///     Test that the tool supports an affected-versions field in the buildmark block.
    /// </summary>
    [Fact]
    public void BuildMark_Report_AffectedVersionsField_ProcessesSuccessfully()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with affected-versions field is included in the report
        // (the affected-versions field was parsed without error and the item was processed)
        Assert.Contains("Bug with affected versions", content);
    }

    /// <summary>
    ///     Test that the affected-versions field uses interval notation.
    /// </summary>
    [Fact]
    public void BuildMark_Report_AffectedVersionsInterval_ParsesNotation()
    {
        // Arrange: generate a report using the controls mock connector
        // (the connector creates an item with interval notation "[1.0.0, 2.0.0)")
        var content = GenerateControlsMockReport();

        // Assert: the item with interval notation was parsed and routed to Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.True(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Bug with affected versions", bugsSection);
    }

    /// <summary>
    ///     Test that the tool recognizes a buildmark block wrapped in an HTML comment.
    /// </summary>
    [Fact]
    public void BuildMark_Report_HiddenBuildmarkBlock_IsRecognized()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with HTML-comment-wrapped buildmark block is recognized and processed
        // The hidden block specifies type: bug, so the item should appear in Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.True(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Hidden block item", bugsSection);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Azure DevOps Integration
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that BuildMark generates a markdown report with version information
    ///     from a mocked Azure DevOps repository.
    /// </summary>
    [Fact]
    public void BuildMark_AzureDevOps_Report_GeneratesMarkdownWithVersionInformation()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Set up a mocked REST handler with a single version tag and commit
            using var mockHandler = new MockAzureDevOpsHttpMessageHandler()
                .AddTagsResponse(new MockAdoTag("v1.0.0", "abc123"))
                .AddCommitsResponse(new MockAdoCommit("abc123"))
                .AddPullRequestsResponse()
                .AddWiqlResponse();

            using var mockHttpClient = new HttpClient(mockHandler);
            var adoConnector = CreateMockAdoConnector(mockHttpClient, "abc123");

            // Create context with Azure DevOps connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "1.0.0", "--report", reportFile, "--silent"],
                () => adoConnector);

            // Act: run the program
            Program.Run(context);

            // Assert: report file contains markdown title and version information
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("# Build Report", content);
            Assert.Contains("## Version Information", content);
            Assert.Contains("1.0.0", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that BuildMark generates a report containing changes and bug fixes
    ///     with hyperlinks from a mocked Azure DevOps repository.
    /// </summary>
    [Fact]
    public void BuildMark_AzureDevOps_Report_ContainsChangesAndBugFixesWithHyperlinks()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Set up mocked REST data with two versions, two pull requests, and linked work items
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
                    new MockAdoWorkItem(200, "Bug work item", "Bug"))
                .AddWiqlResponse();

            using var mockHttpClient = new HttpClient(mockHandler);
            var adoConnector = CreateMockAdoConnector(mockHttpClient, "commit3");

            // Create context with Azure DevOps connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "1.1.0", "--report", reportFile, "--silent"],
                () => adoConnector);

            // Act: run the program
            Program.Run(context);

            // Assert: report contains changes and bug fixes sections with linked items
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("## Changes", content);
            Assert.Contains("## Bugs Fixed", content);
            Assert.Contains("](", content); // markdown hyperlink syntax [text](url)
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that BuildMark shows the correct version range from the previous release
    ///     in a mocked Azure DevOps repository.
    /// </summary>
    [Fact]
    public void BuildMark_AzureDevOps_Report_ShowsVersionRangeFromPreviousRelease()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Set up mocked REST data with three version tags so previous version selection is exercised
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
            var adoConnector = CreateMockAdoConnector(mockHttpClient, "commit3");

            // Create context with Azure DevOps connector injected for deterministic output
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => adoConnector);

            // Act: run the program
            Program.Run(context);

            // Assert: report identifies the previous version as the baseline of the version range
            Assert.Equal(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.Contains("Previous Version", content);
            Assert.Contains("v1.1.0", content);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that the --results flag writes a TRX file when --validate is specified.
    /// </summary>
    [Fact]
    public void BuildMark_ResultsParameter_WritesTrxFile()
    {
        // Arrange: create a temporary TRX results file path
        var resultsFile = Path.ChangeExtension(Path.GetTempFileName(), ".trx");
        try
        {
            // Act: run the application with --validate and --results pointing to a TRX file
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results", resultsFile,
                "--silent");

            // Assert: tool succeeds and writes a TRX file containing the TestRun XML element
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "TRX results file should have been created");
            var content = File.ReadAllText(resultsFile);
            Assert.Contains("<TestRun", content);
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }

    /// <summary>
    ///     Test that the --results flag writes a JUnit XML file when --validate is specified.
    /// </summary>
    [Fact]
    public void BuildMark_ResultsParameter_WritesJUnitFile()
    {
        // Arrange: create a temporary JUnit XML results file path
        var resultsFile = Path.ChangeExtension(Path.GetTempFileName(), ".xml");
        try
        {
            // Act: run the application with --validate and --results pointing to an XML file
            var exitCode = Runner.Run(
                out _,
                "dotnet",
                _dllPath,
                "--validate",
                "--results", resultsFile,
                "--silent");

            // Assert: tool succeeds and writes an XML file containing the testsuites element
            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(resultsFile), "JUnit XML results file should have been created");
            var content = File.ReadAllText(resultsFile);
            Assert.Contains("<testsuites", content);
        }
        finally
        {
            if (File.Exists(resultsFile))
            {
                File.Delete(resultsFile);
            }
        }
    }

    /// <summary>
    ///     Test that the tool reads the connector type from the .buildmark.yaml configuration file.
    /// </summary>
    [Fact]
    public async Task BuildMark_Config_ConnectorType_ReadFromConfigFile()
    {
        // Arrange: create a temporary directory containing a .buildmark.yaml with an explicit connector type
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            // Write a configuration file that explicitly selects the azure-devops connector type
            var configContent = "connector:\n  type: azure-devops\n";
            await File.WriteAllTextAsync(
                Path.Combine(tempDir, ".buildmark.yaml"),
                configContent,
                TestContext.Current.CancellationToken);

            // Act: read configuration and create the connector through the factory
            var loadResult = await BuildMarkConfigReader.ReadAsync(tempDir);
            var connector = RepoConnectorFactory.Create(loadResult.Config?.Connector);

            // Assert: configuration was parsed without errors and the factory created an Azure DevOps connector
            Assert.False(loadResult.HasErrors, "Configuration file should load without errors");
            Assert.NotNull(connector);
            Assert.IsAssignableFrom<AzureDevOpsRepoConnector>(connector);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    /// <summary>
    ///     Creates a mock Azure DevOps connector with pre-configured git command responses
    ///     for use in integration tests.
    /// </summary>
    /// <param name="mockHttpClient">Mock HTTP client for REST API calls.</param>
    /// <param name="currentCommitHash">Current commit hash to return from git rev-parse HEAD.</param>
    /// <returns>Configured MockableAzureDevOpsRepoConnector ready for use in a connector factory.</returns>
    private static MockableAzureDevOpsRepoConnector CreateMockAdoConnector(
        HttpClient mockHttpClient,
        string currentCommitHash)
    {
        var connector = new MockableAzureDevOpsRepoConnector(mockHttpClient);
        connector.SetCommandResponse(
            "git remote get-url origin",
            "https://dev.azure.com/org/project/_git/repo");
        connector.SetCommandResponse("git rev-parse HEAD", currentCommitHash);
        connector.SetCommandResponse(
            "az account get-access-token --resource 499b84ac-1321-427f-aa17-267ca6975798 --query accessToken -o tsv",
            "mock-token");
        return connector;
    }

    /// <summary>
    ///     Generates a report using the ControlsMockConnector and returns the report content.
    /// </summary>
    /// <returns>The markdown report content string.</returns>
    private static string GenerateControlsMockReport()
    {
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with controls mock connector
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--silent"],
                () => new ControlsMockConnector());

            // Run the program
            Program.Run(context);

            // Verify success and return content
            Assert.Equal(0, context.ExitCode);
            return File.ReadAllText(reportFile);
        }
        finally
        {
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Mock connector that exercises item controls by processing descriptions
    ///     through the ItemControlsParser, simulating real connector behavior.
    /// </summary>
    /// <remarks>
    ///     This connector creates items with buildmark code blocks in their
    ///     descriptions and processes them using ItemControlsParser to exercise
    ///     the item controls pipeline end-to-end. The items cover:
    ///     <list type="bullet">
    ///         <item><description>Public visibility (included)</description></item>
    ///         <item><description>Internal visibility (excluded)</description></item>
    ///         <item><description>Type override to bug</description></item>
    ///         <item><description>Type override to feature</description></item>
    ///         <item><description>Affected-versions with interval notation</description></item>
    ///         <item><description>Buildmark block hidden in HTML comment</description></item>
    ///         <item><description>Regular item without controls</description></item>
    ///     </list>
    /// </remarks>
    private sealed class ControlsMockConnector : IRepoConnector
    {
        /// <summary>
        ///     Gets build information by processing items with buildmark control blocks.
        /// </summary>
        /// <param name="version">Optional target version.</param>
        /// <returns>BuildInformation with items processed through item controls.</returns>
        public Task<BuildInformation> GetBuildInformationAsync(VersionTag? version = null)
        {
            // Define items with buildmark control blocks in their descriptions
            var items = new (string Id, string Title, string DefaultType, string Description)[]
            {
                // Item 1: visibility: public - should be included as feature
                ("1", "Public feature X", "feature",
                    "A publicly visible feature\n```buildmark\nvisibility: public\n```"),

                // Item 2: visibility: internal - should be excluded from report
                ("2", "Internal refactoring", "feature",
                    "An internal change\n```buildmark\nvisibility: internal\n```"),

                // Item 3: type: bug override - feature reclassified as bug
                ("3", "Reclassified as bug", "feature",
                    "Was feature, now bug\n```buildmark\ntype: bug\n```"),

                // Item 4: type: feature override - bug reclassified as feature
                ("4", "Reclassified as feature", "bug",
                    "Was bug, now feature\n```buildmark\ntype: feature\n```"),

                // Item 5: affected-versions with interval notation
                ("5", "Bug with affected versions", "bug",
                    "Has version range\n```buildmark\naffected-versions: [1.0.0, 2.0.0)\n```"),

                // Item 6: buildmark block hidden in HTML comment
                ("6", "Hidden block item", "feature",
                    "Hidden metadata\n<!--\n```buildmark\nvisibility: public\ntype: bug\n```\n-->"),

                // Item 7: no buildmark block - regular item
                ("7", "Regular change", "feature",
                    "Just a regular description without any buildmark block")
            };

            // Process each item through ItemControlsParser (same as real connectors)
            List<ItemInfo> changes = [];
            List<ItemInfo> bugs = [];

            foreach (var (id, title, defaultType, description) in items)
            {
                // Parse item controls from description
                var controls = ItemControlsParser.Parse(description);

                // Apply visibility rules
                var forceInclude = controls?.Visibility == "public";
                if (!forceInclude && controls?.Visibility == "internal")
                {
                    continue;
                }

                // Apply type override
                var type = defaultType;
                if (controls?.Type == "bug")
                {
                    type = "bug";
                }
                else if (controls?.Type == "feature")
                {
                    type = "feature";
                }

                // Create item info with affected versions if present
                var item = new ItemInfo(
                    id,
                    title,
                    $"https://example.com/issues/{id}",
                    type,
                    int.Parse(id),
                    controls?.AffectedVersions);

                // Categorize by type
                if (type == "bug")
                {
                    bugs.Add(item);
                }
                else
                {
                    changes.Add(item);
                }
            }

            // Build version information
            var currentVersion = version ?? VersionTag.Create("2.0.0");
            var currentTag = new VersionCommitTag(currentVersion, "abc123def456");
            var baselineVersion = VersionTag.Create("1.0.0");
            var baselineTag = new VersionCommitTag(baselineVersion, "def456ghi789");

            // Return build information
            return Task.FromResult(new BuildInformation(
                baselineTag,
                currentTag,
                changes,
                bugs,
                [],
                null));
        }
    }
}



