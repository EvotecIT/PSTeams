using Microsoft.Extensions.DependencyInjection;

namespace MessageX.Hosting.AspNetCore;

internal sealed class DurableMessageIngressAcceptance : IMessageIngressAcceptance {
    private readonly IServiceProvider _services;
    private readonly IMessageDurableStore _store;
    private readonly MessageDurableStoreInitializer _initializer;
    private readonly MessageDurableIngressHealth _health;
    private readonly MessageReplayGuard _replayGuard;
    private readonly TimeProvider _timeProvider;

    public DurableMessageIngressAcceptance(
        IServiceProvider services,
        IMessageDurableStore store,
        MessageDurableStoreInitializer initializer,
        MessageDurableIngressHealth health,
        MessageReplayGuard replayGuard,
        TimeProvider timeProvider) {
        _services = services ?? throw new ArgumentNullException(nameof(services));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _initializer = initializer ?? throw new ArgumentNullException(nameof(initializer));
        _health = health ?? throw new ArgumentNullException(nameof(health));
        _replayGuard = replayGuard ?? throw new ArgumentNullException(nameof(replayGuard));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async ValueTask<MessageIngressEnqueueStatus> AcceptAsync<TProviderPayload>(
        MessageReceiveResult<TProviderPayload> result,
        CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNull(result);
        var route = result.Route;
        var envelope = result.Envelope;
        if (result.Status != MessageReceiveStatus.DispatchReady || route is null || envelope is null) {
            throw new ArgumentException("Only dispatch-ready receive results can be accepted.", nameof(result));
        }
        if (result.RequiresSynchronousDispatch) {
            var replay = _replayGuard.TryAccept(
                result,
                _timeProvider.GetUtcNow(),
                static () => MessageIngressEnqueueStatus.Accepted);
            return replay switch {
                MessageReplayAcceptance.Accepted => MessageIngressEnqueueStatus.Accepted,
                MessageReplayAcceptance.Duplicate => MessageIngressEnqueueStatus.Duplicate,
                MessageReplayAcceptance.Full => MessageIngressEnqueueStatus.Full,
                MessageReplayAcceptance.Stopping => MessageIngressEnqueueStatus.Stopping,
                _ => throw new InvalidOperationException("The replay guard returned an unsupported state.")
            };
        }
        var codec = _services.GetService<IMessageDurableCodec<TProviderPayload>>();
        if (codec is null) {
            _health.Unavailable(_timeProvider.GetUtcNow());
            return MessageIngressEnqueueStatus.Unavailable;
        }

        try {
            var record = codec.Encode(route, envelope);
            MessageDurableCodecGuard.ValidateEncoded(record, route, envelope, codec.PayloadType);
            await _initializer.EnsureInitializedAsync(cancellationToken).ConfigureAwait(false);
            var acceptance = await _store.AcceptInboxAsync(record, cancellationToken).ConfigureAwait(false);
            _health.Accepted();
            return acceptance.Status == MessageDurableAcceptanceStatus.Accepted
                ? MessageIngressEnqueueStatus.Accepted
                : MessageIngressEnqueueStatus.Duplicate;
        } catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        } catch {
            _health.Unavailable(_timeProvider.GetUtcNow());
            return MessageIngressEnqueueStatus.Unavailable;
        }
    }
}
