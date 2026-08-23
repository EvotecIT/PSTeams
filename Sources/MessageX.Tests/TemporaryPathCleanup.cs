namespace MessageX.Tests;

internal static class TemporaryPathCleanup {
    private const int MaximumRetries = 20;
    private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(25);

    public static void DeleteSqliteDatabase(string databasePath) {
        foreach (var path in new[] { databasePath, databasePath + "-wal", databasePath + "-shm" }) {
            DeleteWithRetry(() => File.Exists(path), () => File.Delete(path));
        }
    }

    public static void DeleteDirectory(string directoryPath) =>
        DeleteWithRetry(
            () => Directory.Exists(directoryPath),
            () => Directory.Delete(directoryPath, recursive: true));

    private static void DeleteWithRetry(Func<bool> exists, Action delete) {
        for (var attempt = 0; exists(); attempt++) {
            try {
                delete();
                return;
            } catch (Exception exception) when (
                attempt < MaximumRetries &&
                exception is IOException or UnauthorizedAccessException) {
                Thread.Sleep(RetryDelay);
            }
        }
    }
}
