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

    /// <summary>
    ///     Adds a command result for testing.
    /// </summary>
    /// <param name="command">Command name.</param>
    /// <param name="arguments">Command arguments.</param>
    /// <param name="result">Expected result.</param>
    public void AddCommandResult(string command, string arguments, string result)
    {
        _commandResults[$"{command} {arguments}"] = result;
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
        if (_commandResults.TryGetValue(key, out var result))
        {
            return Task.FromResult(result);
        }

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
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "tag --sort=creatordate --merged HEAD", "v1.0.0\nv1.1.0\nv2.0.0");

        // Act
        var tags = await connector.GetTagHistoryAsync();

        // Assert
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
    ///     Test that GetPullRequestsBetweenTagsAsync returns expected PRs.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_ReturnsExpectedPRs()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "git",
            "log --oneline --merges v1.0.0..v2.0.0",
            "abc123 Merge pull request #10 from feature/x\ndef456 Merge pull request #11 from bugfix/y");

        // Act
        var v1 = Version.Create("v1.0.0");
        var v2 = Version.Create("v2.0.0");
        Assert.IsNotNull(v1);
        Assert.IsNotNull(v2);
        var prs = await connector.GetPullRequestsBetweenTagsAsync(v1, v2);

        // Assert
        Assert.HasCount(2, prs);
        Assert.AreEqual("10", prs[0]);
        Assert.AreEqual("11", prs[1]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync handles null fromTag.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_HandlesNullFromTag()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "git",
            "log --oneline --merges v1.0.0",
            "abc123 Merge pull request #10 from feature/x");

        // Act
        var version = Version.Create("v1.0.0");
        Assert.IsNotNull(version);
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, version);

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("10", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync handles null toTag.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_HandlesNullToTag()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "git",
            "log --oneline --merges v1.0.0..HEAD",
            "abc123 Merge pull request #11 from feature/y");

        // Act
        var version = Version.Create("v1.0.0");
        Assert.IsNotNull(version);
        var prs = await connector.GetPullRequestsBetweenTagsAsync(version, null);

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("11", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync handles both null tags.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_HandlesBothNullTags()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "git",
            "log --oneline --merges HEAD",
            "abc123 Merge pull request #12 from feature/z");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("12", prs[0]);
    }

    /// <summary>
    ///     Test that GetIssuesForPullRequestAsync returns expected issues.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssuesForPullRequestAsync_ReturnsExpectedIssues()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "gh",
            "pr view 10 --json body --jq .body",
            "This PR fixes #123 and resolves #456");

        // Act
        var issues = await connector.GetIssuesForPullRequestAsync("10");

        // Assert
        Assert.HasCount(2, issues);
        Assert.AreEqual("123", issues[0]);
        Assert.AreEqual("456", issues[1]);
    }

    /// <summary>
    ///     Test that GetIssuesForPullRequestAsync returns empty when no issues.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssuesForPullRequestAsync_ReturnsEmptyWhenNoIssues()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult(
            "gh",
            "pr view 10 --json body --jq .body",
            "This PR has no issue references");

        // Act
        var issues = await connector.GetIssuesForPullRequestAsync("10");

        // Assert
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Test that GetIssueTitleAsync returns expected title.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTitleAsync_ReturnsExpectedTitle()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("gh", "issue view 123 --json title --jq .title", "Add new feature");

        // Act
        var title = await connector.GetIssueTitleAsync("123");

        // Assert
        Assert.AreEqual("Add new feature", title);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync returns bug for bug label.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTypeAsync_ReturnsBugForBugLabel()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq '.labels[].name'", "bug\npriority:high");

        // Act
        var type = await connector.GetIssueTypeAsync("123");

        // Assert
        Assert.AreEqual("bug", type);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync returns feature for enhancement label.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTypeAsync_ReturnsFeatureForEnhancementLabel()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq '.labels[].name'", "enhancement");

        // Act
        var type = await connector.GetIssueTypeAsync("123");

        // Assert
        Assert.AreEqual("feature", type);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync returns other for unknown label.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTypeAsync_ReturnsOtherForUnknownLabel()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq '.labels[].name'", "question");

        // Act
        var type = await connector.GetIssueTypeAsync("123");

        // Assert
        Assert.AreEqual("other", type);
    }

    /// <summary>
    ///     Test that GetHashForTagAsync returns expected hash.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetHashForTagAsync_ReturnsExpectedHash()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        connector.AddCommandResult("git", "rev-parse v1.0.0", "abc123def456789");

        // Act
        var version = Version.Create("v1.0.0");
        Assert.IsNotNull(version);
        var hash = await connector.GetHashForTagAsync(version.Tag);

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
    ///     Test that GetPullRequestsBetweenTagsAsync throws for invalid tag names.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_ThrowsForInvalidTagName()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        // Create a Version with an invalid tag name for testing validation
        var invalidVersion = new Version("v1.0.0; rm -rf /", "1.0.0", "1.0.0", string.Empty, string.Empty, false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await connector.GetPullRequestsBetweenTagsAsync(invalidVersion, null));
        Assert.Contains("Invalid tag name", ex.Message);
    }

    /// <summary>
    ///     Test that GetIssuesForPullRequestAsync throws for invalid PR ID.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssuesForPullRequestAsync_ThrowsForInvalidPRId()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await connector.GetIssuesForPullRequestAsync("10; cat /etc/passwd"));
        Assert.Contains("Invalid ID", ex.Message);
    }

    /// <summary>
    ///     Test that GetIssueTitleAsync throws for invalid issue ID.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTitleAsync_ThrowsForInvalidIssueId()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await connector.GetIssueTitleAsync("123; whoami"));
        Assert.Contains("Invalid ID", ex.Message);
    }

    /// <summary>
    ///     Test that GetIssueTypeAsync throws for invalid issue ID.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetIssueTypeAsync_ThrowsForInvalidIssueId()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ArgumentException>(
            async () => await connector.GetIssueTypeAsync("456 && echo hacked"));
        Assert.Contains("Invalid ID", ex.Message);
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
