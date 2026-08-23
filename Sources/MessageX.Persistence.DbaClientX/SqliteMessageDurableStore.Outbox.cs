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
            var candidates = await transaction.QueryAsListAsync(
                $"""
                SELECT record_id, provider, installation_id, deduplication_key,
                       operation, payload_type, payload, available_at, attempt_count
                FROM messagex_outbox
                WHERE ((status = @pending AND available_at <= @now)
                   OR (status = @leased AND lease_expires_at <= @now))
                  AND payload_type IN ({string.Join(", ", payloadParameters)})
                ORDER BY available_at, record_id
                LIMIT @maximum_count;
                """,
                static row => ReadOutboxCandidate(row),
                parameters,
                cancellationToken: token).ConfigureAwait(false);
            var leases = new List<MessageOutboxLease>(candidates.Count);
            foreach (var candidate in candidates) {
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

    private static OutboxCandidate ReadOutboxCandidate(IDataRecord row) {
        var record = new MessageOutboxRecord(
            row.GetString(1),
            row.GetString(2),
            row.GetString(3),
            row.GetString(4),
            row.GetString(5),
            ReadBytes(row, 6),
            ReadTimestamp(row, 7));
        return new OutboxCandidate(row.GetString(0), record, row.GetInt32(8));
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
