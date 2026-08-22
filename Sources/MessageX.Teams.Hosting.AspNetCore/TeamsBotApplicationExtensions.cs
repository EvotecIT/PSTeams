using MessageX.Hosting;
using Microsoft.Teams.Apps;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Registers MessageX routing on a Microsoft-owned Teams SDK host.</summary>
public static class TeamsBotApplicationExtensions {
    /// <summary>
    /// Adapts verified Microsoft Teams SDK activities into MessageX handlers.
    /// </summary>
    /// <param name="application">Microsoft Teams SDK application that owns HTTP authentication and parsing.</param>
    /// <param name="router">MessageX handler registry.</param>
    /// <param name="installationId">Trusted non-secret installation selected by host configuration.</param>
    /// <returns>The supplied Microsoft Teams application.</returns>
    public static TeamsBotApplication UseMessageXHosting(
        this TeamsBotApplication application,
        MessageRouter router,
        string installationId) {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(router);
        _ = TeamsActivityMapper.MapInstallationId(installationId);
        TeamsHostingRegistrationGuard.Register(application);

        application.OnMessage((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessage(
                context.Activity,
                installationId,
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            router,
            cancellationToken));
        application.OnMessageUpdate((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessageUpdate(
                context.Activity,
                installationId,
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            router,
            cancellationToken));
        application.OnMessageDelete((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessageDelete(
                context.Activity,
                installationId,
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            router,
            cancellationToken));
        application.OnMessageReaction((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapReaction(
                context.Activity,
                installationId,
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            router,
            cancellationToken));
        application.OnAdaptiveCardAction(async (context, cancellationToken) => {
            await DispatchAsync(
                TeamsActivityMapper.MapAdaptiveCardAction(
                    context.Activity,
                    installationId,
                    DateTimeOffset.UtcNow,
                    TeamsVerifiedActivityScope.Current),
                router,
                cancellationToken).ConfigureAwait(false);
            return InvokeResponse.Ok();
        });

        var sdkActivityHandler = application.OnActivity ??
            throw new InvalidOperationException(
                "The Microsoft Teams application does not have an activity handler.");
        application.OnActivity = async (activity, cancellationToken) => {
            using var scope = TeamsVerifiedActivityScope.Push(activity);
            await sdkActivityHandler(activity, cancellationToken).ConfigureAwait(false);
        };

        return application;
    }

    private static async Task DispatchAsync(
        TeamsInboundDispatch dispatch,
        MessageRouter router,
        CancellationToken cancellationToken) {
        _ = await router.DispatchAsync(
            dispatch.Route,
            dispatch.Envelope,
            cancellationToken).ConfigureAwait(false);
    }
}
