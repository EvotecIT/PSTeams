namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounds host-wide requests whose provider acknowledgement waits for synchronous dispatch.</summary>
public sealed class MessageSynchronousDispatchGate {
    private readonly SemaphoreSlim _slots;

    /// <summary>Creates a gate with a strictly positive concurrent request capacity.</summary>
    public MessageSynchronousDispatchGate(int capacity) {
        if (capacity < 1) {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        _slots = new SemaphoreSlim(capacity, capacity);
    }

    internal IDisposable? TryEnter() => _slots.Wait(0) ? new Lease(_slots) : null;

    private sealed class Lease : IDisposable {
        private SemaphoreSlim? _slots;

        public Lease(SemaphoreSlim slots) => _slots = slots;

        public void Dispose() => Interlocked.Exchange(ref _slots, null)?.Release();
    }
}
