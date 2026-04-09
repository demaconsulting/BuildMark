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

namespace DemaConsulting.BuildMark.Configuration;

/// <summary>
///     Represents the severity of a configuration issue.
/// </summary>
public enum ConfigurationIssueSeverity
{
    /// <summary>
    ///     Indicates a warning that does not prevent execution.
    /// </summary>
    Warning,

    /// <summary>
    ///     Indicates an error that prevents execution.
    /// </summary>
    Error
}

/// <summary>
///     Represents one configuration load issue.
/// </summary>
/// <param name="FilePath">The source file path.</param>
/// <param name="Line">The 1-based line number.</param>
/// <param name="Severity">The issue severity.</param>
/// <param name="Description">The issue description.</param>
public sealed record ConfigurationIssue(
    string FilePath,
    int Line,
    ConfigurationIssueSeverity Severity,
    string Description);
