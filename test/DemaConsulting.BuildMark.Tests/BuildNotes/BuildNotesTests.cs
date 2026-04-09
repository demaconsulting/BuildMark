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

using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.Tests.BuildNotes;

/// <summary>
///     Subsystem-level tests for the BuildNotes subsystem.
/// </summary>
[TestClass]
public class BuildNotesTests
{
    /// <summary>
    ///     Test that the BuildNotes subsystem generates correct markdown from a BuildInformation model.
    /// </summary>
    [TestMethod]
    public async Task BuildNotes_ReportModel_GeneratesCorrectMarkdown()
    {
        // Arrange: obtain a BuildInformation model from the mock connector
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Act: render the model to markdown
        var markdown = buildInfo.ToMarkdown();

        // Assert: verify all required sections are present in the rendered output
        Assert.Contains("# Build Report", markdown);
        Assert.Contains("## Version Information", markdown);
        Assert.Contains("## Changes", markdown);
        Assert.Contains("## Bugs Fixed", markdown);
        Assert.Contains("v2.0.0", markdown);
        Assert.Contains("ver-1.1.0", markdown);
    }

    /// <summary>
    ///     Test that the BuildNotes subsystem includes known issues in the rendered markdown when requested.
    /// </summary>
    [TestMethod]
    public async Task BuildNotes_ReportModel_IncludesKnownIssues()
    {
        // Arrange: obtain a BuildInformation model that has known issues
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Act: render with known issues enabled
        var markdown = buildInfo.ToMarkdown(includeKnownIssues: true);

        // Assert: known issues section is present and contains expected items
        Assert.Contains("## Known Issues", markdown);
        Assert.Contains("Known bug A", markdown);
        Assert.Contains("Known bug B", markdown);
    }

    /// <summary>
    ///     Test that the BuildNotes subsystem includes a full changelog link in the rendered markdown.
    /// </summary>
    [TestMethod]
    public async Task BuildNotes_ReportModel_IncludesFullChangelog()
    {
        // Arrange: obtain a BuildInformation model that has a changelog link
        var connector = new MockRepoConnector();
        var buildInfo = await connector.GetBuildInformationAsync(VersionTag.Create("v2.0.0"));

        // Act: render to markdown
        var markdown = buildInfo.ToMarkdown();

        // Assert: changelog section is present with comparison link
        Assert.Contains("## Full Changelog", markdown);
        Assert.Contains("ver-1.1.0...v2.0.0", markdown);
    }
}



