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
using Octokit;

namespace DemaConsulting.BuildMark;

/// <summary>
///     Factory for creating GitHub API clients from repository information.
/// </summary>
internal static partial class GitHubClientFactory
{
    /// <summary>
    ///     Creates a GitHub client from the current repository.
    /// </summary>
    /// <param name="token">Optional authentication token. If not provided, will try GH_TOKEN environment variable or 'gh auth token' command.</param>
    /// <returns>GitHub client, owner, and repository name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when repository information cannot be determined.</exception>
    public static async Task<(GitHubClient client, string owner, string repo)> CreateFromRepositoryAsync(string? token = null)
    {
        // Get git remote URL
        var remoteUrl = await ProcessRunner.RunAsync("git", "remote get-url origin");
        
        // Parse owner and repo from URL
        var (baseUri, owner, repo) = ParseGitHubUrl(remoteUrl);
        
        // Get authentication token
        var authToken = token ?? await GetAuthenticationTokenAsync();
        
        // Create GitHub client
        var client = new GitHubClient(new ProductHeaderValue("BuildMark"), baseUri)
        {
            Credentials = new Credentials(authToken)
        };
        
        return (client, owner, repo);
    }

    /// <summary>
    ///     Parses a GitHub URL to extract base URI, owner, and repository name.
    /// </summary>
    /// <param name="url">Git remote URL.</param>
    /// <returns>Base URI, owner, and repository name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when URL cannot be parsed.</exception>
    private static (Uri baseUri, string owner, string repo) ParseGitHubUrl(string url)
    {
        // Try HTTPS URL format: https://github.com/owner/repo.git
        var httpsMatch = HttpsUrlRegex().Match(url);
        if (httpsMatch.Success)
        {
            var host = httpsMatch.Groups[1].Value;
            var owner = httpsMatch.Groups[2].Value;
            var repo = httpsMatch.Groups[3].Value;
            
            // Remove .git suffix if present
            if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repo = repo[..^4];
            }
            
            // Construct base URI for GitHub API
            // SonarLint S1075: GitHub API URLs are fixed constants
            #pragma warning disable S1075
            var baseUri = host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
                ? new Uri("https://api.github.com/")
                : new Uri($"https://{host}/api/v3/");
            #pragma warning restore S1075
            
            return (baseUri, owner, repo);
        }
        
        // Try SSH URL format: git@github.com:owner/repo.git
        var sshMatch = SshUrlRegex().Match(url);
        if (sshMatch.Success)
        {
            var host = sshMatch.Groups[1].Value;
            var owner = sshMatch.Groups[2].Value;
            var repo = sshMatch.Groups[3].Value;
            
            // Remove .git suffix if present
            if (repo.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repo = repo[..^4];
            }
            
            // Construct base URI for GitHub API
            // SonarLint S1075: GitHub API URLs are fixed constants
            #pragma warning disable S1075
            var baseUri = host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
                ? new Uri("https://api.github.com/")
                : new Uri($"https://{host}/api/v3/");
            #pragma warning restore S1075
            
            return (baseUri, owner, repo);
        }
        
        throw new InvalidOperationException($"Could not parse GitHub URL: {url}");
    }

    /// <summary>
    ///     Gets the authentication token for GitHub API.
    /// </summary>
    /// <returns>Authentication token.</returns>
    /// <exception cref="InvalidOperationException">Thrown when token cannot be obtained.</exception>
    private static async Task<string> GetAuthenticationTokenAsync()
    {
        // Try GH_TOKEN environment variable first
        var token = Environment.GetEnvironmentVariable("GH_TOKEN");
        if (!string.IsNullOrEmpty(token))
        {
            return token;
        }
        
        // Fall back to gh auth token command
        try
        {
            token = await ProcessRunner.RunAsync("gh", "auth token");
            if (!string.IsNullOrEmpty(token))
            {
                return token;
            }
        }
        catch (InvalidOperationException)
        {
            // gh command failed, fall through to error
        }
        
        throw new InvalidOperationException(
            "Could not obtain GitHub authentication token. " +
            "Set GH_TOKEN environment variable or run 'gh auth login'.");
    }

    /// <summary>
    ///     Regular expression to match HTTPS GitHub URLs.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^https://([^/]+)/([^/]+)/([^/]+?)(?:\.git)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex HttpsUrlRegex();

    /// <summary>
    ///     Regular expression to match SSH GitHub URLs.
    /// </summary>
    /// <returns>Compiled regular expression.</returns>
    [GeneratedRegex(@"^git@([^:]+):([^/]+)/([^/]+?)(?:\.git)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex SshUrlRegex();
}
