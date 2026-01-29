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
///     Factory for creating repository connector instances.
/// </summary>
public static class RepoConnectorFactory
{
    /// <summary>
    ///     Creates a repository connector based on the current environment.
    /// </summary>
    /// <returns>Repository connector instance.</returns>
    public static IRepoConnector Create()
    {
        // Check for GitHub environment variables
        if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
            !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GITHUB_WORKSPACE")))
        {
            return new GitHubRepoConnector();
        }

        // Check git remote for GitHub
        if (IsGitHubRepository())
        {
            return new GitHubRepoConnector();
        }

        // Default to GitHub
        return new GitHubRepoConnector();
    }

    /// <summary>
    ///     Checks if the current repository is a GitHub repository.
    /// </summary>
    /// <returns>True if GitHub repository.</returns>
    private static bool IsGitHubRepository()
    {
        var output = ProcessRunner.TryRunAsync("git", "remote get-url origin").Result;
        return output != null && output.Contains("github.com", StringComparison.OrdinalIgnoreCase);
    }
}
