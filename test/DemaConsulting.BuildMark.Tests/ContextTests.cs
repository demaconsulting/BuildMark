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
///     Tests for the Context class.
/// </summary>
[TestClass]
public class ContextTests
{
    /// <summary>
    ///     Test that Context.Create with empty arguments creates a valid context.
    /// </summary>
    [TestMethod]
    public void Context_Create_EmptyArguments_CreatesValidContext()
    {
        // Create context with empty arguments
        using var context = Context.Create([]);

        // Verify properties have expected default values
        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.IsFalse(context.Silent);
        Assert.IsFalse(context.Validate);
        Assert.IsNull(context.BuildVersion);
        Assert.IsNull(context.ReportFile);
        Assert.AreEqual(1, context.ReportDepth);
        Assert.IsNull(context.ResultsFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    ///     Test that Context.Create with -v flag sets Version property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ShortVersionFlag_SetsVersionProperty()
    {
        // Create context with -v flag
        using var context = Context.Create(["-v"]);

        // Verify Version property is set
        Assert.IsTrue(context.Version);
    }

    /// <summary>
    ///     Test that Context.Create with --version flag sets Version property.
    /// </summary>
    [TestMethod]
    public void Context_Create_LongVersionFlag_SetsVersionProperty()
    {
        // Create context with --version flag
        using var context = Context.Create(["--version"]);

        // Verify Version property is set
        Assert.IsTrue(context.Version);
    }

    /// <summary>
    ///     Test that Context.Create with -? flag sets Help property.
    /// </summary>
    [TestMethod]
    public void Context_Create_QuestionMarkHelpFlag_SetsHelpProperty()
    {
        // Create context with -? flag
        using var context = Context.Create(["-?"]);

        // Verify Help property is set
        Assert.IsTrue(context.Help);
    }

    /// <summary>
    ///     Test that Context.Create with -h flag sets Help property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ShortHelpFlag_SetsHelpProperty()
    {
        // Create context with -h flag
        using var context = Context.Create(["-h"]);

        // Verify Help property is set
        Assert.IsTrue(context.Help);
    }

    /// <summary>
    ///     Test that Context.Create with --help flag sets Help property.
    /// </summary>
    [TestMethod]
    public void Context_Create_LongHelpFlag_SetsHelpProperty()
    {
        // Create context with --help flag
        using var context = Context.Create(["--help"]);

        // Verify Help property is set
        Assert.IsTrue(context.Help);
    }

    /// <summary>
    ///     Test that Context.Create with --silent flag sets Silent property.
    /// </summary>
    [TestMethod]
    public void Context_Create_SilentFlag_SetsSilentProperty()
    {
        // Create context with --silent flag
        using var context = Context.Create(["--silent"]);

        // Verify Silent property is set
        Assert.IsTrue(context.Silent);
    }

    /// <summary>
    ///     Test that Context.Create with --validate flag sets Validate property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ValidateFlag_SetsValidateProperty()
    {
        // Create context with --validate flag
        using var context = Context.Create(["--validate"]);

        // Verify Validate property is set
        Assert.IsTrue(context.Validate);
    }

    /// <summary>
    ///     Test that Context.Create with --build-version argument sets BuildVersion property.
    /// </summary>
    [TestMethod]
    public void Context_Create_BuildVersionArgument_SetsBuildVersionProperty()
    {
        // Create context with --build-version argument
        using var context = Context.Create(["--build-version", "1.2.3"]);

        // Verify BuildVersion property is set
        Assert.AreEqual("1.2.3", context.BuildVersion);
    }

    /// <summary>
    ///     Test that Context.Create with --report argument sets ReportFile property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportArgument_SetsReportFileProperty()
    {
        // Create context with --report argument
        using var context = Context.Create(["--report", "report.md"]);

        // Verify ReportFile property is set
        Assert.AreEqual("report.md", context.ReportFile);
    }

    /// <summary>
    ///     Test that Context.Create with --report-depth argument sets ReportDepth property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthArgument_SetsReportDepthProperty()
    {
        // Create context with --report-depth argument
        using var context = Context.Create(["--report-depth", "3"]);

        // Verify ReportDepth property is set
        Assert.AreEqual(3, context.ReportDepth);
    }

    /// <summary>
    ///     Test that Context.Create with --results argument sets ResultsFile property.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultsArgument_SetsResultsFileProperty()
    {
        // Create context with --results argument
        using var context = Context.Create(["--results", "results.trx"]);

        // Verify ResultsFile property is set
        Assert.AreEqual("results.trx", context.ResultsFile);
    }

    /// <summary>
    ///     Test that Context.Create with --log argument creates log file.
    /// </summary>
    [TestMethod]
    public void Context_Create_LogArgument_CreatesLogFile()
    {
        // Create temporary log file path
        var logFile = Path.GetTempFileName();
        try
        {
            // Create context with --log argument
            using var context = Context.Create(["--log", logFile]);

            // Verify log file exists
            Assert.IsTrue(File.Exists(logFile));
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
    ///     Test that Context.Create with multiple arguments sets all properties correctly.
    /// </summary>
    [TestMethod]
    public void Context_Create_MultipleArguments_SetsAllPropertiesCorrectly()
    {
        // Create context with multiple arguments
        using var context = Context.Create(
        [
            "--silent",
            "--validate",
            "--build-version", "1.2.3",
            "--report", "report.md",
            "--report-depth", "2",
            "--results", "results.trx"
        ]);

        // Verify all properties are set correctly
        Assert.IsTrue(context.Silent);
        Assert.IsTrue(context.Validate);
        Assert.AreEqual("1.2.3", context.BuildVersion);
        Assert.AreEqual("report.md", context.ReportFile);
        Assert.AreEqual(2, context.ReportDepth);
        Assert.AreEqual("results.trx", context.ResultsFile);
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException for unsupported argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_UnsupportedArgument_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with unsupported argument
            _ = Context.Create(["--unsupported"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("Unsupported argument '--unsupported'", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --build-version has no value.
    /// </summary>
    [TestMethod]
    public void Context_Create_BuildVersionWithoutValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --build-version but no value
            _ = Context.Create(["--build-version"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--build-version requires a version argument", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --report has no value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportWithoutValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --report but no value
            _ = Context.Create(["--report"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--report requires a filename argument", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --report-depth has no value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthWithoutValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --report-depth but no value
            _ = Context.Create(["--report-depth"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--report-depth requires a depth argument", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --report-depth has non-integer value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthWithNonIntegerValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --report-depth with non-integer value
            _ = Context.Create(["--report-depth", "abc"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--report-depth requires a positive integer", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --report-depth has zero value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthWithZeroValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --report-depth with zero value
            _ = Context.Create(["--report-depth", "0"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--report-depth requires a positive integer", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --report-depth has negative value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepthWithNegativeValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --report-depth with negative value
            _ = Context.Create(["--report-depth", "-1"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--report-depth requires a positive integer", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --results has no value.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultsWithoutValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --results but no value
            _ = Context.Create(["--results"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--results requires a results filename argument", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws ArgumentException when --log has no value.
    /// </summary>
    [TestMethod]
    public void Context_Create_LogWithoutValue_ThrowsArgumentException()
    {
        try
        {
            // Attempt to create context with --log but no value
            _ = Context.Create(["--log"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Verify exception message
            Assert.Contains("--log requires a filename argument", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.Create throws InvalidOperationException when log file cannot be created.
    /// </summary>
    [TestMethod]
    public void Context_Create_InvalidLogFilePath_ThrowsInvalidOperationException()
    {
        try
        {
            // Attempt to create context with invalid log file path
            _ = Context.Create(["--log", "/invalid/path/to/log.txt"]);

            // Fail test if exception was not thrown
            Assert.Fail("Expected InvalidOperationException to be thrown");
        }
        catch (InvalidOperationException ex)
        {
            // Verify exception message
            Assert.Contains("Failed to open log file", ex.Message);
        }
    }

    /// <summary>
    ///     Test that Context.WriteLine writes to console when not in silent mode.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_NotSilent_WritesToConsole()
    {
        // Create context without silent flag
        using var context = Context.Create([]);

        // Capture console output
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write a line
            context.WriteLine("Test message");

            // Verify message was written to console
            Assert.AreEqual("Test message" + Environment.NewLine, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.WriteLine does not write to console when in silent mode.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_Silent_DoesNotWriteToConsole()
    {
        // Create context with silent flag
        using var context = Context.Create(["--silent"]);

        // Capture console output
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write a line
            context.WriteLine("Test message");

            // Verify no output to console
            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.WriteLine writes to log file when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_WithLogFile_WritesToLogFile()
    {
        // Create temporary log file path
        var logFile = Path.GetTempFileName();
        try
        {
            // Create context with log file
            using (var context = Context.Create(["--log", logFile]))
            {
                // Write a line
                context.WriteLine("Test message");
            }

            // Verify message was written to log file
            var logContent = File.ReadAllText(logFile);
            Assert.AreEqual("Test message" + Environment.NewLine, logContent);
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
    ///     Test that Context.WriteError writes to console when not in silent mode.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_NotSilent_WritesToConsole()
    {
        // Create context without silent flag
        using var context = Context.Create([]);

        // Capture console output
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write an error
            context.WriteError("Error message");

            // Verify message was written to console
            Assert.AreEqual("Error message" + Environment.NewLine, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.WriteError does not write to console when in silent mode.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_Silent_DoesNotWriteToConsole()
    {
        // Create context with silent flag
        using var context = Context.Create(["--silent"]);

        // Capture console output
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write an error
            context.WriteError("Error message");

            // Verify no output to console
            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.WriteError writes to log file when logging is enabled.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_WithLogFile_WritesToLogFile()
    {
        // Create temporary log file path
        var logFile = Path.GetTempFileName();
        try
        {
            // Create context with log file
            using (var context = Context.Create(["--log", logFile]))
            {
                // Write an error
                context.WriteError("Error message");
            }

            // Verify message was written to log file
            var logContent = File.ReadAllText(logFile);
            Assert.AreEqual("Error message" + Environment.NewLine, logContent);
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
    ///     Test that Context.WriteError sets ExitCode to 1.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_SetsExitCodeToOne()
    {
        // Create context
        using var context = Context.Create([]);

        // Verify initial exit code is 0
        Assert.AreEqual(0, context.ExitCode);

        // Capture console output to avoid displaying error
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write an error
            context.WriteError("Error message");

            // Verify exit code is now 1
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.ExitCode remains 0 when no errors are written.
    /// </summary>
    [TestMethod]
    public void Context_ExitCode_NoErrors_RemainsZero()
    {
        // Create context
        using var context = Context.Create([]);

        // Capture console output to avoid displaying messages
        using var output = new StringWriter();
        var originalOut = Console.Out;
        try
        {
            Console.SetOut(output);

            // Write some normal messages
            context.WriteLine("Message 1");
            context.WriteLine("Message 2");

            // Verify exit code remains 0
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    ///     Test that Context.Dispose closes log file properly.
    /// </summary>
    [TestMethod]
    public void Context_Dispose_ClosesLogFileProperly()
    {
        // Create temporary log file path
        var logFile = Path.GetTempFileName();
        try
        {
            // Create and dispose context with log file
            using (var context = Context.Create(["--log", logFile]))
            {
                context.WriteLine("Test message");
            }

            // Verify we can delete the log file (it's been closed)
            File.Delete(logFile);
            Assert.IsFalse(File.Exists(logFile));
        }
        finally
        {
            // Clean up log file if it still exists
            if (File.Exists(logFile))
            {
                File.Delete(logFile);
            }
        }
    }
}
