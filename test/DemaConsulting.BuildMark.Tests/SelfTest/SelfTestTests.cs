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
///     Subsystem-level tests for the SelfTest subsystem.
/// </summary>
[TestClass]
public class SelfTestTests
{
    /// <summary>
    ///     Test that the SelfTest subsystem writes TRX results when --validate and --results are specified with a .trx file.
    /// </summary>
    [TestMethod]
    public void SelfTest_Validation_WithTrxFile_WritesResults()
    {
        // Arrange: create a temporary directory and define a TRX results file path
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_subsystem_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var trxFile = Path.Combine(tempDir, "results.trx");
            var args = new[] { "--validate", "--results", trxFile, "--silent" };

            // Act: run the validation subsystem
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert: TRX file was created and contains expected content
            Assert.IsTrue(File.Exists(trxFile), "TRX file should be created");
            var trxContent = File.ReadAllText(trxFile);
            Assert.Contains("TestRun", trxContent);
            Assert.Contains("BuildMark Self-Validation", trxContent);
        }
        finally
        {
            // Cleanup temporary directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that the SelfTest subsystem writes JUnit XML results when --validate and --results are specified with an .xml file.
    /// </summary>
    [TestMethod]
    public void SelfTest_Validation_WithXmlFile_WritesResults()
    {
        // Arrange: create a temporary directory and define an XML results file path
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_subsystem_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var xmlFile = Path.Combine(tempDir, "results.xml");
            var args = new[] { "--validate", "--results", xmlFile, "--silent" };

            // Act: run the validation subsystem
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert: XML file was created and contains expected content
            Assert.IsTrue(File.Exists(xmlFile), "XML file should be created");
            var xmlContent = File.ReadAllText(xmlFile);
            Assert.Contains("testsuites", xmlContent);
            Assert.Contains("BuildMark Self-Validation", xmlContent);
        }
        finally
        {
            // Cleanup temporary directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that the SelfTest subsystem creates a TRX results output file.
    /// </summary>
    [TestMethod]
    public void SelfTest_ResultsOutput_WithTrxFile_CreatesFile()
    {
        // Arrange: create a temporary directory and define a TRX results file path
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_subsystem_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var trxFile = Path.Combine(tempDir, "output.trx");
            var args = new[] { "--validate", "--results", trxFile, "--silent" };

            // Act: run validation and check results file creation
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert: results file exists and has non-zero content
            Assert.IsTrue(File.Exists(trxFile), "TRX results file should be created");
            var fileInfo = new FileInfo(trxFile);
            Assert.IsGreaterThan(0, fileInfo.Length, "TRX results file should have content");
        }
        finally
        {
            // Cleanup temporary directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that the SelfTest subsystem creates a JUnit XML results output file.
    /// </summary>
    [TestMethod]
    public void SelfTest_ResultsOutput_WithXmlFile_CreatesFile()
    {
        // Arrange: create a temporary directory and define an XML results file path
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_subsystem_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var xmlFile = Path.Combine(tempDir, "output.xml");
            var args = new[] { "--validate", "--results", xmlFile, "--silent" };

            // Act: run validation and check results file creation
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert: results file exists and has non-zero content
            Assert.IsTrue(File.Exists(xmlFile), "XML results file should be created");
            var fileInfo = new FileInfo(xmlFile);
            Assert.IsGreaterThan(0, fileInfo.Length, "XML results file should have content");
        }
        finally
        {
            // Cleanup temporary directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    /// <summary>
    ///     Test that the SelfTest subsystem completes self-validation without error when no --results file is specified.
    /// </summary>
    [TestMethod]
    public void SelfTest_Qualification_WithoutResultsFile_Succeeds()
    {
        // Arrange: create a temporary directory and define a log file path (no results file)
        var tempDir = Path.Combine(Path.GetTempPath(), $"buildmark_subsystem_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(tempDir);

        try
        {
            var logFile = Path.Combine(tempDir, "validation.log");
            var args = new[] { "--validate", "--log", logFile, "--silent" };

            // Act: run the validation subsystem without specifying --results
            using var context = Context.Create(args, () => new MockRepoConnector());
            Validation.Run(context);

            // Assert: validation ran and produced log output; no results file was created
            Assert.IsTrue(File.Exists(logFile), "Log file should be created");
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("BuildMark Self-validation", logContent);
            Assert.Contains("Total Tests:", logContent);
        }
        finally
        {
            // Cleanup temporary directory
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
