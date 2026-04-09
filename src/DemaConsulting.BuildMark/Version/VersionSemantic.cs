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
///     Represents a complete semantic version including the comparable portion and optional build metadata.
///     Follows semantic versioning specification where metadata does not affect version precedence.
/// </summary>
/// <param name="Comparable">The comparable portion of the version (major.minor.patch[-pre-release]).</param>
/// <param name="Metadata">The build metadata (e.g., build.5, linux.x64), or null if no metadata.</param>
public record VersionSemantic(VersionComparable Comparable, string? Metadata)
{
    /// <summary>
    ///     The full semantic version string (major.minor.patch[-pre-release]+metadata).
    /// </summary>
    public string FullVersion => string.IsNullOrEmpty(Metadata) 
        ? Comparable.CompareVersion 
        : $"{Comparable.CompareVersion}+{Metadata}";

    /// <summary>
    ///     Gets the major version number.
    /// </summary>
    public int Major => Comparable.Major;

    /// <summary>
    ///     Gets the minor version number.
    /// </summary>
    public int Minor => Comparable.Minor;

    /// <summary>
    ///     Gets the patch version number.
    /// </summary>
    public int Patch => Comparable.Patch;

    /// <summary>
    ///     Gets the core semantic numbers (major.minor.patch).
    /// </summary>
    public string Numbers => Comparable.Numbers;

    /// <summary>
    ///     Gets the pre-release identifier, or empty string if not a pre-release.
    /// </summary>
    public string PreRelease => Comparable.PreRelease ?? string.Empty;

    /// <summary>
    ///     Gets whether this is a pre-release version.
    /// </summary>
    public bool IsPreRelease => Comparable.IsPreRelease;

    /// <summary>
    ///     Gets the version for comparison (major.minor.patch[-pre-release]).
    /// </summary>
    public string CompareVersion => Comparable.CompareVersion;

    /// <summary>
    ///     Tries to create a VersionSemantic from a semantic version string.
    /// </summary>
    /// <param name="versionString">Version string to parse (e.g., "1.2.3+build.5", "2.0.0-alpha.1").</param>
    /// <returns>VersionSemantic instance if string matches format, null otherwise.</returns>
    public static VersionSemantic? TryCreate(string versionString)
    {
        if (string.IsNullOrWhiteSpace(versionString))
        {
            return null;
        }

        // Split on '+' to separate version from metadata
        var parts = versionString.Split('+', 2);
        var versionPart = parts[0];
        var metadata = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? parts[1] : null;

        // Parse the comparable portion
        var comparable = VersionComparable.TryCreate(versionPart);
        if (comparable is null)
        {
            return null;
        }

        return new VersionSemantic(comparable, metadata);
    }

    /// <summary>
    ///     Creates a VersionSemantic from a semantic version string.
    /// </summary>
    /// <param name="versionString">Version string to parse.</param>
    /// <returns>VersionSemantic instance.</returns>
    /// <exception cref="ArgumentException">Thrown if string doesn't match semantic version format.</exception>
    public static VersionSemantic Create(string versionString)
    {
        // Try to create version from string
        var version = TryCreate(versionString);

        // Throw exception if string format is invalid
        return version ?? throw new ArgumentException($"Version string '{versionString}' does not match semantic version format", nameof(versionString));
    }
}