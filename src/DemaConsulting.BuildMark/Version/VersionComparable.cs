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

namespace DemaConsulting.BuildMark.Version;

/// <summary>
///     Represents the comparable portion of a semantic version (major.minor.patch[-pre-release]).
///     Provides analytical version comparison where version numbers are compared numerically
///     (e.g., 1.2.3 &lt; 1.11.2) rather than lexicographically.
/// </summary>
/// <param name="Major">The major version number.</param>
/// <param name="Minor">The minor version number.</param>
/// <param name="Patch">The patch version number.</param>
/// <param name="PreRelease">The pre-release identifier (e.g., rc.4, alpha.1), or null if not a pre-release.</param>
public partial record VersionComparable(int Major, int Minor, int Patch, string? PreRelease) : IComparable<VersionComparable>
{
    /// <summary>
    ///     Represents a parsed pre-release segment that can be either numeric or text.
    ///     Enables efficient comparison without repeated string parsing during version comparison operations.
    /// </summary>
    private readonly record struct PreReleaseSegment
    {
        /// <summary>
        ///     Initializes a new instance of the PreReleaseSegment struct with a numeric value.
        /// </summary>
        /// <param name="numericValue">The numeric value for this segment.</param>
        public PreReleaseSegment(int numericValue)
        {
            NumericValue = numericValue;
            TextValue = null;
            IsNumeric = true;
        }

        /// <summary>
        ///     Initializes a new instance of the PreReleaseSegment struct with a text value.
        /// </summary>
        /// <param name="textValue">The text value for this segment.</param>
        public PreReleaseSegment(string textValue)
        {
            NumericValue = 0;
            TextValue = textValue;
            IsNumeric = false;
        }

        /// <summary>
        ///     Gets the numeric value if this segment is numeric, otherwise 0.
        /// </summary>
        private int NumericValue { get; }

        /// <summary>
        ///     Gets the text value if this segment is non-numeric, otherwise null.
        /// </summary>
        private string? TextValue { get; }

        /// <summary>
        ///     Gets a value indicating whether this segment represents a numeric value.
        /// </summary>
        private bool IsNumeric { get; }

        /// <summary>
        ///     Compares this PreReleaseSegment to another according to SemVer specification.
        ///     Numeric identifiers are always less than non-numeric identifiers.
        ///     When both are numeric, compares numerically. When both are text, compares lexicographically.
        /// </summary>
        /// <param name="other">The other PreReleaseSegment to compare to.</param>
        /// <returns>-1 if this is less than other, 0 if equal, 1 if greater.</returns>
        public int CompareTo(PreReleaseSegment other)
        {
            return IsNumeric switch
            {
                // Numeric identifiers are always less than non-numeric identifiers per SemVer spec
                true when !other.IsNumeric => -1,
                false when other.IsNumeric => 1,

                // Both are same type - compare accordingly
                true => NumericValue.CompareTo(other.NumericValue),

                // Both are non-numeric - compare lexically (case-insensitive per SemVer)
                _ => string.Compare(TextValue, other.TextValue, StringComparison.OrdinalIgnoreCase)
            };
        }
    }

    /// <summary>
    ///     Cached parsed pre-release segments for efficient comparison.
    /// </summary>
    private readonly PreReleaseSegment[]? _preReleaseSegments = ParsePreReleaseSegments(PreRelease);

    /// <summary>
    ///     Parses pre-release string into segments at construction time for efficient comparison.
    ///     Eliminates repeated string parsing during comparison operations by pre-processing segments.
    /// </summary>
    /// <param name="preRelease">Pre-release string to parse.</param>
    /// <returns>Array of parsed segments, or null if no pre-release.</returns>
    private static PreReleaseSegment[]? ParsePreReleaseSegments(string? preRelease)
    {
        // Skip parsing if no pre-release data is provided
        if (string.IsNullOrEmpty(preRelease))
        {
            return null;
        }

        // Split pre-release string into dot-separated parts
        var parts = preRelease.Split('.');
        var segments = new PreReleaseSegment[parts.Length];

        // Convert each part into either numeric or text segment
        for (var i = 0; i < parts.Length; i++)
        {
            if (int.TryParse(parts[i], out var numericValue))
            {
                segments[i] = new PreReleaseSegment(numericValue);
            }
            else
            {
                segments[i] = new PreReleaseSegment(parts[i]);
            }
        }

        return segments;
    }
    /// <summary>
    ///     The core semantic numbers (major.minor.patch).
    /// </summary>
    public string Numbers => $"{Major}.{Minor}.{Patch}";

