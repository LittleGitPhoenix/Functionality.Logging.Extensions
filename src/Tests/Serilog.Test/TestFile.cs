 namespace Serilog.Test;

sealed class TestFile : IDisposable
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

    private static DirectoryInfo _directory;

    #endregion

    #region Properties

    public FileInfo File { get; }

    #endregion

    #region (De)Constructors

    public TestFile(string name, string content)
    {
        // Save parameters.

        // Initialize fields.
        this.File = TestFile.CreateTempFile(name, content);
    }

    private static FileInfo CreateTempFile(string name, string content)
    {
        var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), $".temp_{Guid.NewGuid()}");
        _directory = new DirectoryInfo(directoryPath);
        _directory.Create();
			
        var filePath = Path.Combine(_directory.FullName, name);
        var file = new FileInfo(filePath);
        {
            using var fileStream = file.Open(FileMode.Create, FileAccess.ReadWrite);
            using var writer = new StreamWriter(fileStream);
            writer.Write(content);
        }
        file.Refresh();
        return file;
    }

    #endregion

    #region Methods

    /// <inheritdoc />
    public void Dispose()
    {
        _directory?.Delete(true);
    }

    #endregion
}