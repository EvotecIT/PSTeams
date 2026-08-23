namespace MessageX.Hosting.AspNetCore;

/// <summary>Schedules lease renewal from relative durations without mixing store and host clocks.</summary>
internal static class MessageLeaseRenewalSchedule {
    /// <summary>Returns one third of the effective lease duration, with a positive minimum delay.</summary>
    public static TimeSpan GetDelay(TimeSpan configuredDuration, TimeSpan? storeDuration) {
        var duration = storeDuration ?? configuredDuration;
        return TimeSpan.FromTicks(Math.Max(1, duration.Ticks / 3));
    }
}
