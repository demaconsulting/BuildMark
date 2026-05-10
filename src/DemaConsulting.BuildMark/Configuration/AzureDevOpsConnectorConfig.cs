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
///     Represents the Azure DevOps connector settings.
/// </summary>
public sealed record AzureDevOpsConnectorConfig
{
    /// <summary>
    ///     Gets or sets the Azure DevOps organization URL override
    ///     (e.g., "https://dev.azure.com/myorg").
    /// </summary>
    public string? OrganizationUrl { get; init; }

    /// <summary>
    ///     Gets or sets the Azure DevOps organization name override.
    /// </summary>
    public string? Organization { get; init; }

    /// <summary>
    ///     Gets or sets the Azure DevOps project name override.
    /// </summary>
    public string? Project { get; init; }

    /// <summary>
    ///     Gets or sets the repository name override within the project.
    /// </summary>
    public string? Repository { get; init; }

    /// <summary>
    ///     Gets or sets the name of the environment variable that holds the Azure DevOps access token.
    /// </summary>
    /// <remarks>
    ///     When set, the connector reads the token exclusively from this environment variable and does
    ///     not fall back to well-known names (AZURE_DEVOPS_PAT, SYSTEM_ACCESSTOKEN, etc.) or the az
    ///     CLI. If the variable is absent or empty the connector throws <see cref="InvalidOperationException"/>
    ///     with a message that identifies the expected variable. The token is always treated as a Basic
    ///     (PAT) credential when loaded from a custom variable.
    /// </remarks>
    public string? TokenVariable { get; init; }

    /// <summary>
    ///     Gets or sets the Area Path used to scope the known-issues WIQL query.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         When <see langword="null"/> (the default), the connector automatically scopes the
    ///         known-issues WIQL query to <c>{Project}\{Repository}</c>, which is the conventional
    ///         area path for a single repository inside an ADO project. This prevents bugs that
    ///         belong to other products or repositories in the same project from appearing in the
    ///         generated build notes.
    ///     </para>
    ///     <para>
    ///         Set this property to an explicit value when your team's area hierarchy does not
    ///         follow the <c>{Project}\{Repository}</c> convention — for example,
    ///         <c>MyProject\TeamA\Backend</c>. The connector will then use
    ///         <c>[System.AreaPath] UNDER '{AreaPath}'</c> to include the specified area and all
    ///         of its descendants.
    ///     </para>
    ///     <para>
    ///         Set this property to an empty string (<c>""</c>) to disable area-path filtering
    ///         entirely and query all bugs in the ADO project.
    ///     </para>
    /// </remarks>
    public string? AreaPath { get; init; }
}
