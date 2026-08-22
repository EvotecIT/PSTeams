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
                    ["available_at"] = Timestamp(record.ReceivedAt),
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
        var now = StoreNow();
        var nowText = Timestamp(now);
        var leaseExpires = now.ToUniversalTime().Add(leaseDuration);
        var payloadParameters = supportedPayloadTypes
            .Select((_, index) => $"@payload_type_{index}")
            .ToArray();
        var parameters = new Dictionary<string, object?> {
            ["pending"] = (int)MessageDurableStatus.Pending,
            ["leased"] = (int)MessageDurableStatus.Leased,
            ["now"] = nowText,
            ["maximum_count"] = maximumCount
        };
        for (var index = 0; index < supportedPayloadTypes.Length; index++) {
            parameters[$"payload_type_{index}"] = supportedPayloadTypes[index];
        }
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var candidates = await transaction.QueryAsListAsync(
                $"""
                SELECT record_id, provider, installation_id, deduplication_key,
                       route_kind, event_kind, route_name, route_qualifier,
                       received_at, payload_type, payload, attempt_count
                FROM messagex_inbox
                WHERE ((status = @pending AND available_at <= @now)
                   OR (status = @leased AND lease_expires_at <= @now))
                  AND payload_type IN ({string.Join(", ", payloadParameters)})
                ORDER BY available_at, received_at, record_id
                LIMIT @maximum_count;
                """,
                static row => ReadInboxCandidate(row),
                parameters,
                cancellationToken: token).ConfigureAwait(false);
            var leases = new List<MessageDurableLease>(candidates.Count);
            foreach (var candidate in candidates) {
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
                        candidate.Record));
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
    public async Task<bool> CompleteInboxAsync(
        string recordId,
        string leaseToken,
        MessageOutboxBatch? outbox = null,
        CancellationToken cancellationToken = default) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        var completedAt = StoreNow();
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
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

    private static InboxCandidate ReadInboxCandidate(IDataRecord row) {
        var route = MessageRoute.FromDurableCoordinates(
            (MessageRouteKind)row.GetInt32(4),
            (MessageEventKind)row.GetInt32(5),
            row.IsDBNull(6) ? null : row.GetString(6),
            row.IsDBNull(7) ? null : row.GetString(7));
        var record = new MessageDurableRecord(
            row.GetString(1),
            row.GetString(2),
            row.GetString(3),
            route,
            ReadTimestamp(row, 8),
            row.GetString(9),
            ReadBytes(row, 10));
        return new InboxCandidate(row.GetString(0), record, row.GetInt32(11));
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
