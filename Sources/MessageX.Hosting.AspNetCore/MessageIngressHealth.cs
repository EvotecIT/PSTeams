namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageIngressHealth {
    private readonly int _capacity;
    private long _accepted;
    private long _completed;
    private long _failed;
    private long _lastCompletedTicks;
    private long _lastFailureTicks;
    private int _stopping;

    public MessageIngressHealth(int capacity) => _capacity = capacity;

    public void Accepted() {
        Interlocked.Increment(ref _accepted);
    }

    public void Unaccepted() {
        Interlocked.Decrement(ref _accepted);
    }

    public void Completed(DateTimeOffset at) {
        Interlocked.Increment(ref _completed);
        Interlocked.Exchange(ref _lastCompletedTicks, at.UtcTicks);
    }

    public void Failed(DateTimeOffset at) {
        Interlocked.Increment(ref _failed);
        Interlocked.Exchange(ref _lastFailureTicks, at.UtcTicks);
    }

    public void Stopping() => Interlocked.Exchange(ref _stopping, 1);

    public MessageIngressHealthSnapshot Snapshot(int queued) => new(
        _capacity,
        queued,
        Interlocked.Read(ref _accepted),
        Interlocked.Read(ref _completed),
        Interlocked.Read(ref _failed),
        Volatile.Read(ref _stopping) != 0,
        ReadTimestamp(ref _lastCompletedTicks),
        ReadTimestamp(ref _lastFailureTicks));

    private static DateTimeOffset? ReadTimestamp(ref long location) {
        var ticks = Interlocked.Read(ref location);
        return ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
    }
}
