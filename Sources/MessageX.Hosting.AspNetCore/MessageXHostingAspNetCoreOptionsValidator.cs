using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageXHostingAspNetCoreOptionsValidator : IValidateOptions<MessageXHostingAspNetCoreOptions> {
    private const int MaximumAllowedBodyBytes = 16 * 1024 * 1024;
    private const int MaximumAllowedQueueCapacity = 65536;

    public ValidateOptionsResult Validate(string? name, MessageXHostingAspNetCoreOptions options) {
        if (options.MaximumRequestBodyBytes is < 1 or > MaximumAllowedBodyBytes) {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.MaximumRequestBodyBytes)} must be between 1 and {MaximumAllowedBodyBytes} bytes.");
        }
        if (options.QueueCapacity is < 1 or > MaximumAllowedQueueCapacity) {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.QueueCapacity)} must be between 1 and {MaximumAllowedQueueCapacity}.");
        }
        if (options.ReplayCapacity is < 1 or > MaximumAllowedQueueCapacity) {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.ReplayCapacity)} must be between 1 and {MaximumAllowedQueueCapacity}.");
        }
        if (options.ReplayRetention <= TimeSpan.Zero || options.ReplayRetention > TimeSpan.FromDays(7)) {
            return ValidateOptionsResult.Fail(
                $"{nameof(options.ReplayRetention)} must be greater than zero and at most seven days.");
        }
        return ValidateOptionsResult.Success;
    }
}
