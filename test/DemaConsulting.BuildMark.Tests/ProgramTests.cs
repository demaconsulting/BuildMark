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
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the Program class.
/// </summary>
public class ProgramTests
{
    /// <summary>
    ///     Test that the version property returns a valid version string.
    /// </summary>
    [Fact]
    public void Program_Version_ReturnsValidVersion()
    {
        // Retrieve version string from Program
        var version = Program.Version;

        // Verify version is not null or empty
        Assert.NotNull(version);
        Assert.False(string.IsNullOrWhiteSpace(version));
    }

    /// <summary>
    ///     Test that Run with version flag outputs version to console.
    /// </summary>
    [Fact]
    public void Program_Run_VersionFlag_OutputsVersionToConsole()
    {
        // Create context with version flag
        using var context = Context.Create(["-v"]);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Run program
            Program.Run(context);

            // Verify version is written to console
            var output = writer.ToString();
            Assert.Contains(Program.Version, output);
        }
        finally
        {
            // Restore console output
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with help flag outputs help message.
    /// </summary>
    [Fact]
    public void Program_Run_HelpFlag_OutputsHelpMessage()
    {
        AssertHelpFlagOutputsHelpMessage("-h");
    }

    /// <summary>
    ///     Test that Run with question-mark help flag outputs help message.
    /// </summary>
    [Fact]
    public void Program_Run_QuestionMarkFlag_OutputsHelpMessage()
    {
        AssertHelpFlagOutputsHelpMessage("-?");
    }

    /// <summary>
    ///     Test that Run with long help flag outputs help message.
    /// </summary>
    [Fact]
    public void Program_Run_LongHelpFlag_OutputsHelpMessage()
    {
        AssertHelpFlagOutputsHelpMessage("--help");
    }

    /// <summary>
    ///     Asserts that running the program with the specified help flag outputs the standard help message.
    /// </summary>
    /// <param name="helpFlag">The help flag to test (e.g. "-?", "-h", "--help").</param>
    private static void AssertHelpFlagOutputsHelpMessage(string helpFlag)
    {
        // Create context with the specified help flag
        using var context = Context.Create([helpFlag]);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Run program
            Program.Run(context);

            // Verify help is written to console
            var output = writer.ToString();
            Assert.Contains("Usage: buildmark", output);
            Assert.Contains("Options:", output);
            Assert.Contains("--lint", output);
        }
        finally
        {
            // Restore console output
            Console.SetOut(originalOut);
        }
    }


    /// <summary>
    ///     Test that Run with validate flag outputs validation message.
    /// </summary>
    [Fact]
    public void Program_Run_ValidateFlag_OutputsValidationMessage()
    {
        // Create context with validate flag
        using var context = Context.Create(["--validate"]);

        // Capture console output
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Run program
            Program.Run(context);

            // Verify validation message is written to console
            var output = writer.ToString();
            Assert.Contains("Self-validation", output);
        }
        finally
        {
            // Restore console output
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Run with report and include-known-issues flags generates report with known issues.
    /// </summary>
    [Fact]
    public void Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues()
    {
        // Create temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with report and include-known-issues flags
            using var context = Context.Create(
                ["--build-version", "2.0.0", "--report", reportFile, "--include-known-issues", "--silent"],
                () => new MockRepoConnector());

            // Verify IncludeKnownIssues property is set
            Assert.True(context.IncludeKnownIssues);

            // Capture console output
            var originalOut = Console.Out;
            using var writer = new StringWriter();
            Console.SetOut(writer);

            try
            {
                // Run program
                Program.Run(context);

                // Verify report file was created
                Assert.True(File.Exists(reportFile));

                // Verify the context flag was set correctly
                Assert.True(context.IncludeKnownIssues);
            }
            finally
            {
                // Restore console output
                Console.SetOut(originalOut);
            }
        }
        finally
        {
            // Clean up report file
            if (File.Exists(reportFile))
            {
                File.Delete(reportFile);
            }
        }
    }

    /// <summary>
    ///     Test that Run with lint flag succeeds when no configuration file is present.
    /// </summary>
    [Fact]
    public void Program_Run_LintFlagWithoutConfiguration_LeavesExitCodeAtZero()
    {
        // Arrange
        using var context = Context.Create(["--lint", "--silent"]);

        // Act
        Program.Run(context);

        // Assert
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Run with an invalid build version format writes an error and exits with code 1.
    /// </summary>
    [Fact]
    public void Program_Run_InvalidBuildVersion_WritesErrorAndSetsExitCode()
    {
        // Arrange
        using var context = Context.Create(
            ["--build-version", "not-a-version"],
            () => new MockRepoConnector());

        // Capture Console.Error and Console.Out
        using var errorOutput = new StringWriter();
        using var stdOutput = new StringWriter();
        var originalError = Console.Error;
        var originalOut = Console.Out;
        try
        {
            Console.SetError(errorOutput);
            Console.SetOut(stdOutput);

            // Act
            Program.Run(context);
        }
        finally
        {
            Console.SetError(originalError);
            Console.SetOut(originalOut);
        }

        // Assert
        Assert.Equal(1, context.ExitCode);
        Assert.Contains("Error", errorOutput.ToString());
    }

    /// <summary>
    ///     Test that Run when the connector throws InvalidOperationException during data retrieval
    ///     writes an error and exits with code 1.
    /// </summary>
    [Fact]
    public void Program_Run_ConnectorThrowsInvalidOperationException_WritesErrorAndSetsExitCode()
    {
        // Arrange: inject a connector whose GetBuildInformationAsync throws
        using var context = Context.Create(
            ["--build-version", "2.0.0"],
            () => new ThrowingConnector());

        // Capture Console.Error and Console.Out
        using var errorOutput = new StringWriter();
        using var stdOutput = new StringWriter();
        var originalError = Console.Error;
        var originalOut = Console.Out;
        try
        {
            Console.SetError(errorOutput);
            Console.SetOut(stdOutput);

            // Act
            Program.Run(context);
        }
        finally
        {
            Console.SetError(originalError);
            Console.SetOut(originalOut);
        }

        // Assert
        Assert.Equal(1, context.ExitCode);
        Assert.Contains("Error", errorOutput.ToString());
    }

    /// <summary>
    ///     Stub connector that throws <see cref="InvalidOperationException"/> on
    ///     <see cref="GetBuildInformationAsync"/> to simulate a connector failure.
    /// </summary>
    private sealed class ThrowingConnector : IRepoConnector
    {
        /// <inheritdoc/>
        public Task<BuildInformation> GetBuildInformationAsync(VersionTag? version = null)
        {
            throw new InvalidOperationException("Simulated connector failure");
        }
    }
}
