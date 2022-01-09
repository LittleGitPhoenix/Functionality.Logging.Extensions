#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Provides interaction logic for a seq server.
/// </summary>
/// <remarks> This is a wrapper for the static functionality from <see cref="SeqServerHelper"/>. </remarks>
public class SeqServer
{
    #region Delegates / Events
    #endregion

    #region Constants
    #endregion

    #region Fields
    #endregion

    #region Properties
		
    /// <summary> Connection data for the seq server. </summary>
    public SeqServerConnectionData ConnectionData { get; }

    #endregion

    #region (De)Constructors

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="seqHost"> <see cref="SeqServerConnectionData.Host"/> </param>
    /// <param name="seqPort"> <see cref="SeqServerConnectionData.Port"/> </param>
    /// <param name="configurationApiKey"> <see cref="SeqServerConnectionData.ApiKey"/> </param>
    public SeqServer(string seqHost, ushort? seqPort, string? configurationApiKey = null)
        : this(new SeqServerConnectionData(seqHost, seqPort, configurationApiKey))
    { }

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="connectionData"> The <see cref="SeqServerConnectionData"/> used to connect to the seq server. </param>
    public SeqServer(SeqServerConnectionData connectionData)
    {
        // Save parameters.
        this.ConnectionData = connectionData;
    }

    #endregion

    #region Methods

    #region Application registration

    /// <summary>
    /// Registers the application with <paramref name="applicationTitle"/> with the seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
    /// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual string RegisterApplication(string applicationTitle, TimeSpan timeout)
        => this.RegisterApplication(new SeqServerApplicationInformation(applicationTitle), timeout);

    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual string RegisterApplication(SeqServerApplicationInformation applicationInformation, TimeSpan timeout)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        return this.RegisterApplication(applicationInformation, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Registers the application with <paramref name="applicationTitle"/> with the seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual string RegisterApplication(string applicationTitle, CancellationToken cancellationToken = default)
        => this.RegisterApplication(new SeqServerApplicationInformation(applicationTitle), cancellationToken);

    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual string RegisterApplication(SeqServerApplicationInformation applicationInformation, CancellationToken cancellationToken = default)
    {
        var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationInformation.Identifier);
        this.RegisterApplication(applicationInformation, apiKey, cancellationToken);
        return apiKey;
    }

    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> and <paramref name="apiKey"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="apiKey"> The api key. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    private void RegisterApplication(SeqServerApplicationInformation applicationInformation, string apiKey, CancellationToken cancellationToken = default)
    {
        try
        {
            this.RegisterApplicationAsync(applicationInformation, apiKey, cancellationToken).Wait(CancellationToken.None);
        }
        //! Catching SeqServerApplicationRegisterException should not be necessary, as "Wait()" should per specification throw an AggregateException, but unit test proved otherwise.
        catch (SeqServerApplicationRegisterException)
        {
            throw;
        }
        catch (OperationCanceledException ex)
        {
            throw new SeqServerApplicationRegisterException(apiKey, $"Registering the application '{applicationInformation.Identifier}' with the seq server '{this.ConnectionData.Url}' was cancelled.", new[] { ex });
        }
        catch (AggregateException ex) when (ex.InnerExceptions.FirstOrDefault() is SeqServerApplicationRegisterException seqServerApplicationRegisterException)
        {
            throw seqServerApplicationRegisterException;
        }
        catch (Exception ex)
        {
            throw new SeqServerApplicationRegisterException(apiKey, $"An error occurred registering the application '{applicationInformation.Identifier}' with the seq server '{this.ConnectionData.Url}'. See the inner exception for more details.", new[] { ex });
        }
    }

    /// <summary>
    /// Registers the application with <paramref name="applicationTitle"/> with the seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
    /// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual Task<string> RegisterApplicationAsync(string applicationTitle, TimeSpan timeout)
        => this.RegisterApplicationAsync(new SeqServerApplicationInformation(applicationTitle), timeout);

    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="timeout"> The amount of time to wait until registering has to be successful. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual Task<string> RegisterApplicationAsync(SeqServerApplicationInformation applicationInformation, TimeSpan timeout)
    {
        using var cancellationTokenSource = new CancellationTokenSource(timeout);
        return this.RegisterApplicationAsync(applicationInformation, cancellationTokenSource.Token);
    }

