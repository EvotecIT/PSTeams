using System.Collections;

namespace MessageX.Hosting;

/// <summary>Reports committed outbox leases and malformed records dead-lettered during the same claim.</summary>
public sealed class MessageOutboxClaimResult : IReadOnlyList<MessageOutboxLease> {
    private readonly MessageOutboxLease[] _leases;

    /// <summary>Creates a claim result with a snapshot of its leases.</summary>
    public MessageOutboxClaimResult(IEnumerable<MessageOutboxLease> leases, int malformedRecordsDeadLettered = 0) {
        if (leases is null) throw new ArgumentNullException(nameof(leases));
        if (malformedRecordsDeadLettered < 0) throw new ArgumentOutOfRangeException(nameof(malformedRecordsDeadLettered));
        _leases = leases.ToArray();
        if (_leases.Any(static lease => lease is null)) throw new ArgumentException("Leases cannot contain null entries.", nameof(leases));
        MalformedRecordsDeadLettered = malformedRecordsDeadLettered;
    }

    /// <summary>Gets the number of malformed records durably dead-lettered by this claim.</summary>
    public int MalformedRecordsDeadLettered { get; }

    /// <inheritdoc />
    public int Count => _leases.Length;

    /// <inheritdoc />
    public MessageOutboxLease this[int index] => _leases[index];

    /// <inheritdoc />
    public IEnumerator<MessageOutboxLease> GetEnumerator() => ((IEnumerable<MessageOutboxLease>)_leases).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
