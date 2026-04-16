namespace Serilog.Seq.Test;

internal static class TestConfiguration
{
	public static string SeqHost => GetEnvironmentVariable("SEQ_HOST", "https://your-seq-server.example.com");
	
	public static ushort SeqPort => ushort.Parse(GetEnvironmentVariable("SEQ_PORT", "443"));
	
	public static string ConfigurationApiKey => GetEnvironmentVariable("SEQ_CONFIGURATION_API_KEY", "your-configuration-api-key-here");

	private static string GetEnvironmentVariable(string name, string defaultValue)
	{
		var value = Environment.GetEnvironmentVariable(name);
		return string.IsNullOrWhiteSpace(value) ? defaultValue : value;
	}
}
