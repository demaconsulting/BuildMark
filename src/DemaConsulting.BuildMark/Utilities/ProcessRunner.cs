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

using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace DemaConsulting.BuildMark.Utilities;

/// <summary>
///     Helper class for running external processes and capturing output.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>
    ///     Runs a command and returns its output.
    /// </summary>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <returns>Command output.</returns>
    /// <exception cref="InvalidOperationException">Thrown when command fails or is not found.</exception>
    public static async Task<string> RunAsync(string command, params string[] arguments)
    {
        // Configure process to capture output, routing through cmd on Windows
        var startInfo = CreateStartInfo(command, arguments);

        // Initialize process with output and error buffers
        using var process = new Process { StartInfo = startInfo };
        var output = new StringBuilder();
        var error = new StringBuilder();

        // Set up handlers to capture output and error streams
        process.OutputDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                output.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data != null)
            {
                error.AppendLine(e.Data);
            }
        };

        // Start process and begin reading streams, catching Win32Exception for missing commands
        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException(
                $"Command '{command}' was not found or could not be started. " +
                "Ensure it is installed and available in the system PATH.",
                ex);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        // Throw exception if command failed
        if (process.ExitCode != 0)
        {
            var args = string.Join(" ", arguments);
            throw new InvalidOperationException(
                $"Command '{command} {args}' failed with exit code {process.ExitCode}: {error}");
        }

        // Return captured output
        return output.ToString().TrimEnd();
    }

    /// <summary>
    ///     Tries to run a command and returns its output.
    /// </summary>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <returns>Command output, or null if the command fails.</returns>
    public static async Task<string?> TryRunAsync(string command, params string[] arguments)
    {
        try
        {
            // Configure process to capture output, routing through cmd on Windows
            var startInfo = CreateStartInfo(command, arguments);

            // Execute process and capture output
            using var process = new Process { StartInfo = startInfo };
            process.Start();
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();
            await Task.WhenAll(outputTask, errorTask);
            await process.WaitForExitAsync();

            // Return trimmed output only if command succeeded
            return process.ExitCode == 0 ? outputTask.Result.TrimEnd() : null;
        }
        catch
        {
            // Return null on any error
            return null;
        }
    }

    /// <summary>
    ///     Creates a <see cref="ProcessStartInfo" /> for the given command.
    /// </summary>
    /// <remarks>
    ///     On Windows, commands are routed through <c>cmd /c</c> so that
    ///     <c>.cmd</c> and <c>.bat</c> scripts (such as the Azure CLI) are
    ///     resolved correctly. The <see cref="ProcessStartInfo.ArgumentList" />
    ///     collection is used so that the Process class handles argument quoting
    ///     correctly.
    /// </remarks>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <returns>Configured <see cref="ProcessStartInfo" />.</returns>
    private static ProcessStartInfo CreateStartInfo(string command, string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // On Windows, route non-empty commands through cmd.exe so .cmd/.bat scripts are found
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && !string.IsNullOrWhiteSpace(command))
        {
            startInfo.FileName = "cmd";
            startInfo.ArgumentList.Add("/c");
            startInfo.ArgumentList.Add(command);
        }
        else
        {
            // On non-Windows platforms, invoke the command directly
            startInfo.FileName = command;
        }

        // Add all arguments using ArgumentList for correct quoting
        foreach (var arg in arguments)
        {
            startInfo.ArgumentList.Add(arg);
        }

        return startInfo;
    }
}
