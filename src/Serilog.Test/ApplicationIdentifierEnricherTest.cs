using System;
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

			LoggerConfiguration? t = new LoggerConfiguration()
				.Enrich.WithApplicationIdentifier(identifier)
				.WriteTo.InMemory();

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

		[Test]
		[TestCase("")]
		[TestCase("SomeApplicationName")]
		[TestCase("acc55406-a8c0-48de-86ad-2c0761ac6e9b")]
		public void Check_Identifier_Length(string value)
		{
			// Act
			var identifier = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(value);
			
			// Assert
			Assert.That(identifier, Has.Length.EqualTo(20));
		}

		/// <summary> Checks that identifier remain identical between different runs, even if the application is restarted. </summary>
		[Test]
		[TestCase("", "ZxlCi0LOMkoTT4qMK4a6")]
		[TestCase("SomeApplicationName", "AcYpzZ7Ps2RYKtz4AMGj")]
		[TestCase("acc55406-a8c0-48de-86ad-2c0761ac6e9b", "kCHNQw3YxfMRuZDpcX0U")]
		public void Check_Identifier_Consistency(string value, string target)
		{
			// Act
			var identifier = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(value);

			// Assert
			Assert.AreEqual(identifier, target);
		}

		[Test]
		public void Check_If_Identical_Values_Produces_Same_Identifier()
		{
			// Arrange
			var objects1 = new String[]
			{
				"ApplicationName",
				"MachineName",
				"7",
			};
			var objects2 = new String[]
			{
				"ApplicationName",
				"MachineName",
				"7",
			};

			// Act
			var identifier1 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects1);
			var identifier2 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects2);

			// Assert
			Assert.AreEqual(identifier1, identifier2);
		}

		[Test]
		public void Check_If_Different_Sorting_Produces_Same_Identifier()
		{
			// Arrange
			var objects1 = new String[]
			{
				"ApplicationName",
				"MachineName",
				"7",
			};
			var objects2 = new String[]
			{
				"7",
				"MachineName",
				"ApplicationName"
			};

			// Act
			var identifier1 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects1);
			var identifier2 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects2);

			// Assert
			Assert.AreEqual(identifier1, identifier2);
		}

		[Test]
		public void Check_If_Different_Values_Produces_Different_Identifier()
		{
			// Arrange
			var objects1 = new String[]
			{
				"ApplicationName",
				"MachineName",
				"7",
			};
			var objects2 = new String[]
			{
				"Unknown",
				"Unknown",
				"0",
			};

			// Act
			var identifier1 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects1);
			var identifier2 = ApplicationIdentifierEnricher.BuildAlphanumericIdentifier(objects2);

			// Assert
			Assert.AreNotEqual(identifier1, identifier2);
		}
	}
}