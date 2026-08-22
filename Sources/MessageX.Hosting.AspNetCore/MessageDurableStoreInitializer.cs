namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableStoreInitializer : IDisposable {
    private readonly IMessageDurableStore _store;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private int _initialized;

    public MessageDurableStoreInitializer(IMessageDurableStore store) =>
        _store = store ?? throw new ArgumentNullException(nameof(store));

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken) {
        if (Volatile.Read(ref _initialized) != 0) {
            return;
        }
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            if (_initialized == 0) {
                await _store.InitializeAsync(cancellationToken).ConfigureAwait(false);
                Volatile.Write(ref _initialized, 1);
            }
        } finally {
            _gate.Release();
        }
    }

    public void Dispose() => _gate.Dispose();
}
