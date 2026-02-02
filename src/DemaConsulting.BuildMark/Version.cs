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

namespace DemaConsulting.BuildMark;

/// <summary>
///     Represents a version with parsed semantic version information.
/// </summary>
/// <param name="Tag">The tag name.</param>
/// <param name="FullVersion">The full semantic version (major.minor.patch-prerelease+metadata) with leading non-version characters removed.</param>
/// <param name="SemanticVersion">The core semantic version (major.minor.patch).</param>
/// <param name="PreRelease">The pre-release identifier (e.g., rc.4, alpha.1), or empty string if not a pre-release.</param>
/// <param name="Metadata">The build metadata (e.g., build.5, linux.x64), or empty string if no metadata.</param>
/// <param name="IsPreRelease">Whether this is a pre-release version.</param>
public partial record Version(string Tag, string FullVersion, string SemanticVersion, string PreRelease, string Metadata, bool IsPreRelease)
{
    /// <summary>
    ///     Regex pattern for parsing version tags with optional prefix, pre-release, and build metadata.
    ///     Format: [prefix]major.minor.patch[.|-)prerelease][+build-metadata]
    ///     Accepts both dot and hyphen as pre-release separators.
    ///     Examples: 
    ///     - Rel_1.2.3.rc.4+build.5 -> version: 1.2.3, prerelease: rc.4, metadata: build.5
    ///     - v2.0.0-alpha.1 -> version: 2.0.0, prerelease: alpha.1
    /// </summary>
    /// <returns>Compiled regex for parsing tags.</returns>
    [GeneratedRegex(@"^(?:[a-zA-Z_-]+)?(?<version>\d+\.\d+\.\d+)(?<separator>[-.])?(?<pre_release>[a-zA-Z0-9.-]+?)?(?:\+(?<metadata>[a-zA-Z0-9.-]+))?$")]
    private static partial Regex TagPattern();

    /// <summary>
    ///     Tries to create a Version from a tag string.
    /// </summary>
    /// <param name="tag">Tag name to parse.</param>
    /// <returns>Version instance if tag matches version format, null otherwise.</returns>
    public static Version? TryCreate(string tag)
    {
        // Attempt to match tag against version pattern
        var match = TagPattern().Match(tag);
        if (!match.Success)
        {
            return null;
        }

        // Extract version components from regex match
        var version = match.Groups["version"].Value;
        var separator = match.Groups["separator"];
        var preReleaseGroup = match.Groups["pre_release"];
        var metadataGroup = match.Groups["metadata"];
        
        // Determine if pre-release based on separator and content
        var hasPreRelease = separator.Success && preReleaseGroup.Success && !string.IsNullOrEmpty(preReleaseGroup.Value);
        
        // Get pre-release and metadata strings
        var preRelease = hasPreRelease ? preReleaseGroup.Value : string.Empty;
        var metadata = metadataGroup.Success ? metadataGroup.Value : string.Empty;
        
        // Construct full version string from components
        var fullVersion = version;
        if (hasPreRelease)
        {
            fullVersion += $"{separator.Value}{preRelease}";
        }
        if (metadataGroup.Success)
        {
            fullVersion += $"+{metadata}";
        }

        // Create and return version record
        return new Version(tag, fullVersion, version, preRelease, metadata, hasPreRelease);
    }

    /// <summary>
    ///     Creates a Version from a tag string.
    /// </summary>
    /// <param name="tag">Tag name to parse.</param>
    /// <returns>Version instance.</returns>
    /// <exception cref="ArgumentException">Thrown if tag doesn't match version format.</exception>
    public static Version Create(string tag)
    {
        // Try to create version from tag
        var version = TryCreate(tag);

        // Throw exception if tag format is invalid
        if (version == null)
        {
            throw new ArgumentException($"Tag '{tag}' does not match version format", nameof(tag));
        }

        return version;
    }
}
