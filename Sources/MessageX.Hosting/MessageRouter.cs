namespace MessageX.Hosting;

/// <summary>Thread-safe host-neutral registry and dispatcher for typed messaging handlers.</summary>
public sealed class MessageRouter {
    private readonly object _sync = new();
    private readonly Dictionary<HandlerKey, IHandlerRegistration> _handlers = new();

    /// <summary>Registers a handler for one provider-neutral event kind.</summary>
    public void OnEvent<TProviderPayload>(
        MessageEventKind eventKind,
        MessageEventHandler<TProviderPayload> handler) =>
        Register(MessageRoute.ForEvent(eventKind), handler);

    /// <summary>Registers a named command handler.</summary>
    public void OnCommand<TProviderPayload>(
        string name,
        MessageEventHandler<TProviderPayload> handler) =>
        Register(MessageRoute.ForCommand(name), handler);

    /// <summary>Registers an application-mention handler.</summary>
    public void OnMention<TProviderPayload>(MessageEventHandler<TProviderPayload> handler) =>
        Register(MessageRoute.ForMention(), handler);

    /// <summary>Registers a direct-message handler.</summary>
    public void OnDirectMessage<TProviderPayload>(MessageEventHandler<TProviderPayload> handler) =>
        Register(MessageRoute.ForDirectMessage(), handler);

    /// <summary>Registers a named interactive-action handler.</summary>
    public void OnAction<TProviderPayload>(
        string name,
        MessageEventHandler<TProviderPayload> handler) =>
        Register(MessageRoute.ForAction(name), handler);

    /// <summary>Dispatches a verified event to an exact route and payload-type registration.</summary>
    public async Task<MessageDispatchResult> DispatchAsync<TProviderPayload>(
        MessageRoute route,
        MessageEventEnvelope<TProviderPayload> envelope,
        CancellationToken cancellationToken = default) {
        if (route is null) {
            throw new ArgumentNullException(nameof(route));
        }
        if (envelope is null) {
            throw new ArgumentNullException(nameof(envelope));
        }
        if (route.EventKind != envelope.Kind) {
            throw new ArgumentException(
                "The selected route must match the verified event classification.",
                nameof(route));
        }
        cancellationToken.ThrowIfCancellationRequested();

        IHandlerRegistration? registration;
        lock (_sync) {
            _handlers.TryGetValue(HandlerKey.Create<TProviderPayload>(route), out registration);
        }
        if (registration is null) {
            return MessageDispatchResult.NotMatched();
        }

        var result = await registration
            .InvokeAsync(route, envelope, cancellationToken)
            .ConfigureAwait(false);
        return MessageDispatchResult.Matched(result);
    }

    private void Register<TProviderPayload>(
        MessageRoute route,
        MessageEventHandler<TProviderPayload> handler) {
        if (handler is null) {
            throw new ArgumentNullException(nameof(handler));
        }
        var key = HandlerKey.Create<TProviderPayload>(route);
        lock (_sync) {
            if (_handlers.ContainsKey(key)) {
                throw new InvalidOperationException(
                    "A handler is already registered for this route and provider payload type.");
            }
            _handlers.Add(key, new HandlerRegistration<TProviderPayload>(handler));
        }
    }

    private interface IHandlerRegistration {
        Task<MessageHandlerResult> InvokeAsync(
            MessageRoute route,
            object envelope,
            CancellationToken cancellationToken);
    }

    private sealed class HandlerRegistration<TProviderPayload> : IHandlerRegistration {
        private readonly MessageEventHandler<TProviderPayload> _handler;

        public HandlerRegistration(MessageEventHandler<TProviderPayload> handler) {
            _handler = handler;
        }

        public async Task<MessageHandlerResult> InvokeAsync(
            MessageRoute route,
            object envelope,
            CancellationToken cancellationToken) {
            var result = await _handler(
                new MessageHandlerContext<TProviderPayload>(
                    route,
                    (MessageEventEnvelope<TProviderPayload>)envelope),
                cancellationToken).ConfigureAwait(false);
            return result ?? throw new InvalidOperationException("Message handlers cannot return null.");
        }
    }

    private readonly struct HandlerKey : IEquatable<HandlerKey> {
        private HandlerKey(
            Type payloadType,
            MessageRouteKind routeKind,
            MessageEventKind eventKind,
            string? name) {
            PayloadType = payloadType;
            RouteKind = routeKind;
            EventKind = eventKind;
            Name = name;
        }

        private Type PayloadType { get; }

        private MessageRouteKind RouteKind { get; }

        private MessageEventKind EventKind { get; }

        private string? Name { get; }

        public static HandlerKey Create<TProviderPayload>(MessageRoute route) => new(
            typeof(TProviderPayload),
            route.Kind,
            route.EventKind,
            route.Name);

        public bool Equals(HandlerKey other) =>
            PayloadType == other.PayloadType &&
            RouteKind == other.RouteKind &&
            EventKind == other.EventKind &&
            string.Equals(Name, other.Name, StringComparison.OrdinalIgnoreCase);

        public override bool Equals(object? obj) => obj is HandlerKey other && Equals(other);

        public override int GetHashCode() {
            unchecked {
                var hashCode = PayloadType.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)RouteKind;
                hashCode = (hashCode * 397) ^ (int)EventKind;
                hashCode = (hashCode * 397) ^ (Name is null
                    ? 0
                    : StringComparer.OrdinalIgnoreCase.GetHashCode(Name));
                return hashCode;
            }
        }
    }
}
