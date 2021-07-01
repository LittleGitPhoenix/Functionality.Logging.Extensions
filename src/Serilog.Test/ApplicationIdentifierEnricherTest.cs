using System;
using System.Reflection;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog;
using Serilog;
using Serilog.Sinks.InMemory;
using Serilog.Sinks.InMemory.Assertions;

namespace Serilog.Test
{
	public class ApplicationIdentifierEnricherTest
	{
		[SetUp]
		public void Setup() { }

		[Test]
		public void Check_If_Log_Has_Been_Enriched_With_ApplicationIdentifier()
		{
			// Arrange
			var identifier = Guid.NewGuid().ToString();
			var logger = Log.Logger = new LoggerConfiguration()
				.Enrich.WithApplicationIdentifier(identifier)
				.WriteTo.InMemory()
				.CreateLogger()
				;
			
			// Act
			logger.Information(String.Empty);

			// Assert
			InMemorySink.Instance
				.Should()
				.HaveMessage(String.Empty)
				.Appearing().Once()
				.WithProperty(ApplicationIdentifierEnricher.PropertyName)
				.WithValue(identifier)
				;
		}
	}
}