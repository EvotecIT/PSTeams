namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded installation-scoped replay suppression for accepted in-memory ingress.</summary>
public sealed class MessageReplayGuard {
    private readonly object _sync = new();
    private readonly Dictionary<string, LinkedListNode<Entry>> _accepted = new(StringComparer.Ordinal);
    private readonly LinkedList<Entry> _expirations = new();
    private readonly int _capacity;
    private readonly TimeSpan _retention;
    private DateTimeOffset _lastObservedAt = DateTimeOffset.MinValue;

    /// <summary>Creates a bounded replay guard.</summary>
    public MessageReplayGuard(int capacity, TimeSpan retention) {
        if (capacity < 1) {
            throw new ArgumentOutOfRangeException(nameof(capacity));
        }
        if (retention <= TimeSpan.Zero) {
            throw new ArgumentOutOfRangeException(nameof(retention));
        }
        _capacity = capacity;
        _retention = retention;
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
            if (now < _lastObservedAt) {
                now = _lastObservedAt;
            } else {
                _lastObservedAt = now;
            }
            Prune(now);
            if (_accepted.ContainsKey(key)) {
                return MessageReplayAcceptance.Duplicate;
            }
            if (_accepted.Count >= _capacity) {
                return MessageReplayAcceptance.Full;
            }
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
            var expiresAt = now.Add(_retention);
            var node = _expirations.AddLast(new Entry(key, expiresAt));
            _accepted.Add(key, node);
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
            if (_accepted.Remove(key, out var node)) {
                _expirations.Remove(node);
                node.Value.Acknowledgement.TrySetCanceled();
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
            if (!_accepted.TryGetValue(key, out var node)) {
                throw new InvalidOperationException("The synchronous replay reservation is no longer available.");
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
            if (_accepted.TryGetValue(key, out var node)) {
                node.Value.Acknowledgement.TrySetResult(acknowledgement);
            }
        }
    }

    private void Prune(DateTimeOffset now) {
        while (_expirations.First is not null && _expirations.First.Value.ExpiresAt <= now) {
            var expired = _expirations.First;
            _expirations.RemoveFirst();
            _accepted.Remove(expired.Value.Key);
            expired.Value.Acknowledgement.TrySetCanceled();
        }
    }

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
