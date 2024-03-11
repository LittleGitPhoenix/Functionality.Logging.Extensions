using System.Data;
using System.Globalization;
using AutoFixture;
using AutoFixture.AutoMoq;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Microsoft;

using l10nLocal = Microsoft.Test.Localization.l10n;
using LoggerExtensions = Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerExtensions;

namespace Microsoft.Test;

public class LoggerExtensionsTest
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
	}

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTests() { }

	#endregion

	#region Data

	class MyLogger : ILogger
	{
		#region Implementation of ILogger

		/// <inheritdoc />
		public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

		/// <inheritdoc />
		public bool IsEnabled(LogLevel logLevel) => false;

		/// <inheritdoc />
		public IDisposable BeginScope<TState>(TState state) => null;

		#endregion
	}

	public class InstanceWrapper
	{
		private readonly ILogger _logger;

		public InstanceWrapper(ILogger logger)
		{
			_logger = logger;
		}

		public virtual IDisposable CreateScopeAndLog((LogScope Scope, LogEvent Event)? log) => _logger.CreateScopeAndLog(log);

		public virtual IDisposable CreateScopeAndLog<TIdentifier>((LogScope<TIdentifier> Scope, LogEvent Event)? log) where TIdentifier : notnull => _logger.CreateScopeAndLog(log);
	}

	#endregion

	#region Tests

	/// <summary>
	/// Checks that logging f rom the extension methods does not throw exceptions.
	/// </summary>
	[Test]
	public void LogDoesNotThrowExceptionDueToArgumentMismatch()
	{
		// Arrange
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		Mock.Get(logger)
			.Setup
			(
				mock => mock.Log
				(
					It.IsAny<LogLevel>(),
					It.IsAny<EventId>(),
					It.IsAny<It.IsAnyType>(),
					It.IsAny<Exception>(),
					(Func<It.IsAnyType, Exception?, string>) It.IsAny<object>()
				)
			)
			.Throws<Exception>()
			;

		// Act + Assert
		Assert.DoesNotThrow(() => LoggerExtensions.Log(logger, (EventId) 0, null, LogLevel.Information, _fixture.Create<string>(), _fixture.Create<LogScope?>(), _fixture.Create<string>(), _fixture.Create<string>()));
	}

	/// <summary>
	/// Checks that the output message from <see cref="Phoenix.Functionality.Logging.Extensions.Microsoft.LoggerExtensions.LogEventFromResource"/> is properly translated.
	/// </summary>
	/// <param name="culture"> The destination culture in which the output message must be translated. </param>
	[Test]
	[TestCase("en")]
	[TestCase("de")]
	public void OutputMessageIsTranslated(string cultureIdentifier)
	{
		void ChangeCulture(string ci)
		{
			var culture = new CultureInfo(ci);
			CultureInfo.CurrentUICulture = culture;
			CultureInfo.DefaultThreadCurrentCulture = culture;
			CultureInfo.DefaultThreadCurrentUICulture = culture;
		}

		// Arrange
		var resourceManager = l10nLocal.ResourceManager;
		var resourceName = nameof(l10nLocal.StartIteration);
		var destinationCulture = CultureInfo.CreateSpecificCulture(cultureIdentifier);
		var user = _fixture.Create<string>();
		var dataSetId = _fixture.Create<ushort>();
		var logger = _fixture.Create<Mock<ILogger>>().Object;
		var targetOutputMessage = String.Format(resourceManager.GetString(resourceName, destinationCulture), dataSetId);
		ChangeCulture(cultureIdentifier);

		// Act
		var outputMessage = logger.Log(_fixture.Create<int>(), LogLevel.Debug, resourceManager, resourceName, new object[] { user, dataSetId }, new object[] { dataSetId });

		// Assert
		Assert.That(outputMessage, Is.EqualTo(targetOutputMessage));
	}

	[Test]
	public void CreateScopeAndLogOverloadForGroupsIsReflected()
	{
		Assert.That(LoggerExtensions.CreateScopeForGroupsAndLogMethod, Is.Not.Null);
	}

	[Test]
	public void CreateScopeAndLogCannotInferGenericTypeParameterInsideValueTuple()
	{
		// Arrange
		var logScope = new LogScope<string>("MyGroup", ("Property", "Value"));
		var logEvent = new LogEvent(0, LogLevel.Debug, "My Message");
		var log = (logScope, logEvent); //! Generic type parameter of logScope (string) is wrapped inside a value tuple, which "disables" automatic type inference.
		var instanceWrapper = _fixture.Create<Mock<InstanceWrapper>>().Object;

		// Act
		/*
		* Since automatic type inference does not work if the generic type parameter is inside a ValueTuple, below call will currently invoke the log method that does not apply the scope to the group identified by the generic parameter.
		* If this test some when fails, this means that type inference does work for value tuples (which is what I would have expected to be the case all along).
		* Therefore the changes made in 'LoggerExtensions.CreateScopeAndLog' can then be removed for the .NET version that does handle this properly.
		
		* Side node: The 'LoggerExtensions.CreateScopeAndLog' functions cannot be tested directly, as those are extension methods, which must be static functions, which in turn cannot be easily substituted.
		*/
		instanceWrapper.CreateScopeAndLog(log);

		// Assert
		Mock.Get(instanceWrapper).Verify(mock => mock.CreateScopeAndLog(It.IsAny<(LogScope, LogEvent)>()), Times.Once);
		Mock.Get(instanceWrapper).Verify(mock => mock.CreateScopeAndLog<string>(It.IsAny<(LogScope<string>, LogEvent)>()), Times.Never);
	}

	[Test]
	public void PayloadIsAppliedAsScope()
	{
		// Arrange
		var logEvent = new LogEvent(0, LogLevel.Debug, "My Message")
		{
			PayLoad = new LogScope(("Property", "Value"))
		};
		var logger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		logger.Log(logEvent);

		// Assert
		Mock.Get(logger).Verify(mock => mock.BeginScope(It.IsAny<IDictionary<string, object>>()), Times.Once);
	}

	[Test]
	public void NullPayloadIsNotAppliedAsScope()
	{
		// Arrange
		var logEvent = new LogEvent(0, LogLevel.Debug, "My Message")
		{
			PayLoad = null
		};
		var logger = _fixture.Create<Mock<ILogger>>().Object;

		// Act
		logger.Log(logEvent);

		// Assert
		Mock.Get(logger).Verify(mock => mock.BeginScope(It.IsAny<IDictionary<string, object>>()), Times.Never);
	}

	#endregion
}