using System.Data;
using DBAClientX;

namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    /// <inheritdoc />
    public async Task<MessageDurableAcceptance> AcceptInboxAsync(
        MessageDurableRecord record,
        CancellationToken cancellationToken = default) {
        ArgumentNullException.ThrowIfNull(record);
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var recordId = NewId();
            var inserted = await transaction.ExecuteNonQueryAsync(
                """
                INSERT INTO messagex_inbox (
                    record_id, provider, installation_id, deduplication_key,
                    route_kind, event_kind, route_name, route_qualifier, received_at, payload_type, payload,
                    status, attempt_count, available_at, failure_kind)
                VALUES (
                    @record_id, @provider, @installation_id, @deduplication_key,
                    @route_kind, @event_kind, @route_name, @route_qualifier, @received_at, @payload_type, @payload,
                    @status, 0, @available_at, @failure_kind)
                ON CONFLICT (provider, installation_id, deduplication_key) DO NOTHING;
                """,
                new Dictionary<string, object?> {
                    ["record_id"] = recordId,
                    ["provider"] = record.Provider,
                    ["installation_id"] = record.InstallationId,
                    ["deduplication_key"] = record.DeduplicationKey,
                    ["route_kind"] = (int)record.Route.Kind,
                    ["event_kind"] = (int)record.Route.EventKind,
                    ["route_name"] = record.Route.Name,
                    ["route_qualifier"] = record.Route.Qualifier,
                    ["received_at"] = Timestamp(record.ReceivedAt),
                    ["payload_type"] = record.PayloadType,
                    ["payload"] = record.CopyPayload(),
                    ["status"] = (int)MessageDurableStatus.Pending,
                    ["available_at"] = Timestamp(StoreNow()),
                    ["failure_kind"] = (int)MessageDurableFailureKind.None
                },
                token).ConfigureAwait(false);
            if (inserted == 1) {
                return new MessageDurableAcceptance(recordId, MessageDurableAcceptanceStatus.Accepted);
            }

            var existing = await transaction.QueryAsListAsync(
                "SELECT record_id, status FROM messagex_inbox WHERE provider = @provider AND installation_id = @installation_id AND deduplication_key = @deduplication_key;",
                static row => new StoredState(row.GetString(0), (MessageDurableStatus)row.GetInt32(1)),
                new Dictionary<string, object?> {
                    ["provider"] = record.Provider,
                    ["installation_id"] = record.InstallationId,
                    ["deduplication_key"] = record.DeduplicationKey
                },
                cancellationToken: token).ConfigureAwait(false);
            var state = existing.Single();
            var status = state.Status switch {
                MessageDurableStatus.Pending => MessageDurableAcceptanceStatus.AlreadyPending,
                MessageDurableStatus.Leased => MessageDurableAcceptanceStatus.AlreadyPending,
                MessageDurableStatus.Completed => MessageDurableAcceptanceStatus.AlreadyCompleted,
                MessageDurableStatus.DeadLettered => MessageDurableAcceptanceStatus.DeadLettered,
                _ => throw new InvalidOperationException("The durable inbox contains an unsupported state.")
            };
            return new MessageDurableAcceptance(state.RecordId, status);
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MessageDurableLease>> ClaimInboxAsync(
        string ownerId,
        int maximumCount,
        TimeSpan leaseDuration,
        IReadOnlyCollection<string> payloadTypes,
        CancellationToken cancellationToken = default) {
        ValidateClaim(ownerId, maximumCount, leaseDuration);
        var supportedPayloadTypes = ValidatePayloadTypes(payloadTypes);
        ownerId = ownerId.Trim();
        var payloadParameters = supportedPayloadTypes
            .Select((_, index) => $"@payload_type_{index}")
            .ToArray();
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var now = StoreNow();
            var nowText = Timestamp(now);
            var leaseExpires = now.ToUniversalTime().Add(leaseDuration);
            var parameters = new Dictionary<string, object?> {
                ["pending"] = (int)MessageDurableStatus.Pending,
                ["leased"] = (int)MessageDurableStatus.Leased,
                ["now"] = nowText,
                ["maximum_count"] = maximumCount
            };
            for (var index = 0; index < supportedPayloadTypes.Length; index++) {
                parameters[$"payload_type_{index}"] = supportedPayloadTypes[index];
            }
            var storedCandidates = await transaction.QueryAsListAsync(
                $"""
                SELECT rowid, record_id, provider, installation_id, deduplication_key,
                       route_kind, event_kind, route_name, route_qualifier,
                       received_at, payload_type, payload, attempt_count
                FROM messagex_inbox
                WHERE ((status = @pending AND available_at <= @now)
                   OR (status = @leased AND lease_expires_at <= @now))
                  AND payload_type IN ({string.Join(", ", payloadParameters)})
                ORDER BY available_at, received_at, record_id
                LIMIT @maximum_count;
                """,
                static row => ReadStoredInboxCandidate(row),
                parameters,
                cancellationToken: token).ConfigureAwait(false);
            var leases = new List<MessageDurableLease>(storedCandidates.Count);
            foreach (var storedCandidate in storedCandidates) {
                InboxCandidate candidate;
                try {
                    candidate = storedCandidate.Materialize();
                } catch (Exception exception) when (IsStoredCandidateException(exception)) {
                    await DeadLetterMalformedInboxAsync(
                        transaction,
                        storedCandidate.RowId,
                        nowText,
                        token).ConfigureAwait(false);
                    continue;
                }
                var leaseToken = NewId();
                var updated = await transaction.ExecuteNonQueryAsync(
                    """
                    UPDATE messagex_inbox
                    SET status = @leased, lease_owner = @owner, lease_token = @lease_token,
                        lease_expires_at = @lease_expires, attempt_count = attempt_count + 1
                    WHERE record_id = @record_id
                      AND ((status = @pending AND available_at <= @now)
                        OR (status = @leased AND lease_expires_at <= @now));
                    """,
                    new Dictionary<string, object?> {
                        ["leased"] = (int)MessageDurableStatus.Leased,
                        ["owner"] = ownerId,
                        ["lease_token"] = leaseToken,
                        ["lease_expires"] = Timestamp(leaseExpires),
                        ["record_id"] = candidate.RecordId,
                        ["pending"] = (int)MessageDurableStatus.Pending,
                        ["now"] = nowText
                    },
                    token).ConfigureAwait(false);
                if (updated == 1) {
                    leases.Add(new MessageDurableLease(
                        candidate.RecordId,
                        leaseToken,
                        leaseExpires,
                        candidate.AttemptCount + 1,
                        candidate.Record,
                        leaseDuration));
                }
            }
            return (IReadOnlyList<MessageDurableLease>)leases;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<MessageLeaseRenewal?> RenewInboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default) =>
        RenewLeaseAsync(
            "messagex_inbox",
            recordId,
            leaseToken,
            leaseDuration,
            cancellationToken);

    /// <inheritdoc />
    public async Task<bool> ReleaseInboxAsync(
        string recordId,
        string leaseToken,
        TimeSpan retryDelay,
        CancellationToken cancellationToken = default) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        ValidateDelay(retryDelay, nameof(retryDelay));
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var now = StoreNow();
            var updated = await transaction.ExecuteNonQueryAsync(
                """
                UPDATE messagex_inbox
                SET status = @pending, available_at = @available_at, failure_kind = @failure_kind,
                    attempt_count = CASE WHEN attempt_count > 0 THEN attempt_count - 1 ELSE 0 END,
                    lease_owner = NULL, lease_token = NULL, lease_expires_at = NULL
                WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token
                  AND lease_expires_at > @now;
                """,
                new Dictionary<string, object?> {
                    ["pending"] = (int)MessageDurableStatus.Pending,
                    ["available_at"] = Timestamp(now.Add(retryDelay)),
                    ["failure_kind"] = (int)MessageDurableFailureKind.None,
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken,
                    ["now"] = Timestamp(now)
                },
                token).ConfigureAwait(false);
            return updated == 1;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        MessageOutboxBatch? outbox = null,
        CancellationToken cancellationToken = default) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var completedAt = StoreNow();
            var updated = await transaction.ExecuteNonQueryAsync(
                """
                UPDATE messagex_inbox
                SET status = @completed, completed_at = @completed_at,
                    lease_owner = NULL, lease_token = NULL, lease_expires_at = NULL
                WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token
                  AND lease_expires_at > @completed_at;
                """,
                new Dictionary<string, object?> {
                    ["completed"] = (int)MessageDurableStatus.Completed,
                    ["completed_at"] = Timestamp(completedAt),
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken
                },
                token).ConfigureAwait(false);
            if (updated != 1) {
                return false;
            }
            foreach (var item in outbox ?? MessageOutboxBatch.Empty) {
                await InsertOutboxAsync(transaction, recordId, item, token).ConfigureAwait(false);
            }
            return true;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<MessageDurableFailureResult> FailInboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default) =>
        FailAsync(
            "messagex_inbox",
            recordId,
            leaseToken,
            failureKind,
            retryDelay,
            maximumAttempts,
            cancellationToken);

    private static StoredInboxCandidate ReadStoredInboxCandidate(IDataRecord row) => new(
        row.GetInt64(0),
        row.GetValue(1),
        row.GetValue(2),
        row.GetValue(3),
        row.GetValue(4),
        row.GetValue(5),
        row.GetValue(6),
        row.IsDBNull(7) ? null : row.GetValue(7),
        row.IsDBNull(8) ? null : row.GetValue(8),
        row.GetValue(9),
        row.GetValue(10),
        row.GetValue(11),
        row.GetValue(12));

    private static bool IsStoredCandidateException(Exception exception) =>
        exception is ArgumentException or FormatException or InvalidCastException or
        InvalidOperationException or OverflowException;

    private static async Task DeadLetterMalformedInboxAsync(
        SQLiteAsyncSession transaction,
        long rowId,
        string now,
        CancellationToken cancellationToken) {
        await transaction.ExecuteNonQueryAsync(
            """
            UPDATE messagex_inbox
            SET status = @dead_lettered, completed_at = @completed_at,
                available_at = @completed_at, failure_kind = @failure_kind,
                lease_owner = NULL, lease_token = NULL, lease_expires_at = NULL
            WHERE rowid = @row_id
              AND ((status = @pending AND available_at <= @completed_at)
                OR (status = @leased AND lease_expires_at <= @completed_at));
            """,
            new Dictionary<string, object?> {
                ["dead_lettered"] = (int)MessageDurableStatus.DeadLettered,
                ["completed_at"] = now,
                ["failure_kind"] = (int)MessageDurableFailureKind.Permanent,
                ["row_id"] = rowId,
                ["pending"] = (int)MessageDurableStatus.Pending,
                ["leased"] = (int)MessageDurableStatus.Leased
            },
            cancellationToken).ConfigureAwait(false);
    }

    private sealed class StoredInboxCandidate {
        private readonly object _recordId;
        private readonly object _provider;
        private readonly object _installationId;
        private readonly object _deduplicationKey;
        private readonly object _routeKind;
        private readonly object _eventKind;
        private readonly object? _routeName;
        private readonly object? _routeQualifier;
        private readonly object _receivedAt;
        private readonly object _payloadType;
        private readonly object _payload;
        private readonly object _attemptCount;

        public StoredInboxCandidate(
            long rowId,
            object recordId,
            object provider,
            object installationId,
            object deduplicationKey,
            object routeKind,
            object eventKind,
            object? routeName,
            object? routeQualifier,
            object receivedAt,
            object payloadType,
            object payload,
            object attemptCount) {
            RowId = rowId;
            _recordId = recordId;
            _provider = provider;
            _installationId = installationId;
            _deduplicationKey = deduplicationKey;
            _routeKind = routeKind;
            _eventKind = eventKind;
            _routeName = routeName;
            _routeQualifier = routeQualifier;
            _receivedAt = receivedAt;
            _payloadType = payloadType;
            _payload = payload;
            _attemptCount = attemptCount;
        }

        public long RowId { get; }

        public InboxCandidate Materialize() {
            var recordId = _recordId is string storedRecordId
                ? RequiredOpaque(storedRecordId, "recordId")
                : throw new InvalidCastException("The durable inbox record identifier is not text.");
            var route = MessageRoute.FromDurableCoordinates(
                (MessageRouteKind)Convert.ToInt32(_routeKind, System.Globalization.CultureInfo.InvariantCulture),
                (MessageEventKind)Convert.ToInt32(_eventKind, System.Globalization.CultureInfo.InvariantCulture),
                _routeName is null
                    ? null
                    : Convert.ToString(_routeName, System.Globalization.CultureInfo.InvariantCulture),
                _routeQualifier is null
                    ? null
                    : Convert.ToString(_routeQualifier, System.Globalization.CultureInfo.InvariantCulture));
            var record = MessageDurableRecord.FromStoredCoordinates(
                Convert.ToString(_provider, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_installationId, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_deduplicationKey, System.Globalization.CultureInfo.InvariantCulture)!,
                route,
                DateTimeOffset.Parse(
                    Convert.ToString(_receivedAt, System.Globalization.CultureInfo.InvariantCulture)!,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime(),
                Convert.ToString(_payloadType, System.Globalization.CultureInfo.InvariantCulture)!,
                _payload as byte[] ?? throw new InvalidCastException("The durable inbox payload is not a BLOB."));
            return new InboxCandidate(
                recordId,
                record,
                Convert.ToInt32(_attemptCount, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private sealed class InboxCandidate {
        public InboxCandidate(string recordId, MessageDurableRecord record, int attemptCount) {
            RecordId = recordId;
            Record = record;
            AttemptCount = attemptCount;
        }

        public string RecordId { get; }

        public MessageDurableRecord Record { get; }

        public int AttemptCount { get; }
    }
}