    /// <summary>
    /// Registers the application with <paramref name="applicationTitle"/> with the seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that will be used to create the api key. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual Task<string> RegisterApplicationAsync(string applicationTitle, CancellationToken cancellationToken = default)
        => this.RegisterApplicationAsync(new SeqServerApplicationInformation(applicationTitle), cancellationToken);

    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> The generated api key. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    public virtual async Task<string> RegisterApplicationAsync(SeqServerApplicationInformation applicationInformation, CancellationToken cancellationToken = default)
    {
        var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationInformation.Identifier);
        await this.RegisterApplicationAsync(applicationInformation, apiKey, cancellationToken).ConfigureAwait(false);
        return apiKey;
    }
		
    /// <summary>
    /// Registers the application via <paramref name="applicationInformation"/> and <paramref name="apiKey"/> with the seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application to register. </param>
    /// <param name="apiKey"> The api key. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> An awaitable <see cref="Task"/>. </returns>
    /// <exception cref="SeqServerApplicationRegisterException"> Thrown registering the application failed. </exception>
    /// <remarks> <see cref="OperationCanceledException"/> will be wrapped into an <see cref="SeqServerApplicationRegisterException"/>. </remarks>
    internal virtual async Task RegisterApplicationAsync(SeqServerApplicationInformation applicationInformation, string apiKey, CancellationToken cancellationToken = default)
    {
        var connection = SeqServerHelper.ConnectToSeq(this.ConnectionData.Host, this.ConnectionData.Port, this.ConnectionData.ApiKey);
        try
        {
            await SeqServerHelper.RegisterApiKeyAsync(applicationInformation.Identifier, apiKey, connection, cancellationToken).ConfigureAwait(false);
        }
        catch (SeqServerException ex)
        {
            throw SeqServerApplicationRegisterException.CreateFromSeqServerException(apiKey, ex);
        }
        catch (OperationCanceledException ex)
        {
            throw new SeqServerApplicationRegisterException(apiKey, $"Registering the application '{applicationInformation.Identifier}' with the seq server '{this.ConnectionData.Url}' was cancelled.", new[] { ex });
        }
        catch (Exception ex)
        {
            throw new SeqServerApplicationRegisterException(apiKey, $"An error occurred registering the application '{applicationInformation.Identifier}' with the seq server '{this.ConnectionData.Url}'. See the inner exception for more details.", new[] { ex });
        }
        finally
        {
            connection.Dispose();
        }
    }

    #endregion

    #region Log file transmission

    /// <summary>
    /// Sends log events to a seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that produced the log file. This will be used to create the api key. </param>
    /// <param name="logFile"> A reference to the json log file. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
    /// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
    public virtual void SendLogFile(string applicationTitle, FileInfo logFile, CancellationToken cancellationToken = default)
        => this.SendLogFile(new SeqServerApplicationInformation(applicationTitle), logFile, cancellationToken);

    /// <summary>
    /// Sends log events to a seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application that produced the log file. </param>
    /// <param name="logFile"> A reference to the json log file. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
    /// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
    public virtual void SendLogFile(SeqServerApplicationInformation applicationInformation, FileInfo logFile, CancellationToken cancellationToken = default)
    {
        try
        {
            this.SendLogFileAsync(applicationInformation.Identifier, logFile, cancellationToken).Wait(CancellationToken.None);
        }
        //! Catching SeqServerException should not be necessary, as "Wait()" should per specification throw an AggregateException, but unit test proved otherwise.
        catch (SeqServerException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SeqServerException($"An error occurred while sending the log events to the seq server '{this.ConnectionData.Url}'. See the inner exception for more details.", new[] {ex});
        }
    }

    /// <summary>
    /// Sends log events to a seq server.
    /// </summary>
    /// <param name="applicationTitle"> The title/name of the application that produced the log file. </param>
    /// <param name="logFile"> A reference to the json log file. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> An awaitable <see cref="Task"/>. </returns>
    /// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
    /// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
    public virtual Task SendLogFileAsync(string applicationTitle, FileInfo logFile, CancellationToken cancellationToken = default)
        => this.SendLogFileAsync(new SeqServerApplicationInformation(applicationTitle), logFile, cancellationToken);

    /// <summary>
    /// Sends log events to a seq server.
    /// </summary>
    /// <param name="applicationInformation"> The <see cref="SeqServerApplicationInformation"/> of the application that produced the log file. </param>
    /// <param name="logFile"> A reference to the json log file. </param>
    /// <param name="cancellationToken"> A <see cref="CancellationToken"/>. </param>
    /// <returns> An awaitable <see cref="Task"/>. </returns>
    /// <exception cref="OperationCanceledException"> Thrown if the <paramref name="cancellationToken"/> triggered. </exception>
    /// <exception cref="SeqServerException"> Thrown if sending the log events to the seq server failed. </exception>
    public virtual async Task SendLogFileAsync(SeqServerApplicationInformation applicationInformation, FileInfo logFile, CancellationToken cancellationToken = default)
    {
        var apiKey = IdentifierBuilder.BuildAlphanumericIdentifier(applicationInformation.Identifier);
        var connection = SeqServerHelper.ConnectToSeq(this.ConnectionData.Host, this.ConnectionData.Port, this.ConnectionData.ApiKey);
        try
        {
            await SeqServerHelper.SendLogFileToServerAsync(apiKey, logFile, connection, cancellationToken).ConfigureAwait(false);
        }

        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SeqServerException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new SeqServerException($"An error occurred while sending the log events to the seq server '{this.ConnectionData.Url}'. See the inner exception for more details.", new[] {ex});
        }
        finally
        {
            connection.Dispose();
        }
    }

    #endregion

    #endregion
}