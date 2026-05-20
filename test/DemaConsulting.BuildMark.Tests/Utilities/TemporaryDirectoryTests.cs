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

using DemaConsulting.BuildMark.Utilities;

namespace DemaConsulting.BuildMark.Tests.Utilities;

/// <summary>
///     Unit tests for the TemporaryDirectory class.
/// </summary>
[Collection("Sequential")]
public class TemporaryDirectoryTests
{
    /// <summary>
    ///     Test that the constructor creates the directory on disk.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_CreatesDirectory()
    {
        // Act
        using var tmpDir = new TemporaryDirectory();

        // Assert
        Assert.True(Directory.Exists(tmpDir.DirectoryPath),
            "Directory should exist after construction.");
    }

    /// <summary>
    ///     Test that two instances produce distinct directory paths.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_CreatesUniqueDirectories()
    {
        // Act
        using var tmpDir1 = new TemporaryDirectory();
        using var tmpDir2 = new TemporaryDirectory();

        // Assert
        Assert.NotEqual(tmpDir1.DirectoryPath, tmpDir2.DirectoryPath);
    }

    /// <summary>
    ///     Test that GetFilePath returns a path located under the temporary directory.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory()
    {
        // Arrange
        using var tmpDir = new TemporaryDirectory();

        // Act
        var filePath = tmpDir.GetFilePath("output.md");

        // Assert
        Assert.StartsWith(tmpDir.DirectoryPath, filePath, StringComparison.OrdinalIgnoreCase);
        Assert.EndsWith("output.md", filePath, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     Test that GetFilePath with a nested relative path creates intermediate subdirectories.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories()
    {
        // Arrange
        using var tmpDir = new TemporaryDirectory();

        // Act
        var filePath = tmpDir.GetFilePath(Path.Combine("sub", "nested", "output.md"));

        // Assert: intermediate directories were created
        Assert.True(Directory.Exists(Path.GetDirectoryName(filePath)),
            "Intermediate subdirectories should be created by GetFilePath.");
    }

    /// <summary>
    ///     Test that GetFilePath rejects a path-traversal attempt with ArgumentException.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException()
    {
        // Arrange
        using var tmpDir = new TemporaryDirectory();

        // Act + Assert
        Assert.Throws<ArgumentException>(() => tmpDir.GetFilePath("../escaped.txt"));
    }

    /// <summary>
    ///     Test that Dispose deletes the temporary directory and its contents.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_DeletesDirectory()
    {
        // Arrange
        string dirPath;
        using (var tmpDir = new TemporaryDirectory())
        {
            dirPath = tmpDir.DirectoryPath;
            File.WriteAllText(tmpDir.GetFilePath("file.txt"), "content");
        }

        // Assert
        Assert.False(Directory.Exists(dirPath),
            "Directory should be deleted after disposal.");
    }

    /// <summary>
    ///     Test that Dispose is safe to call when the directory has already been deleted.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow()
    {
        // Arrange
        var tmpDir = new TemporaryDirectory();
        Directory.Delete(tmpDir.DirectoryPath, recursive: true);

        // Act + Assert: second disposal should not throw
        var exception = Record.Exception(() => tmpDir.Dispose());
        Assert.Null(exception);
    }
}
