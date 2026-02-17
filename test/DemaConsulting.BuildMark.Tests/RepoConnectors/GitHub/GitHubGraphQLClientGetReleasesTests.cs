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
///     Tests for the GitHubGraphQLClient GetReleasesAsync method.
/// </summary>
[TestClass]
public class GitHubGraphQLClientGetReleasesTests
{
    /// <summary>
    ///     Test that GetReleasesAsync returns expected release tag names with valid response.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_ValidResponse_ReturnsReleaseTagNames()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""releases"": {
                        ""nodes"": [
                            { ""tagName"": ""v1.0.0"" },
                            { ""tagName"": ""v0.9.0"" },
                            { ""tagName"": ""v0.8.5"" }
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
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.HasCount(3, releaseNodes);
        Assert.AreEqual("v1.0.0", releaseNodes[0].TagName);
        Assert.AreEqual("v0.9.0", releaseNodes[1].TagName);
        Assert.AreEqual("v0.8.5", releaseNodes[2].TagName);
    }

    /// <summary>
    ///     Test that GetReleasesAsync returns empty list when no releases are found.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_NoReleases_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""releases"": {
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
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.IsEmpty(releaseNodes);
    }

    /// <summary>
    ///     Test that GetReleasesAsync returns empty list when response has missing data.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_MissingData_ReturnsEmptyList()
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
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.IsEmpty(releaseNodes);
    }

    /// <summary>
    ///     Test that GetReleasesAsync returns empty list on HTTP error.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_HttpError_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = @"{ ""message"": ""Not Found"" }";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.NotFound);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.IsEmpty(releaseNodes);
    }

    /// <summary>
    ///     Test that GetReleasesAsync returns empty list on invalid JSON.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_InvalidJson_ReturnsEmptyList()
    {
        // Arrange
        var mockResponse = "This is not valid JSON";

        using var httpClient = CreateMockHttpClient(mockResponse, HttpStatusCode.OK);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.IsEmpty(releaseNodes);
    }

    /// <summary>
    ///     Test that GetReleasesAsync returns single release tag correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_SingleRelease_ReturnsOneTagName()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""releases"": {
                        ""nodes"": [
                            { ""tagName"": ""v2.0.0-beta1"" }
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
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.HasCount(1, releaseNodes);
        Assert.AreEqual("v2.0.0-beta1", releaseNodes[0].TagName);
    }

    /// <summary>
    ///     Test that GetReleasesAsync handles nodes with missing tagName property.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_MissingTagNameProperty_SkipsInvalidNodes()
    {
        // Arrange
        var mockResponse = @"{
            ""data"": {
                ""repository"": {
                    ""releases"": {
                        ""nodes"": [
                            { ""tagName"": ""v1.0.0"" },
                            { ""name"": ""Missing tag name"" },
                            { ""tagName"": ""v0.9.0"" }
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
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.HasCount(2, releaseNodes);
        Assert.AreEqual("v1.0.0", releaseNodes[0].TagName);
        Assert.AreEqual("v0.9.0", releaseNodes[1].TagName);
    }

    /// <summary>
    ///     Test that GetReleasesAsync handles pagination correctly.
    /// </summary>
    [TestMethod]
    public async Task GitHubGraphQLClient_GetReleasesAsync_WithPagination_ReturnsAllReleases()
    {
        // Arrange - Create mock handler that returns different responses for different pages
        var mockHandler = new ReleasePaginationMockHttpMessageHandler();
        using var httpClient = new HttpClient(mockHandler);
        using var client = new GitHubGraphQLClient(httpClient);

        // Act
        var releaseNodes = await client.GetReleasesAsync("owner", "repo");

        // Assert
        Assert.IsNotNull(releaseNodes);
        Assert.HasCount(3, releaseNodes);
        Assert.AreEqual("v3.0.0", releaseNodes[0].TagName);
        Assert.AreEqual("v2.0.0", releaseNodes[1].TagName);
        Assert.AreEqual("v1.0.0", releaseNodes[2].TagName);
    }

    /// <summary>
    ///     Mock HTTP message handler for testing release pagination.
    /// </summary>
    private sealed class ReleasePaginationMockHttpMessageHandler : HttpMessageHandler
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
                            ""releases"": {
                                ""nodes"": [
                                    { ""tagName"": ""v3.0.0"" }
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
                            ""releases"": {
                                ""nodes"": [
                                    { ""tagName"": ""v2.0.0"" }
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
                            ""releases"": {
                                ""nodes"": [
                                    { ""tagName"": ""v1.0.0"" }
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
