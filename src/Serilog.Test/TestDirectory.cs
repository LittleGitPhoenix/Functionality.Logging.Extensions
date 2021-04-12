using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Serilog.Test
{
	sealed class TestDirectory : IDisposable
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		#endregion

		#region Properties

		public DirectoryInfo Directory { get; }
		
		#endregion

		#region (De)Constructors

		public TestDirectory()
		{
			// Save parameters.

			// Initialize fields.
			this.Directory = TestDirectory.CreateTempDirectory();
		}

		private static DirectoryInfo CreateTempDirectory()
		{
			var directoryPath = Path.Combine(System.IO.Directory.GetCurrentDirectory(), $".temp_{Guid.NewGuid()}");
			var directory = new DirectoryInfo(directoryPath);
			directory.Create();
			return directory;
		}

		internal DirectoryInfo CreateDirectory(string name)
		{
			var directoryPath = Path.Combine(this.Directory.FullName, name);
			var directory = new DirectoryInfo(directoryPath);
			directory.Create();
			return directory;
		}

		internal FileInfo CreateFile(string name, string content)
		{
			var filePath = Path.Combine(this.Directory.FullName, name);
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
			this.Directory?.Delete(true);
		}

		#endregion
	}
}