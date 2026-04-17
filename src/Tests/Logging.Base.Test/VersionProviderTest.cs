using System.Reflection;
using Phoenix.Functionality.Logging.Base;

namespace Logging.Base.Test;

public class VersionProviderTest
{
	#region Setup

	[OneTimeSetUp]
	public void BeforeAllTests() { }

	[SetUp]
	public void BeforeEachTest() { }

	[TearDown]
	public void AfterEachTest() { }

	[OneTimeTearDown]
	public void AfterAllTests() { }

	#endregion

	#region Data
	#endregion

	#region Tests

	[Test]
	public void ReturnsNullsWithoutEntryAssembly()
	{
#if NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() does not return null in .NET Core or later.");
#else
		// Arrange + Act
		var (assemblyVersion, fileVersion, informationalVersion) = VersionProvider.GetVersions();

		// Assert
		Assert.That(assemblyVersion, Is.Null);
		Assert.That(fileVersion, Is.Null);
		Assert.That(informationalVersion, Is.Null);
#endif
	}

	[Test]
	public void AssemblyVersionMatchesEntryAssembly()
	{
#if !NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() returns null for tests in .NET Framework.");
#else
		// Arrange
		var expectedAssemblyVersion = Assembly.GetEntryAssembly()?.GetName().Version;

		// Act
		var (assemblyVersion, _, _) = VersionProvider.GetVersions();

		// Assert
		Assert.That(assemblyVersion, Is.EqualTo(expectedAssemblyVersion));
#endif
	}

	[Test]
	public void FileVersionMatchesEntryAssembly()
	{
#if !NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() returns null for tests in .NET Framework.");
#else
		// Arrange
		var entryAssembly = Assembly.GetEntryAssembly();
		var fileVersionString = entryAssembly?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
		Version? expectedFileVersion = null;
		if (!String.IsNullOrWhiteSpace(fileVersionString))
		{
			try { expectedFileVersion = new Version(fileVersionString); }
			catch (Exception) { /* invalid version string — leave expectedFileVersion null */ }
		}

		// Act
		var (_, fileVersion, _) = VersionProvider.GetVersions();

		// Assert
		Assert.That(fileVersion, Is.EqualTo(expectedFileVersion));
#endif
	}

	[Test]
	public void InformationalVersionMatchesEntryAssembly()
	{
#if !NETCOREAPP3_0_OR_GREATER
		Assert.Inconclusive("Assembly.GetEntryAssembly() returns null for tests in .NET Framework.");
#else
		// Arrange
		var entryAssembly = Assembly.GetEntryAssembly();
		var informationalVersionString = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
		var expectedInformationalVersion = String.IsNullOrWhiteSpace(informationalVersionString) ? null : informationalVersionString;

		// Act
		var (_, _, informationalVersion) = VersionProvider.GetVersions();

		// Assert
		Assert.That(informationalVersion, Is.EqualTo(expectedInformationalVersion));
#endif
	}

	#endregion
}
