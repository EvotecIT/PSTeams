namespace MessageX.Hosting;

/// <summary>Outcome of matching and invoking one registered application handler.</summary>
public sealed class MessageDispatchResult {
    private MessageDispatchResult(bool routeMatched, MessageHandlerResult? handlerResult) {
        RouteMatched = routeMatched;
        HandlerResult = handlerResult;
    }

    /// <summary>Whether a handler was registered for the route and payload type.</summary>
    public bool RouteMatched { get; }

    /// <summary>Handler outcome, or null when no route matched.</summary>
    public MessageHandlerResult? HandlerResult { get; }

    internal static MessageDispatchResult NotMatched() => new(false, null);

    internal static MessageDispatchResult Matched(MessageHandlerResult result) => new(true, result);
}
