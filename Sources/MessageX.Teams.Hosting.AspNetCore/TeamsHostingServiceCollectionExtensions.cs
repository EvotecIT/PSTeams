using MessageX.Hosting.AspNetCore;
using Microsoft.Extensions.DependencyInjection;

namespace MessageX.Teams.Hosting.AspNetCore;

/// <summary>Registers the safe MessageX Teams hosting persistence boundary.</summary>
public static class TeamsHostingServiceCollectionExtensions {
    /// <summary>Adds the Teams durable activity codec used by volatile or durable MessageX ingress.</summary>
    public static IServiceCollection AddMessageXTeamsHosting(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);
        services.AddMessageXHostingAspNetCore();
        services.AddMessageXDurableCodec<TeamsInboundActivity, TeamsInboundActivityDurableCodec>();
        return services;
    }
}
