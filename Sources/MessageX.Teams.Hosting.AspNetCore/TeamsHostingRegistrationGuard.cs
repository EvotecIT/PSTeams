using System.Runtime.CompilerServices;

namespace MessageX.Teams.Hosting.AspNetCore;

internal static class TeamsHostingRegistrationGuard {
    private static readonly ConditionalWeakTable<object, object> Registrations = new();

    public static void Register(object application) {
        ArgumentNullException.ThrowIfNull(application);
        if (!Registrations.TryAdd(application, new object())) {
            throw new InvalidOperationException(
                "MessageX hosting is already registered on this Microsoft Teams application.");
        }
    }
}
