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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the MockRepoConnector class.
/// </summary>
[TestClass]
public class MockRepoConnectorTests
{
    /// <summary>
    ///     Test that MockRepoConnector can be instantiated.
    /// </summary>
    [TestMethod]
    public void MockRepoConnector_Constructor_CreatesInstance()
    {
        // Create connector
        var connector = new MockRepoConnector();

        // Verify instance
        Assert.IsNotNull(connector);
        Assert.IsInstanceOfType<MockRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that MockRepoConnector implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void MockRepoConnector_ImplementsInterface()
    {
        // Create connector
        var connector = new MockRepoConnector();

        // Verify interface implementation
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that GetBuildInformationAsync returns build information with expected version.
    /// </summary>
    [TestMethod]
    public async Task MockRepoConnector_GetBuildInformationAsync_ReturnsExpectedVersion()
    {
        // Arrange
        var connector = new MockRepoConnector();
        var version = Version.Create("v2.0.0");

        // Act
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual(version.Tag, buildInfo.ToVersion.Tag);
    }
}
