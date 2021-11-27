using System;
using System.IO;
using System.Linq;
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
		public string ValidConfigurationContent => @"
		{
		  ""Serilog"": {
		    ""Using"":  [ ""Serilog.Sinks.Debug"", ""Serilog.Sinks.Console"" ],
		    ""MinimumLevel"": ""Fatal"",
		    ""WriteTo"": [
		      { ""Name"": ""Debug"" },
		      { ""Name"": ""Console"" },
		    ],
		    ""Enrich"": [ ""FromLogContext"", ""WithMachineName"", ""WithThreadId"" ],
		    ""Properties"": {
		        ""Application"": ""Sample""
		    }
		  }
		}
		";

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
			var configurationFile = testDirectory.CreateFile("serilog.config", this.ValidConfigurationContent);

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

		[Test]
		public void Check_Configuration_File_Is_Copied_To_Working_Directory()
		{
			// Arrange
			var configFileName = "serilog.config";
			var applicationDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			var defaultConfigFile = new FileInfo(Path.Combine(applicationDirectory.FullName, configFileName));
			if (defaultConfigFile.Exists) defaultConfigFile.Delete();
			using var testDirectory = new TestDirectory();

			try
			{
				// Arrange
				File.WriteAllText(defaultConfigFile.FullName, this.ValidConfigurationContent);
				Directory.SetCurrentDirectory(testDirectory.Directory.FullName);
				
				// Act + Assert
				Assert.DoesNotThrow(() => new LoggerConfiguration().ReadFrom.JsonFile(configFileName));
				Assert.DoesNotThrow(() => testDirectory.Directory.EnumerateFiles().Single(file => file.Name == configFileName));
			}
			finally
			{
				Directory.SetCurrentDirectory(applicationDirectory.FullName);
				defaultConfigFile.Refresh();
				if (defaultConfigFile.Exists) defaultConfigFile.Delete();
			}
		}
	}
}