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
///     Tests for the GitHubGraphQLClient GetPullRequestsAsync method.
/// </summary>
[TestClass]
public class GitHubGraphQLClientGetPullRequestsTests
{
    /// <summary>
    ///     Test that GetPullRequestsAsync returns expected pull requests with valid response.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_ValidResponse_ReturnsPullRequests()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequests"": {
                        ""nodes"": [
                            {
                                ""number"": 1,
                                ""title"": ""Add feature A"",
                                ""url"": ""https://github.com/owner/repo/pull/1"",
                                ""merged"": true,
                                ""mergeCommit"": {
                                    ""oid"": ""abc123""
                                },
                                ""headRefOid"": ""def456"",
                                ""labels"": {
                                    ""nodes"": [
                                        { ""name"": ""feature"" }
                                    ]
                                }
                            },
                            {
                                ""number"": 2,
                                ""title"": ""Fix bug B"",
                                ""url"": ""https://github.com/owner/repo/pull/2"",
                                ""merged"": false,
                                ""mergeCommit"": null,
                                ""headRefOid"": ""ghi789"",
                                ""labels"": {
                                    ""nodes"": [
                                        { ""name"": ""bug"" }
                                    ]
                                }
                            }
                        ],
                        ""pageInfo"": {
                            ""hasNextPage"": false,
                            ""endCursor"": null
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.HasCount(2, pullRequests);

        Assert.AreEqual(1, pullRequests[0].Number);
        Assert.AreEqual("Add feature A", pullRequests[0].Title);
        Assert.AreEqual("https://github.com/owner/repo/pull/1", pullRequests[0].Url);
        Assert.IsTrue(pullRequests[0].Merged);
        Assert.AreEqual("abc123", pullRequests[0].MergeCommit?.Oid);
        Assert.AreEqual("def456", pullRequests[0].HeadRefOid);
        Assert.IsNotNull(pullRequests[0].Labels?.Nodes);
        Assert.HasCount(1, pullRequests[0].Labels!.Nodes!);
        Assert.AreEqual("feature", pullRequests[0].Labels!.Nodes![0].Name);

        Assert.AreEqual(2, pullRequests[1].Number);
        Assert.AreEqual("Fix bug B", pullRequests[1].Title);
        Assert.IsFalse(pullRequests[1].Merged);
        Assert.IsNull(pullRequests[1].MergeCommit);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync returns empty list when no pull requests are found.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_NoPullRequests_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequests"": {
                        ""nodes"": [],
                        ""pageInfo"": {
                            ""hasNextPage"": false,
                            ""endCursor"": null
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.IsEmpty(pullRequests);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync returns empty list when response has missing data.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_MissingData_ReturnsEmptyList()
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
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.IsEmpty(pullRequests);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync returns empty list on HTTP error.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{ ""message"": ""Not Found"" }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.NotFound);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.IsEmpty(pullRequests);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync returns empty list on invalid JSON.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = "This is not valid JSON";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.IsEmpty(pullRequests);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync returns single pull request correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_SinglePullRequest_ReturnsOnePullRequest()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequests"": {
                        ""nodes"": [
                            {
                                ""number"": 42,
                                ""title"": ""Important PR"",
                                ""url"": ""https://github.com/owner/repo/pull/42"",
                                ""merged"": true,
                                ""mergeCommit"": {
                                    ""oid"": ""merge123""
                                },
                                ""headRefOid"": ""head456"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            }
                        ],
                        ""pageInfo"": {
                            ""hasNextPage"": false,
                            ""endCursor"": null
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.HasCount(1, pullRequests);
        Assert.AreEqual(42, pullRequests[0].Number);
        Assert.AreEqual("Important PR", pullRequests[0].Title);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync handles nodes with missing number or title property.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_MissingNumberOrTitle_SkipsInvalidNodes()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""pullRequests"": {
                        ""nodes"": [
                            {
                                ""number"": 1,
                                ""title"": ""Valid PR"",
                                ""url"": ""https://github.com/owner/repo/pull/1"",
                                ""merged"": false,
                                ""mergeCommit"": null,
                                ""headRefOid"": ""abc123"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            },
                            {
                                ""title"": ""Missing number"",
                                ""url"": ""https://github.com/owner/repo/pull/2"",
                                ""merged"": false,
                                ""mergeCommit"": null,
                                ""headRefOid"": ""def456"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            },
                            {
                                ""number"": 3,
                                ""url"": ""https://github.com/owner/repo/pull/3"",
                                ""merged"": false,
                                ""mergeCommit"": null,
                                ""headRefOid"": ""ghi789"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            },
                            {
                                ""number"": 4,
                                ""title"": ""Another valid PR"",
                                ""url"": ""https://github.com/owner/repo/pull/4"",
                                ""merged"": false,
                                ""mergeCommit"": null,
                                ""headRefOid"": ""jkl012"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            }
                        ],
                        ""pageInfo"": {
                            ""hasNextPage"": false,
                            ""endCursor"": null
                        }
                    }
                }
            }
        }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.HasCount(2, pullRequests);
        Assert.AreEqual(1, pullRequests[0].Number);
        Assert.AreEqual("Valid PR", pullRequests[0].Title);
        Assert.AreEqual(4, pullRequests[1].Number);
        Assert.AreEqual("Another valid PR", pullRequests[1].Title);
    }

    /// <summary>
    ///     Test that GetPullRequestsAsync handles pagination correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetPullRequestsAsync_WithPagination_ReturnsAllPullRequests()
    {
        // Arrange - Create mock handler that returns different responses for different pages
        var mockHandler = new PullRequestPaginationMockHttpMessageHandler();
        using var httpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var pullRequests = await client.GetPullRequestsAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(pullRequests);
        Assert.HasCount(3, pullRequests);
        Assert.AreEqual(1, pullRequests[0].Number);
        Assert.AreEqual("PR from page 1", pullRequests[0].Title);
        Assert.AreEqual(2, pullRequests[1].Number);
        Assert.AreEqual("PR from page 2", pullRequests[1].Title);
        Assert.AreEqual(3, pullRequests[2].Number);
        Assert.AreEqual("PR from page 3", pullRequests[2].Title);
    }

    /// <summary>
    ///     Mock HTTP message handler for testing pull request pagination.
    /// </summary>
    private sealed class PullRequestPaginationMockHttpMessageHandler : HttpMessageHandler
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
                            ""pullRequests"": {
                                ""nodes"": [
                                    {
                                        ""number"": 1,
                                        ""title"": ""PR from page 1"",
                                        ""url"": ""https://github.com/owner/repo/pull/1"",
                                        ""merged"": false,
                                        ""mergeCommit"": null,
                                        ""headRefOid"": ""page1"",
                                        ""labels"": {
                                            ""nodes"": []
                                        }
                                    }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": true,
                                    ""endCursor"": ""cursor1""
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
                            ""pullRequests"": {
                                ""nodes"": [
                                    {
                                        ""number"": 2,
                                        ""title"": ""PR from page 2"",
                                        ""url"": ""https://github.com/owner/repo/pull/2"",
                                        ""merged"": false,
                                        ""mergeCommit"": null,
                                        ""headRefOid"": ""page2"",
                                        ""labels"": {
                                            ""nodes"": []
                                        }
                                    }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": true,
                                    ""endCursor"": ""cursor2""
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
                            ""pullRequests"": {
                                ""nodes"": [
                                    {
                                        ""number"": 3,
                                        ""title"": ""PR from page 3"",
                                        ""url"": ""https://github.com/owner/repo/pull/3"",
                                        ""merged"": false,
                                        ""mergeCommit"": null,
                                        ""headRefOid"": ""page3"",
                                        ""labels"": {
                                            ""nodes"": []
                                        }
                                    }
                                ],
                                ""pageInfo"": {
                                    ""hasNextPage"": false,
                                    ""endCursor"": null
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
}
