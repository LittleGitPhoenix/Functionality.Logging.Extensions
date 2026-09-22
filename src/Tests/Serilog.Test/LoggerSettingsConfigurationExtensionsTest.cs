using System.Reflection;
using NUnit.Framework;
using Phoenix.Functionality.Logging.Extensions.Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Serilog.Test;

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
		Assert.That(levelSwitchField, Is.Not.Null, "Expected _levelSwitch field to be present");
        var levelSwitch = (LoggingLevelSwitch) levelSwitchField!.GetValue(logger)!;
        Assert.That(levelSwitch?.MinimumLevel, Is.EqualTo(LogEventLevel.Fatal));

        // Assert: Sinks
        var aggregateSinkField = type.GetField("_sink", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(aggregateSinkField, Is.Not.Null, "Expected _sink field to be present");
        var aggregateSink = (ILogEventSink) aggregateSinkField!.GetValue(logger)!;
        type = aggregateSink?.GetType();
        var sinksField = type?.GetField("_sinks", BindingFlags.Instance | BindingFlags.NonPublic);
		Assert.That(sinksField, Is.Not.Null, "Expected _sinks field to be present");
        var sinks = (Array) sinksField!.GetValue(aggregateSink)!;
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

    /// <summary>
    /// Verifies that malformed JSON is wrapped in a <see cref="SerilogSettingsException"/>.
    /// </summary>
    [Test]
    public void Check_Malformed_File_Throws()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var settingsFile = testDirectory.CreateFile("serilog.config", "{ malformed");

        // Act
        var exception = Assert.Throws<SerilogSettingsException>(() => new LoggerConfiguration().ReadFrom.JsonFile(settingsFile));

        // Assert
        Assert.That(exception, Is.Not.Null);
        Assert.That(exception!.InnerException, Is.Not.Null);
    }

    /// <summary>
    /// Verifies that a missing configuration file in the application directory is reported when it cannot be copied.
    /// </summary>
    [Test]
    public void Check_Missing_Default_File_Throws()
    {
        // Arrange
        var applicationDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
        var configFileName = $"missing-{Guid.NewGuid():N}.config";
        using var testDirectory = new TestDirectory();
        var originalDirectory = Directory.GetCurrentDirectory();

        try
        {
            Directory.SetCurrentDirectory(testDirectory.Directory.FullName);

            // Act
            var exception = Assert.Throws<SerilogSettingsException>(() => new LoggerConfiguration().ReadFrom.JsonFile(configFileName));

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception!.Message, Does.Contain(configFileName));
            Assert.That(exception.Message, Does.Contain(applicationDirectory.FullName));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDirectory);
        }
    }

    /// <summary>
    /// Verifies that a missing custom section falls back to the default Serilog section.
    /// </summary>
    [Test]
    public void Check_Missing_Custom_Section_Uses_Default_Section()
    {
        // Arrange
        using var testDirectory = new TestDirectory();
        var settingsFile = testDirectory.CreateFile("serilog.config", this.ValidConfigurationContent);

        // Act
        var configuration = new LoggerConfiguration().ReadFrom.JsonFile(settingsFile, "CustomSerilog");

        // Assert
        Assert.That(configuration, Is.Not.Null);
    }
}