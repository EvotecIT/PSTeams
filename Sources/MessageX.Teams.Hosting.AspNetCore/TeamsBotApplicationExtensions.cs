using MessageX.Hosting;
using MessageX.Hosting.AspNetCore;
using Microsoft.Teams.Apps;
using System.Text;
using System.Text.Json;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Registers MessageX routing on a Microsoft-owned Teams SDK host.</summary>
public static class TeamsBotApplicationExtensions {
    /// <summary>
    /// Adapts verified Microsoft Teams SDK activities into MessageX handlers.
    /// </summary>
    /// <param name="application">Microsoft Teams SDK application that owns HTTP authentication and parsing.</param>
    /// <param name="acceptance">Configured volatile or durable MessageX ingress acceptance boundary.</param>
    /// <param name="router">MessageX handler registry used for synchronous Adaptive Card actions.</param>
    /// <param name="installationResolver">Host-owned mapping from verified Teams coordinates to installation identity.</param>
    /// <returns>The supplied Microsoft Teams application.</returns>
    public static TeamsBotApplication UseMessageXHosting(
        this TeamsBotApplication application,
        IMessageIngressAcceptance acceptance,
        MessageRouter router,
        ITeamsInstallationResolver installationResolver) {
        ArgumentNullException.ThrowIfNull(application);
        ArgumentNullException.ThrowIfNull(acceptance);
        ArgumentNullException.ThrowIfNull(router);
        ArgumentNullException.ThrowIfNull(installationResolver);
        TeamsHostingRegistrationGuard.Register(application);

        application.OnMessage((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessage(
                context.Activity,
                ResolveInstallation(context.Activity, installationResolver),
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            acceptance,
            cancellationToken));
        application.OnMessageUpdate((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessageUpdate(
                context.Activity,
                ResolveInstallation(context.Activity, installationResolver),
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            acceptance,
            cancellationToken));
        application.OnMessageDelete((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapMessageDelete(
                context.Activity,
                ResolveInstallation(context.Activity, installationResolver),
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            acceptance,
            cancellationToken));
        application.OnMessageReaction((context, cancellationToken) => DispatchAsync(
            TeamsActivityMapper.MapReaction(
                context.Activity,
                ResolveInstallation(context.Activity, installationResolver),
                DateTimeOffset.UtcNow,
                TeamsVerifiedActivityScope.Current),
            acceptance,
            cancellationToken));
        application.OnAdaptiveCardAction(async (context, cancellationToken) => {
            var result = await DispatchAdaptiveCardAsync(
                TeamsActivityMapper.MapAdaptiveCardAction(
                    context.Activity,
                    ResolveInstallation(context.Activity, installationResolver),
                    DateTimeOffset.UtcNow,
                    TeamsVerifiedActivityScope.Current),
                acceptance,
                router,
                cancellationToken).ConfigureAwait(false);
            return CreateInvokeResponse(result?.HandlerResult?.Acknowledgement);
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

    internal static async Task DispatchAsync(
        TeamsInboundDispatch dispatch,
        IMessageIngressAcceptance acceptance,
        CancellationToken cancellationToken) {
        var result = MessageReceiveResult<TeamsInboundActivity>.Dispatch(
            dispatch.Route,
            dispatch.Envelope,
            MessageAcknowledgement.Empty(200));
        var accepted = await acceptance.AcceptAsync(result, cancellationToken).ConfigureAwait(false);
        if (accepted is not (MessageIngressEnqueueStatus.Accepted or MessageIngressEnqueueStatus.Duplicate)) {
            throw new InvalidOperationException("The MessageX Teams ingress boundary is unavailable.");
        }
    }

    internal static async Task<MessageDispatchResult?> DispatchAdaptiveCardAsync(
        TeamsInboundDispatch dispatch,
        IMessageIngressAcceptance acceptance,
        MessageRouter router,
        CancellationToken cancellationToken) {
        var result = MessageReceiveResult<TeamsInboundActivity>.Dispatch(
            dispatch.Route,
            dispatch.Envelope,
            MessageAcknowledgement.Empty(200),
            requiresSynchronousDispatch: true);
        var accepted = await acceptance.AcceptAsync(result, cancellationToken).ConfigureAwait(false);
        if (accepted == MessageIngressEnqueueStatus.Duplicate) {
            return null;
        }
        if (accepted != MessageIngressEnqueueStatus.Accepted) {
            throw new InvalidOperationException("The MessageX Teams ingress boundary is unavailable.");
        }
        try {
            return await router.DispatchAsync(
                dispatch.Route,
                dispatch.Envelope,
                cancellationToken).ConfigureAwait(false);
        } catch {
            if (acceptance is IMessageIngressReservationRelease reservationRelease) {
                reservationRelease.Release(result);
            }
            throw;
        }
    }

    internal static string ResolveInstallation(
        Microsoft.Teams.Apps.Schema.TeamsActivity activity,
        ITeamsInstallationResolver resolver) {
        var context = TeamsActivityMapper.MapInstallationContext(
            activity,
            TeamsVerifiedActivityScope.Current);
        return TeamsActivityMapper.MapInstallationId(resolver.ResolveInstallationId(context));
    }

    internal static InvokeResponse CreateInvokeResponse(MessageAcknowledgement? acknowledgement) {
        if (acknowledgement is null) {
            return InvokeResponse.Ok();
        }
        object? body = null;
        var bytes = acknowledgement.CopyBody();
        if (bytes.Length > 0) {
            if (acknowledgement.ContentType?.StartsWith("application/json", StringComparison.OrdinalIgnoreCase) == true) {
                using var document = JsonDocument.Parse(bytes);
                body = document.RootElement.Clone();
            } else {
                body = Encoding.UTF8.GetString(bytes);
            }
        }
        return new InvokeResponse(acknowledgement.StatusCode, body);
    }
}
