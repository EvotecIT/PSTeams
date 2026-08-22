using System.Data;
using System.Globalization;
using DBAClientX;

namespace MessageX.Persistence.DbaClientX;

/// <summary>DbaClientX-backed SQLite durable inbox and transactional outbox.</summary>
public sealed partial class SqliteMessageDurableStore : IMessageDurableStore, IDisposable {
    private readonly SQLite _client;
    private readonly string _databasePath;
    private readonly bool _ownsClient;
    private bool _disposed;

    /// <summary>Creates a durable store using an owned DbaClientX SQLite client.</summary>
    public SqliteMessageDurableStore(string databasePath)
        : this(databasePath, new SQLite(), ownsClient: true) {
    }

    /// <summary>Creates a durable store using a caller-supplied DbaClientX SQLite client.</summary>
    public SqliteMessageDurableStore(string databasePath, SQLite client)
        : this(databasePath, client, ownsClient: false) {
    }

    private SqliteMessageDurableStore(string databasePath, SQLite client, bool ownsClient) {
        if (string.IsNullOrWhiteSpace(databasePath) || databasePath.Any(char.IsControl)) {
            throw new ArgumentException("A SQLite database path is required.", nameof(databasePath));
        }
        _databasePath = databasePath.Trim();
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = ownsClient;
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposed) {
            return;
        }
        if (_ownsClient) {
            _client.Dispose();
        }
        _disposed = true;
    }

    private async Task<SQLiteAsyncSession> OpenSessionAsync(CancellationToken cancellationToken) {
        ObjectDisposedException.ThrowIf(_disposed, this);
        return await _client.OpenSessionAsync(_databasePath, cancellationToken).ConfigureAwait(false);
    }

    private static string NewId() => Guid.NewGuid().ToString("N");

    private static string Timestamp(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ReadTimestamp(IDataRecord record, int ordinal) =>
        DateTimeOffset.Parse(
            record.GetString(ordinal),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind).ToUniversalTime();

    private static byte[] ReadBytes(IDataRecord record, int ordinal) =>
        (byte[])record.GetValue(ordinal);

    private static string Required(string? value, string parameterName, int maximumLength = 256) {
        if (value is null || value.Length > maximumLength || value.Any(char.IsControl)) {
            throw new ArgumentException("A bounded durable coordinate is required.", parameterName);
        }
        var normalized = value.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("A bounded durable coordinate is required.", parameterName)
            : normalized;
    }

    private static void ValidateClaim(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration) {
        Required(ownerId, nameof(ownerId));
        if (maximumCount is < 1 or > 100) {
            throw new ArgumentOutOfRangeException(nameof(maximumCount));
        }
        if (leaseDuration < TimeSpan.FromSeconds(1) || leaseDuration > TimeSpan.FromHours(1)) {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        }
    }

    private static void ValidateFailure(
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts) {
        if (failureKind == MessageDurableFailureKind.None) {
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        }
        if (retryDelay < TimeSpan.Zero || retryDelay > TimeSpan.FromDays(7)) {
            throw new ArgumentOutOfRangeException(nameof(retryDelay));
        }
        if (maximumAttempts is < 1 or > 100) {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }
    }

    private sealed class StoredState {
        public StoredState(string recordId, MessageDurableStatus status) {
            RecordId = recordId;
            Status = status;
        }

        public string RecordId { get; }

        public MessageDurableStatus Status { get; }
    }

    private sealed class FailureLeaseState {
        public FailureLeaseState(int attemptCount) => AttemptCount = attemptCount;

        public int AttemptCount { get; }
    }
}
