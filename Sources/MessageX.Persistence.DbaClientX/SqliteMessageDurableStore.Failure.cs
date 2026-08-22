namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    private async Task<MessageDurableFailureResult> FailAsync(
        string table,
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        DateTimeOffset now,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken) {
        recordId = Required(recordId, nameof(recordId));
        leaseToken = Required(leaseToken, nameof(leaseToken));
        ValidateFailure(failureKind, retryDelay, maximumAttempts);
        if (table is not ("messagex_inbox" or "messagex_outbox")) {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var leases = await transaction.QueryAsListAsync(
                $"SELECT attempt_count FROM {table} WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token;",
                static row => new FailureLeaseState(row.GetInt32(0)),
                new Dictionary<string, object?> {
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken
                },
                cancellationToken: token).ConfigureAwait(false);
            if (leases.Count == 0) {
                return new MessageDurableFailureResult(MessageDurableFailureStatus.LeaseLost);
            }

            var deadLetter = failureKind == MessageDurableFailureKind.Permanent ||
                leases[0].AttemptCount >= maximumAttempts;
            var status = deadLetter ? MessageDurableStatus.DeadLettered : MessageDurableStatus.Pending;
            await transaction.ExecuteNonQueryAsync(
                $"""
                UPDATE {table}
                SET status = @status, available_at = @available_at, failure_kind = @failure_kind,
                    lease_owner = NULL, lease_token = NULL, lease_expires_at = NULL
                WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token;
                """,
                new Dictionary<string, object?> {
                    ["status"] = (int)status,
                    ["available_at"] = Timestamp(now.ToUniversalTime().Add(retryDelay)),
                    ["failure_kind"] = (int)failureKind,
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken
                },
                token).ConfigureAwait(false);
            return new MessageDurableFailureResult(deadLetter
                ? MessageDurableFailureStatus.DeadLettered
                : MessageDurableFailureStatus.RetryScheduled);
        }, cancellationToken).ConfigureAwait(false);
    }
}
