using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Test
{
	public class LoggerSettingsConfigurationExtensionsTest
	{
		[SetUp]
		public void Setup() { }

		[Test]
		public void Check_Empty_File_Throws()
		{
			// Arrange
			using var testDirectory = new TestDirectory();
			var settingsFile = testDirectory.CreateFile("serilog.config", "");
			
			// Act + Assert
			Assert.Catch<SerilogSettingsException>(() => new LoggerConfiguration().ReadFrom.JsonFile(settingsFile));
		}

		[Test]
		public void Check_Missing_Section_Throws()
		{
			// Arrange
			using var testDirectory = new TestDirectory();
			var settingsFile = testDirectory.CreateFile("serilog.config", "{}");

			// Act + Assert
			Assert.Catch<SerilogSettingsException>(() => new LoggerConfiguration().ReadFrom.JsonFile(settingsFile, "MISSING"));
		}

		[Test]
		public void Check_Valid_File_Succeeds()
		{
			// Arrange
			using var testDirectory = new TestDirectory();
			var contentBuilder = new StringBuilder();
			contentBuilder.AppendLine(@"{");
			contentBuilder.AppendLine(@"  ""Serilog"": {");
			contentBuilder.AppendLine(@"    ""Using"":  [ ""Serilog.Sinks.Debug"", ""Serilog.Sinks.Console"" ],");
			contentBuilder.AppendLine(@"    ""MinimumLevel"": ""Fatal"",");
			contentBuilder.AppendLine(@"    ""WriteTo"": [");
			contentBuilder.AppendLine(@"      { ""Name"": ""Debug"" },");
			contentBuilder.AppendLine(@"      { ""Name"": ""Console"" },");
			contentBuilder.AppendLine(@"    ],");
			contentBuilder.AppendLine(@"    ""Enrich"": [ ""FromLogContext"", ""WithMachineName"", ""WithThreadId"" ],");
			contentBuilder.AppendLine(@"    ""Properties"": {");
			contentBuilder.AppendLine(@"        ""Application"": ""Sample""");
			contentBuilder.AppendLine(@"    }");
			contentBuilder.AppendLine(@"  }");
			contentBuilder.AppendLine(@"}");
			var configurationFile = testDirectory.CreateFile("serilog.config", contentBuilder.ToString());

			// Act
			var configuration = new LoggerConfiguration().ReadFrom.JsonFile(configurationFile);
			var logger = configuration.CreateLogger();
			var type = logger.GetType();

			// Assert: LogLevel
			var levelSwitchField = type.GetField("_levelSwitch", BindingFlags.Instance | BindingFlags.NonPublic);
			var levelSwitch = (LoggingLevelSwitch) levelSwitchField?.GetValue(logger);
			Assert.That(levelSwitch?.MinimumLevel, Is.EqualTo(LogEventLevel.Fatal));

			// Assert: Sinks
			var aggregateSinkField = type.GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic);
			var aggregateSink = (ILogEventSink) aggregateSinkField?.GetValue(logger);
			type = aggregateSink?.GetType();
			var sinksField = type?.GetField("_sinks", BindingFlags.Instance | BindingFlags.NonPublic);
			var sinks = (Array) sinksField?.GetValue(aggregateSink);
			Assert.That(sinks, Has.Length.EqualTo(2));
		}
	}
}