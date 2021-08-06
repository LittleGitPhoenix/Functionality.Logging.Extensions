#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Seq.Api;
using Seq.Api.Client;
using Seq.Api.Model.Inputs;
using Seq.Api.Model.Security;

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq
{
	/// <summary>
	/// Provides helper methods for directly interacting with a seq server.
	/// </summary>
	internal class SeqServerHelper
	{
		/// <summary>
		/// Builds a seq url from <paramref name="seqHost"/> and <paramref name="seqPort"/>.
		/// </summary>
		/// <param name="seqHost"> The host of the seq server. This can already include the port like 'http://localhost:5431'. </param>
		/// <param name="seqPort"> An optional port that is added to <paramref name="seqHost"/>. </param>
		/// <returns> The full url of the seq server. </returns>
		internal static string BuildSeqUrl(string seqHost, ushort? seqPort)
		{
			return $"{seqHost}{(seqPort.HasValue ? $":{seqPort}" : String.Empty)}";
		}

		/// <summary>
		/// Creates a connection to a seq server.
		/// </summary>
		/// <param name="seqHost"> The host of the seq server. This can already include the port like 'http://localhost:5431'. </param>
		/// <param name="seqPort"> An optional port that is added to <paramref name="seqHost"/>. </param>
		/// <param name="configurationApiKey"> Optional api key that allows changes to the seq server configuration. </param>
		/// <returns> A <see cref="SeqConnection"/> connection object. </returns>
		internal static SeqConnection ConnectToSeq(string seqHost, ushort? seqPort, string? configurationApiKey = null)
		{
			return new SeqConnection(SeqServerHelper.BuildSeqUrl(seqHost, seqPort), configurationApiKey);
		}
		
		/// <summary>
		/// Registers the <paramref name="apiKey"/> of an application with <paramref name="title"/> in the seq server.
		/// </summary>
		internal static async Task RegisterApiKeyAsync(string title, string apiKey, string seqHost, ushort? seqPort, string? configurationApiKey = null, CancellationToken cancellationToken = default)
		{
			// Establish a connection to the seq server.
			using var connection = SeqServerHelper.ConnectToSeq(seqHost, seqPort, configurationApiKey);
			await SeqServerHelper.RegisterApiKeyAsync(title, apiKey, connection, cancellationToken);
		}

		/// <summary>
		/// Registers the <paramref name="apiKey"/> of an application with <paramref name="title"/> in the seq server using an already established <paramref name="connection"/>.
		/// </summary>
		/// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
		/// <exception cref="SeqServerException"> Thrown registering failed. </exception>
		internal static async Task RegisterApiKeyAsync(string title, string apiKey, SeqConnection connection, CancellationToken cancellationToken = default)
		{
			// Create the default api key.	
			var newApiKey = new ApiKeyEntity()
			{
				//Id = title, //! Id is always overridden by the seq server.
				Title = title,
				Token = apiKey,
				AssignedPermissions = new HashSet<Permission>
				(
					new[]
					{
						Permission.Ingest
					}
				),
				InputSettings = new InputSettingsPart()
				{
					MinimumLevel = global::Seq.Api.Model.LogEvents.LogEventLevel.Verbose, // By default log everything.
					AppliedProperties = new List<InputAppliedPropertyPart>()
					{
						new InputAppliedPropertyPart()
						{
							Name = "Application",
							Value = title
						}
					},
					Filter = null,
				}
			};

			try
			{
				// Try to create the api key. If it - or the token - already exists, this throws.
				await connection.ApiKeys.AddAsync(newApiKey, cancellationToken).ConfigureAwait(false);
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				// Upon error try to update the api key.
				var result = await AddOrUpdateAppliedPropertiesOfApiKeysAsync(title, connection, newApiKey.InputSettings.AppliedProperties, cancellationToken: cancellationToken).ConfigureAwait(false);
				if (result <= 0) throw new SeqServerException($"Could neither add nor update '{title}'. This indicates, that the seq server is not available.", new[] { ex });
			}
		}

		/// <summary>
		/// Gets the <see cref="ApiKeyEntity"/>s of applications with <paramref name="title"/> from the seq server.
		/// </summary>
		internal static async Task<ICollection<ApiKeyEntity>> GetApiKeysByTitleAsync(string title, string seqHost, ushort? seqPort, string? configurationApiKey = null, CancellationToken cancellationToken = default)
		{
			// Establish a connection to the seq server.
			using var connection = SeqServerHelper.ConnectToSeq(seqHost, seqPort, configurationApiKey);
			return await SeqServerHelper.GetApiKeysByTitleAsync(title, connection, cancellationToken);
		}

		/// <summary>
		/// Gets the <see cref="ApiKeyEntity"/>s of applications with <paramref name="title"/> from the seq server using an already established <paramref name="connection"/>.
		/// </summary>
		internal static async Task<ICollection<ApiKeyEntity>> GetApiKeysByTitleAsync(string title, SeqConnection connection, CancellationToken cancellationToken = default)
		{
			// Get all api keys and filter those that have a matching title.
			var matchingApiKeys = (await connection.ApiKeys.ListAsync(cancellationToken: cancellationToken).ConfigureAwait(false))
					.Where(apiKey => apiKey.Title == title)
				.ToArray()
				;
			return matchingApiKeys;
		}

		/// <summary>
		/// Adds or updates <paramref name="appliedProperties"/> to all <see cref="ApiKeyEntity"/>s of applications matching <paramref name="title"/> from the seq server.
		/// </summary>
		/// <param name="throwIfMultipleApiKeysAreFound"> Optional flag specifying if this function throws if more than one <see cref="ApiKeyEntity"/> was found. </param>
		internal static async Task<int> AddOrUpdateAppliedPropertiesOfApiKeysAsync(string title, string seqHost, ushort? seqPort, ICollection<InputAppliedPropertyPart> appliedProperties, string? configurationApiKey = null, bool throwIfMultipleApiKeysAreFound = true, CancellationToken cancellationToken = default)
		{
			// Establish a connection to the seq server.
			using var connection = SeqServerHelper.ConnectToSeq(seqHost, seqPort, configurationApiKey);
			return await SeqServerHelper.AddOrUpdateAppliedPropertiesOfApiKeysAsync(title, connection, appliedProperties, throwIfMultipleApiKeysAreFound, cancellationToken);
		}

		/// <summary>
		/// Adds or updates <paramref name="appliedProperties"/> to all <see cref="ApiKeyEntity"/>s of applications matching <paramref name="title"/> from the seq server using an already established <paramref name="connection"/>.
		/// </summary>
		/// <param name="throwIfMultipleApiKeysAreFound"> Optional flag specifying if this function throws if more than one <see cref="ApiKeyEntity"/> was found. </param>
		internal static async Task<int> AddOrUpdateAppliedPropertiesOfApiKeysAsync(string title, SeqConnection connection, ICollection<InputAppliedPropertyPart> appliedProperties, bool throwIfMultipleApiKeysAreFound = true, CancellationToken cancellationToken = default)
		{
			// Get all matching api keys.
			var apiKeys = await SeqServerHelper.GetApiKeysByTitleAsync(title, connection, cancellationToken).ConfigureAwait(false);
			if (apiKeys.Count == 0) return -1;
			if (apiKeys.Count > 1 && throwIfMultipleApiKeysAreFound)
			{
				throw new SeqServerException($"Could not update the automatically applied properties of the api keys with title '{title}', because more than one match was found. Consider setting the {nameof(throwIfMultipleApiKeysAreFound)} parameter to true.");
			}

			// Update the properties.
			var exceptions = new List<Exception>();
			foreach (var apiKey in apiKeys)
			{
				foreach (var newProperty in appliedProperties)
				{
					var exists = apiKey.InputSettings.AppliedProperties.Any(existingProperty => existingProperty.Name == newProperty.Name);
					if (!exists) apiKey.InputSettings.AppliedProperties.Add(newProperty);
				}
				try
				{
					await connection.ApiKeys.UpdateAsync(apiKey, cancellationToken).ConfigureAwait(false);
				}
				catch (Exception ex)
				{
					exceptions.Add(ex);
				}
			}
			if (exceptions.Any()) throw new SeqServerException($"An error occurred while updating the applied properties of the api keys with title '{title}'. See the inner exception for more details.", exceptions);
			return apiKeys.Count;
		}

		/// <summary>
		/// Deletes all <see cref="ApiKeyEntity"/>s of applications matching <paramref name="title"/> from the seq server.
		/// </summary>
		/// <param name="throwIfMultipleApiKeysAreFound"> Optional flag specifying if this function throws if more than one <see cref="ApiKeyEntity"/> was found. </param>
		internal static async Task<int> DeleteApiKeysAsync(string title, string seqHost, ushort? seqPort, string? configurationApiKey = null, bool throwIfMultipleApiKeysAreFound = true, CancellationToken cancellationToken = default)
		{
			// Establish a connection to the seq server.
			using var connection = SeqServerHelper.ConnectToSeq(seqHost, seqPort, configurationApiKey);
			return await SeqServerHelper.DeleteApiKeysAsync(title, connection, throwIfMultipleApiKeysAreFound, cancellationToken);
		}

		/// <summary>
		/// Deletes all <see cref="ApiKeyEntity"/>s of applications matching <paramref name="title"/> from the seq server using an already established <paramref name="connection"/>.
		/// </summary>
		/// <param name="throwIfMultipleApiKeysAreFound"> Optional flag specifying if this function throws if more than one <see cref="ApiKeyEntity"/> was found. </param>
		internal static async Task<int> DeleteApiKeysAsync(string title, SeqConnection connection, bool throwIfMultipleApiKeysAreFound = true, CancellationToken cancellationToken = default)
		{
			// Get all matching api keys.
			var apiKeys = await SeqServerHelper.GetApiKeysByTitleAsync(title, connection, cancellationToken).ConfigureAwait(false);
			if (apiKeys.Count == 0) return -1;
			if (apiKeys.Count > 1 && throwIfMultipleApiKeysAreFound)
			{
				throw new SeqServerException($"Could not delete the api keys with title '{title}', because more than one match was found. Consider setting the {nameof(throwIfMultipleApiKeysAreFound)} parameter to true.");
			}

			// Try to delete all the matching api keys.
			foreach (var apiKey in apiKeys)
			{
				await connection.ApiKeys.RemoveAsync(apiKey, cancellationToken).ConfigureAwait(false);
			}

			return apiKeys.Count;
		}

		/// <summary>
		/// Sends log events to a seq server.
		/// </summary>
		/// <param name="apiKey"> The api key to use when sending the log events. </param>
		/// <param name="logFile"> A reference to the json log file. </param>
		/// <param name="connection"> The <see cref="SeqConnection"/> to use. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> An awaitable <see cref="Task"/>. </returns>
		/// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
		/// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
		/// <remarks> https://docs.datalust.co/docs/posting-raw-events </remarks>
		internal static async Task SendLogFileToServerAsync(string apiKey, FileInfo logFile, SeqConnection connection, CancellationToken cancellationToken = default)
		{
			using var streamReader = logFile.OpenText();
			var content = await streamReader.ReadToEndAsync().ConfigureAwait(false);

			await SeqServerHelper.SendLogEventsToServerAsync(apiKey, content, connection, cancellationToken).ConfigureAwait(false);
		}

		/// <summary>
		/// Sends log events to a seq server.
		/// </summary>
		/// <param name="apiKey"> The api key to use when sending the log events. </param>
		/// <param name="logFileContent"> A <see cref="System.Environment.NewLine"/> separated string of log events. </param>
		/// <param name="connection"> The <see cref="SeqConnection"/> to use. </param>
		/// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
		/// <returns> An awaitable <see cref="Task"/>. </returns>
		/// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
		/// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
		/// <remarks> https://docs.datalust.co/docs/posting-raw-events </remarks>
		internal static async Task SendLogEventsToServerAsync(string apiKey, string logFileContent, SeqConnection connection, CancellationToken cancellationToken = default)
		{
			try
			{
				HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"{connection.Client.ServerUrl}/api/events/raw/");
				request.Headers.Add("X-Seq-ApiKey", apiKey);
				request.Content = new StringContent(logFileContent, Encoding.UTF8, "application/vnd.serilog.clef");

				var response = await connection.Client.HttpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
				if (!response.IsSuccessStatusCode)
				{
#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
					var responseMessage = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
					var responseMessage = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
					throw new SeqServerException($"Could not send the log events to the seq server '{connection.Client.ServerUrl}'. The response was '{responseMessage}' with status code '{response.StatusCode} ({(int) response.StatusCode})'.");
				}
			}
			catch (OperationCanceledException)
			{
				throw;
			}
			catch (Exception ex)
			{
				throw new SeqServerException($"An error occurred while sending the log events to the seq server '{connection.Client.ServerUrl}'. See the inner exception for more details.", new[] {ex});
			}
		}
	}
}