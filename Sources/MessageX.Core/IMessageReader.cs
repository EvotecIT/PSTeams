namespace MessageX.Core;

/// <summary>Retrieves an application-owned provider message through a safe persisted reference.</summary>
/// <typeparam name="TMessage">Provider-native retrieved-message type.</typeparam>
public interface IMessageReader<TMessage> {
    /// <summary>Retrieves one message.</summary>
    Task<TMessage> GetAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default);
}
