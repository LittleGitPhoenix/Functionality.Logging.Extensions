 namespace Serilog.Test;

sealed class TestFile : IDisposable
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields

    private static DirectoryInfo Directory;

    #endregion

    #region Properties

    public FileInfo File { get; }

    #endregion

    #region (De)Constructors

    static TestFile()
    {
        var directoryPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $".temp_{Guid.NewGuid()}");
        Directory = new DirectoryInfo(directoryPath);
        Directory.Create();        
    }

    public TestFile(string name, string content)
    {
        // Save parameters.

        // Initialize fields.
        this.File = TestFile.CreateTempFile(name, content);
    }

    private static FileInfo CreateTempFile(string name, string content)
    {			
        var filePath = Path.Combine(Directory.FullName, name);
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
		if (Directory.Exists) Directory?.Delete(true);
    }

    #endregion
}