    /// <summary>
    ///     Whether this is a pre-release version.
    /// </summary>
    public bool IsPreRelease => !string.IsNullOrEmpty(PreRelease);

    /// <summary>
    ///     The complete comparable version string (major.minor.patch[-pre-release]).
    /// </summary>
    public string CompareVersion => IsPreRelease ? $"{Numbers}-{PreRelease}" : Numbers;

    /// <summary>
    ///     Regex pattern for parsing comparable version strings.
    ///     Format: major.minor.patch[-prerelease]
    ///     Examples: 1.2.3, 2.0.0-alpha.1, 1.5.0-rc.2
    /// </summary>
    /// <returns>Compiled regex for parsing comparable versions.</returns>
    [GeneratedRegex(@"^(?<numbers>\d+\.\d+\.\d+)(?:-(?<pre_release>[a-zA-Z0-9.-]+))?$")]
    private static partial Regex ComparablePattern();

    /// <summary>
    ///     Tries to create a VersionComparable from a version string.
    ///     Provides safe parsing with null return for invalid input formats.
    /// </summary>
    /// <param name="versionString">Version string to parse (e.g., "1.2.3", "2.0.0-alpha.1").</param>
    /// <returns>VersionComparable instance if string matches format, null otherwise.</returns>
    public static VersionComparable? TryCreate(string versionString)
    {
        // Validate input parameter to prevent processing invalid data
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return null;
        }

        // Attempt to match version string against comparable pattern
        var match = ComparablePattern().Match(versionString);
        if (!match.Success)
        {
            return null;
        }

        // Extract and parse version numbers from regex groups
        var numbersString = match.Groups["numbers"].Value;
        var numberParts = numbersString.Split('.');

        // Validate that we have exactly 3 numeric components
        if (numberParts.Length != 3 ||
            !int.TryParse(numberParts[0], out var major) ||
            !int.TryParse(numberParts[1], out var minor) ||
            !int.TryParse(numberParts[2], out var patch))
        {
            return null;
        }

        // Extract pre-release identifier if present
        var preReleaseGroup = match.Groups["pre_release"];
        var preRelease = preReleaseGroup.Success && !string.IsNullOrEmpty(preReleaseGroup.Value)
            ? preReleaseGroup.Value
            : null;

