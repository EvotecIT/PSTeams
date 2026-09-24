namespace MessageX.Hosting.AspNetCore;

/// <summary>Exposes payload-free operational state for durable outbound delivery.</summary>
public interface IMessageDurableOutboxHealth {
    /// <summary>Returns process-local delivery counters without provider data or exception details.</summary>
    MessageDurableOutboxHealthSnapshot GetHealthSnapshot();
}
