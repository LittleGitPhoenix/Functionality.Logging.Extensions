using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;
using Seq.Api.Model.Inputs;
using Seq.Api.Model.Shared;
using Seq.Api.Streams;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.Seq.Test
{

	public class SeqServerHelperTest
	{
		private string _title;

		private string _apiKey;

		private string _seqHost;

		private ushort _seqPort;

		private const string ConfigurationApiKey = "pYHlGsUQw5RsLSFTJHKF";

		[SetUp]
		public void Setup()
		{
			_title = Guid.NewGuid().ToString();
			_apiKey = Guid.NewGuid().ToString().Replace("-", String.Empty);
			_seqHost = "http://localhost";
			_seqPort = (ushort) 5341;
		}

		[Test]
		public async Task Check_Api_Key_Is_Created_Async()
		{
			try
			{
				// Act
				await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

				// Assert
				var apiKeys = await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
				Assert.That(apiKeys.Count, Is.EqualTo(1));
				Assert.That(apiKeys.Single().Title, Is.EqualTo(_title));
				Assert.True(_apiKey.StartsWith(apiKeys.Single().TokenPrefix));
			}
			finally
			{
				await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
			}
		}

		[Test]
		public void Check_Api_Key_Is_Created_Sync()
		{
			try
			{
				// Act
				SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

				// Assert
				var apiKeys = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result;
				Assert.That(apiKeys.Count, Is.EqualTo(1));
				Assert.That(apiKeys.Single().Title, Is.EqualTo(_title));
				Assert.True(_apiKey.StartsWith(apiKeys.Single().TokenPrefix));
			}
			finally
			{
				SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
			}
		}

		[Test]
		public async Task Check_Api_Key_Contains_Application_Property_Async()
		{
			try
			{
				// Act
				await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

				// Assert
				var apiKeyEntity = (await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey)).Single();
				var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == "Application" && property.Value.ToString() == _title);
				Assert.NotNull(existingProperty);
			}
			finally
			{
				await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
			}
		}

		[Test]
		public void Check_Api_Key_Contains_Application_Property_Sync()
		{
			try
			{
				// Act
				SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

				// Assert
				var apiKeyEntity = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result.Single();
				var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == "Application" && property.Value.ToString() == _title);
				Assert.NotNull(existingProperty);
			}
			finally
			{
				SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
			}
		}

		[Test]
		public async Task Check_Api_Key_Creation_Fails_Async()
		{
			// Arrange
			var wrongSeqHost = "http://not-existing-seq-server";

			try
			{
				// Act + Assert
				Assert.CatchAsync(() => SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, wrongSeqHost, _seqPort, ConfigurationApiKey));
			}
			finally
			{
				try
				{
					// Deletion should only be required if the above test failed.
					await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
				}
				catch { /* ignore */ }
			}
		}

		[Test]
		public void Check_Api_Key_Creation_Fails_Sync()
		{
			// Arrange
			var wrongSeqHost = "http://not-existing-seq-server";

			try
			{
				// Act + Assert
				Assert.Catch(() => SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, wrongSeqHost, _seqPort, ConfigurationApiKey).Wait());
			}
			finally
			{
				try
				{
					// Deletion should only be required if the above test failed.
					SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
				}
				catch { /* ignore */ }
			}
		}

		[Test]
		public async Task Check_Api_Key_Is_Updated_Async()
		{
			// Arrange
			var propertyName = Guid.NewGuid().ToString();
			var propertyValue = Guid.NewGuid().ToString();
			var newAppliedProperties = new[]
			{
				new EventPropertyPart(propertyName, propertyValue)
			};
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

			try
			{
				// Act
				await SeqServerHelper.AddOrUpdateAppliedPropertiesOfApiKeysAsync(_title, _seqHost, _seqPort, newAppliedProperties, ConfigurationApiKey);

				// Assert
				var apiKeyEntity = (await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey)).Single();
				var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == propertyName && property.Value.ToString() == propertyValue);
				Assert.NotNull(existingProperty);
			}
			finally
			{
				await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
			}
		}

		[Test]
		public void Check_Api_Key_Is_Updated_Sync()
		{
			// Arrange
			var propertyName = Guid.NewGuid().ToString();
			var propertyValue = Guid.NewGuid().ToString();
			var newAppliedProperties = new[]
			{
				new EventPropertyPart(propertyName, propertyValue)
			};
			SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

			try
			{
				// Act
				SeqServerHelper.AddOrUpdateAppliedPropertiesOfApiKeysAsync(_title, _seqHost, _seqPort, newAppliedProperties, ConfigurationApiKey).Wait();

				// Assert
				var apiKeyEntity = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result.Single();
				var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == propertyName && property.Value.ToString() == propertyValue);
				Assert.NotNull(existingProperty);
			}
			finally
			{
				SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
			}
		}

		[Test]
		public async Task Check_Api_Key_Is_Deleted_Async()
		{
			// Arrange
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

			// Act
			var deletedCount = await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);

			// Assert
			Assert.That(deletedCount, Is.EqualTo(1));
			var apiKeys = await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
			Assert.That(apiKeys.Count, Is.EqualTo(0));
		}

		[Test]
		public void Check_Api_Key_Is_Deleted_Sync()
		{
			// Arrange
			SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

			// Act
			var deletedCount = SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result;

			// Assert
			Assert.That(deletedCount, Is.EqualTo(1));
			var apiKeys = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result;
			Assert.That(apiKeys.Count, Is.EqualTo(0));
		}

		/// <summary>
		/// Checks that sending log events via POST succeeds for a properly registered application.
		/// </summary>
		[Test]
		public async Task Check_Sending_Events_Via_Post_Succeeds_Async()
		{
			using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
			try
			{
				// Arrange
				await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, connection);
				var content = new[]
				{
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"alice\"}",
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"bob\"}"
				};
				var receivedLogEventCount = 0;
				var filter = $"Application = '{_title}'";
				using var stream = await connection.Events.StreamAsync<Newtonsoft.Json.Linq.JObject>(filter: filter);
				using var subscription = stream
					.Select
					(
						jObject =>
						{
							return LogEventReader.ReadFromJObject(jObject);
						}
					)
					.Subscribe
					(
						logEvent =>
						{
							receivedLogEventCount++;
						}
					)
					;

				// Act + Assert
				Assert.DoesNotThrowAsync(() => SeqServerHelper.SendLogEventsToServerAsync(_apiKey, String.Join(System.Environment.NewLine, content), connection));
				
				// Wait a little bit for the events to reach the server and for the observable to be notified.
				await Task.Delay(TimeSpan.FromMilliseconds(2000));
				subscription.Dispose();
				stream.Dispose();

				// Assert
				Assert.That(receivedLogEventCount, Is.EqualTo(content.Length));
			}
			finally
			{
				await SeqServerHelper.DeleteApiKeysAsync(_title, connection);
			}
		}

		/// <summary>
		/// Checks that sending log events via POST succeeds for a properly registered application.
		/// </summary>
		[Test]
		public void Check_Sending_Events_Via_Post_Succeeds_Sync()
		{
			using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
			try
			{
				// Arrange
				SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, connection).Wait();
				var content = new[]
				{
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"alice\"}",
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"bob\"}"
				};
				var receivedLogEventCount = 0;
				var filter = $"Application = '{_title}'";
				using var stream = connection.Events.StreamAsync<Newtonsoft.Json.Linq.JObject>(filter: filter).Result;
				using var subscription = stream
					.Select
					(
						jObject =>
						{
							return LogEventReader.ReadFromJObject(jObject);
						}
					)
					.Subscribe
					(
						logEvent =>
						{
							receivedLogEventCount++;
						}
					)
					;

				// Act + Assert
				Assert.DoesNotThrowAsync(() => SeqServerHelper.SendLogEventsToServerAsync(_apiKey, String.Join(System.Environment.NewLine, content), connection));
				
				// Wait a little bit for the events to reach the server and for the observable to be notified.
				Thread.Sleep(TimeSpan.FromMilliseconds(2000));
				subscription.Dispose();
				stream.Dispose();

				// Assert
				Assert.That(receivedLogEventCount, Is.EqualTo(content.Length));
			}
			finally
			{
				SeqServerHelper.DeleteApiKeysAsync(_title, connection).Wait();
			}
		}

		/// <summary>
		/// Checks that sending log events via POST fails, if the application wasn't registered.
		/// </summary>
		/// <remarks> This needs the seq server to enforce having an registered api key for ingestion: Settings → API KEYS → 'Require authentication for HTTP/S ingestion' </remarks>
		[Test]
		public async Task Check_Sending_Events_Via_Post_Fails_Because_Application_Is_Not_Registered_Async()
		{
			using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
			try
			{
				// Arrange
				var content = new[]
				{
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"alice\"}",
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"bob\"}"
				};

				// Act + Assert
				Assert.CatchAsync<SeqServerException>(() => SeqServerHelper.SendLogEventsToServerAsync(_apiKey, String.Join(System.Environment.NewLine, content), connection));
			}
			finally
			{
				await SeqServerHelper.DeleteApiKeysAsync(_title, connection);
			}
		}

		/// <summary>
		/// Checks that sending log events via POST fails, if the application wasn't registered.
		/// </summary>
		/// <remarks> This needs the seq server to enforce having an registered api key for ingestion: Settings → API KEYS → 'Require authentication for HTTP/S ingestion' </remarks>
		[Test]
		public void Check_Sending_Events_Via_Post_Fails_Because_Application_Is_Not_Registered_Sync()
		{
			using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
			try
			{
				// Arrange
				var content = new[]
				{
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"alice\"}",
					"{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Hello, {User}\",\"User\":\"bob\"}"
				};

				// Act
				var exception = Assert.Catch(() => SeqServerHelper.SendLogEventsToServerAsync(_apiKey, String.Join(System.Environment.NewLine, content), connection).Wait());

				// Assert
				//! In contrast to await the Wait() method does not throw the first exception but always wraps all exception in an AggregateException.
				var aggregateException = exception as AggregateException;
				Assert.NotNull(aggregateException);
				Assert.True(aggregateException?.InnerExceptions.First() is SeqServerException);
			}
			finally
			{
				SeqServerHelper.DeleteApiKeysAsync(_title, connection).Wait();
			}
		}
	}
}