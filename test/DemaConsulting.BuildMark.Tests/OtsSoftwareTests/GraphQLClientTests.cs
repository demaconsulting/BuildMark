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

using System.Net;
using System.Text.Json;
using DemaConsulting.BuildMark.Tests.RepoConnectors.GitHub;
using GraphQL;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;

namespace DemaConsulting.BuildMark.Tests.OtsSoftwareTests;

/// <summary>
///     OTS tests for GraphQL.Client. These drive <see cref="GraphQLHttpClient" /> directly, without
///     the GitHubGraphQLClient wrapper that catches its exceptions, against a mocked HTTP endpoint.
/// </summary>
public class GraphQLClientTests
{
    /// <summary>
    ///     Test that GraphQL.Client raises an HTTP request exception for an HTTP error status.
    /// </summary>
    [Fact]
    public async Task GraphQLClient_SendQueryAsync_HttpError_ThrowsGraphQLHttpRequestException()
    {
        // Arrange
        using var handler = new MockGitHubGraphQLHttpMessageHandler()
            .SetDefaultResponse(@"{ ""message"": ""Not Found"" }", HttpStatusCode.NotFound);
        using var client = CreateClient(handler);

        // Act
        var exception = await Assert.ThrowsAsync<GraphQLHttpRequestException>(
            () => client.SendQueryAsync<JsonElement>(CreateRequest(), TestContext.Current.CancellationToken));

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, exception.StatusCode);
    }

    /// <summary>
    ///     Test that GraphQL.Client raises a JSON exception for a response that is not valid JSON.
    /// </summary>
    [Fact]
    public async Task GraphQLClient_SendQueryAsync_InvalidJson_ThrowsJsonException()
    {
        // Arrange
        using var handler = new MockGitHubGraphQLHttpMessageHandler()
            .SetDefaultResponse("This is not valid JSON");
        using var client = CreateClient(handler);

        // Act and Assert
        await Assert.ThrowsAnyAsync<JsonException>(
            () => client.SendQueryAsync<JsonElement>(CreateRequest(), TestContext.Current.CancellationToken));
    }

    /// <summary>
    ///     Creates a GraphQL HTTP client over the specified mocked handler.
    /// </summary>
    /// <param name="handler">The mocked HTTP message handler.</param>
    /// <returns>The GraphQL HTTP client.</returns>
    private static GraphQLHttpClient CreateClient(HttpMessageHandler handler)
    {
        var options = new GraphQLHttpClientOptions { EndPoint = new Uri("https://api.github.com/graphql") };
        return new GraphQLHttpClient(options, new SystemTextJsonSerializer(), new HttpClient(handler));
    }

    /// <summary>
    ///     Creates a minimal GraphQL request.
    /// </summary>
    /// <returns>The GraphQL request.</returns>
    private static GraphQLRequest CreateRequest()
    {
        return new GraphQLRequest { Query = "query { viewer { login } }" };
    }
}
