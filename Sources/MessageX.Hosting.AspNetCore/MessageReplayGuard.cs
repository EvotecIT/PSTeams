namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded installation-scoped replay suppression for accepted in-memory ingress.</summary>
public sealed class MessageReplayGuard : IDisposable {
    private static readonly TimeSpan MaximumTimerDueTime =
        TimeSpan.FromMilliseconds(int.MaxValue - 1L);
    private static readonly MessageAcknowledgement RetryableAcknowledgement =
        MessageAcknowledgement.Empty(503);
    private readonly object _sync = new();
    private readonly Dictionary<string, LinkedListNode<Entry>> _accepted = new(StringComparer.Ordinal);
    private readonly LinkedList<Entry> _expirations = new();
    private readonly int _capacity;
    private readonly TimeSpan _retention;
    private readonly int _acknowledgementBodyCapacity;
    private readonly TimeProvider _timeProvider;
    private ITimer? _expirationTimer;
    private DateTimeOffset _lastObservedAt = DateTimeOffset.MinValue;
    private int _acknowledgementBodyBytes;
    private bool _disposed;

    /// <summary>Creates a bounded replay guard.</summary>
    public MessageReplayGuard(int capacity, TimeSpan retention) : this(
        capacity,
        retention,
        MessageXHostingAspNetCoreOptions.DefaultReplayAcknowledgementBodyBytes,
        TimeProvider.System) {
    }

    /// <summary>Creates a replay guard with an explicit acknowledgement-body budget and clock.</summary>
    public MessageReplayGuard(
        int capacity,
        TimeSpan retention,
        int acknowledgementBodyCapacity,
        TimeProvider timeProvider) {
        if (capacity < 1) {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        if (retention <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(retention));
        }
        if (acknowledgementBodyCapacity is < 1 or
            > MessageXHostingAspNetCoreOptions.MaximumReplayAcknowledgementBodyBytes) {
            throw new ArgumentOutOfRangeException(nameof(acknowledgementBodyCapacity));
        }
        _capacity = capacity;
        _retention = retention;
        _acknowledgementBodyCapacity = acknowledgementBodyCapacity;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    /// <summary>Atomically suppresses duplicates and records the coordinate only after ingress accepts it.</summary>
    public MessageReplayAcceptance TryAccept<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        DateTimeOffset now,
        Func<MessageIngressEnqueueStatus> accept) {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(accept);
        if (result.Envelope is null) {
            throw new ArgumentException("A dispatch-ready envelope is required.", nameof(result));
        }
        var key = string.Join(
            "\n",
            result.Envelope.Provider,
            result.Envelope.InstallationId,
            result.Envelope.DeduplicationKey);
        lock (_sync) {
            ThrowIfDisposed();
            now = ObserveNow(now);
            Prune(now);
            ScheduleExpiration(now);
            if (_accepted.ContainsKey(key)) {
                return MessageReplayAcceptance.Duplicate;
            }
            if (_accepted.Count >= _capacity) {
                return MessageReplayAcceptance.Full;
            }
            var expiresAt = now.Add(_retention);
            var enqueue = accept();
            var rejected = enqueue switch {
                MessageIngressEnqueueStatus.Accepted => (MessageReplayAcceptance?)null,
                MessageIngressEnqueueStatus.Duplicate => MessageReplayAcceptance.Duplicate,
                MessageIngressEnqueueStatus.Full => MessageReplayAcceptance.Full,
                MessageIngressEnqueueStatus.Stopping => MessageReplayAcceptance.Stopping,
                MessageIngressEnqueueStatus.Unavailable => MessageReplayAcceptance.Unavailable,
                _ => throw new InvalidOperationException("The ingress owner returned an unsupported acceptance state.")
            };
            if (rejected.HasValue) {
                return rejected.Value;
            }
            var node = _expirations.AddLast(new Entry(key, expiresAt));
            _accepted.Add(key, node);
            ScheduleExpiration(now);
            return MessageReplayAcceptance.Accepted;
        }
    }

