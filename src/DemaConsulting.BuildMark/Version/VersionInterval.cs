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
///     Represents a version interval with optional lower and upper bounds.
/// </summary>
/// <param name="LowerBound">Lower bound version string (null means unbounded).</param>
/// <param name="LowerInclusive">Whether the lower bound is inclusive.</param>
/// <param name="UpperBound">Upper bound version string (null means unbounded).</param>
/// <param name="UpperInclusive">Whether the upper bound is inclusive.</param>
public record VersionInterval(
    string? LowerBound,
    bool LowerInclusive,
    string? UpperBound,
    bool UpperInclusive)
{
    /// <summary>
    ///     Determines whether a candidate semantic version falls within the interval.
    /// </summary>
    /// <param name="version">Semantic version text to evaluate.</param>
    /// <returns>True when the version is within the interval; otherwise false.</returns>
    public bool Contains(string version)
    {
        // Reject invalid semantic version text.
        var candidateVersion = VersionComparable.TryCreate(version);
        if (candidateVersion == null)
        {
            return false;
        }

        return Contains(candidateVersion);
    }

    /// <summary>
    ///     Determines whether a candidate VersionComparable falls within the interval.
    /// </summary>
    /// <param name="version">VersionComparable to evaluate.</param>
    /// <returns>True when the version is within the interval; otherwise false.</returns>
    public bool Contains(VersionComparable version)
    {
        // Reject versions below the lower bound.
        if (LowerBound != null)
        {
            var lowerBoundVersion = VersionComparable.TryCreate(LowerBound);
            if (lowerBoundVersion != null)
            {
                var lowerComparison = version.CompareTo(lowerBoundVersion);
                if (lowerComparison < 0 || (lowerComparison == 0 && !LowerInclusive))
                {
                    return false;
                }
            }
        }

        // Reject versions above the upper bound.
        if (UpperBound != null)
        {
            var upperBoundVersion = VersionComparable.TryCreate(UpperBound);
            if (upperBoundVersion != null)
            {
                var upperComparison = version.CompareTo(upperBoundVersion);
                if (upperComparison > 0 || (upperComparison == 0 && !UpperInclusive))
                {
                    return false;
                }
            }
        }

        return true;
    }

    /// <summary>
    ///     Determines whether a candidate BuildMark version falls within the interval.
    /// </summary>
    /// <param name="version">BuildMark version to evaluate.</param>
    /// <returns>True when the version is within the interval; otherwise false.</returns>
    public bool Contains(VersionTag version)
    {
        return Contains(version.Semantic.Comparable);
    }

    /// <summary>
    ///     Parses a version interval from text.
    /// </summary>
    /// <param name="text">Text to parse (e.g., "[1.0.0,2.0.0)", "(,1.0.0]").</param>
    /// <returns>Parsed VersionInterval, or null if text is not a valid interval.</returns>
    public static VersionInterval? Parse(string? text)
    {
        // Return null for null/empty input
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        // Trim whitespace
        text = text.Trim();

        // Verify first char is '[' or '(' and last char is ']' or ')'
        if (text.Length < 2)
        {
            return null;
        }

        var firstChar = text[0];
        var lastChar = text[^1];

        if ((firstChar != '[' && firstChar != '(') || (lastChar != ']' && lastChar != ')'))
        {
            return null;
        }

        // Determine LowerInclusive from opening bracket
        var lowerInclusive = firstChar == '[';

        // Determine UpperInclusive from closing bracket
        var upperInclusive = lastChar == ']';

        // Get interior text (between outer brackets)
        var interior = text[1..^1];

        // Find the FIRST comma in interior text to split lower/upper bounds
        var commaIndex = interior.IndexOf(',');
        if (commaIndex < 0)
        {
            return null;
        }

        // Trim each bound; empty string → null (unbounded)
        var lowerStr = interior[..commaIndex].Trim();
        var upperStr = interior[(commaIndex + 1)..].Trim();

        var lowerBound = string.IsNullOrEmpty(lowerStr) ? null : lowerStr;
        var upperBound = string.IsNullOrEmpty(upperStr) ? null : upperStr;

        // Return parsed interval
        return new VersionInterval(lowerBound, lowerInclusive, upperBound, upperInclusive);
    }
}
