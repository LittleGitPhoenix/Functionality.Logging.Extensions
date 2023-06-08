#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Base;

/// <summary>
/// <see cref="LogApplicationInformation"/> builder.
/// </summary>
public class LogApplicationInformationBuilder
	: ILogApplicationInformationBuilder,
		ILogApplicationAuxiliaryInformationBuilder,
		ILogApplicationInformationCreator
{
	#region Delegates / Events
	#endregion

	#region Constants
	#endregion

	#region Fields

	private readonly System.Text.StringBuilder _identifierBuilder;

	#endregion

	#region Properties
	#endregion

	#region (De)Constructors

	internal LogApplicationInformationBuilder()
	{
		// Save parameters.

		// Initialize fields.
		_identifierBuilder = new();
	}

	#endregion

	#region Methods

	#region Information

	/// <inheritdoc />
	public ILogApplicationInformationCreator StartingWithApplicationName(string? fallback = null)
		=> this.Append(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? fallback ?? "[UNKNOWN]");

	/// <inheritdoc />
	public ILogApplicationInformationCreator StartingWith(string information)
		=> this.Append(information);

	/// <inheritdoc />
	public ILogApplicationInformationCreator AndMachineName()
		=> this.Append(Environment.MachineName);

	/// <inheritdoc />
	public ILogApplicationInformationCreator AndUserDomain()
		=> this.Append(Environment.UserDomainName);

	/// <inheritdoc />
	public ILogApplicationInformationCreator AndUserName()
		=> this.Append(Environment.UserName);

	/// <inheritdoc />
	public ILogApplicationInformationCreator AndOperatingSystemInformation(char? replacer = null)
		=> this.Append(Environment.OSVersion.ToString().Replace(' ', replacer ?? '_'));

	/// <inheritdoc />
	public ILogApplicationInformationCreator And(string information)
		=> this.Append(information);

	#endregion

	#region Separation

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByAt()
		=> this.Append('@');

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByDash()
		=> this.Append('-');

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByHash()
		=> this.Append('#');

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByUnderscore()
		=> this.Append('_');

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedBy(char separator)
		=> this.Append(separator);

	/// <inheritdoc />
	public ILogApplicationAuxiliaryInformationBuilder SeparatedBy(string separator)
		=> this.Append(separator);

	#endregion

	#region Build

	private LogApplicationInformationBuilder Append(char value)
	{
		_identifierBuilder.Append(value);
		return this;
	}

	private LogApplicationInformationBuilder Append(string value)
	{
		_identifierBuilder.Append(value);
		return this;
	}

	/// <inheritdoc />
	public LogApplicationInformation Build()
	{
		var name = _identifierBuilder.ToString();
		return new LogApplicationInformation(name);
	}

	#endregion

	#endregion
}

/// <summary>
/// Partial builder interface for <see cref="LogApplicationInformation"/>.
/// </summary>
public interface ILogApplicationInformationBuilder
{
	/// <summary>
	/// Adds the name of the currently running application as start of the <see cref="LogApplicationInformation.Name"/>.
	/// </summary>
	/// <param name="fallback"> Optional fallback if the application name could not be determined. Default is [UNKNOWN]. </param>
	ILogApplicationInformationCreator StartingWithApplicationName(string? fallback = null);

	/// <summary>
	/// Adds <paramref name="information"/> as start of the <see cref="LogApplicationInformation.Name"/>.
	/// </summary>
	ILogApplicationInformationCreator StartingWith(string information);
}

/// <summary>
/// Partial builder interface for <see cref="LogApplicationInformation"/>.
/// </summary>
public interface ILogApplicationAuxiliaryInformationBuilder
{
	/// <summary>
	/// Adds the name of the machine (host).
	/// </summary>
	ILogApplicationInformationCreator AndMachineName();

	/// <summary>
	/// Adds the name of the domain name of the currently logged-in user.
	/// </summary>
	ILogApplicationInformationCreator AndUserDomain();

	/// <summary>
	/// Adds the name of the currently logged-in user.
	/// </summary>
	ILogApplicationInformationCreator AndUserName();

	/// <summary>
	/// Adds the operating system identifier.
	/// </summary>
	/// <param name="replacer"> An optional replacer used to escape whitespace characters. Default is '_'. </param>
	ILogApplicationInformationCreator AndOperatingSystemInformation(char? replacer = null);

	/// <summary>
	/// Adds <paramref name="information"/>.
	/// </summary>
	ILogApplicationInformationCreator And(string information);
}

/// <summary>
/// Partial builder interface for <see cref="LogApplicationInformation"/>.
/// </summary>
public interface ILogApplicationInformationCreator
{
	/// <summary>
	/// Adds a '@' as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByAt();

	/// <summary>
	/// Adds a '-' as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByDash();

	/// <summary>
	/// Adds a '#' as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByHash();

	/// <summary>
	/// Adds a '_' as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedByUnderscore();

	/// <summary>
	/// Adds <paramref name="separator"/> as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedBy(char separator);

	/// <summary>
	/// Adds <paramref name="separator"/> as separator.
	/// </summary>
	public ILogApplicationAuxiliaryInformationBuilder SeparatedBy(string separator);

	/// <summary>
	/// Builds the <see cref="LogApplicationInformation"/>.
	/// </summary>
	public LogApplicationInformation Build();
}