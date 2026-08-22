using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Registers provider-neutral MessageX ingress services.</summary>
public static class MessageXHostingAspNetCoreServiceCollectionExtensions {
    /// <summary>Adds bounded ingress, dispatch, health, and exact HTTP I/O services.</summary>
    public static IServiceCollection AddMessageXHostingAspNetCore(
        this IServiceCollection services,
        Action<MessageXHostingAspNetCoreOptions>? configure = null) {
        ArgumentNullException.ThrowIfNull(services);
        var options = services.AddOptions<MessageXHostingAspNetCoreOptions>();
        if (configure is not null) {
            options.Configure(configure);
        }
        options.ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<MessageXHostingAspNetCoreOptions>,
            MessageXHostingAspNetCoreOptionsValidator>());
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<MessageRouter>();
        services.TryAddSingleton<MessageInboundRequestReader>();
        services.TryAddSingleton<MessageAcknowledgementWriter>();
        services.TryAddSingleton(provider => {
            var value = provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value;
            return new MessageReplayGuard(value.ReplayCapacity, value.ReplayRetention);
        });
        services.TryAddSingleton(provider => {
            var value = provider.GetRequiredService<IOptions<MessageXHostingAspNetCoreOptions>>().Value;
            return new MessageSynchronousDispatchGate(value.SynchronousDispatchCapacity);
        });
        services.TryAddSingleton<MessageIngressQueue>();
        services.TryAddSingleton<IMessageIngressQueue>(provider =>
            provider.GetRequiredService<MessageIngressQueue>());
        services.TryAddSingleton<IMessageIngressAcceptance, QueuedMessageIngressAcceptance>();
        services.TryAddSingleton<MessageReceiveResultProcessor>();
        services.AddHostedService<MessageIngressWorker>();
        return services;
    }
}
