using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;

namespace Phoenix.Functionality.Logging.Extensions.Serilog
{
	/// <summary>
	/// Provides extension methods for handling serilog settings.
	/// </summary>
	public static class LoggerSettingsConfigurationExtensions
	{
		private const string DefaultSerilogSectionName = "Serilog";

		/// <summary>
		/// Creates a <see cref="LoggerConfiguration"/> directly from a json file.
		/// </summary>
		/// <param name="loggerSettingsConfiguration"> The extended <see cref="LoggerSettingsConfiguration"/> instance. </param>
		/// <param name="serilogConfigurationFilePath"> The path to the settings file. </param>
		/// <param name="serilogSectionName"> Optional name of the json section that contains the serilog settings. Default is 'Serilog'. </param>
		/// <returns> A new <see cref="LoggerConfiguration"/> instance. </returns>
		/// <exception cref="SerilogSettingsException"> Thrown if the json file could not be parsed or if it contained invalid data. </exception>
		public static LoggerConfiguration JsonFile(this LoggerSettingsConfiguration loggerSettingsConfiguration, string serilogConfigurationFilePath, string serilogSectionName = DefaultSerilogSectionName)
			=> loggerSettingsConfiguration.JsonFile(new FileInfo(serilogConfigurationFilePath), serilogSectionName);

		/// <summary>
		/// Creates a <see cref="LoggerConfiguration"/> directly from a json file.
		/// </summary>
		/// <param name="loggerSettingsConfiguration"> The extended <see cref="LoggerSettingsConfiguration"/> instance. </param>
		/// <param name="serilogConfigurationFile"> The settings file. </param>
		/// <param name="serilogSectionName"> Optional name of the json section that contains the serilog settings. Default is 'Serilog'. </param>
		/// <returns> A new <see cref="LoggerConfiguration"/> instance. </returns>
		/// <exception cref="SerilogSettingsException"> Thrown if the json file could not be parsed or if it contained invalid data. </exception>
		public static LoggerConfiguration JsonFile(this LoggerSettingsConfiguration loggerSettingsConfiguration, FileInfo serilogConfigurationFile, string serilogSectionName = DefaultSerilogSectionName)
		{
			var configuration = LoggerSettingsConfigurationExtensions.LoadConfiguration(serilogConfigurationFile);
			var isValid = LoggerSettingsConfigurationExtensions.VerifyConfiguration(configuration, serilogSectionName, out var foundSerilogSectionName);
			if (!isValid)
			{
				var message = (serilogSectionName.ToLower() == DefaultSerilogSectionName.ToLower())
					? $"The content of file {serilogConfigurationFile.FullName} does not contain a section named '{serilogSectionName}'."
					: $"The content of file {serilogConfigurationFile.FullName} does not contain a section named either '{serilogSectionName}' or '{DefaultSerilogSectionName}'.";
				throw new SerilogSettingsException(message);
			}

			try
			{
				var loggerConfiguration = loggerSettingsConfiguration.Configuration(configuration, foundSerilogSectionName!);
				return loggerConfiguration;
			}
			catch (Exception ex)
			{
				throw new SerilogSettingsException($"The content of file {serilogConfigurationFile.FullName} does not provided serilog configuration or it is invalid.", ex);
			}
		}

		private static IConfiguration LoadConfiguration(FileInfo serilogConfigurationFile)
		{
			try
			{
				return (
							new ConfigurationBuilder()
								.AddJsonFile(serilogConfigurationFile.FullName, false, true)
								.Build()
						)
						?? throw new SerilogSettingsException($"The file {serilogConfigurationFile.FullName} could not be parsed.")
						;
			}
			catch (Exception ex)
			{
				throw new SerilogSettingsException($"The file {serilogConfigurationFile.FullName} could not be parsed.", ex);
			}
		}

#if NETSTANDARD2_0 || NETSTANDARD1_6 || NETSTANDARD1_5 || NETSTANDARD1_4 || NETSTANDARD1_3 || NETSTANDARD1_2 || NETSTANDARD1_1 || NETSTANDARD1_0
		private static bool VerifyConfiguration(IConfiguration configuration, string serilogSectionName, out string? foundSerilogSectionName)
#else
		private static bool VerifyConfiguration(IConfiguration configuration, string serilogSectionName, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out string? foundSerilogSectionName)
#endif

		{
			var serilogSection = configuration.GetSection(serilogSectionName);
			if (serilogSection.Exists())
			{
				foundSerilogSectionName = serilogSectionName;
				return true;
			}

			if (!DefaultSerilogSectionName.Equals(serilogSectionName, StringComparison.OrdinalIgnoreCase))
			{
				return LoggerSettingsConfigurationExtensions.VerifyConfiguration(configuration, DefaultSerilogSectionName, out foundSerilogSectionName);
			}
			
			foundSerilogSectionName = null;
			return false;
		}
	}
}