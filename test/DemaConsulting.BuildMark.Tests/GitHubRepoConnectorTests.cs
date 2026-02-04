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
    ///     Test that GetPullRequestsBetweenTagsAsync returns expected PRs.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_ReturnsExpectedPRs()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        
        // Mock git rev-parse to indicate v2.0.0 tag exists
        connector.AddCommandResult("git", "rev-parse --verify v2.0.0", "abc123def456");
        
        // Mock GitHub API command to get commits between tags
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/compare/v1.0.0...v2.0.0 --jq .commits[].sha",
            "abc123def456\ndef456789abc");
        
        // Mock piped gh pr list command (stdin is the commit hashes from above)
        var stdinKey = "abc123def456\ndef456789abc";
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{stdinKey.Length}>",
            "10\n11");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(
            Version.Create("v1.0.0"), 
            Version.Create("v2.0.0"));

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
        
        // Mock git rev-parse to indicate v1.0.0 tag exists
        connector.AddCommandResult("git", "rev-parse --verify v1.0.0", "abc123def456");
        
        // Mock GitHub API command to get commits up to v1.0.0
        var commitOutput = "abc123def456";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/commits?sha=v1.0.0 --paginate --jq .[].sha",
            commitOutput);
        
        // Mock piped gh pr list command
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "10");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, Version.Create("v1.0.0"));

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
        
        // Mock git log for commit hashes
        // Mock GitHub API command to compare v1.0.0 to HEAD
        var commitOutput = "abc123def456";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/compare/v1.0.0...HEAD --jq .commits[].sha",
            commitOutput);
        
        // Mock piped gh pr list command
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "11");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(Version.Create("v1.0.0"), null);

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
        
        // Mock git log for commit hashes
        // Mock GitHub API command to get all commits
        var commitOutput = "abc123def456";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/commits --paginate --jq .[].sha",
            commitOutput);
        
        // Mock piped gh pr list command
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "12");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("12", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync uses HEAD when toTag doesn't exist.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_UsesHeadWhenToTagDoesNotExist()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        
        // Mock git rev-parse to indicate tag doesn't exist (throws exception)
        connector.AddCommandException("git", "rev-parse --verify 0.0.0-run.50");
        
        // Mock GitHub API command to compare v1.0.0 to HEAD (since 0.0.0-run.50 doesn't exist)
        var commitOutput = "abc123def456";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/compare/v1.0.0...HEAD --jq .commits[].sha",
            commitOutput);
        
        // Mock piped gh pr list command
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "15");

        // Act - using a version that doesn't exist as a tag
        var prs = await connector.GetPullRequestsBetweenTagsAsync(
            Version.Create("v1.0.0"), 
            Version.Create("0.0.0-run.50"));

        // Assert
        Assert.HasCount(1, prs);
        Assert.AreEqual("15", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync uses GitHub API to find PRs by commit hash.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_UsesGitHubApiToFindPRs()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        
        // Mock GitHub API command to get all commits
        var commitOutput = "abc123def456\ndef456789abc\n789abcdef123";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/commits --paginate --jq .[].sha",
            commitOutput);
        
        // Mock piped gh pr list command - returns all PRs
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "18\n19\n20");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(3, prs);
        Assert.AreEqual("18", prs[0]);
        Assert.AreEqual("19", prs[1]);
        Assert.AreEqual("20", prs[2]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync deduplicates PRs when multiple commits belong to same PR.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_DeduplicatesPRs()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        
        // Mock GitHub API command to get all commits (multiple commits from same PR)
        var commitOutput = "5e541195f387259ee8d72d33b70579a0f7b6fde4\nc3eb81cd24b9d054a626a9785b16975f0808ecb2";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/commits --paginate --jq .[].sha",
            commitOutput);
        
        // Mock piped gh pr list command - should deduplicate PR 20 that appears twice
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "20\n20"); // Duplicate PR 20

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(1, prs); // Should have deduplicated PR 20
        Assert.AreEqual("20", prs[0]);
    }

    /// <summary>
    ///     Test that GetPullRequestsBetweenTagsAsync handles commits that are in multiple PRs.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_HandlesCommitInMultiplePRs()
    {
        // Arrange
        var connector = new TestableGitHubRepoConnector();
        
        // Mock GitHub API command to get all commits
        var commitOutput = "91545652f4eeabfef6d7189ac4a3a859166655dc";
        connector.AddCommandResult(
            "gh",
            "api repos/:owner/:repo/commits --paginate --jq .[].sha",
            commitOutput);
        
        // Mock piped gh pr list command to return multiple PRs for commits
        connector.AddCommandResult(
            "gh",
            $"pr list --state all --json number --jq .[].number <stdin:{commitOutput.Length}>",
            "16\n20");

        // Act
        var prs = await connector.GetPullRequestsBetweenTagsAsync(null, null);

        // Assert
        Assert.HasCount(2, prs); // Should find both PR 16 and PR 20
        Assert.Contains("16", prs);
        Assert.Contains("20", prs);
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
            "pr view 10 --json closingIssuesReferences --jq .closingIssuesReferences[].number",
            "123\n456");

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
            "pr view 10 --json closingIssuesReferences --jq .closingIssuesReferences[].number",
            "");

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
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq .labels[].name", "bug\npriority:high");

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
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq .labels[].name", "enhancement");

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
        connector.AddCommandResult("gh", "issue view 123 --json labels --jq .labels[].name", "question");

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
    ///     Test that GetPullRequestsBetweenTagsAsync throws for invalid tag names.
    /// </summary>
    [TestMethod]
    public async Task GitHubRepoConnector_GetPullRequestsBetweenTagsAsync_ThrowsForInvalidTagName()
    {
        // Create connector and version with malicious tag name
        var connector = new TestableGitHubRepoConnector();
        var invalidVersion = new Version("v1.0.0; rm -rf /", "1.0.0", "1.0.0", string.Empty, string.Empty, false);

        // Verify exception is thrown for invalid tag
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