    /// <summary>Releases a reserved coordinate when accepted synchronous work cannot be dispatched.</summary>
    public void Release<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Envelope is null) {
            return;
        }
        var key = string.Join(
            "\n",
            result.Envelope.Provider,
            result.Envelope.InstallationId,
            result.Envelope.DeduplicationKey);
        lock (_sync) {
            ThrowIfDisposed();
            if (_accepted.Remove(key, out var node)) {
                Remove(node, RetryableAcknowledgement);
                ScheduleExpiration(ObserveNow(_timeProvider.GetUtcNow()));
            }
        }
    }

    /// <summary>Waits for the original synchronous dispatch acknowledgement.</summary>
    public ValueTask<MessageAcknowledgement> WaitForAcknowledgementAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken) {
        var key = GetKey(result);
        Task<MessageAcknowledgement> acknowledgement;
        lock (_sync) {
            ThrowIfDisposed();
            if (!_accepted.TryGetValue(key, out var node)) {
                return ValueTask.FromResult(RetryableAcknowledgement);
            }
            acknowledgement = node.Value.Acknowledgement.Task;
        }
        return new ValueTask<MessageAcknowledgement>(acknowledgement.WaitAsync(cancellationToken));
    }

    /// <summary>Publishes the original synchronous dispatch acknowledgement.</summary>
    public void Complete<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        MessageAcknowledgement acknowledgement) {
        ArgumentNullException.ThrowIfNull(acknowledgement);
        var key = GetKey(result);
        lock (_sync) {
            ThrowIfDisposed();
            if (!_accepted.TryGetValue(key, out var node) || node.Value.IsCompleted) {
                return;
            }
            if (_acknowledgementBodyBytes >
                _acknowledgementBodyCapacity - acknowledgement.BodyLength) {
                _accepted.Remove(key);
                Remove(node, RetryableAcknowledgement);
                ScheduleExpiration(ObserveNow(_timeProvider.GetUtcNow()));
                return;
            }
            if (node.Value.Acknowledgement.TrySetResult(acknowledgement)) {
                node.Value.IsCompleted = true;
                node.Value.RetainedBodyBytes = acknowledgement.BodyLength;
                _acknowledgementBodyBytes += acknowledgement.BodyLength;
            }
        }
    }

    /// <summary>Stops expiry work and releases every retained acknowledgement.</summary>
    public void Dispose() {
        lock (_sync) {
            if (_disposed) {
                return;
            }
            _disposed = true;
            _expirationTimer?.Dispose();
            _expirationTimer = null;
            while (_expirations.First is { } node) {
                _accepted.Remove(node.Value.Key);
                Remove(node, RetryableAcknowledgement);
            }
        }
    }

    private void Prune(DateTimeOffset now) {
        while (_expirations.First is not null && _expirations.First.Value.ExpiresAt <= now) {
            var expired = _expirations.First;
            _accepted.Remove(expired.Value.Key);
            Remove(expired, RetryableAcknowledgement);
        }
    }

    private void Expire() {
        lock (_sync) {
            if (_disposed) {
                return;
            }
            var now = ObserveNow(_timeProvider.GetUtcNow());
            Prune(now);
            ScheduleExpiration(now);
        }
    }

    private DateTimeOffset ObserveNow(DateTimeOffset now) {
        if (now < _lastObservedAt) {
            return _lastObservedAt;
        }
        _lastObservedAt = now;
        return now;
    }

    private void ScheduleExpiration(DateTimeOffset now) {
        if (_disposed) {
            return;
        }
        if (_expirations.First is null) {
            _expirationTimer?.Dispose();
            _expirationTimer = null;
            return;
        }
        var dueTime = _expirations.First.Value.ExpiresAt - now;
        if (dueTime < TimeSpan.Zero) {
            dueTime = TimeSpan.Zero;
        } else if (dueTime > MaximumTimerDueTime) {
            dueTime = MaximumTimerDueTime;
        }
        if (_expirationTimer is null) {
            _expirationTimer = _timeProvider.CreateTimer(
                static state => ((MessageReplayGuard)state!).Expire(),
                this,
                dueTime,
                Timeout.InfiniteTimeSpan);
        } else {
            _expirationTimer.Change(dueTime, Timeout.InfiniteTimeSpan);
        }
    }

    private void Remove(
        LinkedListNode<Entry> node,
        MessageAcknowledgement unsettledAcknowledgement) {
        _expirations.Remove(node);
        _acknowledgementBodyBytes -= node.Value.RetainedBodyBytes;
        node.Value.RetainedBodyBytes = 0;
        if (!node.Value.IsCompleted) {
            node.Value.Acknowledgement.TrySetResult(unsettledAcknowledgement);
            node.Value.IsCompleted = true;
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private static string GetKey<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Envelope is null) {
            throw new ArgumentException("A dispatch-ready envelope is required.", nameof(result));
        }
        return string.Join(
            "\n",
            result.Envelope.Provider,
            result.Envelope.InstallationId,
            result.Envelope.DeduplicationKey);
    }

    private sealed class Entry {
        public Entry(string key, DateTimeOffset expiresAt) {
            Key = key;
            ExpiresAt = expiresAt;
        }

        public string Key { get; }
        public DateTimeOffset ExpiresAt { get; }
        public bool IsCompleted { get; set; }
        public int RetainedBodyBytes { get; set; }
        public TaskCompletionSource<MessageAcknowledgement> Acknowledgement { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}

/// <summary>Outcome of replay-guarded in-memory ingress acceptance.</summary>
public enum MessageReplayAcceptance {
    /// <summary>The envelope was accepted and its coordinate retained.</summary>
    Accepted = 0,

    /// <summary>The envelope coordinate was already accepted.</summary>
    Duplicate = 1,

    /// <summary>The queue or replay guard is full.</summary>
    Full = 2,

    /// <summary>The queue is stopping.</summary>
    Stopping = 3,

    /// <summary>The configured ingress owner is unavailable.</summary>
    Unavailable = 4
}
