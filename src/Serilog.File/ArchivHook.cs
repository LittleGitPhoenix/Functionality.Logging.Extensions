#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System.IO.Compression;
using Serilog.Sinks.File;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.File;

/// <summary>
/// Special <see cref="FileLifecycleHooks"/> for the serilog file sink, that compresses log files into zip archives and also only keeps a configurable amount of archived files.
/// </summary>
/// <remarks> This should be used with the 'retainedFileCountLimit' set to 1. </remarks>
public sealed class ArchiveHook : FileLifecycleHooks
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

    private readonly int _amountOfFilesToKeep;

    private readonly CompressionLevel _compressionLevel;

    private readonly DirectoryInfo? _archiveDirectory;

    #endregion

    #region Properties
    #endregion

    #region (De)Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="amountOfFilesToKeep"> Optional amount of archived files that should be kept. Default is 30. </param>
    /// <param name="compressionLevel"> Optional <see cref="CompressionLevel"/> to use. Default is <see cref="CompressionLevel.Fastest"/>. </param>
    /// <param name="archiveDirectory"> Optional directory where the zipped log files are saved. If this is null, then the directory of the log file is used. </param>
    public ArchiveHook(int amountOfFilesToKeep = 30, CompressionLevel compressionLevel = CompressionLevel.Fastest, DirectoryInfo? archiveDirectory = null)
    {
        // Save parameters.
        _amountOfFilesToKeep = amountOfFilesToKeep;
        _compressionLevel = compressionLevel;
        _archiveDirectory = archiveDirectory;

        // Initialize fields.
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public override void OnFileDeleting(string filePath)
    {
        try
        {
            // Get a reference to the file and the archive directory.
            var logFile = new FileInfo(filePath);
            var archiveDirectory = _archiveDirectory ?? logFile.Directory ?? new DirectoryInfo(Directory.GetCurrentDirectory());

            // Create the archive directory if needed.
            if (!archiveDirectory.Exists)
            {
                archiveDirectory.Create();
                archiveDirectory.Refresh();
            }

            // Delete old files in the archive directory.
            ArchiveHook.DeleteOldFiles(archiveDirectory, _amountOfFilesToKeep);

            // Zip the file.
            ArchiveHook.CreateZipFile(logFile, _compressionLevel, archiveDirectory);
        }
        // ReSharper disable once EmptyGeneralCatchClause → If archiving failed, this may not throw an exception that could lead to an application crash.
        catch { }
        finally
        {
            base.OnFileDeleting(filePath);
        }
    }

    /// <summary>
    /// Deletes files from <paramref name="archiveDirectory"/>, but keeps as many as <paramref name="amountOfFilesToKeep"/> specifies.
    /// </summary>
    /// <param name="archiveDirectory"> The <see cref="DirectoryInfo"/> where to delete files. </param>
    /// <param name="amountOfFilesToKeep"> The amount of files that are kept. </param>
    /// <returns> The amount of deleted files. </returns>
    internal static int DeleteOldFiles(DirectoryInfo archiveDirectory, int amountOfFilesToKeep)
    {
        var filesToDelete = archiveDirectory
                .EnumerateFiles("*.zip")
                .OrderByDescending(file => file.CreationTime)
                .Skip(amountOfFilesToKeep)
                .ToArray()
            ;
			
        foreach (var fileToDelete in filesToDelete)
        {
            fileToDelete.Delete();
        }

        return filesToDelete.Length;
    }

    /// <summary>
    /// Zips the <paramref name="logFile"/>.
    /// </summary>
    /// <param name="logFile"> The file to zip. </param>
    /// <param name="compressionLevel"> The <see cref="CompressionLevel"/> to use. </param>
    /// <param name="archiveDirectory"> The <see cref="DirectoryInfo"/> where to save the zip file to. </param>
    /// <returns> The <see cref="FileInfo"/> instance of the zip file. </returns>
    internal static FileInfo CreateZipFile(FileInfo logFile, CompressionLevel compressionLevel, DirectoryInfo archiveDirectory)
    {
        // Create the archive.
        var zipFilePath = Path.Combine(archiveDirectory.FullName, $"{Path.GetFileNameWithoutExtension(logFile.Name)}.zip");
        using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Update);
			
        // Create an entry in the zip archive.
        var entry = archive.CreateEntry(logFile.Name, compressionLevel);
        entry.LastWriteTime = new DateTimeOffset(DateTime.SpecifyKind(logFile.LastWriteTimeUtc, DateTimeKind.Utc));

        // Write the log file to the entry.
        using var logFileStream = logFile.OpenRead();
        using var entryStream = entry.Open();
        logFileStream.CopyTo(entryStream);

        return new FileInfo(zipFilePath);
    }

    #endregion
}