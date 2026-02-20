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
///     Tests for the GitHubGraphQLClient GetAllIssuesAsync method.
/// </summary>
[TestClass]
public class GitHubGraphQLClientGetAllIssuesTests
{
    /// <summary>
    ///     Test that GetAllIssuesAsync returns expected issues with valid response.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_ValidResponse_ReturnsIssues()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""issues"": {
                        ""nodes"": [
                            {
                                ""number"": 1,
                                ""title"": ""Bug: Application crashes on startup"",
                                ""url"": ""https://github.com/owner/repo/issues/1"",
                                ""state"": ""OPEN"",
                                ""labels"": {
                                    ""nodes"": [
                                        { ""name"": ""bug"" }
                                    ]
                                }
                            },
                            {
                                ""number"": 2,
                                ""title"": ""Feature: Add dark mode"",
                                ""url"": ""https://github.com/owner/repo/issues/2"",
                                ""state"": ""CLOSED"",
                                ""labels"": {
                                    ""nodes"": [
                                        { ""name"": ""feature"" },
                                        { ""name"": ""enhancement"" }
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
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.HasCount(2, issues);

        Assert.AreEqual(1, issues[0].Number);
        Assert.AreEqual("Bug: Application crashes on startup", issues[0].Title);
        Assert.AreEqual("https://github.com/owner/repo/issues/1", issues[0].Url);
        Assert.AreEqual("OPEN", issues[0].State);
        Assert.IsNotNull(issues[0].Labels?.Nodes);
        Assert.HasCount(1, issues[0].Labels!.Nodes!);
        Assert.AreEqual("bug", issues[0].Labels!.Nodes![0].Name);

        Assert.AreEqual(2, issues[1].Number);
        Assert.AreEqual("Feature: Add dark mode", issues[1].Title);
        Assert.AreEqual("https://github.com/owner/repo/issues/2", issues[1].Url);
        Assert.AreEqual("CLOSED", issues[1].State);
        Assert.IsNotNull(issues[1].Labels?.Nodes);
        Assert.HasCount(2, issues[1].Labels!.Nodes!);
        Assert.AreEqual("feature", issues[1].Labels!.Nodes![0].Name);
        Assert.AreEqual("enhancement", issues[1].Labels!.Nodes![1].Name);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync returns empty list when no issues are found.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_NoIssues_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""issues"": {
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
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync returns empty list when response has missing data.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_MissingData_ReturnsEmptyList()
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
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync handles null nodes gracefully.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_NullNodes_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""issues"": {
                        ""nodes"": null,
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
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync filters out issues with missing required fields.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_InvalidIssues_FiltersThemOut()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""issues"": {
                        ""nodes"": [
                            {
                                ""number"": null,
                                ""title"": ""Invalid issue - no number"",
                                ""url"": ""https://github.com/owner/repo/issues/invalid"",
                                ""state"": ""OPEN"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            },
                            {
                                ""number"": 1,
                                ""title"": null,
                                ""url"": ""https://github.com/owner/repo/issues/1"",
                                ""state"": ""OPEN"",
                                ""labels"": {
                                    ""nodes"": []
                                }
                            },
                            {
                                ""number"": 2,
                                ""title"": ""Valid issue"",
                                ""url"": ""https://github.com/owner/repo/issues/2"",
                                ""state"": ""OPEN"",
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
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.HasCount(1, issues);
        Assert.AreEqual(2, issues[0].Number);
        Assert.AreEqual("Valid issue", issues[0].Title);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync handles pagination correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_WithPagination_ReturnsAllIssues()
    {
        // Arrange - Create mock handler that returns different responses for different pages
        var mockHandler = new IssuePaginationMockHttpMessageHandler();
        using var httpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.HasCount(3, issues);
        Assert.AreEqual(1, issues[0].Number);
        Assert.AreEqual("Issue from page 1", issues[0].Title);
        Assert.AreEqual(2, issues[1].Number);
        Assert.AreEqual("Issue from page 2", issues[1].Title);
        Assert.AreEqual(3, issues[2].Number);
        Assert.AreEqual("Issue from page 3", issues[2].Title);
    }

    /// <summary>
    ///     Test that GetAllIssuesAsync returns empty list on exception.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetAllIssuesAsync_Exception_ReturnsEmptyList()
    {
        // Arrange
        using var httpClient = CreateMockHttpClient("invalid json", HttpStatusCode.InternalServerError);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var issues = await client.GetAllIssuesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(issues);
        Assert.IsEmpty(issues);
    }

    /// <summary>
    ///     Creates a mock HttpClient for testing.
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
    ///     Mock HTTP message handler for pagination testing.
    /// </summary>
    private sealed class IssuePaginationMockHttpMessageHandler : HttpMessageHandler
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
                            ""issues"": {
                                ""nodes"": [
                                    {
                                        ""number"": 1,
                                        ""title"": ""Issue from page 1"",
                                        ""url"": ""https://github.com/owner/repo/issues/1"",
                                        ""state"": ""OPEN"",
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
                            ""issues"": {
                                ""nodes"": [
                                    {
                                        ""number"": 2,
                                        ""title"": ""Issue from page 2"",
                                        ""url"": ""https://github.com/owner/repo/issues/2"",
                                        ""state"": ""CLOSED"",
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
                            ""issues"": {
                                ""nodes"": [
                                    {
                                        ""number"": 3,
                                        ""title"": ""Issue from page 3"",
                                        ""url"": ""https://github.com/owner/repo/issues/3"",
                                        ""state"": ""OPEN"",
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
            }

            _requestCount++;
            var content = new StringContent(responseContent, Encoding.UTF8, "application/json");
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = content
            };

            return response;
        }
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
