#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.IO;
using System.Threading.Tasks;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Provides interaction logic for a seq server.
	/// </summary>
	/// <remarks> This is a wrapper for the static functionality from <see cref="SeqServerHelper"/>. </remarks>
	public class SeqServer
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly string? _configurationApiKey;

		#endregion

		#region Properties

		/// <summary> The host address of the seq server. </summary>
		public string Host { get; }

		/// <summary> The port where the seq server listens. </summary>
		public ushort? Port { get; }
		
		/// <summary> A combination of <see cref="Host"/> and <see cref="Port"/>. </summary>
		public string Url { get; }

		#endregion

		#region (De)Constructors

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="seqHost"> <see cref="Host"/> </param>
		/// <param name="seqPort"> <see cref="Port"/> </param>
		/// <param name="configurationApiKey"> THe api key used to configure the seq server. </param>
		public SeqServer(string seqHost, ushort? seqPort, string? configurationApiKey = null)
		{
			// Save parameters.
			this.Host = seqHost;
			this.Port = seqPort;
			_configurationApiKey = configurationApiKey;

			// Initialize fields.
			this.Url = SeqServerHelper.BuildSeqUrl(seqHost, seqPort);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> and <paramref name="apiKey"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application to register. </param>
		/// <param name="apiKey"> The api key. </param>
		/// <returns> True on success, otherwise false. </returns>
		internal virtual async Task<bool> RegisterApplicationAsync(string applicationTitle, string apiKey)
		{
			var connection = SeqServerHelper.ConnectToSeq(this.Host, this.Port, _configurationApiKey);
			try
			{
				await SeqServerHelper.RegisterApiKeyAsync(applicationTitle, apiKey, connection).ConfigureAwait(false);
				return true;
			}
			catch (Exception ex)
			{
				/* ignore */
			}
			finally
			{
				connection.Dispose();
			}

			return false;
		}

		/// <summary>
		/// Sends log events to a seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that produced the log file. This will be used to create the api key. </param>
		/// <param name="logFile"> A reference to the json log file. </param>
		/// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
		/// <returns> An awaitable <see cref="Task"/>. </returns>
		public virtual async Task SendLogFileAsync(string applicationTitle, FileInfo logFile)
		{
			var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationTitle);
			var connection = SeqServerHelper.ConnectToSeq(this.Host, this.Port, _configurationApiKey);
			try
			{
				await SeqServerHelper.SendLogFileToServerAsync(apiKey, logFile, connection).ConfigureAwait(false);
			}
			finally
			{
				connection.Dispose();
			}
		}

		#endregion
	}
}