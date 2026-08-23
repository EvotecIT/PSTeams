namespace MessageX.Hosting.AspNetCore;

/// <summary>Releases an accepted synchronous replay reservation after dispatch cannot complete.</summary>
public interface IMessageIngressReservationRelease {
    /// <summary>Releases one previously accepted dispatch-ready coordinate.</summary>
    void Release<TProviderPayload>(MessageReceiveResult<TProviderPayload> result);
}
