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

namespace DemaConsulting.BuildMark.Configuration;

/// <summary>
///     Represents GitHub-specific connector settings.
/// </summary>
public sealed record GitHubConnectorConfig
{
    /// <summary>
    ///     Gets or sets the repository owner override.
    /// </summary>
    public string? Owner { get; init; }

    /// <summary>
    ///     Gets or sets the repository name override.
    /// </summary>
    public string? Repo { get; init; }

    /// <summary>
    ///     Gets or sets the optional GitHub GraphQL base endpoint override.
    /// </summary>
    public string? BaseUrl { get; init; }

    /// <summary>
    ///     Gets or sets the name of the environment variable that holds the GitHub access token.
    /// </summary>
    /// <remarks>
    ///     When set, the connector reads the token exclusively from this environment variable and does
    ///     not fall back to well-known names (GH_TOKEN, GITHUB_TOKEN) or the gh CLI. If the variable
    ///     is absent or empty the connector throws <see cref="InvalidOperationException"/> with a
    ///     message that identifies the expected variable, so operators receive a clear diagnostic.
    /// </remarks>
    public string? TokenVariable { get; init; }
}
