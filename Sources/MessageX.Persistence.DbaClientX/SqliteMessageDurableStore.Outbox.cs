using System.Data;
using DBAClientX;

namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    /// <inheritdoc />
    public async Task<IReadOnlyList<MessageOutboxLease>> ClaimOutboxAsync(
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
                       operation, payload_type, payload, available_at, attempt_count
                FROM messagex_outbox
                WHERE ((status = @pending AND available_at <= @now)
                   OR (status = @leased AND lease_expires_at <= @now))
                  AND payload_type IN ({string.Join(", ", payloadParameters)})
                ORDER BY available_at, record_id
                LIMIT @maximum_count;
                """,
                static row => ReadStoredOutboxCandidate(row),
                parameters,
                cancellationToken: token).ConfigureAwait(false);
            var leases = new List<MessageOutboxLease>(storedCandidates.Count);
            foreach (var storedCandidate in storedCandidates) {
                OutboxCandidate candidate;
                try {
                    candidate = storedCandidate.Materialize();
                } catch (Exception exception) when (IsStoredCandidateException(exception)) {
                    await DeadLetterMalformedOutboxAsync(
                        transaction,
                        storedCandidate.RowId,
                        nowText,
                        token).ConfigureAwait(false);
                    continue;
                }
                var leaseToken = NewId();
                var updated = await transaction.ExecuteNonQueryAsync(
                    """
                    UPDATE messagex_outbox
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
                    leases.Add(new MessageOutboxLease(
                        candidate.RecordId,
                        leaseToken,
                        leaseExpires,
                        candidate.AttemptCount + 1,
                        candidate.Record));
                }
            }
            return (IReadOnlyList<MessageOutboxLease>)leases;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<MessageLeaseRenewal?> RenewOutboxLeaseAsync(
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default) =>
        RenewLeaseAsync(
            "messagex_outbox",
            recordId,
            leaseToken,
            leaseDuration,
            cancellationToken);

    /// <inheritdoc />
    public async Task<bool> CompleteOutboxAsync(
        string recordId,
        string leaseToken,
        CancellationToken cancellationToken = default) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var completedAt = StoreNow();
            var updated = await transaction.ExecuteNonQueryAsync(
                """
                UPDATE messagex_outbox
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
            return updated == 1;
        }, cancellationToken).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task<MessageDurableFailureResult> FailOutboxAsync(
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken = default) =>
        FailAsync(
            "messagex_outbox",
            recordId,
            leaseToken,
            failureKind,
            retryDelay,
            maximumAttempts,
            cancellationToken);

    private static async Task InsertOutboxAsync(
        SQLiteAsyncSession transaction,
        string inboxRecordId,
        MessageOutboxRecord item,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(item);
        await transaction.ExecuteNonQueryAsync(
            """
            INSERT INTO messagex_outbox (
                record_id, inbox_record_id, provider, installation_id, deduplication_key,
                operation, payload_type, payload, status, attempt_count, available_at, failure_kind)
            VALUES (
                @record_id, @inbox_record_id, @provider, @installation_id, @deduplication_key,
                @operation, @payload_type, @payload, @status, 0, @available_at, @failure_kind)
            ON CONFLICT (provider, installation_id, deduplication_key) DO NOTHING;
            """,
            new Dictionary<string, object?> {
                ["record_id"] = NewId(),
                ["inbox_record_id"] = inboxRecordId,
                ["provider"] = item.Provider,
                ["installation_id"] = item.InstallationId,
                ["deduplication_key"] = item.DeduplicationKey,
                ["operation"] = item.Operation,
                ["payload_type"] = item.PayloadType,
                ["payload"] = item.CopyPayload(),
                ["status"] = (int)MessageDurableStatus.Pending,
                ["available_at"] = Timestamp(item.AvailableAt),
                ["failure_kind"] = (int)MessageDurableFailureKind.None
            },
            cancellationToken).ConfigureAwait(false);
    }

    private static StoredOutboxCandidate ReadStoredOutboxCandidate(IDataRecord row) => new(
        row.GetInt64(0),
        row.GetValue(1),
        row.GetValue(2),
        row.GetValue(3),
        row.GetValue(4),
        row.GetValue(5),
        row.GetValue(6),
        row.GetValue(7),
        row.GetValue(8),
        row.GetValue(9));

    private static async Task DeadLetterMalformedOutboxAsync(
        SQLiteAsyncSession transaction,
        long rowId,
        string now,
        CancellationToken cancellationToken) {
        await transaction.ExecuteNonQueryAsync(
            """
            UPDATE messagex_outbox
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

    private sealed class StoredOutboxCandidate {
        private readonly object _recordId;
        private readonly object _provider;
        private readonly object _installationId;
        private readonly object _deduplicationKey;
        private readonly object _operation;
        private readonly object _payloadType;
        private readonly object _payload;
        private readonly object _availableAt;
        private readonly object _attemptCount;

        public StoredOutboxCandidate(
            long rowId,
            object recordId,
            object provider,
            object installationId,
            object deduplicationKey,
            object operation,
            object payloadType,
            object payload,
            object availableAt,
            object attemptCount) {
            RowId = rowId;
            _recordId = recordId;
            _provider = provider;
            _installationId = installationId;
            _deduplicationKey = deduplicationKey;
            _operation = operation;
            _payloadType = payloadType;
            _payload = payload;
            _availableAt = availableAt;
            _attemptCount = attemptCount;
        }

        public long RowId { get; }

        public OutboxCandidate Materialize() {
            var recordId = _recordId is string storedRecordId
                ? RequiredOpaque(storedRecordId, "recordId")
                : throw new InvalidCastException("The durable outbox record identifier is not text.");
            var record = MessageOutboxRecord.FromStoredCoordinates(
                Convert.ToString(_provider, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_installationId, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_deduplicationKey, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_operation, System.Globalization.CultureInfo.InvariantCulture)!,
                Convert.ToString(_payloadType, System.Globalization.CultureInfo.InvariantCulture)!,
                _payload as byte[] ?? throw new InvalidCastException("The durable outbox payload is not a BLOB."),
                DateTimeOffset.Parse(
                    Convert.ToString(_availableAt, System.Globalization.CultureInfo.InvariantCulture)!,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind).ToUniversalTime());
            return new OutboxCandidate(
                recordId,
                record,
                Convert.ToInt32(_attemptCount, System.Globalization.CultureInfo.InvariantCulture));
        }
    }

    private sealed class OutboxCandidate {
        public OutboxCandidate(string recordId, MessageOutboxRecord record, int attemptCount) {
            RecordId = recordId;
            Record = record;
            AttemptCount = attemptCount;
        }

        public string RecordId { get; }

        public MessageOutboxRecord Record { get; }

        public int AttemptCount { get; }
    }
}
