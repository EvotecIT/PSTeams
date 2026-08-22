using MessageX.Core;
using MessageX.Hosting;

namespace MessageX.Teams.Hosting.AspNetCore;

internal sealed class TeamsInboundDispatch {
    public TeamsInboundDispatch(
        MessageRoute route,
        MessageEventEnvelope<TeamsInboundActivity> envelope) {
        Route = route;
        Envelope = envelope;
    }

    public MessageRoute Route { get; }

    public MessageEventEnvelope<TeamsInboundActivity> Envelope { get; }
}
