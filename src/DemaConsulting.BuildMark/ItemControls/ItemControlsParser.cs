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

using System.Text.RegularExpressions;

namespace DemaConsulting.BuildMark.ItemControls;

/// <summary>
///     Parses item controls from issue or pull request descriptions.
/// </summary>
public static class ItemControlsParser
{
    /// <summary>
    ///     Regex pattern for matching HTML comment delimiters.
    ///     Strips only the comment delimiters (not content), so that a buildmark block
    ///     wrapped in an HTML comment is still visible to the parser after stripping.
    /// </summary>
    private static readonly Regex HtmlCommentDelimitersRegex = new(@"<!--|-->", RegexOptions.Compiled);

    /// <summary>
    ///     Valid visibility value: public.
    /// </summary>
    private const string VisibilityPublic = "public";

    /// <summary>
    ///     Valid visibility value: internal.
    /// </summary>
    private const string VisibilityInternal = "internal";

    /// <summary>
    ///     Valid type value: bug.
    /// </summary>
    private const string TypeBug = "bug";

    /// <summary>
    ///     Valid type value: feature.
    /// </summary>
    private const string TypeFeature = "feature";

    /// <summary>
    ///     Parses item controls from a description string.
    /// </summary>
    /// <param name="description">Issue or pull request description text.</param>
    /// <returns>Parsed ItemControlsInfo, or null if no buildmark block found or no recognized keys.</returns>
    public static ItemControlsInfo? Parse(string? description)
    {
        // Return null if description is null or empty
        if (string.IsNullOrEmpty(description))
        {
            return null;
        }

        // Strip HTML comment delimiters (<!-- and -->) so that a buildmark block
        // wrapped in an HTML comment is exposed and can be detected by the parser
        var stripped = HtmlCommentDelimitersRegex.Replace(description, string.Empty);

        // Locate buildmark code fence: line starting with 3+ backticks followed by "buildmark"
        // The fence delimiter is 3+ backticks followed immediately by "buildmark" (case-insensitive)
        var lines = stripped.Split('\n');
        string? fenceDelimiter = null;
        int blockStart = -1;

        for (var i = 0; i < lines.Length; i++)
        {
            var trimmed = lines[i].TrimEnd('\r').Trim();

            if (fenceDelimiter == null)
            {
                // Looking for opening fence
                if (IsOpeningFence(trimmed, out var fence))
                {
                    fenceDelimiter = fence;
                    blockStart = i + 1;
                }
            }
            else
            {
                // Looking for closing fence (same number of backticks, nothing else)
                if (trimmed == fenceDelimiter)
                {
                    // Found the closing fence - process lines from blockStart to i-1
                    var blockLines = lines[blockStart..i];
                    return ParseBlockLines(blockLines);
                }
            }
        }

        // Return null if no fence found
        return null;
    }

    /// <summary>
    ///     Checks if a line is an opening buildmark fence.
    /// </summary>
    /// <param name="line">Trimmed line to check.</param>
    /// <param name="fence">Output fence delimiter (e.g., "```").</param>
    /// <returns>True if the line is a buildmark fence opener.</returns>
    private static bool IsOpeningFence(string line, out string fence)
    {
        fence = string.Empty;

        // Count leading backticks (minimum 3)
        var count = 0;
        while (count < line.Length && line[count] == '`')
        {
            count++;
        }

        if (count < 3)
        {
            return false;
        }

        // Check remaining text is "buildmark" (case-insensitive)
        var remainder = line[count..].Trim();
        if (!remainder.Equals("buildmark", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        // Set fence to the backtick sequence only
        fence = new string('`', count);
        return true;
    }

    /// <summary>
    ///     Parses key-value lines from a buildmark block body.
    /// </summary>
    /// <param name="blockLines">Lines from inside the buildmark fence.</param>
    /// <returns>Parsed ItemControlsInfo, or null if no recognized keys found.</returns>
    private static ItemControlsInfo? ParseBlockLines(string[] blockLines)
    {
        string? visibility = null;
        string? type = null;
        VersionIntervalSet? affectedVersions = null;

        // Parse each line as "key: value" (split on first ':')
        foreach (var rawLine in blockLines)
        {
            var line = rawLine.TrimEnd('\r').Trim();
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            // Split on first ':'
            var colonIdx = line.IndexOf(':');
            if (colonIdx < 0)
            {
                continue;
            }

            var key = line[..colonIdx].Trim().ToLowerInvariant();
            var value = line[(colonIdx + 1)..].Trim();

            // Keys: "visibility" → "public"/"internal"
            if (key == "visibility" && (value == VisibilityPublic || value == VisibilityInternal))
            {
                visibility = value;
            }
            // type → "bug"/"feature"
            else if (key == "type" && (value == TypeBug || value == TypeFeature))
            {
                type = value;
            }
            // affected-versions
            else if (key == "affected-versions" && !string.IsNullOrEmpty(value))
            {
                affectedVersions = VersionIntervalSet.Parse(value);
            }
            // Unknown keys/values silently ignored
        }

        // Return ItemControlsInfo if any recognized key found, else null
        if (visibility == null && type == null && affectedVersions == null)
        {
            return null;
        }

        return new ItemControlsInfo(visibility, type, affectedVersions);
    }
}
