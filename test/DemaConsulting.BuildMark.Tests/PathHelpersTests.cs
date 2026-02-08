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

namespace DemaConsulting.BuildMark.Tests;

/// <summary>
///     Tests for the PathHelpers class.
/// </summary>
[TestClass]
public class PathHelpersTests
{
    /// <summary>
    ///     Test that SafePathCombine correctly combines valid paths.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_ValidPaths_CombinesCorrectly()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "subfolder/file.txt";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert
        Assert.AreEqual(Path.Combine(basePath, relativePath), result);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path traversal with double dots.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_PathTraversalWithDoubleDots_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "../etc/passwd";

        ArgumentException? caughtException = null;

        try
        {
            // Act
            PathHelpers.SafePathCombine(basePath, relativePath);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception
        Assert.IsNotNull(caughtException);
        Assert.Contains("Invalid path component", caughtException.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for rooted path.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_RootedPath_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "/etc/passwd";

        ArgumentException? caughtException = null;

        try
        {
            // Act
            PathHelpers.SafePathCombine(basePath, relativePath);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception
        Assert.IsNotNull(caughtException);
        Assert.Contains("Invalid path component", caughtException.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for Windows rooted path.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_WindowsRootedPath_ThrowsArgumentException()
    {
        // Skip test on non-Windows platforms where Windows paths are not considered rooted
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive("Test only applies on Windows");
            return;
        }

        // Arrange
        var basePath = "C:\\Users\\project";
        var relativePath = "C:\\Windows\\System32\\file.txt";

        ArgumentException? caughtException = null;

        try
        {
            // Act
            PathHelpers.SafePathCombine(basePath, relativePath);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception
        Assert.IsNotNull(caughtException);
        Assert.Contains("Invalid path component", caughtException.Message);
    }

    /// <summary>
    ///     Test that SafePathCombine throws ArgumentException for path with double dots in middle.
    /// </summary>
    [TestMethod]
    public void PathHelpers_SafePathCombine_DoubleDotsInMiddle_ThrowsArgumentException()
    {
        // Arrange
        var basePath = "/home/user/project";
        var relativePath = "subfolder/../../../etc/passwd";

        ArgumentException? caughtException = null;

        try
        {
            // Act
            PathHelpers.SafePathCombine(basePath, relativePath);

            // Assert - Fail if no exception is thrown
            Assert.Fail("Expected ArgumentException to be thrown");
        }
        catch (ArgumentException ex)
        {
            // Store exception for verification
            caughtException = ex;
        }

        // Assert - Verify exception
        Assert.IsNotNull(caughtException);
        Assert.Contains("Invalid path component", caughtException.Message);
    }
}
