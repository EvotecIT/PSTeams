namespace MessageX.Persistence.DbaClientX;

public sealed partial class SqliteMessageDurableStore {
    private const string CreateInboxSql = """
        CREATE TABLE IF NOT EXISTS messagex_inbox (
            record_id TEXT NOT NULL PRIMARY KEY,
            provider TEXT NOT NULL COLLATE BINARY,
            installation_id TEXT NOT NULL COLLATE BINARY,
            deduplication_key TEXT NOT NULL COLLATE BINARY,
            route_kind INTEGER NOT NULL,
            event_kind INTEGER NOT NULL,
            route_name TEXT NULL,
            route_qualifier TEXT NULL,
            received_at TEXT NOT NULL,
            payload_type TEXT NOT NULL,
            payload BLOB NOT NULL,
            status INTEGER NOT NULL,
            attempt_count INTEGER NOT NULL,
            available_at TEXT NOT NULL,
            lease_owner TEXT NULL,
            lease_token TEXT NULL,
            lease_expires_at TEXT NULL,
            completed_at TEXT NULL,
            failure_kind INTEGER NOT NULL,
            UNIQUE (provider, installation_id, deduplication_key)
        );
        """;

    private const string CreateOutboxSql = """
        CREATE TABLE IF NOT EXISTS messagex_outbox (
            record_id TEXT NOT NULL PRIMARY KEY,
            inbox_record_id TEXT NOT NULL,
            provider TEXT NOT NULL COLLATE BINARY,
            installation_id TEXT NOT NULL COLLATE BINARY,
            deduplication_key TEXT NOT NULL COLLATE BINARY,
            operation TEXT NOT NULL,
            payload_type TEXT NOT NULL,
            payload BLOB NOT NULL,
            status INTEGER NOT NULL,
            attempt_count INTEGER NOT NULL,
            available_at TEXT NOT NULL,
            lease_owner TEXT NULL,
            lease_token TEXT NULL,
            lease_expires_at TEXT NULL,
            completed_at TEXT NULL,
            failure_kind INTEGER NOT NULL,
            UNIQUE (provider, installation_id, deduplication_key)
        );
        """;

    /// <inheritdoc />
    public async Task InitializeAsync(CancellationToken cancellationToken = default) {
        await using var session = await OpenSessionAsync(cancellationToken).ConfigureAwait(false);
        await session.RunInTransactionAsync(async (transaction, token) => {
            await transaction.ExecuteNonQueryAsync(CreateInboxSql, cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(CreateOutboxSql, cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "CREATE INDEX IF NOT EXISTS ix_messagex_inbox_available ON messagex_inbox(status, available_at, lease_expires_at);",
                cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "CREATE INDEX IF NOT EXISTS ix_messagex_outbox_available ON messagex_outbox(status, available_at, lease_expires_at);",
                cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "CREATE INDEX IF NOT EXISTS ix_messagex_inbox_terminal ON messagex_inbox(status, completed_at);",
                cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "CREATE INDEX IF NOT EXISTS ix_messagex_outbox_terminal ON messagex_outbox(status, completed_at);",
                cancellationToken: token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "UPDATE messagex_inbox SET completed_at = available_at WHERE status = @dead_lettered AND completed_at IS NULL;",
                new Dictionary<string, object?> {
                    ["dead_lettered"] = (int)MessageDurableStatus.DeadLettered
                },
                token).ConfigureAwait(false);
            await transaction.ExecuteNonQueryAsync(
                "UPDATE messagex_outbox SET completed_at = available_at WHERE status = @dead_lettered AND completed_at IS NULL;",
                new Dictionary<string, object?> {
                    ["dead_lettered"] = (int)MessageDurableStatus.DeadLettered
                },
                token).ConfigureAwait(false);
        }, cancellationToken).ConfigureAwait(false);
    }
}
