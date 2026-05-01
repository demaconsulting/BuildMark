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
using DemaConsulting.BuildMark.RepoConnectors.Mock;
using DemaConsulting.BuildMark.Version;

namespace DemaConsulting.BuildMark.Tests.RepoConnectors.Mock;

/// <summary>
///     Sub-subsystem tests for the Mock sub-subsystem.
///     These tests verify the contract exposed by the Mock sub-subsystem as a whole.
/// </summary>
[TestClass]
public class MockTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // BuildMark-Mock-SubSystem
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    ///     Test that the Mock sub-subsystem provides a connector that implements IRepoConnector.
    /// </summary>
    [TestMethod]
    public void Mock_ImplementsInterface_ReturnsTrue()
    {
        // Arrange: create a MockRepoConnector instance from the Mock sub-subsystem
        var connector = new MockRepoConnector();

        // Assert: the sub-subsystem connector satisfies the shared IRepoConnector interface
        Assert.IsInstanceOfType<IRepoConnector>(connector);
    }

    /// <summary>
    ///     Test that the Mock sub-subsystem returns build information with the expected version.
    /// </summary>
    /// <remarks>
    ///     What is being tested: Mock sub-subsystem produces version-correct build information
    ///     What the assertions prove: The connector version tag matches the requested version
    /// </remarks>
    [TestMethod]
    public async Task Mock_GetBuildInformation_ReturnsExpectedVersion()
    {
        // Arrange: create connector and specify a known version
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("v2.0.0");

        // Act: retrieve build information for the specified version
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert: build information contains the correct version tag
        Assert.IsNotNull(buildInfo);
        Assert.AreEqual(version.Tag, buildInfo.CurrentVersionTag.VersionTag.Tag);
    }

    /// <summary>
    ///     Test that the Mock sub-subsystem returns a complete BuildInformation structure.
    /// </summary>
    /// <remarks>
    ///     What is being tested: Mock sub-subsystem returns a fully populated BuildInformation
    ///     What the assertions prove: All required collections (Changes, Bugs, KnownIssues) are present
    /// </remarks>
    [TestMethod]
    public async Task Mock_GetBuildInformation_ReturnsCompleteInformation()
    {
        // Arrange: create connector with a version that has associated changes
        var connector = new MockRepoConnector();
        var version = VersionTag.Create("v2.0.0");

        // Act: retrieve build information
        var buildInfo = await connector.GetBuildInformationAsync(version);

        // Assert: all required data structures are present
        Assert.IsNotNull(buildInfo, "BuildInformation should not be null");
        Assert.IsNotNull(buildInfo.Changes, "Changes list should not be null");
        Assert.IsNotNull(buildInfo.Bugs, "Bugs list should not be null");
        Assert.IsNotNull(buildInfo.KnownIssues, "KnownIssues list should not be null");
        Assert.IsNotNull(buildInfo.CurrentVersionTag, "CurrentVersionTag should not be null");
    }
}
