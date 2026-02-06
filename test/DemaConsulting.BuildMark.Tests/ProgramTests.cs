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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the Program class.
/// </summary>
[TestClass]
public class ProgramTests
{
    /// <summary>
    ///     Test that the version property returns a valid version string.
    /// </summary>
    [TestMethod]
    public void Program_Version_ReturnsValidVersion()
    {
        // Retrieve version string from Program
        var version = Program.Version;

        // Verify version is not null or empty
        Assert.IsNotNull(version);
        Assert.IsFalse(string.IsNullOrWhiteSpace(version));
    }

    /// <summary>
    ///     Test that Run with version flag outputs version to console.
    /// </summary>
    [TestMethod]
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
    [TestMethod]
    public void Program_Run_HelpFlag_OutputsHelpMessage()
    {
        // Create context with help flag and silent mode to capture output
        using var context = Context.Create(["-h"]);

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
    [TestMethod]
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
    [TestMethod]
    public void Program_Run_ReportWithIncludeKnownIssuesFlag_GeneratesReportWithKnownIssues()
    {
        // Create temporary report file path
        var reportFile = Path.GetTempFileName();
        try
        {
            // Create context with report and include-known-issues flags
            using var context = Context.Create(["--report", reportFile, "--include-known-issues"]);

            // Verify IncludeKnownIssues property is set
            Assert.IsTrue(context.IncludeKnownIssues);

            // Capture console output
            var originalOut = Console.Out;
            using var writer = new StringWriter();
            Console.SetOut(writer);

            try
            {
                // Run program
                Program.Run(context);

                // Verify report file was created
                Assert.IsTrue(File.Exists(reportFile));

                // Verify the context flag was set correctly
                Assert.IsTrue(context.IncludeKnownIssues);
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
}
