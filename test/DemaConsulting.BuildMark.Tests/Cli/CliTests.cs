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

using DemaConsulting.BuildMark.Cli;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Subsystem-level tests for the Cli subsystem.
/// </summary>
[TestClass]
public class CliTests
{
    /// <summary>
    ///     Test that the Cli subsystem creates a valid context from empty arguments.
    /// </summary>
    [TestMethod]
    public void Cli_Context_EmptyArguments_CreatesValidContext()
    {
        // Arrange & Act: create context with no arguments
        using var context = Context.Create([]);

        // Assert: all properties have expected defaults
        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.IsFalse(context.Silent);
        Assert.IsFalse(context.Validate);
        Assert.IsNull(context.BuildVersion);
        Assert.IsNull(context.ReportFile);
        Assert.AreEqual(1, context.ReportDepth);
        Assert.IsFalse(context.IncludeKnownIssues);
        Assert.IsNull(context.ResultsFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the Version property when --version is specified.
    /// </summary>
    [TestMethod]
    public void Cli_VersionFlag_SetsProperty()
    {
        // Arrange & Act: create context with --version flag
        using var context = Context.Create(["--version"]);

        // Assert: Version property is set
        Assert.IsTrue(context.Version);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the Help property when --help is specified.
    /// </summary>
    [TestMethod]
    public void Cli_HelpFlag_SetsProperty()
    {
        // Arrange & Act: create context with --help flag
        using var context = Context.Create(["--help"]);

        // Assert: Help property is set
        Assert.IsTrue(context.Help);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the Silent property when --silent is specified.
    /// </summary>
    [TestMethod]
    public void Cli_SilentFlag_SetsProperty()
    {
        // Arrange & Act: create context with --silent flag
        using var context = Context.Create(["--silent"]);

        // Assert: Silent property is set
        Assert.IsTrue(context.Silent);
    }

    /// <summary>
    ///     Test that the Cli subsystem suppresses console output when --silent is specified.
    /// </summary>
    [TestMethod]
    public void Cli_SilentFlag_SuppressesConsoleOutput()
    {
        // Arrange: create context with silent flag and capture console output
        using var context = Context.Create(["--silent"]);
        using var output = new StringWriter();
        var originalOut = Console.Out;

        try
        {
            Console.SetOut(output);

            // Act: write a line through the context
            context.WriteLine("Test message");

            // Assert: no output was written to the console
            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            // Restore console output
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the BuildVersion property when --build-version is specified.
    /// </summary>
    [TestMethod]
    public void Cli_BuildVersionFlag_SetsProperty()
    {
        // Arrange & Act: create context with --build-version argument
        using var context = Context.Create(["--build-version", "1.2.3"]);

        // Assert: BuildVersion property is set to the specified value
        Assert.AreEqual("1.2.3", context.BuildVersion);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets report properties when --report and --report-depth are specified.
    /// </summary>
    [TestMethod]
    public void Cli_ReportFlags_SetProperties()
    {
        // Arrange & Act: create context with --report and --report-depth arguments
        using var context = Context.Create(["--report", "output.md", "--report-depth", "3", "--include-known-issues"]);

        // Assert: report properties are set to the specified values
        Assert.AreEqual("output.md", context.ReportFile);
        Assert.AreEqual(3, context.ReportDepth);
        Assert.IsTrue(context.IncludeKnownIssues);
    }

    /// <summary>
    ///     Test that the Cli subsystem creates a log file when --log is specified.
    /// </summary>
    [TestMethod]
    public void Cli_LogFlag_CreatesLogFile()
    {
        // Arrange: create a temporary log file path
        var logFile = Path.GetTempFileName();

        try
        {
            // Act: create context with --log argument and write a message
            using (var context = Context.Create(["--log", logFile]))
            {
                context.WriteLine("Subsystem log test");
            }

            // Assert: log file exists and contains the written message
            Assert.IsTrue(File.Exists(logFile));
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Subsystem log test", logContent);
        }
        finally
        {
            // Clean up log file
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the Validate property when --validate is specified.
    /// </summary>
    [TestMethod]
    public void Cli_ValidateFlag_SetsProperty()
    {
        // Arrange & Act: create context with --validate flag
        using var context = Context.Create(["--validate"]);

        // Assert: Validate property is set
        Assert.IsTrue(context.Validate);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets the ResultsFile property when --results is specified.
    /// </summary>
    [TestMethod]
    public void Cli_ResultsFlag_SetsProperty()
    {
        // Arrange & Act: create context with --results argument
        using var context = Context.Create(["--results", "results.trx"]);

        // Assert: ResultsFile property is set to the specified value
        Assert.AreEqual("results.trx", context.ResultsFile);
    }

    /// <summary>
    ///     Test that the Cli subsystem writes error messages to stderr.
    /// </summary>
    [TestMethod]
    public void Cli_ErrorOutput_WritesToStderr()
    {
        // Arrange: create context and capture stderr
        using var context = Context.Create([]);
        using var errorOutput = new StringWriter();
        var originalError = Console.Error;

        try
        {
            Console.SetError(errorOutput);

            // Act: write an error through the context
            context.WriteError("Subsystem error test");

            // Assert: error message was written to stderr
            Assert.Contains("Subsystem error test", errorOutput.ToString());
        }
        finally
        {
            // Restore console error output
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that the Cli subsystem throws an exception for an invalid argument.
    /// </summary>
    [TestMethod]
    public void Cli_InvalidArgument_ThrowsException()
    {
        // Arrange & Act & Assert: attempt to create context with an unsupported argument
        ArgumentException? caughtException = null;

        try
        {
            _ = Context.Create(["--unsupported"]);
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            caughtException = ex;
        }

        Assert.IsNotNull(caughtException);
        Assert.Contains("Unsupported argument '--unsupported'", caughtException.Message);
    }

    /// <summary>
    ///     Test that the Cli subsystem throws an exception when a required argument value is missing.
    /// </summary>
    [TestMethod]
    public void Cli_MissingArgumentValue_ThrowsException()
    {
        // Arrange & Act & Assert: attempt to create context with --build-version but no value
        ArgumentException? caughtException = null;

        try
        {
            _ = Context.Create(["--build-version"]);
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            caughtException = ex;
        }

        Assert.IsNotNull(caughtException);
        Assert.Contains("--build-version requires a version argument", caughtException.Message);
    }

    /// <summary>
    ///     Test that the Cli subsystem defaults ExitCode to zero.
    /// </summary>
    [TestMethod]
    public void Cli_ExitCode_DefaultsToZero()
    {
        // Arrange & Act: create context with no arguments
        using var context = Context.Create([]);

        // Assert: exit code defaults to zero
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that the Cli subsystem sets ExitCode to one after WriteError is called.
    /// </summary>
    [TestMethod]
    public void Cli_WriteError_SetsExitCodeToOne()
    {
        // Arrange: create a silent context to avoid console output
        using var context = Context.Create(["--silent"]);
        Assert.AreEqual(0, context.ExitCode);

        // Act: write an error
        context.WriteError("Error occurred");

        // Assert: exit code is now 1
        Assert.AreEqual(1, context.ExitCode);
    }
}
