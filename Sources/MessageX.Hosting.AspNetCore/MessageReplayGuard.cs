namespace MessageX.Hosting.AspNetCore;

/// <summary>Bounded installation-scoped replay suppression for accepted in-memory ingress.</summary>
public sealed class MessageReplayGuard {
    private readonly object _sync = new();
    private readonly Dictionary<string, DateTimeOffset> _accepted = new(StringComparer.Ordinal);
    private readonly Queue<Entry> _expirations = new();
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
            if (enqueue == MessageIngressEnqueueStatus.Full) {
                return MessageReplayAcceptance.Full;
            }
            if (enqueue == MessageIngressEnqueueStatus.Stopping) {
                return MessageReplayAcceptance.Stopping;
            }
            var expiresAt = now.Add(_retention);
            _accepted.Add(key, expiresAt);
            _expirations.Enqueue(new Entry(key, expiresAt));
            return MessageReplayAcceptance.Accepted;
        }
    }

    private void Prune(DateTimeOffset now) {
        while (_expirations.Count > 0 && _expirations.Peek().ExpiresAt <= now) {
            var expired = _expirations.Dequeue();
            if (_accepted.TryGetValue(expired.Key, out var current) && current == expired.ExpiresAt) {
                _accepted.Remove(expired.Key);
            }
        }
    }

    private sealed record Entry(string Key, DateTimeOffset ExpiresAt);
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
    Stopping = 3
}
