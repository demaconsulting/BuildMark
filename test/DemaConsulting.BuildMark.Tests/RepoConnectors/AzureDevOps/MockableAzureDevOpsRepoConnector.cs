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

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.AzureDevOps;

/// <summary>
///     Mock implementation of AzureDevOpsRepoConnector for testing purposes.
/// </summary>
/// <remarks>
///     This class allows tests to mock both git command execution and Azure DevOps REST API calls
///     by overriding the RunCommandAsync and CreateRestClient methods.
/// </remarks>
internal sealed class MockableAzureDevOpsRepoConnector : AzureDevOpsRepoConnector
{
    /// <summary>
    ///     Mock responses for RunCommandAsync.
    /// </summary>
    private readonly Dictionary<string, string> _commandResponses = [];

    /// <summary>
    ///     Mock HttpClient for REST API requests.
    /// </summary>
    private readonly HttpClient? _mockHttpClient;

    /// <summary>
    ///     Initializes a new instance of the <see cref="MockableAzureDevOpsRepoConnector"/> class.
    /// </summary>
    /// <param name="mockHttpClient">
    ///     Optional mock HttpClient for REST API requests. When provided, the connector
    ///     uses this client instead of creating a real authenticated client.
    /// </param>
    /// <param name="config">
    ///     Optional Azure DevOps connector configuration with organization URL, project,
    ///     and repository overrides.
    /// </param>
    public MockableAzureDevOpsRepoConnector(
        HttpClient? mockHttpClient = null,
        AzureDevOpsConnectorConfig? config = null) : base(config)
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
    protected override Task<string> RunCommandAsync(string command, params string[] arguments)
    {
        var key = $"{command} {string.Join(" ", arguments)}";
        return Task.FromResult(_commandResponses.TryGetValue(key, out var response) ? response : string.Empty);
    }

    /// <summary>
    ///     Creates an Azure DevOps REST client for API operations.
    /// </summary>
    /// <param name="organizationUrl">Azure DevOps organization URL.</param>
    /// <param name="project">Azure DevOps project name.</param>
    /// <param name="token">Authentication token.</param>
    /// <param name="isBearer">True for Bearer auth, false for Basic (PAT) auth.</param>
    /// <returns>A new AzureDevOpsRestClient instance.</returns>
    internal override AzureDevOpsRestClient CreateRestClient(
        string organizationUrl,
        string project,
        string token,
        bool isBearer)
    {
        return _mockHttpClient != null
            ? new AzureDevOpsRestClient(_mockHttpClient, organizationUrl, project)
            : base.CreateRestClient(organizationUrl, project, token, isBearer);
    }
}
