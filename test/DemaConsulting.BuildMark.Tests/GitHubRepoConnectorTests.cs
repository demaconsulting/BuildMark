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

using DemaConsulting.BuildMark.RepoConnectors;
using NSubstitute;
using Octokit;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the GitHubRepoConnector class.
/// </summary>
[TestClass]
public class GitHubRepoConnectorTests
{
    /// <summary>
    ///     Test that GitHubRepoConnector can be instantiated.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_Constructor_CreatesInstance()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify instance
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<GitHubRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GitHubRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ImplementsInterface_ReturnsTrue()
    {
        // Create connector
        var connector = new GitHubRepoConnector();

        // Verify interface implementation
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that ParseGitHubUrl correctly parses SSH URL.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_SshUrl_ReturnsOwnerAndRepo()
    {
        // Arrange
        var url = "git@github.com:owner/repo.git";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseGitHubUrl(url);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseGitHubUrl correctly parses HTTPS URL.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_HttpsUrl_ReturnsOwnerAndRepo()
    {
        // Arrange
        var url = "https://github.com/owner/repo.git";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseGitHubUrl(url);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseGitHubUrl correctly parses URL without .git suffix.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_NoGitSuffix_ReturnsOwnerAndRepo()
    {
        // Arrange
        var url = "https://github.com/owner/repo";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseGitHubUrl(url);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseGitHubUrl throws ArgumentException for invalid URL format.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_InvalidUrl_ThrowsArgumentException()
    {
        // Arrange
        var url = "invalid-url";

        try
        {
            // Act
            GitHubRepoConnector.ParseGitHubUrl(url);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception for invalid URL format
        }
    }

    /// <summary>
    ///     Test that ParseGitHubUrl throws ArgumentException for URL with wrong host.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_WrongHost_ThrowsArgumentException()
    {
        // Arrange
        var url = "https://gitlab.com/owner/repo.git";

        try
        {
            // Act
            GitHubRepoConnector.ParseGitHubUrl(url);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception for wrong host
        }
    }

    /// <summary>
    ///     Test that ParseGitHubUrl handles URL with whitespace.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseGitHubUrl_UrlWithWhitespace_ReturnsOwnerAndRepo()
    {
        // Arrange
        var url = "  https://github.com/owner/repo.git  ";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseGitHubUrl(url);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseOwnerRepo correctly parses path with .git suffix.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseOwnerRepo_PathWithGitSuffix_ReturnsOwnerAndRepo()
    {
        // Arrange
        var path = "owner/repo.git";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseOwnerRepo(path);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseOwnerRepo correctly parses path without .git suffix.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseOwnerRepo_PathWithoutGitSuffix_ReturnsOwnerAndRepo()
    {
        // Arrange
        var path = "owner/repo";

        // Act
        var (owner, repo) = GitHubRepoConnector.ParseOwnerRepo(path);

        // Assert
        Assert.AreEqual("owner", owner);
        Assert.AreEqual("repo", repo);
    }

    /// <summary>
    ///     Test that ParseOwnerRepo throws ArgumentException for invalid path.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseOwnerRepo_InvalidPath_ThrowsArgumentException()
    {
        // Arrange
        var path = "invalid";

        try
        {
            // Act
            GitHubRepoConnector.ParseOwnerRepo(path);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception for invalid path format
        }
    }

    /// <summary>
    ///     Test that ParseOwnerRepo throws ArgumentException for path with too many segments.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_ParseOwnerRepo_TooManySegments_ThrowsArgumentException()
    {
        // Arrange
        var path = "owner/repo/extra";

        try
        {
            // Act
            GitHubRepoConnector.ParseOwnerRepo(path);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException)
        {
            // Expected exception for too many path segments
        }
    }

    /// <summary>
    ///     Test that GetTypeFromLabels returns "other" for empty labels.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_GetTypeFromLabels_EmptyLabels_ReturnsOther()
    {
        // Arrange
        var labels = new List<Label>();

        // Act
        var type = GitHubRepoConnector.GetTypeFromLabels(labels);

        // Assert
        Assert.AreEqual("other", type);
    }

    /// <summary>
    ///     Test that GetCommitsInRange returns empty list when toHash not found.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_GetCommitsInRange_ToHashNotFound_ReturnsEmptyList()
    {
        // Arrange - empty list of commits
        var commits = new List<GitHubCommit>();

        // Act
        var result = GitHubRepoConnector.GetCommitsInRange(commits, "hash1", "hash4");

        // Assert
        Assert.IsEmpty(result);
    }

    /// <summary>
    ///     Test that GenerateGitHubChangelogLink creates correct link.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_GenerateGitHubChangelogLink_ValidTags_ReturnsWebLink()
    {
        // Act
        var link = GitHubRepoConnector.GenerateGitHubChangelogLink("owner", "repo", "v1.0.0", "v2.0.0");

        // Assert
        Assert.IsNotNull(link);
        Assert.AreEqual("v1.0.0...v2.0.0", link.LinkText);
        Assert.AreEqual("https://github.com/owner/repo/compare/v1.0.0...v2.0.0", link.TargetUrl);
    }

    /// <summary>
    ///     Test that GenerateGitHubChangelogLink returns null when oldTag is null.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_GenerateGitHubChangelogLink_NullOldTag_ReturnsNull()
    {
        // Act
        var link = GitHubRepoConnector.GenerateGitHubChangelogLink("owner", "repo", null, "v2.0.0");

        // Assert
        Assert.IsNull(link);
    }

    /// <summary>
    ///     Test that DetermineTargetVersion returns provided version when specified.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineTargetVersion_ProvidedVersion_ReturnsProvidedVersion()
    {
        // Arrange
        var version = Version.Create("v1.0.0");
        var currentHash = "abc123";
        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [],
            [],
            [],
            []);

        // Act
        var (toVersion, toHash) = GitHubRepoConnector.DetermineTargetVersion(version, currentHash, lookupData);

        // Assert
        Assert.AreEqual(version, toVersion);
        Assert.AreEqual(currentHash, toHash);
    }

    /// <summary>
    ///     Test that DetermineTargetVersion throws when no version and no releases.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineTargetVersion_NoVersionNoReleases_ThrowsInvalidOperationException()
    {
        // Arrange
        var currentHash = "abc123";
        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [],
            [],
            [],
            []);

        InvalidOperationException? caughtException = null;

        try
        {
            // Act
            GitHubRepoConnector.DetermineTargetVersion(null, currentHash, lookupData);

            // Fail if no exception is thrown
            Assert.Fail("Expected InvalidOperationException to be thrown");
        }
        catch (InvalidOperationException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception message contains expected text
        Assert.IsNotNull(caughtException);
        Assert.Contains("No releases found", caughtException.Message);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion returns null when no releases.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_NoReleases_ReturnsNull()
    {
        // Arrange
        var toVersion = Version.Create("v1.0.0");
        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [],
            [],
            [],
            []);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.IsNull(fromVersion);
        Assert.IsNull(fromHash);
    }

    /// <summary>
    ///     Test that DetermineTargetVersion throws when current commit doesn't match latest tag.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineTargetVersion_CommitNotMatchingLatestTag_ThrowsInvalidOperationException()
    {
        // Arrange
        var currentHash = "different123";
        var tag1 = CreateMockTag("v1.0.0", "commit1");
        var release1 = CreateMockRelease("v1.0.0");
        var releases = new List<Release> { release1 };
        var version1 = Version.Create("v1.0.0");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            releases,
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 } },
            [version1]);

        InvalidOperationException? caughtException = null;

        try
        {
            // Act
            GitHubRepoConnector.DetermineTargetVersion(null, currentHash, lookupData);

            // Fail if no exception is thrown
            Assert.Fail("Expected InvalidOperationException to be thrown");
        }
        catch (InvalidOperationException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception message contains expected text
        Assert.IsNotNull(caughtException);
        Assert.Contains("does not match any release tag", caughtException.Message);
    }

    /// <summary>
    ///     Test that DetermineTargetVersion uses latest release when commit matches.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineTargetVersion_CommitMatchesLatestTag_ReturnsLatestVersion()
    {
        // Arrange
        var currentHash = "commit1";
        var tag1 = CreateMockTag("v1.0.0", currentHash);
        var release1 = CreateMockRelease("v1.0.0");
        var releases = new List<Release> { release1 };
        var version1 = Version.Create("v1.0.0");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            releases,
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 } },
            [version1]);

        // Act
        var (toVersion, toHash) = GitHubRepoConnector.DetermineTargetVersion(null, currentHash, lookupData);

        // Assert
        Assert.AreEqual(version1, toVersion);
        Assert.AreEqual(currentHash, toHash);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion handles pre-release with previous version.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_PreReleaseWithPreviousVersion_ReturnsPreviousVersion()
    {
        // Arrange
        var toVersion = Version.Create("v2.0.0-beta1");
        var v1 = Version.Create("v1.0.0");
        var v2Beta = Version.Create("v2.0.0-beta1");

        var release1 = CreateMockRelease("v1.0.0");
        var release2 = CreateMockRelease("v2.0.0-beta1");
        var tag1 = CreateMockTag("v1.0.0", "hash1");
        var tag2 = CreateMockTag("v2.0.0-beta1", "hash2");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [release2, release1],
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 }, { "v2.0.0-beta1", tag2 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 }, { "v2.0.0-beta1", release2 } },
            [v2Beta, v1]);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.AreEqual(v1, fromVersion);
        Assert.AreEqual("hash1", fromHash);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion handles release skipping pre-releases.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_ReleaseSkipsPreReleases_ReturnsPreviousRelease()
    {
        // Arrange
        var toVersion = Version.Create("v2.0.0");
        var v1 = Version.Create("v1.0.0");
        var v2Beta = Version.Create("v2.0.0-beta1");
        var v2 = Version.Create("v2.0.0");

        var release1 = CreateMockRelease("v1.0.0");
        var release2Beta = CreateMockRelease("v2.0.0-beta1");
        var release2 = CreateMockRelease("v2.0.0");
        var tag1 = CreateMockTag("v1.0.0", "hash1");
        var tag2Beta = CreateMockTag("v2.0.0-beta1", "hash2beta");
        var tag2 = CreateMockTag("v2.0.0", "hash2");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [release2, release2Beta, release1],
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 }, { "v2.0.0-beta1", tag2Beta }, { "v2.0.0", tag2 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 }, { "v2.0.0-beta1", release2Beta }, { "v2.0.0", release2 } },
            [v2, v2Beta, v1]);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.AreEqual(v1, fromVersion);
        Assert.AreEqual("hash1", fromHash);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion returns null when version not in history and is release.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_NewReleaseNotInHistory_ReturnsLatestRelease()
    {
        // Arrange
        var toVersion = Version.Create("v3.0.0");
        var v1 = Version.Create("v1.0.0");
        var v2 = Version.Create("v2.0.0");

        var release1 = CreateMockRelease("v1.0.0");
        var release2 = CreateMockRelease("v2.0.0");
        var tag1 = CreateMockTag("v1.0.0", "hash1");
        var tag2 = CreateMockTag("v2.0.0", "hash2");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [release2, release1],
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 }, { "v2.0.0", tag2 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 }, { "v2.0.0", release2 } },
            [v2, v1]);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.AreEqual(v2, fromVersion);
        Assert.AreEqual("hash2", fromHash);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion returns null when version is oldest.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_OldestVersion_ReturnsNull()
    {
        // Arrange
        var toVersion = Version.Create("v1.0.0");
        var v1 = Version.Create("v1.0.0");

        var release1 = CreateMockRelease("v1.0.0");
        var tag1 = CreateMockTag("v1.0.0", "hash1");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [release1],
            new Dictionary<string, RepositoryTag> { { "v1.0.0", tag1 } },
            new Dictionary<string, Release> { { "v1.0.0", release1 } },
            [v1]);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.IsNull(fromVersion);
        Assert.IsNull(fromHash);
    }

    /// <summary>
    ///     Test that DetermineBaselineVersion returns null hash when tag not found.
    /// </summary>
    [TestMethod]
    public void GitHubRepoConnector_DetermineBaselineVersion_TagNotFound_ReturnsNullHash()
    {
        // Arrange
        var toVersion = Version.Create("v2.0.0");
        var v1 = Version.Create("v1.0.0");
        var v2 = Version.Create("v2.0.0");

        var release1 = CreateMockRelease("v1.0.0");
        var release2 = CreateMockRelease("v2.0.0");

        var lookupData = new GitHubRepoConnector.LookupData(
            [],
            [],
            [release2, release1],
            [],  // Empty tags dictionary
            new Dictionary<string, Release> { { "v1.0.0", release1 }, { "v2.0.0", release2 } },
            [v2, v1]);

        // Act
        var (fromVersion, fromHash) = GitHubRepoConnector.DetermineBaselineVersion(toVersion, lookupData);

        // Assert
        Assert.AreEqual(v1, fromVersion);
        Assert.IsNull(fromHash);
    }

    /// <summary>
    ///     Helper method to create a mock RepositoryTag using NSubstitute.
    /// </summary>
    private static RepositoryTag CreateMockTag(string name, string sha)
    {
        var commitRef = Substitute.For<GitReference>();
        commitRef.Sha.Returns(sha);

        var tag = Substitute.For<RepositoryTag>();
        tag.Name.Returns(name);
        tag.Commit.Returns(commitRef);

        return tag;
    }

    /// <summary>
    ///     Helper method to create a mock Release using NSubstitute.
    /// </summary>
    private static Release CreateMockRelease(string tagName)
    {
        var release = Substitute.For<Release>();
        release.TagName.Returns(tagName);
        release.Prerelease.Returns(tagName.Contains("-"));
        return release;
    }
}
