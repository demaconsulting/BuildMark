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
        // Ensure we're using just the filename to avoid absolute path issues
        var dllFileName = "DemaConsulting.BuildMark.dll";
        _dllPath = Path.Combine(baseDir, dllFileName);

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
}
