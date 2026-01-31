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
///     Represents a version tag with parsed semantic version information.
/// </summary>
public class TagInfo
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TagInfo"/> class.
    /// </summary>
    /// <param name="tagName">Tag name.</param>
    public TagInfo(string tagName)
    {
        Tag = tagName;
        FullVersion = ParseVersion(tagName);
        IsPreRelease = DetectPreRelease(FullVersion);
    }

    /// <summary>
    ///     Gets the tag name.
    /// </summary>
    public string Tag { get; }

    /// <summary>
    ///     Gets the full semantic version (major.minor.patch-prerelease) with leading non-version characters removed.
    /// </summary>
    public string FullVersion { get; }

    /// <summary>
    ///     Gets a value indicating whether this is a pre-release version.
    /// </summary>
    public bool IsPreRelease { get; }

    /// <summary>
    ///     Parses a tag name and extracts the semantic version by removing leading non-numeric characters.
    /// </summary>
    /// <param name="tagName">Tag name to parse.</param>
    /// <returns>Semantic version string.</returns>
    private static string ParseVersion(string tagName)
    {
        // Remove any leading alphabetic characters, dashes, and underscores
        // This supports various tag naming conventions like "v1.0.0", "ver-1.0.0", "release_1.0.0", etc.
        var startIndex = 0;
        while (startIndex < tagName.Length)
        {
            var c = tagName[startIndex];
            if (char.IsDigit(c))
            {
                break;
            }

            if (c != '-' && c != '_' && !char.IsLetter(c))
            {
                break;
            }

            startIndex++;
        }

        return startIndex < tagName.Length ? tagName[startIndex..] : tagName;
    }

    /// <summary>
    ///     Detects if a version string represents a pre-release.
    /// </summary>
    /// <param name="version">Version string to check.</param>
    /// <returns>True if the version is a pre-release, false otherwise.</returns>
    private static bool DetectPreRelease(string version)
    {
        // Check if the version contains a hyphen followed by any text
        // This covers semantic versioning pre-release format (e.g., 1.0.0-alpha, 1.0.0-beta.1, 1.0.0-rc.1)
        return version.Contains('-');
    }
}
