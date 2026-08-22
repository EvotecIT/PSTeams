using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageX.Slack.Hosting.AspNetCore;

/// <summary>Registers Slack endpoint services and explicit installation routes.</summary>
public static class SlackEndpointRouteBuilderExtensions {
    /// <summary>Adds the thin Slack ASP.NET Core endpoint adapter.</summary>
    public static IServiceCollection AddMessageXSlackAspNetCore(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMessageXHostingAspNetCore();
        services.TryAddSingleton<SlackHttpEndpointHandler>();
        return services;
    }

    /// <summary>Maps one Slack Events API route to trusted installation configuration.</summary>
    public static RouteHandlerBuilder MapMessageXSlackEvents(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        SlackEndpointConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(configuration);
        return endpoints.MapPost(pattern, (
                HttpContext context,
                SlackHttpEndpointHandler handler,
                CancellationToken cancellationToken) =>
                handler.HandleEventsAsync(context, configuration, cancellationToken))
            .WithSummary("Receive verified Slack Events API requests")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status415UnsupportedMediaType)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }

    /// <summary>Maps one Slack interaction route to trusted installation configuration.</summary>
    public static RouteHandlerBuilder MapMessageXSlackInteractions(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        SlackEndpointConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(configuration);
        return endpoints.MapPost(pattern, (
                HttpContext context,
                SlackHttpEndpointHandler handler,
                CancellationToken cancellationToken) =>
                handler.HandleInteractionsAsync(context, configuration, cancellationToken))
            .WithSummary("Receive verified Slack commands and interactions")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status415UnsupportedMediaType)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }
}
