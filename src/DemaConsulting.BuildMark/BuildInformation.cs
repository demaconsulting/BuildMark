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

namespace DemaConsulting.BuildMark;

/// <summary>
///     Represents build information for a release.
/// </summary>
/// <param name="FromVersion">Starting version (null if from beginning of history).</param>
/// <param name="ToVersion">Ending version.</param>
/// <param name="FromHash">Starting git hash (null if from beginning of history).</param>
/// <param name="ToHash">Ending git hash.</param>
/// <param name="Changes">Non-bug changes performed between versions.</param>
/// <param name="Bugs">Bugs fixed between versions.</param>
/// <param name="KnownIssues">Known issues (unfixed or fixed but not in this build).</param>
public record BuildInformation(
    Version? FromVersion,
    Version ToVersion,
    string? FromHash,
    string ToHash,
    List<ItemInfo> Changes,
    List<ItemInfo> Bugs,
    List<ItemInfo> KnownIssues)
{
    /// <summary>
    ///     Generates a Markdown build report from this build information.
    /// </summary>
    /// <param name="headingDepth">Root markdown heading depth (default 1).</param>
    /// <param name="includeKnownIssues">Flag for whether to include known issues (default false).</param>
    /// <returns>Markdown-formatted build report.</returns>
    public string ToMarkdown(int headingDepth = 1, bool includeKnownIssues = false)
    {
        // Build heading prefix based on requested depth
        var heading = new string('#', headingDepth);
        var subHeading = new string('#', headingDepth + 1);

        // Start building the markdown report
        var markdown = new System.Text.StringBuilder();

        // Add title section
        markdown.AppendLine($"{heading} Build Report");
        markdown.AppendLine();

        // Add version information section
        markdown.AppendLine($"{subHeading} Version Information");
        markdown.AppendLine();
        markdown.AppendLine("| Field | Value |");
        markdown.AppendLine("|-------|-------|");
        markdown.AppendLine($"| **Version** | {ToVersion.Tag} |");
        markdown.AppendLine($"| **Commit Hash** | {ToHash} |");
        if (FromVersion != null)
        {
            markdown.AppendLine($"| **Previous Version** | {FromVersion.Tag} |");
            markdown.AppendLine($"| **Previous Commit Hash** | {FromHash} |");
        }
        else
        {
            markdown.AppendLine("| **Previous Version** | N/A |");
            markdown.AppendLine("| **Previous Commit Hash** | N/A |");
        }
        markdown.AppendLine();

        // Add changes section
        markdown.AppendLine($"{subHeading} Changes");
        markdown.AppendLine();
        markdown.AppendLine("| Issue | Title |");
        markdown.AppendLine("|-------|-------|");
        if (Changes.Count > 0)
        {
            foreach (var issue in Changes)
            {
                markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
            }
        }
        else
        {
            markdown.AppendLine("| N/A | N/A |");
        }
        markdown.AppendLine();

        // Add bugs fixed section
        markdown.AppendLine($"{subHeading} Bugs Fixed");
        markdown.AppendLine();
        markdown.AppendLine("| Issue | Title |");
        markdown.AppendLine("|-------|-------|");
        if (Bugs.Count > 0)
        {
            foreach (var issue in Bugs)
            {
                markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
            }
        }
        else
        {
            markdown.AppendLine("| N/A | N/A |");
        }
        markdown.AppendLine();

        // Add known issues section if requested
        if (includeKnownIssues)
        {
            markdown.AppendLine($"{subHeading} Known Issues");
            markdown.AppendLine();
            markdown.AppendLine("| Issue | Title |");
            markdown.AppendLine("|-------|-------|");
            if (KnownIssues.Count > 0)
            {
                foreach (var issue in KnownIssues)
                {
                    markdown.AppendLine($"| [{issue.Id}]({issue.Url}) | {issue.Title} |");
                }
            }
            else
            {
                markdown.AppendLine("| N/A | N/A |");
            }
            markdown.AppendLine();
        }

        // Return the complete markdown report
        return markdown.ToString();
    }
}
