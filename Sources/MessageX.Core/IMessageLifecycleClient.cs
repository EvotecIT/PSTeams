namespace MessageX.Core;

/// <summary>
/// Updates and deletes application-owned messages through safe persisted references.
/// </summary>
/// <typeparam name="TMessage">Provider-native replacement message type.</typeparam>
/// <typeparam name="TResult">Provider-specific operation result.</typeparam>
public interface IMessageLifecycleClient<in TMessage, TResult>
    where TResult : MessageDeliveryResult {
    /// <summary>Replaces the content of an application-owned message.</summary>
    Task<TResult> UpdateAsync(
        TMessage message,
        MessageReference reference,
        CancellationToken cancellationToken = default);

    /// <summary>Deletes an application-owned message.</summary>
    Task<TResult> DeleteAsync(
        MessageReference reference,
        CancellationToken cancellationToken = default);
}
