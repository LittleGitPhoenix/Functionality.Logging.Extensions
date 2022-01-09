#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


namespace Phoenix.Functionality.Logging.Extensions.Serilog.Seq;

/// <summary>
/// Connection data for a seq server.
/// </summary>
/// <param name="Host"> The host address of the seq server. This can already include the port like 'http://localhost:5431'. </param>
/// <param name="Port"> Optional port where the seq server listens. It will be added to <see cref="Host"/>. </param>
/// <param name="ApiKey"> The api key that allows changes to the seq server configuration. </param>
public record SeqServerConnectionData(string Host, ushort? Port, string? ApiKey)
{
    /// <summary> A combination of <see cref="Host"/> and <see cref="Port"/>. </summary>
    public string Url
    {
        get
        {
            
            return _url ??= SeqServerHelper.BuildSeqUrl(this.Host, this.Port);
        }
    }
    private string? _url;
}