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

namespace DemaConsulting.BuildMark.Configuration;

/// <summary>
///     Represents the result of loading a BuildMark configuration file.
/// </summary>
public sealed record ConfigurationLoadResult(
    BuildMarkConfig? Config,
    IReadOnlyList<ConfigurationIssue> Issues)
{
    /// <summary>
    ///     Gets a value indicating whether any issue is an error.
    /// </summary>
    public bool HasErrors => Issues.Any(issue => issue.Severity == ConfigurationIssueSeverity.Error);

    /// <summary>
    ///     Reports all configuration issues to the supplied context.
    /// </summary>
    /// <param name="context">The destination context.</param>
    internal void ReportTo(Context context)
    {
        // Emit each issue using consistent file/line formatting.
        foreach (var issue in Issues)
        {
            var message = $"{issue.FilePath}:{issue.Line}: {issue.Severity}: {issue.Description}";
            if (issue.Severity == ConfigurationIssueSeverity.Error)
            {
                context.WriteError(message);
            }
            else
            {
                context.WriteLine(message);
            }
        }
    }
}
