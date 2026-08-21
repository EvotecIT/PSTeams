using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Provides shared enterprise HTTP transport parameters to provider delivery cmdlets.</summary>
public abstract class MessageHttpCmdletBase : AsyncPSCmdlet {
    /// <summary>HTTP proxy used for provider requests.</summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    /// <summary>HTTP request timeout in seconds.</summary>
    [Parameter(Mandatory = false)]
    [ValidateRange(1, 3600)]
    public int TimeoutSeconds { get; set; } = 100;

    /// <summary>Optional product user-agent sent with provider requests.</summary>
    [Parameter(Mandatory = false)]
    public string? UserAgent { get; set; }

    /// <summary>Creates reusable transport options from the bound PowerShell parameters.</summary>
    protected MessageHttpTransportOptions CreateTransportOptions() {
        return new MessageHttpTransportOptions {
            ProxyUri = Proxy,
            Timeout = TimeSpan.FromSeconds(TimeoutSeconds),
            UserAgent = UserAgent
        };
    }
}
