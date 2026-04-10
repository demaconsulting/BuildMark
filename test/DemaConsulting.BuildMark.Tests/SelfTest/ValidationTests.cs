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
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.SelfTest;

namespace DemaConsulting.BuildMark.Tests.SelfTest;

/// <summary>
///     Tests for the Validation class.
/// </summary>
[TestClass]
public class ValidationTests
{
    /// <summary>
    ///     Test that Validation.Run writes TRX results file when specified.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithTrxResultsFile_WritesTrxFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var trxFile = Path.Combine(tempDir, "results.trx");
            var args = new[] { "--validate", "--results", trxFile };

            using var outputWriter = new StringWriter();
            using var errorWriter = new StringWriter();

            var originalOut = Console.Out;
            var originalError = Console.Error;
            try
            {
                // Capture console output
                Console.SetOut(outputWriter);
                Console.SetError(errorWriter);

                // Act
                using var context = Context.Create(args, () => new MockRepoConnector());
                Validation.Run(context);

                // Assert - Verify TRX file was created
                Assert.IsTrue(File.Exists(trxFile), "TRX file should be created");

                // Verify TRX file contains expected content
                var trxContent = File.ReadAllText(trxFile);
                Assert.Contains("TestRun", trxContent);
                Assert.Contains("BuildMark Self-Validation", trxContent);
            }
            finally
            {
                // Restore console output
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that Validation.Run writes JUnit XML results file when specified.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithXmlResultsFile_WritesJUnitFile()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var xmlFile = Path.Combine(tempDir, "results.xml");
            var args = new[] { "--validate", "--results", xmlFile };

            using var outputWriter = new StringWriter();
            using var errorWriter = new StringWriter();

            var originalOut = Console.Out;
            var originalError = Console.Error;
            try
            {
                // Capture console output
                Console.SetOut(outputWriter);
                Console.SetError(errorWriter);

                // Act
                using var context = Context.Create(args, () => new MockRepoConnector());
                Validation.Run(context);

                // Assert - Verify XML file was created
                Assert.IsTrue(File.Exists(xmlFile), "XML file should be created");

                // Verify XML file contains expected content
                var xmlContent = File.ReadAllText(xmlFile);
                Assert.Contains("testsuites", xmlContent);
                Assert.Contains("BuildMark Self-Validation", xmlContent);
            }
            finally
            {
                // Restore console output
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that Validation.Run handles unsupported results file extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithUnsupportedResultsFileExtension_ShowsError()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var unsupportedFile = Path.Combine(tempDir, "results.json");
            var args = new[] { "--validate", "--results", unsupportedFile };

            using var errorWriter = new StringWriter();

            var originalError = Console.Error;
            try
            {
                // Capture console error output
                Console.SetError(errorWriter);

                // Act
                using var context = Context.Create(args, () => new MockRepoConnector());
                Validation.Run(context);

                // Assert - Verify error message in error output (WriteError writes to Console.Error)
                var output = errorWriter.ToString();
                Assert.Contains("Unsupported results file format", output);
            }
            finally
            {
                // Restore console error output
                Console.SetError(originalError);
            }
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that Validation.Run handles write failure for results file.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithInvalidResultsFilePath_ShowsError()
    {
        // Arrange
        var invalidPath = Path.Combine("/invalid_path_that_does_not_exist_12345678", "results.trx");
        var args = new[] { "--validate", "--results", invalidPath };

        using var errorWriter = new StringWriter();

        var originalError = Console.Error;
        try
        {
            // Capture console error output
            Console.SetError(errorWriter);

            // Act
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert - Verify error message in error output (WriteError writes to Console.Error)
            var output = errorWriter.ToString();
            Assert.Contains("Failed to write results file", output);
        }
        finally
        {
            // Restore console error output
            Console.SetError(originalError);
        }
    }

    /// <summary>
    ///     Test that Validation.Run completes successfully when no results file is specified.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithoutResultsFile_CompletesSuccessfully()
    {
        // Arrange - no --results argument
        var args = new[] { "--validate", "--silent" };

        // Act
        using var context = Context.Create(args, () => new MockRepoConnector());
        Validation.Run(context);

        // Assert - validation should complete without error
        Assert.AreEqual(0, context.ExitCode, "Validation should succeed with exit code 0");
    }
}



