namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    /// <inheritdoc />
    public async Task<int> PurgeTerminalAsync(
        DateTimeOffset completedBefore,
        int maximumCount,
        CancellationToken cancellationToken = default) {
        if (maximumCount is < 1 or > 10_000) {
            throw new ArgumentOutOfRangeException(nameof(maximumCount));
        }
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        return await session.RunInTransactionAsync(async (transaction, token) => {
            var parameters = new Dictionary<string, object?> {
                ["completed"] = (int)MessageDurableStatus.Completed,
                ["dead_lettered"] = (int)MessageDurableStatus.DeadLettered,
                ["completed_before"] = Timestamp(completedBefore),
                ["maximum_count"] = maximumCount
            };
            var outbox = await transaction.ExecuteNonQueryAsync(
                """
                DELETE FROM messagex_outbox
                WHERE record_id IN (
                    SELECT record_id
                    FROM messagex_outbox
                    WHERE status IN (@completed, @dead_lettered)
                      AND completed_at IS NOT NULL
                      AND completed_at < @completed_before
                    ORDER BY completed_at, record_id
                    LIMIT @maximum_count
                );
                """,
                parameters,
                token).ConfigureAwait(false);
            var inbox = await transaction.ExecuteNonQueryAsync(
                """
                DELETE FROM messagex_inbox
                WHERE record_id IN (
                    SELECT candidate.record_id
                    FROM messagex_inbox AS candidate
                    WHERE candidate.status IN (@completed, @dead_lettered)
                      AND candidate.completed_at IS NOT NULL
                      AND candidate.completed_at < @completed_before
                      AND NOT EXISTS (
                          SELECT 1
                          FROM messagex_outbox AS outbound
                          WHERE outbound.inbox_record_id = candidate.record_id
                      )
                    ORDER BY candidate.completed_at, candidate.record_id
                    LIMIT @maximum_count
                );
                """,
                parameters,
                token).ConfigureAwait(false);
            return inbox + outbox;
        }, cancellationToken).ConfigureAwait(false);
    }
}
