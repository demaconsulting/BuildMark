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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Mock HTTP message handler for testing GitHub GraphQL API interactions.
/// </summary>
/// <remarks>
///     This class provides a reusable way to mock GitHub GraphQL API responses
///     for testing purposes. It can be configured to return specific responses
///     based on the request content or return a default response for all requests.
/// </remarks>
public sealed class MockGraphQLHttpMessageHandler : HttpMessageHandler
{
    /// <summary>
    ///     Dictionary mapping request patterns to response content.
    /// </summary>
    private readonly Dictionary<string, (string content, HttpStatusCode statusCode)> _responses = new();

    /// <summary>
    ///     Default response content to return when no pattern matches.
    /// </summary>
    private string? _defaultResponse;

    /// <summary>
    ///     Default HTTP status code to return with the default response.
    /// </summary>
    private HttpStatusCode _defaultStatusCode = HttpStatusCode.OK;

    /// <summary>
    ///     Adds a response that will be returned when the request body contains the specified pattern.
    /// </summary>
    /// <param name="requestPattern">Pattern to look for in the request body.</param>
    /// <param name="responseContent">Response content to return.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGraphQLHttpMessageHandler AddResponse(
        string requestPattern,
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _responses[requestPattern] = (responseContent, statusCode);
        return this;
    }

    /// <summary>
    ///     Sets the default response that will be returned when no pattern matches.
    /// </summary>
    /// <param name="responseContent">Default response content.</param>
    /// <param name="statusCode">HTTP status code to return (defaults to OK).</param>
    /// <returns>This instance for method chaining.</returns>
    public MockGraphQLHttpMessageHandler SetDefaultResponse(
        string responseContent,
        HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        _defaultResponse = responseContent;
        _defaultStatusCode = statusCode;
        return this;
    }

    /// <summary>
    ///     Sends a mock HTTP response based on the request content.
    /// </summary>
    /// <param name="request">HTTP request message.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Mock HTTP response.</returns>
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Read request body to determine which response to return
        var requestBody = request.Content != null
            ? await request.Content.ReadAsStringAsync(cancellationToken)
            : string.Empty;

        // Find matching response based on request pattern
        foreach (var (pattern, (content, statusCode)) in _responses)
        {
            if (requestBody.Contains(pattern, StringComparison.Ordinal))
            {
                return CreateResponse(content, statusCode);
            }
        }

        // Return default response if no pattern matches
        if (_defaultResponse != null)
        {
            return CreateResponse(_defaultResponse, _defaultStatusCode);
        }

        // Return 500 error if no response is configured
        return CreateResponse(
            @"{""errors"":[{""message"":""No mock response configured""}]}",
            HttpStatusCode.InternalServerError);
    }

    /// <summary>
    ///     Creates an HTTP response message with the specified content and status code.
    /// </summary>
    /// <param name="content">Response content.</param>
    /// <param name="statusCode">HTTP status code.</param>
    /// <returns>HTTP response message.</returns>
    private static HttpResponseMessage CreateResponse(string content, HttpStatusCode statusCode)
    {
        // Create response with content
        // Note: The returned HttpResponseMessage will be disposed by HttpClient,
        // which also disposes the Content. This is the expected pattern for HttpMessageHandler.
        var responseContent = new StringContent(content, Encoding.UTF8, "application/json");
        return new HttpResponseMessage(statusCode)
        {
            Content = responseContent
        };
    }
}
