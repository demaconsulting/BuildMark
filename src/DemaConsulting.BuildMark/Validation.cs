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
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.BuildMark;

/// <summary>
///     Provides self-validation functionality for the BuildMark tool.
/// </summary>
internal static class Validation
{
    /// <summary>
    ///     Runs self-validation tests and optionally writes results to a file.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Print validation header
        PrintValidationHeader(context);

        // Create test results collection
        var testResults = new DemaConsulting.TestResults.TestResults
        {
            Name = "BuildMark Self-Validation"
        };

        // Run core functionality tests using MockRepoConnector
        RunMarkdownReportGenerationTest(context, testResults);
        RunVersionTagParsingTest(context, testResults);
        RunBugCategorizationTest(context, testResults);
        RunKnownIssuesTrackingTest(context, testResults);
        RunBaselineVersionDetectionForReleaseTest(context, testResults);
        RunBaselineVersionDetectionForPreReleaseTest(context, testResults);

        // Calculate totals
        var totalTests = testResults.Results.Count;
        var passedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Passed);
        var failedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Failed);

        // Print summary
        context.WriteLine("");
        context.WriteLine($"Total Tests: {totalTests}");
        context.WriteLine($"Passed: {passedTests}");
        if (failedTests > 0)
        {
            context.WriteError($"Failed: {failedTests}");
        }
        else
        {
            context.WriteLine($"Failed: {failedTests}");
        }

        // Write results file if requested
        if (context.ResultsFile != null)
        {
            WriteResultsFile(context, testResults);
        }
    }

    /// <summary>
    ///     Prints the validation header with system information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintValidationHeader(Context context)
    {
        context.WriteLine("# DEMA Consulting BuildMark Self-validation");
        context.WriteLine("");
        context.WriteLine("| Information         | Value                                              |");
        context.WriteLine("| :------------------ | :------------------------------------------------- |");
        context.WriteLine($"| BuildMark Version   | {Program.Version,-50} |");
        context.WriteLine($"| Machine Name        | {Environment.MachineName,-50} |");
        context.WriteLine($"| OS Version          | {RuntimeInformation.OSDescription,-50} |");
        context.WriteLine($"| DotNet Runtime      | {RuntimeInformation.FrameworkDescription,-50} |");
        context.WriteLine($"| Time Stamp          | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC{"",-29} |");
        context.WriteLine("");
    }

    /// <summary>
    ///     Runs a test for markdown report generation functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunMarkdownReportGenerationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_MarkdownReportGeneration_MockData_ProducesValidMarkdown",
            "Markdown Report Generation Test",
            async () =>
            {
                // Create mock connector and get build information for version 2.0.0
                var connector = new MockRepoConnector();
                var version = Version.Create("2.0.0");
                var buildInfo = await connector.GetBuildInformationAsync(version);

                // Generate markdown report
                var markdown = buildInfo.ToMarkdown(1, true);

                // Validate markdown content
                if (!markdown.Contains("# Build Report"))
                {
                    return "Markdown missing 'Build Report' heading";
                }

                if (!markdown.Contains("## Version Information"))
                {
                    return "Markdown missing 'Version Information' section";
                }

                if (!markdown.Contains("## Changes"))
                {
                    return "Markdown missing 'Changes' section";
                }

                if (!markdown.Contains("## Bugs Fixed"))
                {
                    return "Markdown missing 'Bugs Fixed' section";
                }

                if (!markdown.Contains("## Known Issues"))
                {
                    return "Markdown missing 'Known Issues' section";
                }

                if (!markdown.Contains("2.0.0"))
                {
                    return "Markdown missing version '2.0.0'";
                }

                // When version is explicitly specified, MockRepoConnector uses current hash
                if (!markdown.Contains("current123hash456"))
                {
                    return "Markdown missing commit hash";
                }

                return null; // Success
            });
    }

    /// <summary>
    ///     Runs a test for version tag parsing functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunVersionTagParsingTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_VersionTagParsing_VariousFormats_ParsesCorrectly",
            "Version Tag Parsing Test",
            async () =>
            {
                // Test various version tag formats
                var testCases = new[]
                {
                    ("v1.0.0", "1.0.0", false),
                    ("ver-1.1.0", "1.1.0", false),
                    ("release_2.0.0-beta.1", "2.0.0-beta.1", true),
                    ("v2.0.0-rc.1", "2.0.0-rc.1", true),
                    ("2.0.0", "2.0.0", false)
                };

                foreach (var (tag, expectedVersion, expectedIsPreRelease) in testCases)
                {
                    var version = Version.TryCreate(tag);
                    if (version == null)
                    {
                        return $"Failed to parse tag: {tag}";
                    }

                    if (version.Tag != tag)
                    {
                        return $"Tag mismatch: expected '{tag}', got '{version.Tag}'";
                    }

                    if (!version.FullVersion.StartsWith(expectedVersion.Split('-')[0]))
                    {
                        return $"Version mismatch for tag '{tag}': expected to start with '{expectedVersion.Split('-')[0]}', got '{version.FullVersion}'";
                    }

                    if (version.IsPreRelease != expectedIsPreRelease)
                    {
                        return $"PreRelease flag mismatch for tag '{tag}': expected {expectedIsPreRelease}, got {version.IsPreRelease}";
                    }
                }

                return await Task.FromResult<string?>(null); // Success
            });
    }

    /// <summary>
    ///     Runs a test for bug categorization functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunBugCategorizationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_BugCategorization_MockData_CategorizesBugsCorrectly",
            "Bug Categorization Test",
            async () =>
            {
                // Create mock connector and get build information
                var connector = new MockRepoConnector();
                var version = Version.Create("2.0.0");
                var buildInfo = await connector.GetBuildInformationAsync(version);

                // Verify bug categorization
                if (buildInfo.Bugs.Count == 0)
                {
                    return "No bugs found in build information";
                }

                // Check that all bugs have type "bug"
                foreach (var bug in buildInfo.Bugs)
                {
                    if (bug.Type != "bug")
                    {
                        return $"Item '{bug.Id}' in Bugs list has incorrect type '{bug.Type}'";
                    }
                }

                // Check that Changes don't contain bugs
                foreach (var change in buildInfo.Changes)
                {
                    if (change.Type == "bug")
                    {
                        return $"Item '{change.Id}' in Changes list has type 'bug' but should be in Bugs list";
                    }
                }

                // Verify specific expected bug is present
                var expectedBug = buildInfo.Bugs.FirstOrDefault(b => b.Id == "2");
                if (expectedBug == null)
                {
                    return "Expected bug '2' (Fix bug in Y) not found in Bugs list";
                }

                if (!expectedBug.Title.Contains("bug"))
                {
                    return $"Bug '2' has unexpected title: {expectedBug.Title}";
                }

                return null; // Success
            });
    }

    /// <summary>
    ///     Runs a test for known issues tracking functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunKnownIssuesTrackingTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_KnownIssuesTracking_MockData_TracksOpenBugs",
            "Known Issues Tracking Test",
            async () =>
            {
                // Create mock connector and get build information
                var connector = new MockRepoConnector();
                var version = Version.Create("2.0.0");
                var buildInfo = await connector.GetBuildInformationAsync(version);

                // Verify known issues are tracked
                if (buildInfo.KnownIssues.Count == 0)
                {
                    return "No known issues found in build information";
                }

                // Check that all known issues have type "bug"
                foreach (var issue in buildInfo.KnownIssues)
                {
                    if (issue.Type != "bug")
                    {
                        return $"Known issue '{issue.Id}' has incorrect type '{issue.Type}'";
                    }
                }

                // Verify that known issues are not in the Bugs or Changes lists
                var allFixedIds = new HashSet<string>(
                    buildInfo.Bugs.Select(b => b.Id)
                        .Concat(buildInfo.Changes.Select(c => c.Id)));

                var duplicateIssue = buildInfo.KnownIssues.FirstOrDefault(issue => allFixedIds.Contains(issue.Id));
                if (duplicateIssue != null)
                {
                    return $"Known issue '{duplicateIssue.Id}' is also listed in fixed bugs/changes";
                }

                // Verify specific known issues are present (issues 4 and 5 are open bugs)
                var expectedKnownIssues = new[] { "4", "5" };
                var missingIssue = expectedKnownIssues.FirstOrDefault(expectedId => !buildInfo.KnownIssues.Any(i => i.Id == expectedId));
                if (missingIssue != null)
                {
                    return $"Expected known issue '{missingIssue}' not found in KnownIssues list";
                }

                return null; // Success
            });
    }

    /// <summary>
    ///     Runs a test for baseline version detection for releases.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunBaselineVersionDetectionForReleaseTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_BaselineVersionDetection_Release_SkipsPreReleases",
            "Baseline Version Detection For Release Test",
            async () =>
            {
                // Create mock connector and get build information for version 2.0.0 (release)
                var connector = new MockRepoConnector();
                var version = Version.Create("2.0.0");
                var buildInfo = await connector.GetBuildInformationAsync(version);

                // Verify baseline version is set
                if (buildInfo.BaselineVersionTag == null)
                {
                    return "Baseline version is null for release 2.0.0";
                }

                // Verify baseline version is not a pre-release (should skip v2.0.0-rc.1 and release_2.0.0-beta.1)
                if (buildInfo.BaselineVersionTag.VersionInfo.IsPreRelease)
                {
                    return $"Baseline version '{buildInfo.BaselineVersionTag.VersionInfo.Tag}' is a pre-release but should skip pre-releases for release builds";
                }

                // For 2.0.0, the previous non-pre-release should be ver-1.1.0
                if (!buildInfo.BaselineVersionTag.VersionInfo.Tag.Contains("1.1.0"))
                {
                    return $"Expected baseline version containing '1.1.0' for release 2.0.0, got '{buildInfo.BaselineVersionTag.VersionInfo.Tag}'";
                }

                return null; // Success
            });
    }

    /// <summary>
    ///     Runs a test for baseline version detection for pre-releases.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunBaselineVersionDetectionForPreReleaseTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_BaselineVersionDetection_PreRelease_UsesPreviousTag",
            "Baseline Version Detection For Pre-Release Test",
            async () =>
            {
                // Create mock connector and get build information for v2.0.0-rc.1 (pre-release)
                var connector = new MockRepoConnector();
                var version = Version.Create("v2.0.0-rc.1");
                var buildInfo = await connector.GetBuildInformationAsync(version);

                // Verify baseline version is set
                if (buildInfo.BaselineVersionTag == null)
                {
                    return "Baseline version is null for pre-release v2.0.0-rc.1";
                }

                // For pre-release v2.0.0-rc.1, the previous tag should be release_2.0.0-beta.1
                // Pre-releases use the immediately previous tag
                if (!buildInfo.BaselineVersionTag.VersionInfo.Tag.Contains("beta"))
                {
                    return $"Expected baseline version containing 'beta' for pre-release v2.0.0-rc.1, got '{buildInfo.BaselineVersionTag.VersionInfo.Tag}'";
                }

                return null; // Success
            });
    }

    /// <summary>
    ///     Runs a validation test with common test execution logic.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="displayName">The display name for console output.</param>
    /// <param name="testAction">Function to execute the test. Returns null on success or error message on failure.</param>
    private static void RunValidationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        string testName,
        string displayName,
        Func<Task<string?>> testAction)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult(testName);

        try
        {
            // Execute the test action
            var errorMessage = testAction().GetAwaiter().GetResult();

            if (errorMessage == null)
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                context.WriteLine($"✓ {displayName} - PASSED");
            }
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = errorMessage;
                context.WriteError($"✗ {displayName} - FAILED: {errorMessage}");
            }
        }
        // Generic catch is justified here to handle any exception during test execution
        catch (Exception ex)
        {
            HandleTestException(test, context, displayName, ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Writes test results to a file in TRX or JUnit format.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results to write.</param>
    private static void WriteResultsFile(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        if (context.ResultsFile == null)
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(context.ResultsFile).ToLowerInvariant();
            string content;

            if (extension == ".trx")
            {
                content = TrxSerializer.Serialize(testResults);
            }
            else if (extension == ".xml")
            {
                // Assume JUnit format for .xml extension
                content = JUnitSerializer.Serialize(testResults);
            }
            else
            {
                context.WriteError($"Error: Unsupported results file format '{extension}'. Use .trx or .xml extension.");
                return;
            }

            File.WriteAllText(context.ResultsFile, content);
            context.WriteLine($"Results written to {context.ResultsFile}");
        }
        // Generic catch is justified here as a top-level handler to log file write errors
        catch (Exception ex)
        {
            context.WriteError($"Error: Failed to write results file: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new test result object with common properties.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <returns>A new test result object.</returns>
    private static DemaConsulting.TestResults.TestResult CreateTestResult(string testName)
    {
        return new DemaConsulting.TestResults.TestResult
        {
            Name = testName,
            ClassName = "Validation",
            CodeBase = "BuildMark"
        };
    }

    /// <summary>
    ///     Finalizes a test result by setting its duration and adding it to the collection.
    /// </summary>
    /// <param name="test">The test result to finalize.</param>
    /// <param name="startTime">The start time of the test.</param>
    /// <param name="testResults">The test results collection to add to.</param>
    private static void FinalizeTestResult(
        DemaConsulting.TestResults.TestResult test,
        DateTime startTime,
        DemaConsulting.TestResults.TestResults testResults)
    {
        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Handles test exceptions by setting failure information and logging the error.
    /// </summary>
    /// <param name="test">The test result to update.</param>
    /// <param name="context">The context for output.</param>
    /// <param name="testName">The name of the test for error messages.</param>
    /// <param name="ex">The exception that occurred.</param>
    private static void HandleTestException(
        DemaConsulting.TestResults.TestResult test,
        Context context,
        string testName,
        Exception ex)
    {
        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
        test.ErrorMessage = $"Exception: {ex.Message}";
        context.WriteError($"✗ {testName} - FAILED: {ex.Message}");
    }
}
