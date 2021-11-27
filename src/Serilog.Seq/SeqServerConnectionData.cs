using System;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Connection data for a seq server.
	/// </summary>
	public record SeqServerConnectionData
	{
		/// <summary> The host address of the seq server. This can already include the port like 'http://localhost:5431'. </summary>
		public string Host { get; }

		/// <summary> Optional port where the seq server listens. It will be added to <see cref="Host"/>. </summary>
		public ushort? Port { get; }

		/// <summary> The api key that allows changes to the seq server configuration. </summary>
		public string? ApiKey { get; }

		/// <summary> A combination of <see cref="Host"/> and <see cref="Port"/>. </summary>
		public string Url { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="host"> <see cref="Host"/> </param>
		/// <param name="port"> <see cref="Port"/> </param>
		/// <param name="apiKey"> <see cref="ApiKey"/> </param>
		public SeqServerConnectionData(string host, ushort? port, string? apiKey)
		{
			this.Host = host;
			this.Port = port;
			this.ApiKey = apiKey;
			this.Url = SeqServerHelper.BuildSeqUrl(host, port);
		}
	}
}