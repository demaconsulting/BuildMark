using System;
using System.Diagnostics;
using System.Threading.Tasks;

public class TestStderr
{
    public static async Task Main()
    {
        Console.WriteLine("Testing stderr deadlock scenario...");
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "bash",
            Arguments = "-c 'for i in {1..10000}; do echo \"error line $i\" >&2; done; echo \"stdout done\"'",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = startInfo };
        process.Start();
        
        // This pattern from TryRunAsync - reads stdout before waiting
        var output = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();
        
        Console.WriteLine($"Success! Output: {output.Substring(0, Math.Min(50, output.Length))}");
    }
}
