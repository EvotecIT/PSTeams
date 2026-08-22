using Microsoft.Extensions.Options;

namespace MessageX.Hosting.AspNetCore;

internal sealed class MessageXDurableIngressOptionsValidator : IValidateOptions<MessageXDurableIngressOptions> {
    public ValidateOptionsResult Validate(string? name, MessageXDurableIngressOptions options) {
        if (options.ClaimBatchSize is < 1 or > 100) {
            return ValidateOptionsResult.Fail("ClaimBatchSize must be between 1 and 100.");
        }
        if (options.LeaseDuration < TimeSpan.FromSeconds(1) || options.LeaseDuration > TimeSpan.FromHours(1)) {
            return ValidateOptionsResult.Fail("LeaseDuration must be between one second and one hour.");
        }
        if (options.PollInterval < TimeSpan.FromMilliseconds(10) || options.PollInterval > TimeSpan.FromMinutes(1)) {
            return ValidateOptionsResult.Fail("PollInterval must be between 10 milliseconds and one minute.");
        }
        if (options.RetryDelay < TimeSpan.Zero || options.RetryDelay > TimeSpan.FromDays(7)) {
            return ValidateOptionsResult.Fail("RetryDelay must be between zero and seven days.");
        }
        if (options.MaximumAttempts is < 1 or > 100) {
            return ValidateOptionsResult.Fail("MaximumAttempts must be between 1 and 100.");
        }
        return ValidateOptionsResult.Success;
    }
}
