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

        // Create mock connector factory
        var mockFactory = () => new MockRepoConnector() as IRepoConnector;

        // Run core functionality tests
        RunMarkdownReportGeneration(context, testResults, mockFactory);
        RunGitIntegration(context, testResults, mockFactory);
        RunIssueTracking(context, testResults, mockFactory);
        RunKnownIssuesReporting(context, testResults, mockFactory);

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
    /// <param name="mockFactory">The mock connector factory.</param>
    private static void RunMarkdownReportGeneration(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        Func<IRepoConnector> mockFactory)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_MarkdownReportGeneration",
            "Markdown Report Generation Test",
            mockFactory,
            "build-report.md",
            (logContent, reportContent) =>
            {
                if (reportContent == null)
                {
                    return "Report file not created";
                }

                if (reportContent.Contains("# Build Report") &&
                    reportContent.Contains("## Version Information") &&
                    reportContent.Contains("2.0.0") &&
                    reportContent.Contains("current123hash456"))
                {
                    return null;
                }

                return "Report file missing expected content";
            });
    }

    /// <summary>
    ///     Runs a test for git integration functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="mockFactory">The mock connector factory.</param>
    private static void RunGitIntegration(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        Func<IRepoConnector> mockFactory)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_GitIntegration",
            "Git Integration Test",
            mockFactory,
            null,
            (logContent, _) =>
            {
                if (logContent.Contains("Build Version: 2.0.0") &&
                    logContent.Contains("Commit Hash: current123hash456") &&
                    logContent.Contains("Previous Version: ver-1.1.0"))
                {
                    return null;
                }

                return "Expected git information not found in log";
            });
    }

    /// <summary>
    ///     Runs a test for issue tracking functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="mockFactory">The mock connector factory.</param>
    private static void RunIssueTracking(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        Func<IRepoConnector> mockFactory)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_IssueTracking",
            "Issue Tracking Test",
            mockFactory,
            null,
            (logContent, _) =>
            {
                if (logContent.Contains("Changes: ") &&
                    logContent.Contains("Bugs Fixed: "))
                {
                    return null;
                }

                return "Expected issue tracking information not found in log";
            });
    }

    /// <summary>
    ///     Runs a test for known issues reporting functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="mockFactory">The mock connector factory.</param>
    private static void RunKnownIssuesReporting(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        Func<IRepoConnector> mockFactory)
    {
        RunValidationTest(
            context,
            testResults,
            "BuildMark_KnownIssuesReporting",
            "Known Issues Reporting Test",
            mockFactory,
            "known-issues-report.md",
            (logContent, reportContent) =>
            {
                if (reportContent == null)
                {
                    return "Report file not created";
                }

                if (logContent.Contains("Known Issues: 2") &&
                    reportContent.Contains("## Known Issues"))
                {
                    return null;
                }

                return "Expected known issues not found in report";
            });
    }

    /// <summary>
    ///     Runs a validation test with common test execution logic.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    /// <param name="testName">The name of the test.</param>
    /// <param name="displayName">The display name for console output.</param>
    /// <param name="mockFactory">The mock connector factory.</param>
    /// <param name="reportFileName">Optional report file name to generate.</param>
    /// <param name="validator">Function to validate test results. Returns null on success or error message on failure.</param>
    private static void RunValidationTest(
        Context context,
        DemaConsulting.TestResults.TestResults testResults,
        string testName,
        string displayName,
        Func<IRepoConnector> mockFactory,
        string? reportFileName,
        Func<string, string?, string?> validator)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult(testName);

        try
        {
            using var tempDir = new TemporaryDirectory();
            // Ensure log file name doesn't contain path separators or absolute paths
            var safeTestName = testName.Replace(Path.DirectorySeparatorChar, '_').Replace(Path.AltDirectorySeparatorChar, '_');
            var logFile = Path.Combine(tempDir.DirectoryPath, $"{safeTestName}.log");
            var reportFile = reportFileName != null ? Path.Combine(tempDir.DirectoryPath, Path.GetFileName(reportFileName)) : null;

            // Build command line arguments
            var args = new List<string>
            {
                "--silent",
                "--log", logFile,
                "--build-version", "2.0.0"
            };

            if (reportFile != null)
            {
                args.Add("--report");
                args.Add(reportFile);

                // Include known issues if this is the known issues test
                if (testName.Contains("KnownIssues"))
                {
                    args.Add("--include-known-issues");
                }
            }

            // Run the program
            int exitCode;
            using (var testContext = Context.Create([.. args], mockFactory))
            {
                Program.Run(testContext);
                exitCode = testContext.ExitCode;
            }

            // Check if execution succeeded
            if (exitCode == 0)
            {
                // Read log and report contents
                var logContent = File.ReadAllText(logFile);
                var reportContent = reportFile != null && File.Exists(reportFile)
                    ? File.ReadAllText(reportFile)
                    : null;

                // Validate the results
                var errorMessage = validator(logContent, reportContent);

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
            else
            {
                test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                test.ErrorMessage = $"Program exited with code {exitCode}";
                context.WriteError($"✗ {displayName} - FAILED: Exit code {exitCode}");
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
    /// <param name="displayName">The display name for console output.</param>
    /// <param name="ex">The exception that occurred.</param>
    private static void HandleTestException(
        DemaConsulting.TestResults.TestResult test,
        Context context,
        string displayName,
        Exception ex)
    {
        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
        test.ErrorMessage = $"Exception: {ex.Message}";
        context.WriteError($"✗ {displayName} - FAILED: {ex.Message}");
    }

    /// <summary>
    ///     Represents a temporary directory that is automatically deleted when disposed.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        ///     Gets the path to the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            // Use Guid as filename to ensure it's not an absolute path
            var uniqueName = $"buildmark_validation_{Guid.NewGuid()}";
            DirectoryPath = Path.Combine(Path.GetTempPath(), uniqueName);

            try
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException)
            {
                throw new InvalidOperationException($"Failed to create temporary directory: {ex.Message}", ex);
            }
        }

        /// <summary>
        ///     Deletes the temporary directory and all its contents.
        /// </summary>
        public void Dispose()
        {
            try
            {
                if (Directory.Exists(DirectoryPath))
                {
                    Directory.Delete(DirectoryPath, true);
                }
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                // Ignore cleanup errors during disposal
            }
        }
    }
}
