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
    [PSDefaultValue(Value = 100, Help = "100 (valid range: 1-3600)")]
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

    /// <summary>Whether provider clients may use their shared default HTTP transport.</summary>
    protected static bool UsesDefaultTransport(MessageHttpTransportOptions options) {
        return options.ProxyUri is null &&
            options.Timeout == MessageHttpTransportOptions.DefaultTimeout &&
            string.IsNullOrWhiteSpace(options.UserAgent);
    }
}
