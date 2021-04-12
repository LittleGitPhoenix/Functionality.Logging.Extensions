using System;

namespace Phoenix.Functionality.Logging.Extensions.Serilog
{
	/// <summary>
	/// Special exception used when parsing a settings file for serilog failed.
	/// </summary>
	public class SerilogSettingsException : Exception
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		public SerilogSettingsException(string message) : base(message) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="innerException"> <see cref="Exception.InnerException"/> </param>
		public SerilogSettingsException(Exception innerException) : this(innerException.Message, innerException) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		/// <param name="innerException"> <see cref="Exception.InnerException"/> </param>
		public SerilogSettingsException(string message, Exception innerException) : base(message, innerException) { }
	}
}