namespace MessageX.Core;

/// <summary>
/// Asynchronously sends one provider-native message to one typed provider target.
/// </summary>
/// <typeparam name="TMessage">Provider-native message type.</typeparam>
/// <typeparam name="TTarget">Typed provider target.</typeparam>
/// <typeparam name="TResult">Provider-specific delivery result.</typeparam>
public interface IMessageSender<in TMessage, in TTarget, TResult>
    where TResult : MessageDeliveryResult {
    /// <summary>Sends a message.</summary>
    Task<TResult> SendAsync(TMessage message, TTarget target, CancellationToken cancellationToken = default);
}
