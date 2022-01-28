#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// <see cref="SeqServerApplicationInformation"/> builder.
/// </summary>
public class SeqServerApplicationInformationBuilder
	: ISeqServerApplicationInformationBuilder,
		ISeqServerApplicationAuxiliaryInformationBuilder,
		ISeqServerApplicationInformationCreator
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

	internal SeqServerApplicationInformationBuilder()
	{
		// Save parameters.

		// Initialize fields.
		_identifierBuilder = new ();
	}

	#endregion

	#region Methods

	#region Information

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator StartingWithApplicationName(string? fallback = null)
		=> this.Append(System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name ?? fallback ?? "[UNKNOWN]");

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator StartingWith(string information)
		=> this.Append(information);

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator AndMachineName()
		=> this.Append(Environment.MachineName);

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator AndUserDomain()
		=> this.Append(Environment.UserDomainName);

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator AndUserName()
		=> this.Append(Environment.UserName);

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator AndOperatingSystemInformation(char? replacer = null)
		=> this.Append(Environment.OSVersion.ToString().Replace(' ', replacer ?? '_'));

	/// <inheritdoc />
	public ISeqServerApplicationInformationCreator And(string information)
		=> this.Append(information);

	#endregion

	#region Separation

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByAt()
		=> this.Append('@');

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByDash()
		=> this.Append('-');

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByHash()
		=> this.Append('#');

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByUnderscore()
		=> this.Append('_');

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedBy(char separator)
		=> this.Append(separator);

	/// <inheritdoc />
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedBy(string separator)
		=> this.Append(separator);

	#endregion

	#region Build

	private SeqServerApplicationInformationBuilder Append(char value)
	{
		_identifierBuilder.Append(value);
		return this;
	}

	private SeqServerApplicationInformationBuilder Append(string value)
	{
		_identifierBuilder.Append(value);
		return this;
	}

	/// <inheritdoc />
	public SeqServerApplicationInformation Build()
	{
		return new SeqServerApplicationInformation(_identifierBuilder.ToString());
	}

	#endregion

	#endregion
}

/// <summary>
/// Partial builder interface for <see cref="SeqServerApplicationInformation"/>.
/// </summary>
public interface ISeqServerApplicationInformationBuilder
{
	/// <summary>
	/// Adds the name of the currently running application as start of the <see cref="SeqServerApplicationInformation.Identifier"/>.
	/// </summary>
	/// <param name="fallback"> Optional fallback if the application name could not be determined. Default is [UNKNOWN]. </param>
	ISeqServerApplicationInformationCreator StartingWithApplicationName(string? fallback = null);

	/// <summary>
	/// Adds <paramref name="information"/> as start of the <see cref="SeqServerApplicationInformation.Identifier"/>.
	/// </summary>
	ISeqServerApplicationInformationCreator StartingWith(string information);
}

/// <summary>
/// Partial builder interface for <see cref="SeqServerApplicationInformation"/>.
/// </summary>
public interface ISeqServerApplicationAuxiliaryInformationBuilder
{
	/// <summary>
	/// Adds the name of the machine (host).
	/// </summary>
	ISeqServerApplicationInformationCreator AndMachineName();

	/// <summary>
	/// Adds the name of the domain name of the currently logged-in user.
	/// </summary>
	ISeqServerApplicationInformationCreator AndUserDomain();

	/// <summary>
	/// Adds the name of the currently logged-in user.
	/// </summary>
	ISeqServerApplicationInformationCreator AndUserName();

	/// <summary>
	/// Adds the operating system identifier.
	/// </summary>
	/// <param name="replacer"> An optional replacer used to escape whitespace characters. Default is '_'. </param>
	ISeqServerApplicationInformationCreator AndOperatingSystemInformation(char? replacer = null);
	
	/// <summary>
	/// Adds <paramref name="information"/>.
	/// </summary>
	ISeqServerApplicationInformationCreator And(string information);
}

/// <summary>
/// Partial builder interface for <see cref="SeqServerApplicationInformation"/>.
/// </summary>
public interface ISeqServerApplicationInformationCreator
{
	/// <summary>
	/// Adds a '@' as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByAt();

	/// <summary>
	/// Adds a '-' as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByDash();

	/// <summary>
	/// Adds a '#' as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByHash();

	/// <summary>
	/// Adds a '_' as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedByUnderscore();

	/// <summary>
	/// Adds <paramref name="separator"/> as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedBy(char separator);

	/// <summary>
	/// Adds <paramref name="separator"/> as separator.
	/// </summary>
	public ISeqServerApplicationAuxiliaryInformationBuilder SeparatedBy(string separator);

	/// <summary>
	/// Builds the <see cref="SeqServerApplicationInformation"/>.
	/// </summary>
	public SeqServerApplicationInformation Build();
}