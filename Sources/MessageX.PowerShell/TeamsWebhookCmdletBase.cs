using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>
/// Provides consistent enterprise HTTP transport parameters to Teams webhook cmdlets.
/// </summary>
public abstract class TeamsWebhookCmdletBase : AsyncPSCmdlet {
    private TeamsClientLease? _clientLease;

    /// <summary>HTTP proxy used for the webhook request.</summary>
    [Parameter(Mandatory = false)]
    public Uri? Proxy { get; set; }

    /// <summary>HTTP request timeout in seconds.</summary>
    [Parameter(Mandatory = false)]
    [ValidateRange(1, 3600)]
    public int TimeoutSeconds { get; set; } = 100;

    /// <summary>Optional product user-agent sent with the webhook request.</summary>
    [Parameter(Mandatory = false)]
    public string? UserAgent { get; set; }

    /// <summary>Creates one Teams client for the complete PowerShell cmdlet lifecycle.</summary>
    protected override Task BeginProcessingAsync() {
        _clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(Proxy, TimeoutSeconds, UserAgent);
        return Task.CompletedTask;
    }

    /// <summary>Runs one operation with the lifecycle-scoped Teams client.</summary>
    protected async Task<TeamsDeliveryResult> SendWithClientAsync(
        Func<TeamsClient, Task<TeamsDeliveryResult>> send) {
        if (send is null) {
            throw new ArgumentNullException(nameof(send));
        }

        var lease = _clientLease ?? throw new InvalidOperationException(
            "The Teams webhook client is not available outside the cmdlet processing lifecycle.");
        return await send(lease.Client).ConfigureAwait(false);
    }

    /// <summary>Releases the lifecycle-scoped Teams client after all pipeline records are processed.</summary>
    protected override Task EndProcessingAsync() {
        DisposeClientLease();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose() {
        DisposeClientLease();
        base.Dispose();
    }

    private void DisposeClientLease() {
        var lease = _clientLease;
        _clientLease = null;
        lease?.Dispose();
    }
}
