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

using System.Globalization;

using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.BuildNotes;

/// <summary>
///     Represents build information for a release.
/// </summary>
/// <param name="BaselineVersionTag">Starting version tag (null if from beginning of history).</param>
/// <param name="CurrentVersionTag">Ending version tag.</param>
/// <param name="Changes">Non-bug changes performed between versions.</param>
/// <param name="Bugs">Bugs fixed between versions.</param>
/// <param name="KnownIssues">Known issues (unfixed or fixed but not in this build).</param>
/// <param name="CompleteChangelogLink">Optional link to the full changelog (null if not available).</param>
public record BuildInformation(
    VersionCommitTag? BaselineVersionTag,
    VersionCommitTag CurrentVersionTag,
    List<ItemInfo> Changes,
    List<ItemInfo> Bugs,
    List<ItemInfo> KnownIssues,
    WebLink? CompleteChangelogLink)
{
    /// <summary>
    ///     Markdown placeholder for empty sections.
    /// </summary>
    private const string NoItemsPlaceholder = "- N/A";
    /// <summary>
    ///     Gets or initializes the custom routed sections when rules are configured.
    ///     When non-null and non-empty, <see cref="ToMarkdown"/> renders these sections instead of the legacy
    ///     Changes/Bugs/KnownIssues sections.
    /// </summary>
    public IReadOnlyList<(string SectionId, string SectionTitle, IReadOnlyList<ItemInfo> Items)>? RoutedSections
    {
        get;
        init;
    }

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
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{heading} Build Report");
        markdown.AppendLine();

        // Add version information section
        AppendVersionInformation(markdown, subHeading);

        // Use custom routed sections if configured, otherwise fall back to legacy Changes/Bugs sections
        if (RoutedSections != null && RoutedSections.Count > 0)
        {
            // Render each configured section instead of the legacy sections
            AppendRoutedSections(markdown, subHeading);
        }
        else
        {
            // Add legacy changes section
            AppendChangesSection(markdown, subHeading);

            // Add legacy bugs fixed section
            AppendBugsFixedSection(markdown, subHeading);

            // Add known issues section if requested in legacy mode
            if (includeKnownIssues)
            {
                AppendKnownIssuesSection(markdown, subHeading);
            }
        }

        // Add full changelog section if link is available
        if (CompleteChangelogLink != null)
        {
            AppendCompleteChangelogSection(markdown, subHeading);
        }

        // Return the complete markdown report
        return markdown.ToString();
    }

    /// <summary>
    ///     Appends all routed sections to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendRoutedSections(System.Text.StringBuilder markdown, string subHeading)
    {
        // Render each configured section as a markdown heading with its item list
        // sectionId (first tuple element) is unused here; only title and items are rendered
        foreach (var (_, sectionTitle, items) in RoutedSections!)
        {
            // Add section heading
            markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} {sectionTitle}");
            markdown.AppendLine();

            // Add item bullets or N/A placeholder when the section is empty
            if (items.Count > 0)
            {
                // Render each item as a markdown link bullet
                foreach (var issue in items)
                {
                    markdown.AppendLine(CultureInfo.InvariantCulture, $"- [{issue.Id}]({issue.Url}) - {issue.Title}");
                }
            }
            else
            {
                markdown.AppendLine(NoItemsPlaceholder);
            }

            // Add blank line after section
            markdown.AppendLine();
        }
    }

    /// <summary>
    ///     Appends the version information section to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendVersionInformation(System.Text.StringBuilder markdown, string subHeading)
    {
        // Add version information section header and table structure
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} Version Information");
        markdown.AppendLine();
        markdown.AppendLine("| Field | Value |");
        markdown.AppendLine("|-------|-------|");
        markdown.AppendLine(CultureInfo.InvariantCulture, $"| **Version** | {CurrentVersionTag.VersionTag.Tag} |");
        markdown.AppendLine(CultureInfo.InvariantCulture, $"| **Commit Hash** | {CurrentVersionTag.CommitHash} |");

        // Add previous version information or N/A if this is the first release
        if (BaselineVersionTag != null)
        {
            markdown.AppendLine(CultureInfo.InvariantCulture, $"| **Previous Version** | {BaselineVersionTag.VersionTag.Tag} |");
            markdown.AppendLine(CultureInfo.InvariantCulture, $"| **Previous Commit Hash** | {BaselineVersionTag.CommitHash} |");
        }
        else
        {
            markdown.AppendLine("| **Previous Version** | N/A |");
            markdown.AppendLine("| **Previous Commit Hash** | N/A |");
        }

        // Add blank line after section
        markdown.AppendLine();
    }

    /// <summary>
    ///     Appends the changes section to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendChangesSection(System.Text.StringBuilder markdown, string subHeading)
    {
        // Add changes section header
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} Changes");
        markdown.AppendLine();

        // Add change items or N/A if no changes exist
        if (Changes.Count > 0)
        {
            foreach (var issue in Changes)
            {
                markdown.AppendLine(CultureInfo.InvariantCulture, $"- [{issue.Id}]({issue.Url}) - {issue.Title}");
            }
        }
        else
        {
            markdown.AppendLine(NoItemsPlaceholder);
        }

        // Add blank line after section
        markdown.AppendLine();
    }

    /// <summary>
    ///     Appends the bugs fixed section to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendBugsFixedSection(System.Text.StringBuilder markdown, string subHeading)
    {
        // Add bugs fixed section header
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} Bugs Fixed");
        markdown.AppendLine();

        // Add bug items or N/A if no bugs were fixed
        if (Bugs.Count > 0)
        {
            foreach (var issue in Bugs)
            {
                markdown.AppendLine(CultureInfo.InvariantCulture, $"- [{issue.Id}]({issue.Url}) - {issue.Title}");
            }
        }
        else
        {
            markdown.AppendLine(NoItemsPlaceholder);
        }

        // Add blank line after section
        markdown.AppendLine();
    }

    /// <summary>
    ///     Appends the known issues section to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendKnownIssuesSection(System.Text.StringBuilder markdown, string subHeading)
    {
        // Add known issues section header
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} Known Issues");
        markdown.AppendLine();

        // Add known issue items or N/A if no known issues exist
        if (KnownIssues.Count > 0)
        {
            foreach (var issue in KnownIssues)
            {
                markdown.AppendLine(CultureInfo.InvariantCulture, $"- [{issue.Id}]({issue.Url}) - {issue.Title}");
            }
        }
        else
        {
            markdown.AppendLine(NoItemsPlaceholder);
        }

        // Add blank line after section
        markdown.AppendLine();
    }

    /// <summary>
    ///     Appends the full changelog section to the markdown report.
    /// </summary>
    /// <param name="markdown">StringBuilder containing the markdown report.</param>
    /// <param name="subHeading">Sub-heading prefix.</param>
    private void AppendCompleteChangelogSection(System.Text.StringBuilder markdown, string subHeading)
    {
        // Add full changelog section header and link
        markdown.AppendLine(CultureInfo.InvariantCulture, $"{subHeading} Full Changelog");
        markdown.AppendLine();
        markdown.AppendLine(CultureInfo.InvariantCulture, $"See the full changelog at [{CompleteChangelogLink!.LinkText}]({CompleteChangelogLink.TargetUrl}).");
        markdown.AppendLine();
    }
}
