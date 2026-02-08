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

using DemaConsulting.BuildMark.RepoConnectors;

namespace DemaConsulting.BuildMark.Tests;

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

            StringWriter? outputWriter = null;
            StringWriter? errorWriter = null;

            try
            {
                // Capture console output
                outputWriter = new StringWriter();
                errorWriter = new StringWriter();
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
                var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(standardOutput);
                var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                Console.SetError(standardError);

                outputWriter?.Dispose();
                errorWriter?.Dispose();
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

            StringWriter? outputWriter = null;
            StringWriter? errorWriter = null;

            try
            {
                // Capture console output
                outputWriter = new StringWriter();
                errorWriter = new StringWriter();
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
                var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(standardOutput);
                var standardError = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                Console.SetError(standardError);

                outputWriter?.Dispose();
                errorWriter?.Dispose();
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

            StringWriter? outputWriter = null;

            try
            {
                // Capture console output
                outputWriter = new StringWriter();
                Console.SetOut(outputWriter);

                // Act
                using var context = Context.Create(args, () => new MockRepoConnector());
                Validation.Run(context);

                // Assert - Verify error message in output (WriteError writes to Console.WriteLine)
                var output = outputWriter.ToString();
                Assert.Contains("Unsupported results file format", output);
            }
            finally
            {
                // Restore console output
                var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(standardOutput);

                outputWriter?.Dispose();
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

        StringWriter? outputWriter = null;

        try
        {
            // Capture console output
            outputWriter = new StringWriter();
            Console.SetOut(outputWriter);

            // Act
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert - Verify error message in output (WriteError writes to Console.WriteLine)
            var output = outputWriter.ToString();
            Assert.Contains("Failed to write results file", output);
        }
        finally
        {
            // Restore console output
            var standardOutput = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
            Console.SetOut(standardOutput);

            outputWriter?.Dispose();
        }
    }
}
