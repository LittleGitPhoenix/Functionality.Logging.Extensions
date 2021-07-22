#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Special exception used by the seq sink or the seq server helper classes.
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