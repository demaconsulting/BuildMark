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

using System.Net;
using System.Text;
using DemaConsulting.BuildMark.RepoConnectors.GitHub;

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the GitHubGraphQLClient class.
/// </summary>
[TestClass]
public class GitHubGraphQLClientTests
{
    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns expected issue IDs with valid response.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_ValidResponse_ReturnsIssueIds()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequest"": {
                        ""closingIssuesReferences"": {
                            ""nodes"": [
                                { ""number"": 123 },
                                { ""number"": 456 },
                                { ""number"": 789 }
                            ],
                            ""pageInfo"": {
                                ""hasNextPage"": false,
                                ""endCursor"": null
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 42);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.HasCount(3, issueIds);
        Assert.AreEqual(123, issueIds[0]);
        Assert.AreEqual(456, issueIds[1]);
        Assert.AreEqual(789, issueIds[2]);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns empty list when no issues are linked.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_NoIssues_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequest"": {
                        ""closingIssuesReferences"": {
                            ""nodes"": [],
                            ""pageInfo"": {
                                ""hasNextPage"": false,
                                ""endCursor"": null
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 42);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.IsEmpty(issueIds);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns empty list when response has missing data.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_MissingData_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": null
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 42);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.IsEmpty(issueIds);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns empty list on HTTP error.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{ ""message"": ""Not Found"" }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.NotFound);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 42);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.IsEmpty(issueIds);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns empty list on invalid JSON.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = "This is not valid JSON";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 42);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.IsEmpty(issueIds);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync returns single issue ID correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_SingleIssue_ReturnsOneIssueId()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequest"": {
                        ""closingIssuesReferences"": {
                            ""nodes"": [
                                { ""number"": 999 }
                            ],
                            ""pageInfo"": {
                                ""hasNextPage"": false,
                                ""endCursor"": null
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 1);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.HasCount(1, issueIds);
        Assert.AreEqual(999, issueIds[0]);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync handles nodes with missing number property.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_MissingNumberProperty_SkipsInvalidNodes()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequest"": {
                        ""closingIssuesReferences"": {
                            ""nodes"": [
                                { ""number"": 100 },
                                { ""title"": ""Missing number"" },
                                { ""number"": 200 }
                            ],
                            ""pageInfo"": {
                                ""hasNextPage"": false,
                                ""endCursor"": null
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 5);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.HasCount(2, issueIds);
        Assert.AreEqual(100, issueIds[0]);
        Assert.AreEqual(200, issueIds[1]);
    }

    /// <summary>
    ///     Test that FindIssueIdsLinkedToPullRequestAsync handles pagination correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_FindIssueIdsLinkedToPullRequestAsync_WithPagination_ReturnsAllIssues()
    {
        // Arrange - Create mock handler that returns different responses for different pages
        var mockHandler = new PaginationMockHttpMessageHandler();
        using var httpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issueIds = await client.FindIssueIdsLinkedToPullRequestAsync("owner", "repo", 10);

        // Assert
        Assert.IsNotNull(issueIds);
        Assert.HasCount(3, issueIds);
        Assert.AreEqual(100, issueIds[0]);
        Assert.AreEqual(200, issueIds[1]);
        Assert.AreEqual(300, issueIds[2]);
    }

    /// <summary>
    ///     Creates a mock HttpClient with pre-canned response.
    /// </summary>
    /// <param name="responseContent">Response content to return.</param>
    /// <param name="statusCode">HTTP status code to return.</param>
    /// <returns>HttpClient configured with mock handler.</returns>
    private static HttpClient CreateMockHttpClient(string responseContent, HttpStatusCode statusCode)
    {
        var handler = new MockHttpMessageHandler(responseContent, statusCode);
        return new HttpClient(handler);
    }

    /// <summary>
    ///     Mock HTTP message handler for testing.
    /// </summary>
    private sealed class MockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        ///     Response content to return.
        /// </summary>
        private readonly string _responseContent;

        /// <summary>
        ///     HTTP status code to return.
        /// </summary>
        private readonly HttpStatusCode _statusCode;

        /// <summary>
        ///     Initializes a new instance of the <see cref="MockHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="responseContent">Response content to return.</param>
        /// <param name="statusCode">HTTP status code to return.</param>
        public MockHttpMessageHandler(string responseContent, HttpStatusCode statusCode)
        {
            _responseContent = responseContent;
            _statusCode = statusCode;
        }

        /// <summary>
        ///     Sends a mock HTTP response.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Mock HTTP response.</returns>
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Create response with content
            // Note: The returned HttpResponseMessage will be disposed by HttpClient,
            // which also disposes the Content. This is the expected pattern for HttpMessageHandler.
            var content = new StringContent(_responseContent, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = content
            };

            return Task.FromResult(response);
        }
    }

    /// <summary>
    ///     Mock HTTP message handler for testing pagination.
    /// </summary>
    private sealed class PaginationMockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        ///     Request count to track pagination.
        /// </summary>
        private int _requestCount;

        /// <summary>
        ///     Sends a mock HTTP response with pagination.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Mock HTTP response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Read request body to determine which page to return
            var requestBody = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;
            
            string responseContent;
            if (_requestCount == 0 || !requestBody.Contains("\"after\""))
            {
                // First page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""pullRequest"": {
                                ""closingIssuesReferences"": {
                                    ""nodes"": [
                                        { ""number"": 100 }
                                    ],
                                    ""pageInfo"": {
                                        ""hasNextPage"": true,
                                        ""endCursor"": ""cursor1""
                                    }
                                }
                            }
                        }
                    }
                }";
            }
            else if (requestBody.Contains("\"cursor1\""))
            {
                // Second page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""pullRequest"": {
                                ""closingIssuesReferences"": {
                                    ""nodes"": [
                                        { ""number"": 200 }
                                    ],
                                    ""pageInfo"": {
                                        ""hasNextPage"": true,
                                        ""endCursor"": ""cursor2""
                                    }
                                }
                            }
                        }
                    }
                }";
            }
            else
            {
                // Third (last) page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""pullRequest"": {
                                ""closingIssuesReferences"": {
                                    ""nodes"": [
                                        { ""number"": 300 }
                                    ],
                                    ""pageInfo"": {
                                        ""hasNextPage"": false,
                                        ""endCursor"": null
                                    }
                                }
                            }
                        }
                    }
                }";
            }

            _requestCount++;

            // Create response with content
            // Note: The returned HttpResponseMessage will be disposed by HttpClient,
            // which also disposes the Content. This is the expected pattern for HttpMessageHandler.
            var content = new StringContent(responseContent, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return response;
        }
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns expected commit SHAs with valid response.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_ValidResponse_ReturnsCommitShas()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""ref"": {
                        ""target"": {
                            ""history"": {
                                ""nodes"": [
                                    { ""oid"": ""abc123"" },
                                    { ""oid"": ""def456"" },
                                    { ""oid"": ""ghi789"" }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": false,
                                    ""endCursor"": null
                                }
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.HasCount(3, commitShas);
        Assert.AreEqual("abc123", commitShas[0]);
        Assert.AreEqual("def456", commitShas[1]);
        Assert.AreEqual("ghi789", commitShas[2]);
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns empty list when no commits are found.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_NoCommits_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""ref"": {
                        ""target"": {
                            ""history"": {
                                ""nodes"": [],
                                ""pageInfo"": {
                                    ""hasNextPage"": false,
                                    ""endCursor"": null
                                }
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.IsEmpty(commitShas);
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns empty list when response has missing data.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_MissingData_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": null
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.IsEmpty(commitShas);
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns empty list on HTTP error.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{ ""message"": ""Not Found"" }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.NotFound);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.IsEmpty(commitShas);
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns empty list on invalid JSON.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = "This is not valid JSON";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.IsEmpty(commitShas);
    }

    /// <summary>
    ///     Test that GetCommitsAsync returns single commit SHA correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_SingleCommit_ReturnsOneCommitSha()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""ref"": {
                        ""target"": {
                            ""history"": {
                                ""nodes"": [
                                    { ""oid"": ""abc123def456"" }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": false,
                                    ""endCursor"": null
                                }
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.HasCount(1, commitShas);
        Assert.AreEqual("abc123def456", commitShas[0]);
    }

    /// <summary>
    ///     Test that GetCommitsAsync handles nodes with missing oid property.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_MissingOidProperty_SkipsInvalidNodes()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""ref"": {
                        ""target"": {
                            ""history"": {
                                ""nodes"": [
                                    { ""oid"": ""commit1"" },
                                    { ""author"": ""missing oid"" },
                                    { ""oid"": ""commit2"" }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": false,
                                    ""endCursor"": null
                                }
                            }
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.HasCount(2, commitShas);
        Assert.AreEqual("commit1", commitShas[0]);
        Assert.AreEqual("commit2", commitShas[1]);
    }

    /// <summary>
    ///     Test that GetCommitsAsync handles pagination correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetCommitsAsync_WithPagination_ReturnsAllCommits()
    {
        // Arrange - Create mock handler that returns different responses for different pages
        var mockHandler = new CommitPaginationMockHttpMessageHandler();
        using var httpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var commitShas = await client.GetCommitsAsync("owner", "repo", "main");

        // Assert
        Assert.IsNotNull(commitShas);
        Assert.HasCount(3, commitShas);
        Assert.AreEqual("page1commit", commitShas[0]);
        Assert.AreEqual("page2commit", commitShas[1]);
        Assert.AreEqual("page3commit", commitShas[2]);
    }

    /// <summary>
    ///     Mock HTTP message handler for testing commit pagination.
    /// </summary>
    private sealed class CommitPaginationMockHttpMessageHandler : HttpMessageHandler
    {
        /// <summary>
        ///     Request count to track pagination.
        /// </summary>
        private int _requestCount;

        /// <summary>
        ///     Sends a mock HTTP response with pagination.
        /// </summary>
        /// <param name="request">HTTP request message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Mock HTTP response.</returns>
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            // Read request body to determine which page to return
            var requestBody = request.Content != null
                ? await request.Content.ReadAsStringAsync(cancellationToken)
                : string.Empty;
            
            string responseContent;
            if (_requestCount == 0 || !requestBody.Contains("\"after\""))
            {
                // First page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""ref"": {
                                ""target"": {
                                    ""history"": {
                                        ""nodes"": [
                                            { ""oid"": ""page1commit"" }
                                        ],
                                        ""pageInfo"": {
                                            ""hasNextPage"": true,
                                            ""endCursor"": ""cursor1""
                                        }
                                    }
                                }
                            }
                        }
                    }
                }";
            }
            else if (requestBody.Contains("\"cursor1\""))
            {
                // Second page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""ref"": {
                                ""target"": {
                                    ""history"": {
                                        ""nodes"": [
                                            { ""oid"": ""page2commit"" }
                                        ],
                                        ""pageInfo"": {
                                            ""hasNextPage"": true,
                                            ""endCursor"": ""cursor2""
                                        }
                                    }
                                }
                            }
                        }
                    }
                }";
            }
            else
            {
                // Third (last) page
                responseContent = @"{
                    ""data"": {
                        ""repository"": {
                            ""ref"": {
                                ""target"": {
                                    ""history"": {
                                        ""nodes"": [
                                            { ""oid"": ""page3commit"" }
                                        ],
                                        ""pageInfo"": {
                                            ""hasNextPage"": false,
                                            ""endCursor"": null
                                        }
                                    }
                                }
                            }
                        }
                    }
                }";
            }

            _requestCount++;

            // Create response with content
            // Note: The returned HttpResponseMessage will be disposed by HttpClient,
            // which also disposes the Content. This is the expected pattern for HttpMessageHandler.
            var content = new StringContent(responseContent, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return response;
        }
    }
}
