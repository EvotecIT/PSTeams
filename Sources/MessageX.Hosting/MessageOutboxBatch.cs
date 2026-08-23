using System.Collections;

namespace MessageX.Hosting;

/// <summary>Bounded transactional outbox work committed with one inbox completion.</summary>
public sealed class MessageOutboxBatch : IReadOnlyList<MessageOutboxRecord> {
    /// <summary>Maximum outbound records permitted in one inbox transaction.</summary>
    public const int MaximumCount = 100;

    private readonly MessageOutboxRecord[] _records;

    /// <summary>An empty outbox batch.</summary>
    public static MessageOutboxBatch Empty { get; } = new(Array.Empty<MessageOutboxRecord>());

    /// <summary>Creates a bounded immutable outbox batch.</summary>
    public MessageOutboxBatch(IEnumerable<MessageOutboxRecord> records) {
        if (records is null) {
            throw new ArgumentNullException(nameof(records));
        }
        _records = records.Take(MaximumCount + 1).ToArray();
        if (_records.Length > MaximumCount) {
            throw new ArgumentException(
                $"A transactional outbox batch cannot exceed {MaximumCount} records.",
                nameof(records));
        }
        if (_records.Any(record => record is null)) {
            throw new ArgumentException("Outbox batches cannot contain null records.", nameof(records));
        }
        var coordinates = new HashSet<(string Provider, string InstallationId, string DeduplicationKey)>();
        foreach (var record in _records) {
            if (!coordinates.Add((record.Provider, record.InstallationId, record.DeduplicationKey))) {
                throw new ArgumentException(
                    "Outbox batches require ordinal-unique provider, installation, and deduplication coordinates.",
                    nameof(records));
            }
        }
    }

    /// <inheritdoc />
    public int Count => _records.Length;

    /// <inheritdoc />
    public MessageOutboxRecord this[int index] => _records[index];

    /// <inheritdoc />
    public IEnumerator<MessageOutboxRecord> GetEnumerator() =>
        ((IEnumerable<MessageOutboxRecord>)_records).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
