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
///     Represents a repository tag parsed into semantic version information.
///     Provides the bridge between repository tags (with optional prefixes) and structured semantic versions.
/// </summary>
/// <param name="Tag">The original tag name as it appears in the repository.</param>
/// <param name="Semantic">The parsed semantic version information.</param>
public partial record VersionTag(string Tag, VersionSemantic Semantic)
{
    /// <summary>
    ///     Regex pattern for parsing version tags with optional prefix, pre-release, and build metadata.
    ///     Format: [prefix]major.minor.patch[.|-)prerelease][+build-metadata]
    ///     Accepts both dot and hyphen as pre-release separators.
    ///     Prefix may contain path separators (/) to support hierarchical tags (e.g., Azure DevOps).
    ///     Examples: 
    ///     - Rel_1.2.3.rc.4+build.5 -> version: 1.2.3, pre-release: rc.4, metadata: build.5
    ///     - v2.0.0-alpha.1 -> version: 2.0.0, pre-release: alpha.1
    ///     - release/1.2.3-rc.4 -> version: 1.2.3, pre-release: rc.4
    /// </summary>
    /// <returns>Compiled regex for parsing tags.</returns>
    [GeneratedRegex(@"^(?:[a-zA-Z_/-]+)?(?<numbers>\d+\.\d+\.\d+)(?<separator>[-.])?(?<pre_release>[a-zA-Z0-9.-]+?)?(?:\+(?<metadata>[a-zA-Z0-9.-]+))?$")]
    private static partial Regex TagPattern();

    /// <summary>
    ///     Tries to create a VersionTag from a repository tag string.
    /// </summary>
    /// <param name="tag">Tag name to parse.</param>
    /// <returns>VersionTag instance if tag matches version format, null otherwise.</returns>
    public static VersionTag? TryCreate(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
        {
            return null;
        }

        // Attempt to match tag against version pattern
        var match = TagPattern().Match(tag);
        if (!match.Success)
        {
            return null;
        }

        // Extract version components from regex match
        var numbers = match.Groups["numbers"].Value;
        var separator = match.Groups["separator"];
        var preReleaseGroup = match.Groups["pre_release"];
        var metadataGroup = match.Groups["metadata"];

        // Determine if pre-release based on separator and content
        var hasPreRelease = separator.Success && preReleaseGroup.Success && !string.IsNullOrEmpty(preReleaseGroup.Value);

        // Parse version numbers
        var numberParts = numbers.Split('.');
        if (numberParts.Length != 3 ||
            !int.TryParse(numberParts[0], out var major) ||
            !int.TryParse(numberParts[1], out var minor) ||
            !int.TryParse(numberParts[2], out var patch))
        {
            return null;
        }

        // Get pre-release and metadata strings
        var preRelease = hasPreRelease ? preReleaseGroup.Value : null;
        var metadata = metadataGroup.Success && !string.IsNullOrEmpty(metadataGroup.Value)
            ? metadataGroup.Value
            : null;

        // Create comparable version
        var comparable = new VersionComparable(major, minor, patch, preRelease);

        // Create semantic version
        var semantic = new VersionSemantic(comparable, metadata);

        // Create and return version tag
        return new VersionTag(tag, semantic);
    }

    /// <summary>
    ///     Creates a VersionTag from a repository tag string.
    /// </summary>
    /// <param name="tag">Tag name to parse.</param>
    /// <returns>VersionTag instance.</returns>
    /// <exception cref="ArgumentException">Thrown if tag doesn't match version format.</exception>
    public static VersionTag Create(string tag)
    {
        // Try to create version from tag
        var version = TryCreate(tag);

        // Throw exception if tag format is invalid
        return version ?? throw new ArgumentException($"Tag '{tag}' does not match version format", nameof(tag));
    }

    /// <summary>
    ///     Gets the full semantic version string (major.minor.patch[-pre-release]+metadata) with leading non-version characters removed.
    /// </summary>
    public string FullVersion => Semantic.FullVersion;

    /// <summary>
    ///     Gets the major version number.
    /// </summary>
    public int Major => Semantic.Major;

    /// <summary>
    ///     Gets the minor version number.
    /// </summary>
    public int Minor => Semantic.Minor;

    /// <summary>
    ///     Gets the patch version number.
    /// </summary>
    public int Patch => Semantic.Patch;

    /// <summary>
    ///     Gets the core semantic version numbers (major.minor.patch).
    /// </summary>
    public string Numbers => Semantic.Numbers;

    /// <summary>
    ///     Gets the pre-release identifier, or empty string if not a pre-release.
    /// </summary>
    public string PreRelease => Semantic.PreRelease;

    /// <summary>
    ///     Gets the version for comparison (major.minor.patch[-pre-release]).
    /// </summary>
    public string CompareVersion => Semantic.CompareVersion;

    /// <summary>
    ///     Gets the build metadata, or empty string if no metadata.
    /// </summary>
    public string Metadata => Semantic.Metadata ?? string.Empty;

    /// <summary>
    ///     Gets whether this is a pre-release version.
    /// </summary>
    public bool IsPreRelease => Semantic.IsPreRelease;
}
