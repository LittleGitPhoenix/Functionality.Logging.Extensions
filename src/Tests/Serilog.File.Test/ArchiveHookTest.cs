using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.File;

namespace Serilog.File.Test;

public class ArchiveHookTest
{
    [SetUp]
    public void Setup() { }

    [Test]
    public async Task Check_If_Old_Files_Are_Deleted()
    {
        // Arrange
        var amountOfFilesToKeep = 3;
        var amountOfFilesToCreate = 8;
        using var testDirectory = new TestDirectory();
        for (int i = amountOfFilesToCreate - 1; i >= 0; i--)
        {
            // Create the files with a little delay, so their creation date differs enough.
            await Task.Run
            (
                async () =>
                {
                    _ = testDirectory.CreateFile($"{i:D3}.zip", "");
                    await Task.Delay(200);
                }
            );
        }
			
        // Act
        var deletedFileAmount = ArchiveHook.DeleteOldFiles(testDirectory.Directory, amountOfFilesToKeep);
			
        // Assert
        Assert.That(deletedFileAmount, Is.EqualTo(amountOfFilesToCreate - amountOfFilesToKeep));
        var remainingFiles = testDirectory.Directory.GetFiles();
        Assert.That(remainingFiles, Has.Length.EqualTo(amountOfFilesToKeep));
    }

    [Test]
    public void Check_If_Archiving_File()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var logFile = testDirectory.CreateFile("some.log", "");

        // Act
        var zipFile = ArchiveHook.CreateZipFile(logFile, System.IO.Compression.CompressionLevel.Optimal, testDirectory.Directory);

        // Assert
        Assert.That(zipFile.Exists, Is.True);
        Assert.That(testDirectory.Directory.GetFiles(), Has.Length.EqualTo(2));
    }

    [Test]
    public void Check_If_Archiving_File_To_Another_Directory()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var archiveDirectory = testDirectory.CreateDirectory("archive");
        var logFile = testDirectory.CreateFile("some.log", "");

        // Act
        var zipFile = ArchiveHook.CreateZipFile(logFile, System.IO.Compression.CompressionLevel.Optimal, archiveDirectory);

        // Assert
        Assert.That(zipFile.Exists, Is.True);
        Assert.That(testDirectory.Directory.GetFiles(), Has.Length.EqualTo(1));
        Assert.That(testDirectory.Directory.GetDirectories(), Has.Length.EqualTo(1));
    }

    [Test]
    public void Check_If_OnFileDeleting_Archives_File_In_Log_File_Directory()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var logFile = testDirectory.CreateFile("some.log", "content");
        var archiveHook = new ArchiveHook();

        // Act
        archiveHook.OnFileDeleting(logFile.FullName);

        // Assert
        var archiveFile = new FileInfo(Path.Combine(testDirectory.Directory.FullName, "some.zip"));
        Assert.That(archiveFile.Exists, Is.True);
        using var archive = System.IO.Compression.ZipFile.OpenRead(archiveFile.FullName);
        Assert.That(archive.Entries, Has.Count.EqualTo(1));
    }

    [Test]
    public void Check_If_OnFileDeleting_Creates_Configured_Archive_Directory()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var archiveDirectory = new DirectoryInfo(Path.Combine(testDirectory.Directory.FullName, "archive"));
        var logFile = testDirectory.CreateFile("some.log", "content");
        var archiveHook = new ArchiveHook(archiveDirectory: archiveDirectory);

        // Act
        archiveHook.OnFileDeleting(logFile.FullName);

        // Assert
        var archiveFile = new FileInfo(Path.Combine(archiveDirectory.FullName, "some.zip"));
        Assert.That(archiveDirectory.Exists, Is.True);
        Assert.That(archiveFile.Exists, Is.True);
    }

    [Test]
    public void Check_If_OnFileDeleting_Suppresses_Archiving_Errors()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var missingLogFilePath = Path.Combine(testDirectory.Directory.FullName, "missing.log");
        var archiveHook = new ArchiveHook();

        // Act and Assert
        Assert.DoesNotThrow(() => archiveHook.OnFileDeleting(missingLogFilePath));
    }
}