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

            // Run the program logic
            Run(context);

            // Return the exit code from the context
            return context.ExitCode;
        }
        catch (ArgumentException ex)
        {
            // Print expected argument exceptions and return error code
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            // Print expected operation exceptions and return error code
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            // Print unexpected exceptions and re-throw to generate event logs
            Console.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Runs the program logic based on the provided context.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Priority 1: Version query
        if (context.Version)
        {
            Console.WriteLine(Version);
            return;
        }

        // Print application banner
        PrintBanner(context);

        // Priority 2: Help
        if (context.Help)
        {
            PrintHelp(context);
            return;
        }

        // Priority 3: Self-Validation
        if (context.Validate)
        {
            Validation.Run(context);
            return;
        }

        // Priority 4: Build notes processing
        ProcessBuildNotes(context);
    }

    /// <summary>
    ///     Prints the application banner.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintBanner(Context context)
    {
        context.WriteLine($"BuildMark version {Version}");
        context.WriteLine("Copyright (c) DEMA Consulting");
        context.WriteLine("");
    }

    /// <summary>
    ///     Prints usage information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintHelp(Context context)
    {
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

    /// <summary>
    ///     Processes build notes and generates markdown output.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    private static void ProcessBuildNotes(Context context)
    {
        // Create repository connector
        var connector = RepoConnectorFactory.Create();

        // Parse build version if provided
        DemaConsulting.BuildMark.Version? buildVersion = null;
        if (context.BuildVersion != null)
        {
            try
            {
                buildVersion = DemaConsulting.BuildMark.Version.Create(context.BuildVersion);
            }
            catch (ArgumentException)
            {
                context.WriteError($"Error: Invalid build version format: {context.BuildVersion}");
                return;
            }
        }

        // Create build information
        context.WriteLine("Generating build information...");
        BuildInformation buildInfo;
        try
        {
            buildInfo = connector.GetBuildInformationAsync(buildVersion).GetAwaiter().GetResult();
        }
        catch (InvalidOperationException ex)
        {
            context.WriteError($"Error: {ex.Message}");
            return;
        }

        // Display build information summary
        context.WriteLine($"Build Version: {buildInfo.ToVersion.Tag}");
        context.WriteLine($"Commit Hash: {buildInfo.ToHash}");
        if (buildInfo.FromVersion != null)
        {
            context.WriteLine($"Previous Version: {buildInfo.FromVersion.Tag}");
            context.WriteLine($"Previous Commit Hash: {buildInfo.FromHash}");
        }
        context.WriteLine($"Changes: {buildInfo.Changes.Count}");
        context.WriteLine($"Bugs Fixed: {buildInfo.Bugs.Count}");
        context.WriteLine($"Known Issues: {buildInfo.KnownIssues.Count}");

        // Export markdown report if requested
        if (context.ReportFile != null)
        {
            context.WriteLine($"Writing build report to {context.ReportFile}...");
            try
            {
                var markdown = buildInfo.ToMarkdown(context.ReportDepth);
                File.WriteAllText(context.ReportFile, markdown);
                context.WriteLine("Build report generated successfully.");
            }
            // Generic catch is justified here to handle any file system exception gracefully
            // and allow the program to continue execution rather than crashing
            catch (Exception ex)
            {
                context.WriteError($"Error: Failed to write report: {ex.Message}");
            }
        }
    }
}
