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
///     Testable GitHub repository connector that allows injecting command results.
/// </summary>
internal class TestableGitHubRepoConnector : GitHubRepoConnector
{
    private readonly Dictionary<string, string> _commandResults = new();
    private readonly HashSet<string> _commandExceptions = new();

    /// <summary>
    ///     Initializes a new instance of the <see cref="TestableGitHubRepoConnector" /> class for testing.
    /// </summary>
    public TestableGitHubRepoConnector()
    {
    }

    /// <summary>
    ///     Adds a command result for testing.
    /// </summary>
    /// <param name="command">Command name.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <param name="result">Expected result.</param>
    public void AddCommandResult(string command, string arguments, string result)
    {
        // Store command result for later retrieval
        _commandResults[$"{command} {arguments}"] = result;
    }

    /// <summary>
    ///     Adds a command that should throw an exception for testing.
    /// </summary>
    /// <param name="command">Command name.</param>
    /// <param name="arguments">Command arguments.</param>
    public void AddCommandException(string command, string arguments)
    {
        // Store command that should throw
        _commandExceptions.Add($"{command} {arguments}");
    }

    /// <summary>
    ///     Runs a command and returns its output.
    /// </summary>
    /// <param name="command">Command to run.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <param name="standardInput">Optional input to pipe to the command's stdin.</param>
    /// <returns>Command output.</returns>
    protected override Task<string> RunCommandAsync(string command, string arguments, string? standardInput = null)
    {
        // Look up pre-configured result for command
        // Include stdin in the key if provided to allow different results based on piped input
        var key = standardInput != null
            ? $"{command} {arguments} <stdin:{standardInput.Length}>"
            : $"{command} {arguments}";

        // Check if this command should throw
        if (_commandExceptions.Contains(key))
        {
            throw new InvalidOperationException($"Command failed: {key}");
        }

        if (_commandResults.TryGetValue(key, out var result))
        {
            return Task.FromResult(result);
        }

        // Throw if no result configured for command
        throw new InvalidOperationException($"No result configured for command: {key}");
    }
}

/// <summary>
///     Tests for the GitHubRepoConnector class.
/// </summary>
[TestClass]
public class GitHubRepoConnectorTests
{
    /// <summary>
    ///     Test that GetTagHistoryAsync returns expected tags.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetTagHistoryAsync_ReturnsExpectedTags()
    {
        // Configure connector to return mock tags
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "tag --sort=creatordate --merged HEAD", "v1.0.0\nv1.1.0\nv2.0.0");

        // Get tag history
        var tags = await connector.GetTagHistoryAsync();

        // Verify all tags are parsed correctly
        Assert.HasCount(3, tags);
        Assert.AreEqual("v1.0.0", tags[0].Tag);
        Assert.AreEqual("v1.1.0", tags[1].Tag);
        Assert.AreEqual("v2.0.0", tags[2].Tag);
    }

    /// <summary>
    ///     Test that GetTagHistoryAsync returns empty list when no tags.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetTagHistoryAsync_ReturnsEmptyListWhenNoTags()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "tag --sort=creatordate --merged HEAD", "");

        // Act
        var tags = await connector.GetTagHistoryAsync();

        // Assert
        Assert.IsEmpty(tags);
    }

    /// <summary>
    ///     Test that GetTagHistoryAsync returns empty list when no tags.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetHashForTagAsync_ReturnsExpectedHash()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "rev-parse v1.0.0", "abc123def456789");

        // Act
        var hash = await connector.GetHashForTagAsync(Version.Create("v1.0.0").Tag);

        // Assert
        Assert.AreEqual("abc123def456789", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns current hash for null tag.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetHashForTagAsync_ReturnsCurrentHashForNullTag()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "rev-parse HEAD", "current123hash456");

        // Act
        var hash = await connector.GetHashForTagAsync(null);

        // Assert
        Assert.AreEqual("current123hash456", hash);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync throws for invalid tag name.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetHashForTagAsync_ThrowsForInvalidTagName()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await connector.GetHashForTagAsync("v1.0.0 | echo pwned"));
        Assert.Contains("Invalid tag name", ex.Message);
    }
}
