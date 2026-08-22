namespace MessageX.Core;

/// <summary>Adds and removes the current application's reactions on provider messages.</summary>
/// <typeparam name="TResult">Provider-specific operation result.</typeparam>
public interface IReactionClient<TResult>
    where TResult : MessageDeliveryResult {
    /// <summary>Adds the current application's reaction.</summary>
    Task<TResult> AddReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default);

    /// <summary>Removes the current application's reaction.</summary>
    Task<TResult> RemoveReactionAsync(
        MessageReference reference,
        string reaction,
        CancellationToken cancellationToken = default);
}
