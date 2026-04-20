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
using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.Tests.Utilities;

/// <summary>
///     Subsystem-level tests for the Utilities subsystem.
/// </summary>
[TestClass]
public class UtilitiesTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Utilities-SafePaths
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Utilities subsystem correctly combines valid relative paths.
    /// </summary>
    [TestMethod]
    public void Utilities_SafePaths_ValidPaths_CombinesCorrectly()
    {
        // Arrange: define a base path and relative path
        var basePath = Path.GetTempPath();
        var relativePath = "subdir";

        // Act: combine the paths
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: combined path ends with the relative portion
        Assert.IsTrue(result.EndsWith("subdir", StringComparison.Ordinal));
    }

    /// <summary>
    ///     Test that the Utilities subsystem rejects path traversal sequences.
    /// </summary>
    [TestMethod]
    public void Utilities_SafePaths_TraversalPath_ThrowsException()
    {
        // Arrange: define a base path and a traversal relative path
        var basePath = Path.Combine(Path.GetTempPath(), "safe");

        // Act & Assert: path traversal is rejected
        Assert.Throws<ArgumentException>(
            () => PathHelpers.SafePathCombine(basePath, "../../etc/passwd"));
    }

    /// <summary>
    ///     Test that the Utilities subsystem rejects absolute paths as relative components.
    /// </summary>
    [TestMethod]
    public void Utilities_SafePaths_AbsolutePath_ThrowsException()
    {
        // Arrange: define a base path and an absolute relative path
        var basePath = Path.GetTempPath();
        var absolutePath = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? @"C:\Windows\System32"
            : "/etc/passwd";

        // Act & Assert: absolute path is rejected
        Assert.Throws<ArgumentException>(
            () => PathHelpers.SafePathCombine(basePath, absolutePath));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Utilities-ProcessRunner
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Utilities subsystem runs a valid command and returns output.
    /// </summary>
    [TestMethod]
    public async Task Utilities_ProcessRunner_ValidCommand_ReturnsOutput()
    {
        // Arrange: choose a portable echo command
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "echo";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "/c", "echo", "subsystem_test" }
            : new[] { "subsystem_test" };

        // Act: run the command
        var result = await ProcessRunner.RunAsync(command, arguments);

        // Assert: output contains the expected text
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Contains("subsystem_test", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Test that the Utilities subsystem returns null for an invalid command.
    /// </summary>
    [TestMethod]
    public async Task Utilities_ProcessRunner_InvalidCommand_ReturnsNull()
    {
        // Arrange & Act: run a non-existent command
        var result = await ProcessRunner.TryRunAsync("nonexistent_utility_test_12345");

        // Assert: null is returned
        Assert.IsNull(result);
    }

    /// <summary>
    ///     Test that the Utilities subsystem throws an exception for a failing command.
    /// </summary>
    [TestMethod]
    public async Task Utilities_ProcessRunner_FailingCommand_ThrowsException()
    {
        // Arrange: a command that exits with code 1
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? new[] { "/c", "exit", "1" }
            : new[] { "-c", "exit 1" };

        // Act & Assert: InvalidOperationException is thrown
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await ProcessRunner.RunAsync(command, arguments));
        Assert.Contains("failed with exit code", exception.Message);
    }

}


