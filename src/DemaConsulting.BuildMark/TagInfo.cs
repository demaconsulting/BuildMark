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
///     Represents a version tag with parsed semantic version information.
/// </summary>
/// <param name="Tag">The tag name.</param>
/// <param name="FullVersion">The full semantic version (major.minor.patch-prerelease) with leading non-version characters removed.</param>
/// <param name="IsPreRelease">Whether this is a pre-release version.</param>
public partial record TagInfo(string Tag, string FullVersion, bool IsPreRelease)
{
    /// <summary>
    ///     Regex pattern for parsing version tags with optional prefix and pre-release suffix.
    /// </summary>
    /// <returns>Compiled regex for parsing tags.</returns>
    [GeneratedRegex(@"^(?:[a-zA-Z_-]+)?(?<version>\d+\.\d+\.\d+)(?:-(?<pre_release>[a-zA-Z0-9.-]+))?(?:\.(?<metadata>[a-zA-Z0-9.-]+))?$")]
    private static partial Regex TagPattern();

    /// <summary>
    ///     Creates a TagInfo from a tag string, or returns null if the tag doesn't match version format.
    /// </summary>
    /// <param name="tag">Tag name to parse.</param>
    /// <returns>TagInfo instance if tag matches version format, null otherwise.</returns>
    public static TagInfo? Create(string tag)
    {
        var match = TagPattern().Match(tag);
        if (!match.Success)
        {
            return null;
        }

        var version = match.Groups["version"].Value;
        var preRelease = match.Groups["pre_release"];
        var metadata = match.Groups["metadata"];
        
        // Build full version: version + optional pre-release + optional metadata
        var fullVersion = version;
        if (preRelease.Success)
        {
            fullVersion += $"-{preRelease.Value}";
        }
        if (metadata.Success)
        {
            fullVersion += $".{metadata.Value}";
        }
        
        var isPreRelease = preRelease.Success;

        return new TagInfo(tag, fullVersion, isPreRelease);
    }
}
