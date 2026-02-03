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

using System.Reflection;

namespace DemaConsulting.BuildMark;

/// <summary>
///     Main program entry point for the BuildMark tool.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Gets the application version string.
    /// </summary>
    public static string Version
    {
        get
        {
            // Get the assembly containing this program
            var assembly = typeof(Program).Assembly;

            // Try to get version from assembly attributes, fallback to AssemblyVersion, or default to 0.0.0
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "0.0.0";
        }
    }

    /// <summary>
    ///     Main entry point for the BuildMark tool.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>Exit code: 0 for success, non-zero for failure.</returns>
    private static int Main(string[] args)
    {
        try
        {
            // Create context from command-line arguments
            using var context = Context.Create(args);

            // Handle version display request
            if (context.Version)
            {
                context.WriteLine($"BuildMark version {Version}");
                return context.ExitCode;
            }

            // Handle help display request
            if (context.Help)
            {
                ShowHelp(context);
                return context.ExitCode;
            }

            // Display placeholder message for unhandled arguments
            context.WriteLine("Hello from BuildMark!");
            return context.ExitCode;
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    /// <summary>
    ///     Shows the help message.
    /// </summary>
    /// <param name="context">Context for output.</param>
    private static void ShowHelp(Context context)
    {
        context.WriteLine("BuildMark - Tool to generate Markdown Build Notes");
        context.WriteLine("");
        context.WriteLine("Usage: buildmark [options]");
        context.WriteLine("");
        context.WriteLine("Options:");
        context.WriteLine("  -v, --version                Display version information");
        context.WriteLine("  -?, -h, --help               Display this help message");
        context.WriteLine("  --silent                     Suppress console output");
        context.WriteLine("  --validate                   Run self-validation");
        context.WriteLine("  --results <file>             Write validation results (TRX or JUnit format)");
        context.WriteLine("  --log <file>                 Write output to log file");
        context.WriteLine("  --build-version <version>    Specify the build version");
        context.WriteLine("  --report <file>              Specify the report file name");
        context.WriteLine("  --report-depth <depth>       Specify the report markdown depth (default: 1)");
    }
}
