using System;
using System.Collections.Generic;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Special exception used when for seq sink.
	/// </summary>
	public class SeqServerException : AggregateException
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		public SeqServerException(string message) : base(message) { }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="message"> <see cref="Exception.Message"/> </param>
		/// <param name="innerExceptions"> <see cref="AggregateException.InnerExceptions"/> </param>
		public SeqServerException(string message, IEnumerable<Exception> innerExceptions) : base(message, innerExceptions) { }
	}
}