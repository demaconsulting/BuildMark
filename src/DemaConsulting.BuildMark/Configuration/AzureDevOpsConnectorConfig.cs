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
}
