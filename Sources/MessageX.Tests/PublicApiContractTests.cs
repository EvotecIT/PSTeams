using System.Reflection;
using MessageX.Discord;
using MessageX.Discord.Hosting.AspNetCore;
using MessageX.Hosting;
using MessageX.Persistence.DbaClientX;
using MessageX.Slack;
using MessageX.Slack.Hosting.AspNetCore;
using MessageX.Teams;
using MessageX.Teams.Hosting.AspNetCore;

namespace MessageX.Tests;

public sealed class PublicApiContractTests {
    [Fact]
    public void PublicMessageXModelsDoNotExposeSerializerOwnedTypes() {
        var assemblies = new[] {
            typeof(MessageReference).Assembly,
            typeof(MessageReceiveResult<>).Assembly,
            typeof(DiscordInboundInteraction).Assembly,
            typeof(DiscordInteractionDurableCodec).Assembly,
            typeof(DiscordHttpEndpointHandler).Assembly,
            typeof(SlackInteractionPayload).Assembly,
            typeof(SlackInteractionEventDurableCodec).Assembly,
            typeof(SlackHttpEndpointHandler).Assembly,
            typeof(TeamsMessageRequest).Assembly,
            typeof(TeamsInboundAttachment).Assembly,
            typeof(SqliteMessageDurableStore).Assembly
        }.Distinct();
        var leaks = assemblies
            .SelectMany(static assembly => assembly.GetExportedTypes())
            .SelectMany(PublicSignatures)
            .Where(signature => ContainsSerializerType(signature.Type))
            .Select(signature => signature.Name + " -> " + signature.Type.FullName)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(leaks);
    }

    [Fact]
    public void TeamsInboundModelDoesNotExposeProviderSdkTypes() {
        var leaks = PublicSignatures(typeof(TeamsInboundActivity))
            .Where(signature => signature.Type.Assembly.GetName().Name?.StartsWith(
                "Microsoft.Teams.",
                StringComparison.Ordinal) == true)
            .Select(signature => signature.Name + " -> " + signature.Type.FullName)
            .OrderBy(static value => value, StringComparer.Ordinal)
            .ToArray();

        Assert.Empty(leaks);
    }

    private static IEnumerable<(string Name, Type Type)> PublicSignatures(Type type) {
        const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance |
                                   BindingFlags.Static | BindingFlags.DeclaredOnly;
        if (type.BaseType is { } baseType) {
            yield return ($"{type.FullName} base", baseType);
        }
        foreach (var interfaceType in type.GetInterfaces()) {
            yield return ($"{type.FullName} interface", interfaceType);
        }
        foreach (var genericParameter in type.GetGenericArguments().Where(static value => value.IsGenericParameter)) {
            foreach (var constraint in genericParameter.GetGenericParameterConstraints()) {
                yield return ($"{type.FullName} generic constraint", constraint);
            }
        }
        foreach (var constructor in type.GetConstructors(flags)) {
            foreach (var parameter in constructor.GetParameters()) {
                yield return ($"{type.FullName}.{constructor.Name}({parameter.Name})", parameter.ParameterType);
            }
        }
        foreach (var method in type.GetMethods(flags)) {
            yield return ($"{type.FullName}.{method.Name} return", method.ReturnType);
            foreach (var parameter in method.GetParameters()) {
                yield return ($"{type.FullName}.{method.Name}({parameter.Name})", parameter.ParameterType);
            }
            foreach (var genericParameter in method.GetGenericArguments().Where(static value => value.IsGenericParameter)) {
                foreach (var constraint in genericParameter.GetGenericParameterConstraints()) {
                    yield return ($"{type.FullName}.{method.Name} generic constraint", constraint);
                }
            }
        }
        foreach (var property in type.GetProperties(flags)) {
            yield return ($"{type.FullName}.{property.Name}", property.PropertyType);
            foreach (var parameter in property.GetIndexParameters()) {
                yield return ($"{type.FullName}.{property.Name}({parameter.Name})", parameter.ParameterType);
            }
        }
        foreach (var field in type.GetFields(flags)) {
            yield return ($"{type.FullName}.{field.Name}", field.FieldType);
        }
        foreach (var eventInfo in type.GetEvents(flags)) {
            yield return ($"{type.FullName}.{eventInfo.Name}", eventInfo.EventHandlerType!);
        }
    }

    private static bool ContainsSerializerType(Type type) {
        if (string.Equals(
                type.Assembly.GetName().Name,
                "System.Text.Json",
                StringComparison.Ordinal)) {
            return true;
        }
        if (type.HasElementType && type.GetElementType() is { } elementType &&
            ContainsSerializerType(elementType)) {
            return true;
        }
        return type.IsGenericType && type.GetGenericArguments().Any(ContainsSerializerType);
    }
}
