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
using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Integration tests that run the BuildMark application through dotnet.
/// </summary>
[TestClass]
public class IntegrationTests
{
    private string _dllPath = string.Empty;

    /// <summary>
    ///     Initialize test by locating the BuildMark DLL.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        // The DLL should be in the same directory as the test assembly
        // because the test project references the main project
        var baseDir = AppContext.BaseDirectory;
        _dllPath = PathHelpers.SafePathCombine(baseDir, "DemaConsulting.BuildMark.dll");

        Assert.IsTrue(File.Exists(_dllPath), $"Could not find BuildMark DLL at {_dllPath}");
    }

    /// <summary>
    ///     Test that version flag outputs version information.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_VersionFlag_OutputsVersion()
    {
        // Run the application with --version flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify version is output
        Assert.IsFalse(string.IsNullOrWhiteSpace(output));
        Assert.DoesNotContain("Error", output);
    }

    /// <summary>
    ///     Test that help flag outputs usage information.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_HelpFlag_OutputsUsageInformation()
    {
        // Run the application with --help flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify usage information
        Assert.Contains("Usage: buildmark", output);
        Assert.Contains("Options:", output);
        Assert.Contains("--version", output);
        Assert.Contains("--help", output);
    }

    /// <summary>
    ///     Test that silent flag suppresses output.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_SilentFlag_SuppressesOutput()
    {
        // Run the application with --silent and --help flags
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--silent",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify no banner in output
        Assert.DoesNotContain("BuildMark version", output);
    }

    /// <summary>
    ///     Test that invalid argument shows error.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_InvalidArgument_ShowsError()
    {
        // Run the application with invalid argument
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--invalid-argument");

        // Verify error exit code
        Assert.AreEqual(1, exitCode);

        // Verify error message
        Assert.Contains("Error:", output);
        Assert.Contains("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that validate flag runs self-validation.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ValidateFlag_RunsSelfValidation()
    {
        // Run the application with --validate flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify validation runs
        Assert.IsFalse(string.IsNullOrWhiteSpace(output));
    }

    /// <summary>
    ///     Test that log parameter is accepted.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LogParameter_IsAccepted()
    {
        // Run the application with log parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--log", "test.log",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that report parameter is accepted.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ReportParameter_IsAccepted()
    {
        // Run the application with report parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--report", "output.md",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that report-depth parameter is accepted.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ReportDepthParameter_IsAccepted()
    {
        // Run the application with report-depth parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--report-depth", "2",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that build-version parameter is accepted.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_BuildVersionParameter_IsAccepted()
    {
        // Run the application with build-version parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--build-version", "1.0.0",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that results parameter is accepted.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_ResultsParameter_IsAccepted()
    {
        // Run the application with results parameter
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--results", "results.trx",
            "--help");

        // Verify success
        Assert.AreEqual(0, exitCode);

        // Verify it's not an argument error
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that the report generates a markdown file with version information.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_GeneratesMarkdownWithVersionInformation()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_ContainsChangesAndBugFixesWithHyperlinks()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_ShowsVersionRangeFromPreviousRelease()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_IncludesKnownIssues_WhenFlagIsSet()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_DepthTwo_UsesLevelTwoHeadings()
    {
        // Arrange: create a temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with mock connector and report depth 2
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--report-depth", "2", "--silent"],
                () => new MockRepoConnector());

            // Act: run the program
            Program.Run(context);

            // Assert: report uses level-two heading for the title and level-three for sections
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_LintFlag_IsAccepted()
    {
        // Act: run the application with --lint flag
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--lint");

        // Assert: tool completes successfully
        Assert.AreEqual(0, exitCode);
        Assert.DoesNotContain("Unsupported argument", output);
    }

    /// <summary>
    ///     Test that the tool consumes the .buildmark.yaml configuration file during report generation.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_ConsumesConfigurationFileDuringGeneration()
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
            Assert.AreEqual(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            Assert.IsFalse(string.IsNullOrWhiteSpace(content));
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
    [TestMethod]
    public void IntegrationTest_Report_UsesConnectorForBuildData()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_ContainsSectionDefinitions()
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
            Assert.AreEqual(0, context.ExitCode);
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
    [TestMethod]
    public void IntegrationTest_Report_RoutesItemsToCorrectSections()
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
            Assert.AreEqual(0, context.ExitCode);
            var content = File.ReadAllText(reportFile);
            var changesStart = content.IndexOf("## Changes", StringComparison.Ordinal);
            var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
            Assert.IsTrue(changesStart >= 0, "Report must contain Changes section");
            Assert.IsTrue(bugsStart >= 0, "Report must contain Bugs Fixed section");

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
    [TestMethod]
    public void IntegrationTest_Report_RecognizesBuildmarkCodeBlock()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with buildmark type override was reclassified (proves block was recognized)
        Assert.Contains("Reclassified as bug", content);
    }

    /// <summary>
    ///     Test that the tool supports a visibility field in the buildmark block.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_VisibilityFieldControlsInclusion()
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
    [TestMethod]
    public void IntegrationTest_Report_PublicVisibility_IncludesItem()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with visibility: public is present in the report
        Assert.Contains("Public feature X", content);
    }

    /// <summary>
    ///     Test that the tool excludes an item when visibility is set to internal.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_InternalVisibility_ExcludesItem()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with visibility: internal does not appear in the report
        Assert.DoesNotContain("Internal refactoring", content);
    }

    /// <summary>
    ///     Test that the tool supports a type field in the buildmark block to override classification.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_TypeFieldOverridesClassification()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item originally typed as feature was reclassified to bug (appears in Bugs Fixed)
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.IsTrue(bugsStart >= 0, "Report must contain Bugs Fixed section");
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
    [TestMethod]
    public void IntegrationTest_Report_TypeBug_PlacesItemInBugsFixed()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with type: bug override appears in the Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.IsTrue(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Reclassified as bug", bugsSection);
    }

    /// <summary>
    ///     Test that the tool classifies an item as a feature when type is set to feature.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_TypeFeature_PlacesItemInChanges()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with type: feature override appears in the Changes section
        var changesStart = content.IndexOf("## Changes", StringComparison.Ordinal);
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.IsTrue(changesStart >= 0, "Report must contain Changes section");
        Assert.IsTrue(bugsStart > changesStart, "Bugs Fixed section must follow Changes section");
        var changesSection = content[changesStart..bugsStart];
        Assert.Contains("Reclassified as feature", changesSection);
    }

    /// <summary>
    ///     Test that the tool supports an affected-versions field in the buildmark block.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_AffectedVersionsField_ProcessesSuccessfully()
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
    [TestMethod]
    public void IntegrationTest_Report_AffectedVersionsInterval_ParsesNotation()
    {
        // Arrange: generate a report using the controls mock connector
        // (the connector creates an item with interval notation "[1.0.0, 2.0.0)")
        var content = GenerateControlsMockReport();

        // Assert: the item with interval notation was parsed and routed to Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.IsTrue(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Bug with affected versions", bugsSection);
    }

    /// <summary>
    ///     Test that the tool recognizes a buildmark block wrapped in an HTML comment.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_Report_HiddenBuildmarkBlock_IsRecognized()
    {
        // Arrange: generate a report using the controls mock connector
        var content = GenerateControlsMockReport();

        // Assert: item with HTML-comment-wrapped buildmark block is recognized and processed
        // The hidden block specifies type: bug, so the item should appear in Bugs Fixed section
        var bugsStart = content.IndexOf("## Bugs Fixed", StringComparison.Ordinal);
        Assert.IsTrue(bugsStart >= 0, "Report must contain Bugs Fixed section");
        var bugsSection = content[bugsStart..];
        Assert.Contains("Hidden block item", bugsSection);
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
            Assert.AreEqual(0, context.ExitCode);
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
        public Task<BuildInformation> GetBuildInformationAsync(VersionInfo? version = null)
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
            var changes = new List<ItemInfo>();
            var bugs = new List<ItemInfo>();

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
            var currentVersion = version ?? VersionInfo.Create("2.0.0");
            var currentTag = new VersionTag(currentVersion, "abc123def456");
            var baselineVersion = VersionInfo.Create("1.0.0");
            var baselineTag = new VersionTag(baselineVersion, "def456ghi789");

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
