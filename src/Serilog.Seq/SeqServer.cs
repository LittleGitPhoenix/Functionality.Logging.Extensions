using System;
using System.Threading.Tasks;
using Seq.Api;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Provides instance base access to an seq server. (Basically this wraps the static functionality from <see cref="SeqServerHelper"/>.)
	/// </summary>
	internal class SeqServer
	{
		private readonly string? _configurationApiKey;

		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties

		public string Host { get; }

		public ushort? Port { get; }
		
		public string Url { get; }

		#endregion

		#region (De)Constructors

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
		/// Registers the application with <paramref name="title"/> and <paramref name="apiKey"/> with the seq server.
		/// </summary>
		/// <param name="title"> The application title. </param>
		/// <param name="apiKey"> The api key. </param>
		/// <returns> True on success, otherwise false. </returns>
		internal virtual async Task<bool> RegisterApplicationAsync(string title, string apiKey)
		{
			var connection = SeqServerHelper.ConnectToSeq(this.Host, this.Port, _configurationApiKey);
			try
			{
				await SeqServerHelper.RegisterApiKeyAsync(title, apiKey, connection).ConfigureAwait(false);
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
		
		#endregion
	}
}