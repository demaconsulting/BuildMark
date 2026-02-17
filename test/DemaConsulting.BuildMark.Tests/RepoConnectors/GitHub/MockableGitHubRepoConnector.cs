// Copyright (c) 2026 DEMA Consulting
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

using DemaConsulting.BuildMark.RepoConnectors;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Mock implementation of GitHubRepoConnector for testing purposes.
/// </summary>
/// <remarks>
///     This class allows tests to mock both git command execution and GitHub GraphQL API calls
///     by overriding the RunCommandAsync and CreateGraphQLClient methods.
/// </remarks>
internal sealed class MockableGitHubRepoConnector : GitHubRepoConnector
{
    /// <summary>
    ///     Mock responses for RunCommandAsync.
    /// </summary>
    private readonly Dictionary<string, string> _commandResponses = new();

    /// <summary>
    ///     Mock HttpClient for GraphQL requests.
    /// </summary>
    private readonly HttpClient? _mockHttpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MockableGitHubRepoConnector"/> class.
    /// </summary>
    /// <param name="mockHttpClient">Optional mock HttpClient for GraphQL requests.</param>
    public MockableGitHubRepoConnector(HttpClient? mockHttpClient = null)
    {
        _mockHttpClient = mockHttpClient;
    }

    /// <summary>
    ///     Sets the mock response for a specific command.
    /// </summary>
    /// <param name="command">Command to mock.</param>
    /// <param name="response">Response to return.</param>
    public void SetCommandResponse(string command, string response)
    {
        _commandResponses[command] = response;
    }

    /// <summary>
    ///     Runs a command and returns its output.
    /// </summary>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <returns>Command output.</returns>
    protected override Task<string> RunCommandAsync(string command, string arguments)
    {
        var key = $"{command} {arguments}";
        return Task.FromResult(_commandResponses.TryGetValue(key, out var response) ? response : string.Empty);
    }

    /// <summary>
    ///     Creates a GitHub GraphQL client for API operations.
    /// </summary>
    /// <param name="token">GitHub personal access token for authentication.</param>
    /// <returns>A new GitHubGraphQLClient instance.</returns>
    internal override GitHubGraphQLClient CreateGraphQLClient(string token)
    {
        return _mockHttpClient != null
            ? new GitHubGraphQLClient(_mockHttpClient)
            : base.CreateGraphQLClient(token);
    }
}