        // Create and return comparable version with parsed components
        return new VersionComparable(major, minor, patch, preRelease);
    }

    /// <summary>
    ///     Creates a VersionComparable from a version string.
    ///     Throws ArgumentException for invalid input, providing clear error feedback.
    /// </summary>
    /// <param name="versionString">Version string to parse.</param>
    /// <returns>VersionComparable instance.</returns>
    /// <exception cref="ArgumentException">Thrown if string doesn't match version format.</exception>
    public static VersionComparable Create(string versionString)
    {
        // Try to create version from string using safe parsing method
        var version = TryCreate(versionString);

        // Throw descriptive exception if string format is invalid
        return version ?? throw new ArgumentException($"Version string '{versionString}' does not match comparable version format", nameof(versionString));
    }

    /// <summary>
    ///     Compares this VersionComparable instance to another VersionComparable instance.
    ///     Implements SemVer-compliant comparison with pre-release precedence rules.
    /// </summary>
    /// <param name="other">The other VersionComparable to compare to.</param>
    /// <returns>
    ///     A value less than zero if this instance is less than other;
    ///     zero if they are equal; greater than zero if this instance is greater than other.
    /// </returns>
    public int CompareTo(VersionComparable? other)
    {
        // Handle null comparison cases per IComparable contract
        if (other is null)
        {
            return 1;
        }

        // Compare major version numbers first
        var result = Major.CompareTo(other.Major);
        if (result != 0)
        {
            return result;
        }

        // Compare minor version numbers if major versions are equal
        result = Minor.CompareTo(other.Minor);
        if (result != 0)
        {
            return result;
        }

        // Compare patch version numbers if minor versions are equal
        result = Patch.CompareTo(other.Patch);
        if (result != 0)
        {
            return result;
        }

        // Handle pre-release vs release comparison when base versions are equal
        return IsPreRelease switch
        {
            // Non-pre-release versions are considered greater than pre-release versions per SemVer
            false when other.IsPreRelease => 1,
            true when !other.IsPreRelease => -1,

            // If both are pre-release then compare pre-release segments using optimized method
            true when other.IsPreRelease => ComparePreReleaseSegments(_preReleaseSegments!, other._preReleaseSegments!),

            // Both are non-pre-release with same base version - they are equal
            _ => 0
        };
    }

    /// <summary>
    ///     Compares pre-release segment arrays according to SemVer specification.
    ///     Uses pre-parsed segments for efficient comparison without repeated string parsing.
    /// </summary>
    /// <param name="segments1">First pre-release segments array.</param>
    /// <param name="segments2">Second pre-release segments array.</param>
    /// <returns>-1 if first is less, 0 if equal, 1 if first is greater.</returns>
    private static int ComparePreReleaseSegments(PreReleaseSegment[] segments1, PreReleaseSegment[] segments2)
    {
        var minLength = Math.Min(segments1.Length, segments2.Length);

        for (var i = 0; i < minLength; i++)
        {
            var comparison = segments1[i].CompareTo(segments2[i]);
            if (comparison != 0)
            {
                return comparison;
            }
        }

        // All compared segments are equal - the shorter list is considered less
        return segments1.Length.CompareTo(segments2.Length);
    }

    /// <summary>
    ///     Determines whether the first VersionComparable is less than the second.
    /// </summary>
    /// <param name="left">The first VersionComparable to compare.</param>
    /// <param name="right">The second VersionComparable to compare.</param>
    /// <returns>True if left is less than right; otherwise, false.</returns>
    public static bool operator <(VersionComparable? left, VersionComparable? right)
    {
        if (left is null)
        {
            return right is not null;
        }
        return left.CompareTo(right) < 0;
    }

    /// <summary>
    ///     Determines whether the first VersionComparable is less than or equal to the second.
    /// </summary>
    /// <param name="left">The first VersionComparable to compare.</param>
    /// <param name="right">The second VersionComparable to compare.</param>
    /// <returns>True if left is less than or equal to right; otherwise, false.</returns>
    public static bool operator <=(VersionComparable? left, VersionComparable? right)
    {
        if (left is null)
        {
            return true;
        }
        return left.CompareTo(right) <= 0;
    }

    /// <summary>
    ///     Determines whether the first VersionComparable is greater than the second.
    /// </summary>
    /// <param name="left">The first VersionComparable to compare.</param>
    /// <param name="right">The second VersionComparable to compare.</param>
    /// <returns>True if left is greater than right; otherwise, false.</returns>
    public static bool operator >(VersionComparable? left, VersionComparable? right)
    {
        if (left is null)
        {
            return false;
        }
        return left.CompareTo(right) > 0;
    }

    /// <summary>
    ///     Determines whether the first VersionComparable is greater than or equal to the second.
    /// </summary>
    /// <param name="left">The first VersionComparable to compare.</param>
    /// <param name="right">The second VersionComparable to compare.</param>
    /// <returns>True if left is greater than or equal to right; otherwise, false.</returns>
    public static bool operator >=(VersionComparable? left, VersionComparable? right)
    {
        if (left is null)
        {
            return right is null;
        }
        return left.CompareTo(right) >= 0;
    }
}
