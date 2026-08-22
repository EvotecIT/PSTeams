using System.Net.Http;

namespace MessageX.Slack;

internal static class SlackHttpResponseSupport {
    public static TimeSpan? ReadRetryAfter(HttpResponseMessage response) {
        var retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is not null) {
            return retryAfter.Delta;
        }
        if (retryAfter?.Date is not null) {
            var delay = retryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }
        return null;
    }

    public static string? ReadCorrelationId(HttpResponseMessage response) {
        return response.Headers.TryGetValues("x-slack-req-id", out var values)
            ? values.FirstOrDefault()
            : null;
    }

    public static MessageErrorKind Classify(int statusCode, string? providerCode) {
        var code = providerCode?.ToLowerInvariant();
        if (statusCode == 429 || code is "ratelimited" or "rate_limited") {
            return MessageErrorKind.RateLimited;
        }
        if (statusCode == 401 || code is
            "invalid_auth" or "not_authed" or "token_revoked" or "token_expired" or "account_inactive") {
            return MessageErrorKind.Authentication;
        }
        if (statusCode == 403 ||
            code is "missing_scope" or "no_permission" or "restricted_action" or "action_prohibited" or
                "access_denied" or "app_access_restricted" or "ekm_access_denied" or "not_in_channel" ||
            code?.StartsWith("restricted_action_", StringComparison.Ordinal) == true) {
            return MessageErrorKind.Authorization;
        }
        if (statusCode is 404 or 410 || code is
            "channel_not_found" or "channel_is_archived" or "team_not_found" or "user_not_found") {
            return MessageErrorKind.NotFound;
        }
        if (statusCode == 408 || statusCode >= 500) {
            return MessageErrorKind.Transient;
        }
        if (code is "internal_error" or "fatal_error" or "service_unavailable" or "request_timeout" or
            "org_login_required") {
            return MessageErrorKind.Transient;
        }
        return MessageErrorKind.Validation;
    }
}
