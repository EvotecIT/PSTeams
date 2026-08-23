using System.Data;
using System.Globalization;
using DBAClientX;

namespace MessageX.Persistence.DbaClientX;

/// <summary>DbaClientX-backed SQLite durable inbox and transactional outbox.</summary>
public sealed partial class SqliteMessageDurableStore : IMessageDurableStore, IDisposable {
    private readonly SQLite _client;
    private readonly string _databasePath;
    private readonly bool _ownsClient;
    private readonly TimeProvider _timeProvider;
    private bool _disposed;

    /// <summary>Creates a durable store using an owned DbaClientX SQLite client.</summary>
    public SqliteMessageDurableStore(string databasePath)
        : this(databasePath, new SQLite(), ownsClient: true, TimeProvider.System) {
    }

    /// <summary>Creates a durable store using a caller-supplied DbaClientX SQLite client.</summary>
    public SqliteMessageDurableStore(string databasePath, SQLite client)
        : this(databasePath, client, ownsClient: false, TimeProvider.System) {
    }

    private SqliteMessageDurableStore(string databasePath, TimeProvider timeProvider)
        : this(databasePath, new SQLite(), ownsClient: true, timeProvider) {
    }

    /// <summary>Creates an owned durable store with one host-selected authoritative clock.</summary>
    public static SqliteMessageDurableStore CreateWithTimeProvider(
        string databasePath,
        TimeProvider timeProvider) => new(databasePath, timeProvider);

    private SqliteMessageDurableStore(
        string databasePath,
        SQLite client,
        bool ownsClient,
        TimeProvider timeProvider) {
        if (string.IsNullOrWhiteSpace(databasePath) || databasePath.Any(char.IsControl)) {
            throw new ArgumentException("A SQLite database path is required.", nameof(databasePath));
        }
        var trimmedDatabasePath = databasePath.Trim();
        if (IsInMemoryPath(trimmedDatabasePath)) {
            throw new ArgumentException(
                "In-memory SQLite paths are not durable and cannot back MessageX persistence.",
                nameof(databasePath));
        }
        _databasePath = NormalizeDatabasePath(trimmedDatabasePath);
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _ownsClient = ownsClient;
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
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

    private DateTimeOffset StoreNow() => _timeProvider.GetUtcNow();

    private static string Timestamp(DateTimeOffset value) =>
        value.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);

    private static DateTimeOffset ReadTimestamp(IDataRecord record, int ordinal) =>
        DateTimeOffset.Parse(
            record.GetString(ordinal),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind).ToUniversalTime();

    private static byte[] ReadBytes(IDataRecord record, int ordinal) =>
        (byte[])record.GetValue(ordinal);

    private static bool IsInMemoryPath(string databasePath) {
        if (databasePath.Equals(":memory:", StringComparison.OrdinalIgnoreCase)) {
            return true;
        }
        if (!databasePath.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) {
            return false;
        }
        try {
            var decoded = Uri.UnescapeDataString(databasePath);
            return decoded.Contains(":memory:", StringComparison.OrdinalIgnoreCase) ||
                decoded.Contains("mode=memory", StringComparison.OrdinalIgnoreCase);
        } catch (UriFormatException) {
            return false;
        }
    }

    private static string NormalizeDatabasePath(string databasePath) {
        var normalized = databasePath.Trim();
        if (normalized.StartsWith("file:", StringComparison.OrdinalIgnoreCase)) {
            var suffixIndex = normalized.IndexOfAny(new[] { '?', '#' }, "file:".Length);
            var pathEnd = suffixIndex >= 0 ? suffixIndex : normalized.Length;
            var path = normalized.Substring("file:".Length, pathEnd - "file:".Length);
            if (path.Length == 0) {
                throw new ArgumentException("A SQLite file URI requires a database path.", nameof(databasePath));
            }
            if (!Path.IsPathRooted(path)) {
                path = Path.GetFullPath(path);
            }
            return "file:" + path + normalized.Substring(pathEnd);
        }
        return Path.GetFullPath(normalized);
    }

    private static string Required(string? value, string parameterName, int maximumLength = 256) {
        if (value is null || value.Length > maximumLength || value.Any(char.IsControl)) {
            throw new ArgumentException("A bounded durable coordinate is required.", parameterName);
        }
        var normalized = value.Trim();
        return normalized.Length == 0
            ? throw new ArgumentException("A bounded durable coordinate is required.", parameterName)
            : normalized;
    }

    private static string RequiredOpaque(string? value, string parameterName, int maximumLength = 256) {
        if (value is null ||
            value.Length == 0 ||
            value.Length > maximumLength ||
            value.Any(char.IsControl) ||
            char.IsWhiteSpace(value[0]) ||
            char.IsWhiteSpace(value[value.Length - 1])) {
            throw new ArgumentException(
                "Opaque durable coordinates must already be canonical bounded text.",
                parameterName);
        }
        return value;
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

    private static string[] ValidatePayloadTypes(IReadOnlyCollection<string> payloadTypes) {
        if (payloadTypes is null) {
            throw new ArgumentNullException(nameof(payloadTypes));
        }
        if (payloadTypes.Count is < 1 or > 64) {
            throw new ArgumentOutOfRangeException(nameof(payloadTypes));
        }
        var normalized = payloadTypes
            .Select(value => Required(value, nameof(payloadTypes)))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return normalized.Length == 0
            ? throw new ArgumentException("At least one supported payload type is required.", nameof(payloadTypes))
            : normalized;
    }

    private static void ValidateFailure(
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts) {
        if (failureKind == MessageDurableFailureKind.None ||
            !Enum.IsDefined(typeof(MessageDurableFailureKind), failureKind)) {
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        }
        if (retryDelay < TimeSpan.Zero || retryDelay > TimeSpan.FromDays(7)) {
            throw new ArgumentOutOfRangeException(nameof(retryDelay));
        }
        if (maximumAttempts is < 1 or > 100) {
            throw new ArgumentOutOfRangeException(nameof(maximumAttempts));
        }
    }

    private static void ValidateDelay(TimeSpan delay, string parameterName) {
        if (delay < TimeSpan.Zero || delay > TimeSpan.FromDays(7)) {
            throw new ArgumentOutOfRangeException(parameterName);
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
