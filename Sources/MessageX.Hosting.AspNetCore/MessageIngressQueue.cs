using System.Threading.Channels;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageIngressQueue : IMessageIngressQueue {
    private readonly Channel<IMessageIngressWorkItem> _channel;
    private readonly MessageIngressHealth _health;
    private int _stopping;

    public MessageIngressQueue(IOptions<MessageXHostingAspNetCoreOptions> options) {
        ArgumentNullException.ThrowIfNull(options);
        var capacity = options.Value.QueueCapacity;
        _health = new MessageIngressHealth(capacity);
        _channel = Channel.CreateBounded<IMessageIngressWorkItem>(new BoundedChannelOptions(capacity) {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    public MessageIngressEnqueueStatus TryEnqueue<TProviderPayload>(MessageReceiveResult<TProviderPayload> result) {
        ArgumentNullException.ThrowIfNull(result);
        if (result.Status != MessageReceiveStatus.DispatchReady || result.Route is null || result.Envelope is null) {
            throw new ArgumentException("Only dispatch-ready receive results can be enqueued.", nameof(result));
        }
        if (Volatile.Read(ref _stopping) != 0) {
            return MessageIngressEnqueueStatus.Stopping;
        }
        if (!_channel.Writer.TryWrite(new MessageIngressWorkItem<TProviderPayload>(result.Route, result.Envelope))) {
            return Volatile.Read(ref _stopping) != 0
                ? MessageIngressEnqueueStatus.Stopping
                : MessageIngressEnqueueStatus.Full;
        }
        _health.Accepted();
        return MessageIngressEnqueueStatus.Accepted;
    }

    public MessageIngressHealthSnapshot GetHealthSnapshot() => _health.Snapshot(_channel.Reader.Count);

    public IAsyncEnumerable<IMessageIngressWorkItem> ReadAllAsync(CancellationToken cancellationToken) =>
        _channel.Reader.ReadAllAsync(cancellationToken);

    public void Completed(DateTimeOffset at) => _health.Completed(at);

    public void Failed(DateTimeOffset at) => _health.Failed(at);

    public void Complete() {
        if (Interlocked.Exchange(ref _stopping, 1) == 0) {
            _health.Stopping();
            _channel.Writer.TryComplete();
        }
    }

    internal interface IMessageIngressWorkItem {
        Task<MessageDispatchResult> DispatchAsync(MessageRouter router, CancellationToken cancellationToken);
    }

    private sealed class MessageIngressWorkItem<TProviderPayload> : IMessageIngressWorkItem {
        private readonly MessageRoute _route;
        private readonly MessageEventEnvelope<TProviderPayload> _envelope;

        public MessageIngressWorkItem(
            MessageRoute route,
            MessageEventEnvelope<TProviderPayload> envelope) {
            _route = route;
            _envelope = envelope;
        }

        public Task<MessageDispatchResult> DispatchAsync(
            MessageRouter router,
            CancellationToken cancellationToken) =>
            router.DispatchAsync(_route, _envelope, cancellationToken);
    }
}
