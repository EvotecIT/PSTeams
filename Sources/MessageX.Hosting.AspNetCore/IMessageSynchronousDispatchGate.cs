namespace MessageX.Hosting.AspNetCore;

/// <summary>Acquires one slot from the host-wide synchronous dispatch limit.</summary>
public interface IMessageSynchronousDispatchGate {
    /// <summary>Returns a disposable slot, or null when the host is at capacity.</summary>
    IDisposable? TryEnterSynchronousDispatch();
}
