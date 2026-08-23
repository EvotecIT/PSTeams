namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    private async Task<MessageDurableFailureResult> FailAsync(
        string table,
        string recordId,
        string leaseToken,
        MessageDurableFailureKind failureKind,
        TimeSpan retryDelay,
        int maximumAttempts,
        CancellationToken cancellationToken) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        ValidateFailure(failureKind, retryDelay, maximumAttempts);
        if (table is not ("messagex_inbox" or "messagex_outbox")) {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var now = StoreNow();
            var nowText = Timestamp(now);
            var leases = await transaction.QueryAsListAsync(
                $"SELECT attempt_count FROM {table} WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token AND lease_expires_at > @now;",
                static row => new FailureLeaseState(row.GetInt32(0)),
                new Dictionary<string, object?> {
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken,
                    ["now"] = nowText
                },
                cancellationToken: token).ConfigureAwait(false);
            if (leases.Count == 0) {
                return new MessageDurableFailureResult(MessageDurableFailureStatus.LeaseLost);
            }

            var deadLetter = failureKind == MessageDurableFailureKind.Permanent ||
                leases[0].AttemptCount >= maximumAttempts;
            var status = deadLetter ? MessageDurableStatus.DeadLettered : MessageDurableStatus.Pending;
            var updated = await transaction.ExecuteNonQueryAsync(
                $"""
                UPDATE {table}
                SET status = @status, available_at = @available_at, failure_kind = @failure_kind,
                    completed_at = @completed_at,
                    lease_owner = NULL, lease_token = NULL, lease_expires_at = NULL
                WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token
                  AND lease_expires_at > @now;
                """,
                new Dictionary<string, object?> {
                    ["status"] = (int)status,
                    ["available_at"] = Timestamp(now.ToUniversalTime().Add(retryDelay)),
                    ["completed_at"] = deadLetter ? nowText : null,
                    ["failure_kind"] = (int)failureKind,
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken,
                    ["now"] = nowText
                },
                token).ConfigureAwait(false);
            if (updated != 1) {
                return new MessageDurableFailureResult(MessageDurableFailureStatus.LeaseLost);
            }
            return new MessageDurableFailureResult(deadLetter
                ? MessageDurableFailureStatus.DeadLettered
                : MessageDurableFailureStatus.RetryScheduled);
        }, cancellationToken).ConfigureAwait(false);
    }

    private async Task<MessageLeaseRenewal?> RenewLeaseAsync(
        string table,
        string recordId,
        string leaseToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken) {
        recordId = RequiredOpaque(recordId, nameof(recordId));
        leaseToken = RequiredOpaque(leaseToken, nameof(leaseToken));
        if (leaseDuration < TimeSpan.FromSeconds(1) || leaseDuration > TimeSpan.FromHours(1)) {
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));
        }
        if (table is not ("messagex_inbox" or "messagex_outbox")) {
            throw new ArgumentOutOfRangeException(nameof(table));
        }

        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var now = StoreNow();
            var renewedUntil = now.Add(leaseDuration);
            var updated = await transaction.ExecuteNonQueryAsync(
                $"""
                UPDATE {table}
                SET lease_expires_at = @lease_expires_at
                WHERE record_id = @record_id AND status = @leased AND lease_token = @lease_token
                  AND lease_expires_at > @now;
                """,
                new Dictionary<string, object?> {
                    ["lease_expires_at"] = Timestamp(renewedUntil),
                    ["record_id"] = recordId,
                    ["leased"] = (int)MessageDurableStatus.Leased,
                    ["lease_token"] = leaseToken,
                    ["now"] = Timestamp(now)
                },
                token).ConfigureAwait(false);
            return updated == 1 ? new MessageLeaseRenewal(renewedUntil, leaseDuration) : null;
        }, cancellationToken).ConfigureAwait(false);
    }
}
