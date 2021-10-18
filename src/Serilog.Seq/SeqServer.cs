#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.IO;
using System.Linq;
using System.Threading;
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
		/// Registers the application with <paramref name="applicationTitle"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
		/// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
		/// <returns> The generated api key. </returns>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		public virtual string RegisterApplication(string applicationTitle, TimeSpan timeout)
		{
			using var cancellationTokenSource = new CancellationTokenSource(timeout);
			return this.RegisterApplication(applicationTitle, cancellationTokenSource.Token);
		}

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> The generated api key. </returns>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		public virtual string RegisterApplication(string applicationTitle, CancellationToken cancellationToken = default)
		{
			var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationTitle);
			this.RegisterApplication(applicationTitle, apiKey, cancellationToken);
			return apiKey;
		}

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
		/// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
		/// <returns> The generated api key. </returns>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		public virtual Task<string> RegisterApplicationAsync(string applicationTitle, TimeSpan timeout)
		{
			using var cancellationTokenSource = new CancellationTokenSource(timeout);
			return this.RegisterApplicationAsync(applicationTitle, cancellationTokenSource.Token);
		}

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> The generated api key. </returns>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		public virtual async Task<string> RegisterApplicationAsync(string applicationTitle, CancellationToken cancellationToken = default)
		{
			var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationTitle);
			await this.RegisterApplicationAsync(applicationTitle, apiKey, cancellationToken).ConfigureAwait(false);
			return apiKey;
		}

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> and <paramref name="apiKey"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application to register. </param>
		/// <param name="apiKey"> The api key. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		private void RegisterApplication(string applicationTitle, string apiKey, CancellationToken cancellationToken = default)
		{
			try
			{
				this.RegisterApplicationAsync(applicationTitle, apiKey, cancellationToken).Wait(CancellationToken.None);
			}
			//! Catching SeqServerApplicationRegisterException should not be necessary, as "Wait()" should per specification throw an AggregateException, but unit test proved otherwise.
			catch (SeqServerApplicationRegisterException)
			{
				throw;
			}
			catch (OperationCanceledException ex)
			{
				throw new SeqServerApplicationRegisterException(apiKey, $"Registering the application '{applicationTitle}' with the seq server '{this.Url}' was cancelled.", new[] { ex });
			}
			catch (AggregateException ex) when (ex.InnerExceptions.FirstOrDefault() is SeqServerApplicationRegisterException seqServerApplicationRegisterException)
			{
				throw seqServerApplicationRegisterException;
			}
			catch (Exception ex)
			{
				throw new SeqServerApplicationRegisterException(apiKey, $"An error occurred registering the application '{applicationTitle}' with the seq server '{this.Url}'. See the inner exception for more details.", new[] { ex });
			}
		}

		/// <summary>
		/// Registers the application with <paramref name="applicationTitle"/> and <paramref name="apiKey"/> with the seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application to register. </param>
		/// <param name="apiKey"> The api key. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> An awaitable <see cref="Task"/>. </returns>
		/// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
		/// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
		internal virtual async Task RegisterApplicationAsync(string applicationTitle, string apiKey, CancellationToken cancellationToken = default)
		{
			var connection = SeqServerHelper.ConnectToSeq(this.Host, this.Port, _configurationApiKey);
			try
			{
				await SeqServerHelper.RegisterApiKeyAsync(applicationTitle, apiKey, connection, cancellationToken).ConfigureAwait(false);
			}
			catch (SeqServerException ex)
			{
				throw SeqServerApplicationRegisterException.CreateFromSeqServerException(apiKey, ex);
			}
			catch (OperationCanceledException ex)
			{
				throw new SeqServerApplicationRegisterException(apiKey, $"Registering the application '{applicationTitle}' with the seq server '{this.Url}' was cancelled.", new[] { ex });
			}
			catch (Exception ex)
			{
				throw new SeqServerApplicationRegisterException(apiKey, $"An error occurred registering the application '{applicationTitle}' with the seq server '{this.Url}'. See the inner exception for more details.", new[] { ex });
			}
			finally
			{
				connection.Dispose();
			}
		}

		/// <summary>
		/// Sends log events to a seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that produced the log file. This will be used to create the api key. </param>
		/// <param name="logFile"> A reference to the json log file. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
		/// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
		public virtual void SendLogFile(string applicationTitle, FileInfo logFile, CancellationToken cancellationToken = default)
		{
			try
			{
				this.SendLogFileAsync(applicationTitle, logFile, cancellationToken).Wait(CancellationToken.None);
			}
			//! Catching SeqServerException should not be necessary, as "Wait()" should per specification throw an AggregateException, but unit test proved otherwise.
			catch (SeqServerException)
			{
				throw;
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SeqServerException($"An error occurred while sending the log events to the seq server '{this.Url}'. See the inner exception for more details.", new[] {ex});
			}
		}

		/// <summary>
		/// Sends log events to a seq server.
		/// </summary>
		/// <param name="applicationTitle"> The title/name of the application that produced the log file. This will be used to create the api key. </param>
		/// <param name="logFile"> A reference to the json log file. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> An awaitable <see cref="Task"/>. </returns>
		/// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
		/// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
		public virtual async Task SendLogFileAsync(string applicationTitle, FileInfo logFile, CancellationToken cancellationToken = default)
		{
			var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationTitle);
			var connection = SeqServerHelper.ConnectToSeq(this.Host, this.Port, _configurationApiKey);
			try
			{
				await SeqServerHelper.SendLogFileToServerAsync(apiKey, logFile, connection, cancellationToken).ConfigureAwait(false);
			}

			catch (OperationCanceledException)
			{
				throw;
			}
			catch (SeqServerException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SeqServerException($"An error occurred while sending the log events to the seq server '{this.Url}'. See the inner exception for more details.", new[] {ex});
			}
			finally
			{
				connection.Dispose();
			}
		}

		#endregion
	}
}