namespace MessageX.PowerShell;

/// <summary>
/// Provides consistent enterprise HTTP transport parameters to Teams webhook cmdlets.
/// </summary>
public abstract class TeamsWebhookCmdletBase : MessageHttpCmdletBase {
    private TeamsClientLease? _clientLease;

    /// <summary>Creates one Teams client for the complete PowerShell cmdlet lifecycle.</summary>
    protected override Task BeginProcessingAsync() {
        _clientLease = TeamsPowerShellDeliverySupport.CreateClientLease(CreateTransportOptions());
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
