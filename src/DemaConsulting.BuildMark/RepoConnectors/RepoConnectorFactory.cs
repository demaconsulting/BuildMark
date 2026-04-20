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

using DemaConsulting.BuildMark.Configuration;
using DemaConsulting.BuildMark.RepoConnectors.AzureDevOps;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;
using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.RepoConnectors;

/// <summary>
///     Factory for creating repository connector instances.
/// </summary>
public static class RepoConnectorFactory
{
    /// <summary>
    ///     Creates a repository connector based on the current environment.
    /// </summary>
    /// <param name="config">Optional connector configuration.</param>
    /// <returns>Repository connector instance.</returns>
    public static IRepoConnector Create(ConnectorConfig? config = null)
    {
        // Honor explicit connector selection when configuration is available.
        if (config?.Type != null &&
            config.Type.Equals("azure-devops", StringComparison.OrdinalIgnoreCase))
        {
            return new AzureDevOpsRepoConnector(config.AzureDevOps);
        }

        // Check for Azure DevOps Pipelines environment variable
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("TF_BUILD")))
        {
            return new AzureDevOpsRepoConnector(config?.AzureDevOps);
        }

        // Check for GitHub Actions environment variables
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")))
        {
            return new GitHubRepoConnector(config?.GitHub);
        }

        // Get git remote URL once for inspection.
        // Note: Using .GetAwaiter().GetResult() is acceptable in this tool-oriented entry point
        // as console applications have no synchronization context that could cause deadlocks.
        var remoteUrl = ProcessRunner.TryRunAsync("git", "remote", "get-url", "origin").GetAwaiter().GetResult();
        return CreateFromRemoteUrl(config, remoteUrl);
    }

    /// <summary>
    ///     Creates a repository connector based on the provided git remote URL.
    ///     When the remote URL cannot be retrieved, defaults to a <see cref="GitHubRepoConnector"/>.
    /// </summary>
    /// <param name="config">Optional connector configuration.</param>
    /// <param name="remoteUrl">Git remote URL, or <c>null</c> if unavailable.</param>
    /// <returns>Repository connector instance.</returns>
    internal static IRepoConnector CreateFromRemoteUrl(ConnectorConfig? config, string? remoteUrl)
    {
        // Check if git remote points to Azure DevOps
        if (remoteUrl != null &&
            (remoteUrl.Contains("dev.azure.com", StringComparison.OrdinalIgnoreCase) ||
             remoteUrl.Contains("visualstudio.com", StringComparison.OrdinalIgnoreCase)))
        {
            return new AzureDevOpsRepoConnector(config?.AzureDevOps);
        }

        // Check if git remote points to GitHub
        if (remoteUrl != null && remoteUrl.Contains("github.com", StringComparison.OrdinalIgnoreCase))
        {
            return new GitHubRepoConnector(config?.GitHub);
        }

        // Default to GitHub connector
        return new GitHubRepoConnector(config?.GitHub);
    }
}
