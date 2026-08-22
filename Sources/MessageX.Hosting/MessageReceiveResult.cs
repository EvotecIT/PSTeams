namespace MessageX.Hosting;

/// <summary>Verified receive outcome containing only safe routing state plus a transient acknowledgement.</summary>
/// <typeparam name="TProviderPayload">Provider-native typed payload.</typeparam>
public sealed class MessageReceiveResult<TProviderPayload> {
    private MessageReceiveResult(
        MessageReceiveStatus status,
        MessageReceiveFailureKind failureKind,
        MessageAcknowledgement acknowledgement,
        MessageRoute? route,
        MessageEventEnvelope<TProviderPayload>? envelope,
        bool requiresSynchronousDispatch) {
        Status = status;
        FailureKind = failureKind;
        Acknowledgement = acknowledgement;
        Route = route;
        Envelope = envelope;
        RequiresSynchronousDispatch = requiresSynchronousDispatch;
    }

    /// <summary>Action selected for the host.</summary>
    public MessageReceiveStatus Status { get; }

    /// <summary>Safe rejection classification.</summary>
    public MessageReceiveFailureKind FailureKind { get; }

    /// <summary>Provider acknowledgement that should be returned within its deadline.</summary>
    public MessageAcknowledgement Acknowledgement { get; }

    /// <summary>Application route for a dispatch-ready result.</summary>
    public MessageRoute? Route { get; }

    /// <summary>Verified typed envelope for a dispatch-ready result.</summary>
    public MessageEventEnvelope<TProviderPayload>? Envelope { get; }

    /// <summary>Whether the provider response must be produced by dispatching the handler before acknowledgement.</summary>
    public bool RequiresSynchronousDispatch { get; }

    /// <summary>Creates a rejected receive result.</summary>
    public static MessageReceiveResult<TProviderPayload> Reject(
        MessageReceiveFailureKind failureKind,
        MessageAcknowledgement acknowledgement) {
        if (failureKind == MessageReceiveFailureKind.None) {
            throw new ArgumentOutOfRangeException(nameof(failureKind));
        }
        return new MessageReceiveResult<TProviderPayload>(
            MessageReceiveStatus.Rejected,
            failureKind,
            acknowledgement ?? throw new ArgumentNullException(nameof(acknowledgement)),
            null,
            null,
            false);
    }

    /// <summary>Creates an acknowledgement-only result.</summary>
    public static MessageReceiveResult<TProviderPayload> Acknowledge(MessageAcknowledgement acknowledgement) =>
        new(
            MessageReceiveStatus.Acknowledged,
            MessageReceiveFailureKind.None,
            acknowledgement ?? throw new ArgumentNullException(nameof(acknowledgement)),
            null,
            null,
            false);

    /// <summary>Creates a verified event ready for persistence or enqueueing.</summary>
    public static MessageReceiveResult<TProviderPayload> Dispatch(
        MessageRoute route,
        MessageEventEnvelope<TProviderPayload> envelope,
        MessageAcknowledgement acknowledgement,
        bool requiresSynchronousDispatch = false) {
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
        return new MessageReceiveResult<TProviderPayload>(
            MessageReceiveStatus.DispatchReady,
            MessageReceiveFailureKind.None,
            acknowledgement ?? throw new ArgumentNullException(nameof(acknowledgement)),
            route,
            envelope,
            requiresSynchronousDispatch);
    }
}
