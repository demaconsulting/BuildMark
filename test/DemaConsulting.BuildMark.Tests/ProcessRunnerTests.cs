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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the ProcessRunner class.
/// </summary>
[TestClass]
public class ProcessRunnerTests
{
    /// <summary>
    ///     Test that TryRunAsync returns output when command succeeds.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.TryRunAsync with a valid command
    ///     What the assertions prove: The method returns the command output when successful
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_TryRunAsync_WithValidCommand_ReturnsOutput()
    {
        // Arrange - Set up a simple echo command that will succeed
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "echo";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c echo test" : "test";

        // Act - Execute the command
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert - Verify output is returned and contains expected text
        Assert.IsNotNull(result, "TryRunAsync should return output for successful command");
        Assert.IsTrue(result.Contains("test", StringComparison.OrdinalIgnoreCase), 
            "Output should contain the echoed text");
    }

    /// <summary>
    ///     Test that TryRunAsync returns null when command does not exist.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.TryRunAsync with an invalid command
    ///     What the assertions prove: The method returns null when the command cannot be found
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_TryRunAsync_WithInvalidCommand_ReturnsNull()
    {
        // Arrange - Use a command that definitely doesn't exist
        var command = "nonexistent_command_12345678";
        var arguments = "";

        // Act - Attempt to execute the non-existent command
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert - Verify null is returned for invalid command
        Assert.IsNull(result, "TryRunAsync should return null for non-existent command");
    }

    /// <summary>
    ///     Test that TryRunAsync returns null when command exits with non-zero code.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.TryRunAsync with a command that fails
    ///     What the assertions prove: The method returns null when exit code is non-zero
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_TryRunAsync_WithNonZeroExitCode_ReturnsNull()
    {
        // Arrange - Set up a command that will fail with non-zero exit code
        // On Windows: "cmd /c exit 1" exits with code 1
        // On Unix: "sh -c 'exit 1'" exits with code 1
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c exit 1" : "-c 'exit 1'";

        // Act - Execute the command that will fail
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert - Verify null is returned for failed command
        Assert.IsNull(result, "TryRunAsync should return null when command exits with non-zero code");
    }

    /// <summary>
    ///     Test that TryRunAsync handles exceptions gracefully and returns null.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.TryRunAsync exception handling
    ///     What the assertions prove: The method catches exceptions and returns null instead of throwing
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_TryRunAsync_WithException_ReturnsNull()
    {
        // Arrange - Use a command with malformed arguments that may cause issues
        // Using an empty string as command which should cause an exception
        var command = "";
        var arguments = "";

        // Act - Attempt to execute with problematic input
        var result = await ProcessRunner.TryRunAsync(command, arguments);

        // Assert - Verify null is returned when exception occurs
        Assert.IsNull(result, "TryRunAsync should return null when exception occurs");
    }

    /// <summary>
    ///     Test that RunAsync returns output when command succeeds.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.RunAsync with a valid command
    ///     What the assertions prove: The method returns the command output when successful
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_RunAsync_WithValidCommand_ReturnsOutput()
    {
        // Arrange - Set up a simple echo command that will succeed
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "echo";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c echo test123" : "test123";

        // Act - Execute the command
        var result = await ProcessRunner.RunAsync(command, arguments);

        // Assert - Verify output is returned and contains expected text
        Assert.IsNotNull(result, "RunAsync should return output for successful command");
        Assert.IsTrue(result.Contains("test123", StringComparison.OrdinalIgnoreCase),
            "Output should contain the echoed text");
    }

    /// <summary>
    ///     Test that RunAsync throws exception when command fails.
    /// </summary>
    /// <remarks>
    ///     What is being tested: ProcessRunner.RunAsync error handling
    ///     What the assertions prove: The method throws InvalidOperationException with details when command fails
    /// </remarks>
    [TestMethod]
    public async Task ProcessRunner_RunAsync_WithFailingCommand_ThrowsException()
    {
        // Arrange - Set up a command that will fail
        var command = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "cmd" : "sh";
        var arguments = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "/c exit 1" : "-c 'exit 1'";

        // Act & Assert - Verify exception is thrown and inspect it
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await ProcessRunner.RunAsync(command, arguments));

        // Assert - Verify exception message contains useful information
        Assert.Contains("failed with exit code", exception.Message);
    }
}
