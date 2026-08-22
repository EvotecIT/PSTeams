namespace MessageX.Hosting.AspNetCore;

/// <summary>Verified provider-neutral work item consumed by the ingress worker.</summary>
public interface IMessageIngressWorkItem {
    /// <summary>Dispatches the verified envelope through the configured router.</summary>
    Task<MessageDispatchResult> DispatchAsync(
        MessageRouter router,
        CancellationToken cancellationToken);
}
