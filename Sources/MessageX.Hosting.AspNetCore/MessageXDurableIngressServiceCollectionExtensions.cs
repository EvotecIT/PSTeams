using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

/// <summary>Registers durable MessageX acceptance and provider payload codecs.</summary>
public static class MessageXDurableIngressServiceCollectionExtensions {
    /// <summary>
    /// Replaces volatile queue acceptance with a registered <see cref="IMessageDurableStore"/>.
    /// </summary>
    public static IServiceCollection AddMessageXDurableIngress(
        this IServiceCollection services,
        Action<MessageXDurableIngressOptions>? configure = null) {
        ArgumentNullException.ThrowIfNull(services);
        var options = services.AddOptions<MessageXDurableIngressOptions>();
        if (configure is not null) {
            options.Configure(configure);
        }
        options.ValidateOnStart();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IValidateOptions<MessageXDurableIngressOptions>,
            MessageXDurableIngressOptionsValidator>());
        services.TryAddSingleton<MessageDurableStoreInitializer>();
        services.TryAddSingleton<MessageDurableIngressHealth>();
        services.TryAddSingleton<IMessageDurableIngressHealth>(provider =>
            provider.GetRequiredService<MessageDurableIngressHealth>());
        services.RemoveAll<IMessageIngressAcceptance>();
        services.AddSingleton<IMessageIngressAcceptance, DurableMessageIngressAcceptance>();
        services.AddHostedService<MessageDurableIngressWorker>();
        services.AddHostedService<MessageDurableOutboxWorker>();
        services.AddHostedService<MessageDurableCleanupWorker>();
        return services;
    }

    /// <summary>Registers one safe outbox payload delivery owner.</summary>
    public static IServiceCollection AddMessageXOutboxHandler<THandler>(this IServiceCollection services)
        where THandler : class, IMessageOutboxHandler {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IMessageOutboxHandler, THandler>());
        return services;
    }

    /// <summary>Registers one safe versioned durable codec for a provider payload type.</summary>
    public static IServiceCollection AddMessageXDurableCodec<TProviderPayload, TCodec>(
        this IServiceCollection services)
        where TCodec : class, IMessageDurableCodec<TProviderPayload> {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSingleton<TCodec>();
        services.TryAddSingleton<IMessageDurableCodec<TProviderPayload>>(provider =>
            provider.GetRequiredService<TCodec>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<
            IMessageDurableDispatchCodec,
            MessageDurableDispatchCodec<TProviderPayload>>());
        return services;
    }
}
