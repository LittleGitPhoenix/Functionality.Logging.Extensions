using System;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.File
{
	/// <summary>
	/// Special exception used when archiving log files.
	/// </summary>
	public class SerilogArchiveException : Exception
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		public SerilogArchiveException(string message) : base(message) { }
	}
}