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

namespace DemaConsulting.BuildMark.Version;

/// <summary>
///     Represents a set of version intervals.
/// </summary>
/// <param name="Intervals">Read-only list of version intervals.</param>
public record VersionIntervalSet(IReadOnlyList<VersionInterval> Intervals)
{
    /// <summary>
    ///     Determines whether a candidate semantic version falls within any interval in the set.
    /// </summary>
    /// <param name="version">Semantic version text to evaluate.</param>
    /// <returns>True when the version is within any interval; otherwise false.</returns>
    public bool Contains(string version)
    {
        return Intervals.Any(interval => interval.Contains(version));
    }

    /// <summary>
    ///     Determines whether a candidate VersionComparable falls within any interval in the set.
    /// </summary>
    /// <param name="version">VersionComparable to evaluate.</param>
    /// <returns>True when the version is within any interval; otherwise false.</returns>
    public bool Contains(VersionComparable version)
    {
        return Intervals.Any(interval => interval.Contains(version));
    }

    /// <summary>
    ///     Determines whether a candidate BuildMark version falls within any interval in the set.
    /// </summary>
    /// <param name="version">BuildMark version to evaluate.</param>
    /// <returns>True when the version is within any interval; otherwise false.</returns>
    public bool Contains(VersionTag version)
    {
        return Contains(version.Numbers);
    }

    /// <summary>
    ///     Parses a version interval set from text.
    /// </summary>
    /// <param name="text">Text to parse (e.g., "(,1.0.1],[1.1.0,1.2.0)").</param>
    /// <returns>Parsed VersionIntervalSet.</returns>
    public static VersionIntervalSet Parse(string text)
    {
        // Walk character by character tracking bracket depth
        // Split on ',' when depth==0 (these separate intervals)
        List<VersionInterval> intervals = [];
        var depth = 0;
        var pos = 0;
        var tokenStart = 0;

        // Iterate through characters using a while loop to avoid modifying loop variable
        while (pos < text.Length)
        {
            var ch = text[pos];

            // Increment depth on '[' or '('
            if (ch is '[' or '(')
            {
                depth++;
            }
            // Decrement depth on ']' or ')'
            else if (ch is ']' or ')')
            {
                depth--;

                // When a bracket closes at depth 0, extract the token
                if (depth == 0)
                {
                    var token = text[tokenStart..(pos + 1)].Trim();
                    var interval = VersionInterval.Parse(token);
                    if (interval != null)
                    {
                        intervals.Add(interval);
                    }

                    // Advance past closing bracket and any trailing comma/whitespace
                    pos++;
                    while (pos < text.Length && (text[pos] == ',' || char.IsWhiteSpace(text[pos])))
                    {
                        pos++;
                    }
                    tokenStart = pos;
                    continue;
                }
            }

            pos++;
        }

        // Return VersionIntervalSet wrapping the list
        return new VersionIntervalSet(intervals);
    }
}
