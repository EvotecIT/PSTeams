namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageDurableIngressHealth : IMessageDurableIngressHealth {
    private long _accepted;
    private long _claimed;
    private long _completed;
    private long _retried;
    private long _deadLettered;
    private long _leaseRenewed;
    private long _leaseLost;
    private long _unavailable;
    private long _lastCompletedTicks;
    private long _lastFailureTicks;
    private int _stopping;

    public void Accepted() => Interlocked.Increment(ref _accepted);

    public void Claimed(int count) => Interlocked.Add(ref _claimed, count);

    public void Completed(DateTimeOffset at) {
        Interlocked.Increment(ref _completed);
        Interlocked.Exchange(ref _lastCompletedTicks, at.UtcTicks);
    }

    public void Retried(DateTimeOffset at) {
        Interlocked.Increment(ref _retried);
        Interlocked.Exchange(ref _lastFailureTicks, at.UtcTicks);
    }

    public void DeadLettered(DateTimeOffset at) {
        Interlocked.Increment(ref _deadLettered);
        Interlocked.Exchange(ref _lastFailureTicks, at.UtcTicks);
    }

    public void LeaseRenewed() => Interlocked.Increment(ref _leaseRenewed);

    public void LeaseLost(DateTimeOffset at) {
        Interlocked.Increment(ref _leaseLost);
        Interlocked.Exchange(ref _lastFailureTicks, at.UtcTicks);
    }

    public void Unavailable(DateTimeOffset at) {
        Interlocked.Increment(ref _unavailable);
        Interlocked.Exchange(ref _lastFailureTicks, at.UtcTicks);
    }

    public void Stopping() => Volatile.Write(ref _stopping, 1);

    public MessageDurableIngressHealthSnapshot GetHealthSnapshot() => new(
        Interlocked.Read(ref _accepted),
        Interlocked.Read(ref _claimed),
        Interlocked.Read(ref _completed),
        Interlocked.Read(ref _retried),
        Interlocked.Read(ref _deadLettered),
        Interlocked.Read(ref _leaseRenewed),
        Interlocked.Read(ref _leaseLost),
        Interlocked.Read(ref _unavailable),
        Volatile.Read(ref _stopping) != 0,
        Timestamp(Interlocked.Read(ref _lastCompletedTicks)),
        Timestamp(Interlocked.Read(ref _lastFailureTicks)));

    private static DateTimeOffset? Timestamp(long ticks) =>
        ticks == 0 ? null : new DateTimeOffset(ticks, TimeSpan.Zero);
}
