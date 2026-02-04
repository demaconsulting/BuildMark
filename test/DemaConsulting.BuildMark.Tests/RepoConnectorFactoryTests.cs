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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the RepoConnectorFactory class.
/// </summary>
[TestClass]
public class RepoConnectorFactoryTests
{
    private string? _originalGhToken;

    /// <summary>
    ///     Test initialization - set a dummy GH_TOKEN for testing.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _originalGhToken = Environment.GetEnvironmentVariable("GH_TOKEN");
        Environment.SetEnvironmentVariable("GH_TOKEN", "dummy_token_for_testing");
    }

    /// <summary>
    ///     Test cleanup - restore original GH_TOKEN.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        Environment.SetEnvironmentVariable("GH_TOKEN", _originalGhToken);
    }

    /// <summary>
    ///     Test that CreateAsync returns a connector instance.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectorFactory_CreateAsync_ReturnsConnector()
    {
        // Create a repository connector
        var connector = await RepoConnectorFactory.CreateAsync();

        // Verify connector is created successfully
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that CreateAsync returns GitHubRepoConnector for this repository.
    /// </summary>
    [TestMethod]
    public async Task RepoConnectorFactory_CreateAsync_ReturnsGitHubConnectorForThisRepo()
    {
        // Create connector for this repository
        var connector = await RepoConnectorFactory.CreateAsync();

        // Verify GitHub connector is returned
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }
}
