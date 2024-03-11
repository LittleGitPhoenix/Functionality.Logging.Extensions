using System.Reactive.Linq;
using AutoFixture;
using AutoFixture.AutoMoq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog.Seq;
using Seq.Api.Model.Shared;
using Serilog.Formatting.Compact.Reader;

namespace Serilog.Seq.Test;

public class SeqServerHelperIntegrationTest
{
	#region Setup

#pragma warning disable 8618 // → Always initialized in the 'Setup' method before a test is run.
	private IFixture _fixture;
#pragma warning restore 8618

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest()
	{
		_fixture = new Fixture().Customize(new AutoMoqCustomization());

		_title = Guid.NewGuid().ToString();
		_apiKey = Guid.NewGuid().ToString().Replace("-", String.Empty);
		//_seqHost = "http://localhost";
		_seqHost = "http://seq2021.hyper.home";
		_seqPort = (ushort) 80;
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTest() { }

	#endregion

	#region Data

	private string _title;

	private string _apiKey;

	private string _seqHost;

	private ushort _seqPort;

	private const string ConfigurationApiKey = "pYHlGsUQw5RsLSFTJHKF";

	#endregion

	#region Tests

	[Test]
	public async Task ApiKeyIsCreatedAsync()
	{
		try
		{
			// Act
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

			// Assert
			var apiKeys = await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
			Assert.That(apiKeys.Count, Is.EqualTo(1));
			Assert.That(apiKeys.Single().Title, Is.EqualTo(_title));
			Assert.That(_apiKey.StartsWith(apiKeys.Single().TokenPrefix), Is.True);
		}
		finally
		{
			await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
		}
	}

	[Test]
	public void ApiKeyIsCreatedSync()
	{
		try
		{
			// Act
			SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

			// Assert
			var apiKeys = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result;
			Assert.That(apiKeys.Count, Is.EqualTo(1));
			Assert.That(apiKeys.Single().Title, Is.EqualTo(_title));
			Assert.That(_apiKey.StartsWith(apiKeys.Single().TokenPrefix), Is.True);
		}
		finally
		{
			SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
		}
	}

	[Test]
	public async Task ApiKeyContainsApplicationPropertyAsync()
	{
		try
		{
			// Act
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey);

			// Assert
			var apiKeyEntity = (await SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey)).Single();
			var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == "Application" && property.Value.ToString() == _title);
			Assert.That(existingProperty, Is.Not.Null);
		}
		finally
		{
			await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
		}
	}

	[Test]
	public void ApiKeyContainsApplicationPropertySync()
	{
		try
		{
			// Act
			SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, _seqHost, _seqPort, ConfigurationApiKey).Wait();

			// Assert
			var apiKeyEntity = SeqServerHelper.GetApiKeysByTitleAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Result.Single();
			var existingProperty = apiKeyEntity.InputSettings.AppliedProperties.SingleOrDefault(property => property.Name == "Application" && property.Value.ToString() == _title);
			Assert.That(existingProperty, Is.Not.Null);
		}
		finally
		{
			SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
		}
	}

	[Test]
	public async Task ApiKeyCreationFailsAsync()
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
	public void ApiKeyCreationFailsSync()
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
	public async Task ApiKeyIsUpdatedAsync()
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
			Assert.That(existingProperty, Is.Not.Null);
		}
		finally
		{
			await SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey);
		}
	}

	[Test]
	public void ApiKeyIsUpdatedSync()
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
			Assert.That(existingProperty, Is.Not.Null);
		}
		finally
		{
			SeqServerHelper.DeleteApiKeysAsync(_title, _seqHost, _seqPort, ConfigurationApiKey).Wait();
		}
	}

	[Test]
	public async Task ApiKeyIsDeletedAsync()
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
	public void ApiKeyIsDeletedSync()
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
	public async Task SendingEventsViaPostSucceedsAsync()
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
	public void SendingEventsViaPostSucceedsSync()
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
	public async Task SendingEventsViaPostFailsBecauseApplicationIsNotRegisteredAsync()
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
	public void SendingEventsViaPostFailsBecauseApplicationIsNotRegisteredSync()
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
			Assert.That(aggregateException, Is.Not.Null);
			Assert.That(aggregateException?.InnerExceptions.First() is SeqServerException, Is.True);
		}
		finally
		{
			SeqServerHelper.DeleteApiKeysAsync(_title, connection).Wait();
		}
	}

	/// <summary>
	/// Checks that sending a file exceeding the default payload limit still succeeds, because it is send in chunks.
	/// </summary>
	/// <remarks> This needs the seq server to enforce having an registered api key for ingestion: Settings → API KEYS → 'Require authentication for HTTP/S ingestion' </remarks>
	[Test]
	public async Task LargeFileIsChunkedBeforeSending()
	{
		var logFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), $".{nameof(LargeFileIsChunkedBeforeSending)}.log"));
		using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
		try
		{
			// Arrange
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, connection);
			var targetLogEventCount = 0;
			var receivedLogEventCount = 0;
			var filter = $"Application = '{_title}'";
			using var stream = connection.Events.StreamAsync<Newtonsoft.Json.Linq.JObject>(filter: filter).Result;
			using var subscription = stream
				.Select(LogEventReader.ReadFromJObject)
				.Subscribe(_ => receivedLogEventCount++)
				;
			// Create a file large enough to exceed the payload limit.
			{
				await using var fileStream = logFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				await using var streamWriter = new StreamWriter(fileStream);
				streamWriter.AutoFlush = true;
				while (streamWriter.BaseStream.Length < (SeqServerHelper.AllowedChunkByteSize * 2)) //! Larger file
				{
					await streamWriter.WriteLineAsync("{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Request for {RequestName} will be handled.\",\"RequestName\":\"CheckConnection\",\"EventId\":{\"Id\":1888742473},\"RequestId\":\"" + $"{Guid.NewGuid()}" + "\",\"Scope\":[],\"ThreadId\":1169,\"ApplicationVersion\":\"1.0.0.0\",\"ApplicationId\":1834788672,\"Data\":\"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna.\"}");
					targetLogEventCount++;
				}
			}
			logFile.Refresh();

			// Act
			Assert.DoesNotThrowAsync(() => SeqServerHelper.SendLogFileToServerAsync(_apiKey, logFile, connection));
			
			// Wait a little bit for the events to reach the server and for the observable to be notified.
			Thread.Sleep(TimeSpan.FromMilliseconds(10000));
			subscription.Dispose();
			stream.Dispose();
			
			// Assert
			Assert.That(targetLogEventCount, Is.EqualTo(receivedLogEventCount));
		}
		finally
		{
			if (logFile.Exists) logFile.Delete();
			SeqServerHelper.DeleteApiKeysAsync(_title, connection).Wait();
		}
	}

	/// <summary>
	/// Checks that sending a file that is not exceeding the default payload limit still is send completely.
	/// </summary>
	/// <remarks> This needs the seq server to enforce having an registered api key for ingestion: Settings → API KEYS → 'Require authentication for HTTP/S ingestion' </remarks>
	[Test]
	public async Task SmallFileIsSendCompletely()
	{
		var logFile = new FileInfo(Path.Combine(Directory.GetCurrentDirectory(), $".{nameof(SmallFileIsSendCompletely)}.log"));
		using var connection = SeqServerHelper.ConnectToSeq(_seqHost, _seqPort, ConfigurationApiKey);
		try
		{
			// Arrange
			await SeqServerHelper.RegisterApiKeyAsync(_title, _apiKey, connection);
			var targetLogEventCount = 0;
			var receivedLogEventCount = 0;
			var filter = $"Application = '{_title}'";
			using var stream = connection.Events.StreamAsync<Newtonsoft.Json.Linq.JObject>(filter: filter).Result;
			using var subscription = stream
				.Select(LogEventReader.ReadFromJObject)
				.Subscribe(_ => receivedLogEventCount++)
				;
			// Create a file large enough to exceed the payload limit.
			{
				await using var fileStream = logFile.Open(FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
				await using var streamWriter = new StreamWriter(fileStream);
				streamWriter.AutoFlush = true;
				while (streamWriter.BaseStream.Length < SeqServerHelper.AllowedChunkByteSize) //! Smaller file
				{
					await streamWriter.WriteLineAsync("{\"@t\":\"" + $"{DateTime.UtcNow:O}" + "\",\"@mt\":\"Request for {RequestName} will be handled.\",\"RequestName\":\"CheckConnection\",\"EventId\":{\"Id\":1888742473},\"RequestId\":\"" + $"{Guid.NewGuid()}" + "\",\"Scope\":[],\"ThreadId\":1169,\"ApplicationVersion\":\"1.0.0.0\",\"ApplicationId\":1834788672,\"Data\":\"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenatis vitae, justo. Nullam dictum felis eu pede mollis pretium. Integer tincidunt. Cras dapibus. Vivamus elementum semper nisi. Aenean vulputate eleifend tellus. Aenean leo ligula, porttitor eu, consequat vitae, eleifend ac, enim. Aliquam lorem ante, dapibus in, viverra quis, feugiat a, tellus. Phasellus viverra nulla ut metus varius laoreet. Quisque rutrum. Aenean imperdiet. Etiam ultricies nisi vel augue. Curabitur ullamcorper ultricies nisi. Nam eget dui. Etiam rhoncus. Maecenas tempus, tellus eget condimentum rhoncus, sem quam semper libero, sit amet adipiscing sem neque sed ipsum. Nam quam nunc, blandit vel, luctus pulvinar, hendrerit id, lorem. Maecenas nec odio et ante tincidunt tempus. Donec vitae sapien ut libero venenatis faucibus. Nullam quis ante. Etiam sit amet orci eget eros faucibus tincidunt. Duis leo. Sed fringilla mauris sit amet nibh. Donec sodales sagittis magna.\"}");
					targetLogEventCount++;
				}
			}
			logFile.Refresh();

			// Act
			Assert.DoesNotThrowAsync(() => SeqServerHelper.SendLogFileToServerAsync(_apiKey, logFile, connection));
			
			// Wait a little bit for the events to reach the server and for the observable to be notified.
			Thread.Sleep(TimeSpan.FromMilliseconds(10000));
			subscription.Dispose();
			stream.Dispose();
			
			// Assert
			Assert.That(targetLogEventCount, Is.EqualTo(receivedLogEventCount));
		}
		finally
		{
			if (logFile.Exists) logFile.Delete();
			SeqServerHelper.DeleteApiKeysAsync(_title, connection).Wait();
		}
	}

	#endregion
}