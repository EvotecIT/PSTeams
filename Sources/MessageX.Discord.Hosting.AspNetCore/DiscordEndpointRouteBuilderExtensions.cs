using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace MessageX.Discord.Hosting.AspNetCore;

/// <summary>Registers Discord endpoint services and explicit installation routes.</summary>
public static class DiscordEndpointRouteBuilderExtensions {
    /// <summary>Adds the thin Discord ASP.NET Core endpoint adapter.</summary>
    public static IServiceCollection AddMessageXDiscordAspNetCore(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableCodec<DiscordInboundInteraction, DiscordInteractionDurableCodec>();
        services.TryAddSingleton<DiscordHttpEndpointHandler>();
        return services;
    }

    /// <summary>Maps one Discord interaction route to trusted installation configuration.</summary>
    public static RouteHandlerBuilder MapMessageXDiscordInteractions(
        this IEndpointRouteBuilder endpoints,
        string pattern,
        DiscordEndpointConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(configuration);
        return endpoints.MapPost(pattern, (
                HttpContext context,
                DiscordHttpEndpointHandler handler,
                CancellationToken cancellationToken) =>
                handler.HandleAsync(context, configuration, cancellationToken))
            .WithSummary("Receive verified Discord HTTP interactions")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status413PayloadTooLarge)
            .Produces(StatusCodes.Status415UnsupportedMediaType)
            .Produces(StatusCodes.Status503ServiceUnavailable);
    }
}
