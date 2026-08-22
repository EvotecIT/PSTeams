using System.Globalization;
using System.Net.Http;
using System.Text.Json;

namespace MessageX.Discord;

internal static class DiscordHttpResponseSupport {
    public static DiscordDeliveryResult CreateResult(
        HttpResponseMessage response,
        string responseBody,
        DiscordMessageTarget target,
        DiscordDeliveryMethod deliveryMethod) {
        var parsed = Parse(responseBody);
        var statusCode = (int)response.StatusCode;
        var messageId = string.Empty;
        var channelId = string.Empty;
        var accepted = response.IsSuccessStatusCode && parsed.IsValid &&
            DiscordSnowflake.TryNormalize(parsed.MessageId, out messageId) &&
            DiscordSnowflake.TryNormalize(parsed.ChannelId, out channelId);
        var result = new DiscordDeliveryResult {
            DeliveryMethod = deliveryMethod,
            Target = target.SafeLabel(),
            IsSuccess = accepted,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ProviderCode = accepted ? null : parsed.Code ?? "invalid_response",
            CorrelationId = ReadHeader(response, "cf-ray"),
            RetryAfter = ReadRetryAfter(response, parsed.RetryAfterSeconds),
            RateLimitBucket = NormalizeDiagnosticToken(ReadHeader(response, "x-ratelimit-bucket")),
            RateLimitScope = NormalizeDiagnosticToken(ReadHeader(response, "x-ratelimit-scope")),
            IsGlobalRateLimit = parsed.Global || ReadBooleanHeader(response, "x-ratelimit-global")
        };

        if (accepted) {
            result.Reference = new MessageReference(MessageProviders.Discord, messageId) {
                ScopeId = target.GuildId,
                ConversationId = channelId,
                ThreadId = target.ThreadId,
                Timestamp = parsed.Timestamp,
                CorrelationId = result.CorrelationId,
                Capabilities = deliveryMethod == DiscordDeliveryMethod.IncomingWebhook
                    ? WebhookMessageCapabilities
                    : BotMessageCapabilities
            };
            return result;
        }

        result.ErrorKind = response.IsSuccessStatusCode
            ? MessageErrorKind.Transient
            : Classify(statusCode, result.IsGlobalRateLimit);
        result.ErrorMessage = parsed.Message is null
            ? response.IsSuccessStatusCode
                ? "Discord returned an invalid message response."
                : $"Discord returned HTTP status {statusCode}."
            : $"Discord rejected the message with '{result.ProviderCode}'.";
        return result;
    }

    public static DiscordDeliveryResult CreateStatusResult(
        HttpResponseMessage response,
        string responseBody,
        DiscordMessageTarget target,
        DiscordDeliveryMethod deliveryMethod,
        MessageReference reference,
        MessageCapabilities successCapabilities) {
        var statusCode = (int)response.StatusCode;
        var parsed = Parse(responseBody);
        var result = new DiscordDeliveryResult {
            DeliveryMethod = deliveryMethod,
            Target = target.SafeLabel(),
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = statusCode,
            ResponseBody = responseBody,
            ProviderCode = response.IsSuccessStatusCode ? null : parsed.Code,
            CorrelationId = ReadHeader(response, "cf-ray"),
            RetryAfter = ReadRetryAfter(response, parsed.RetryAfterSeconds),
            RateLimitBucket = NormalizeDiagnosticToken(ReadHeader(response, "x-ratelimit-bucket")),
            RateLimitScope = NormalizeDiagnosticToken(ReadHeader(response, "x-ratelimit-scope")),
            IsGlobalRateLimit = parsed.Global || ReadBooleanHeader(response, "x-ratelimit-global")
        };
        if (result.IsSuccess) {
            result.Reference = CloneReference(reference, successCapabilities, result.CorrelationId);
            return result;
        }
        result.ErrorKind = Classify(statusCode, result.IsGlobalRateLimit);
        result.ProviderCode ??= "http_" + statusCode.ToString(CultureInfo.InvariantCulture);
        result.ErrorMessage = $"Discord rejected the operation with '{result.ProviderCode}'.";
        return result;
    }

    public static MessageReference CloneReference(
        MessageReference source,
        MessageCapabilities capabilities,
        string? correlationId) {
        return new MessageReference(MessageProviders.Discord, source.MessageId) {
            InstallationId = source.InstallationId,
            ScopeId = source.ScopeId,
            ConversationId = source.ConversationId,
            ThreadId = source.ThreadId,
            Timestamp = source.Timestamp,
            CorrelationId = correlationId ?? source.CorrelationId,
            Capabilities = capabilities
        };
    }

    public static DiscordDeliveryResult RequireMatchingCoordinates(
        DiscordDeliveryResult result,
        MessageReference source) {
        if (!result.IsSuccess || result.Reference is null) {
            return result;
        }
        if (string.Equals(result.Reference.MessageId, source.MessageId, StringComparison.Ordinal) &&
            string.Equals(result.Reference.ConversationId, source.ConversationId, StringComparison.Ordinal)) {
            result.Reference.InstallationId = source.InstallationId;
            return result;
        }

        result.IsSuccess = false;
        result.Reference = null;
        result.ProviderCode = "invalid_response";
        result.ErrorKind = MessageErrorKind.Transient;
        result.ErrorMessage = "Discord returned mismatched message coordinates.";
        return result;
    }

    public static MessageErrorKind Classify(int statusCode, bool globalRateLimit = false) {
        if (statusCode == 429 || globalRateLimit) {
            return MessageErrorKind.RateLimited;
        }
        if (statusCode == 401) {
            return MessageErrorKind.Authentication;
        }
        if (statusCode == 403) {
            return MessageErrorKind.Authorization;
        }
        if (statusCode == 404) {
            return MessageErrorKind.NotFound;
        }
        if (statusCode == 408 || statusCode >= 500) {
            return MessageErrorKind.Transient;
        }
        return MessageErrorKind.Validation;
    }

    public static TimeSpan? ReadRetryAfter(HttpResponseMessage response, double? bodySeconds = null) {
        if (response.Headers.RetryAfter?.Delta is not null) {
            return response.Headers.RetryAfter.Delta;
        }
        if (response.Headers.RetryAfter?.Date is not null) {
            var delay = response.Headers.RetryAfter.Date.Value - DateTimeOffset.UtcNow;
            return delay > TimeSpan.Zero ? delay : TimeSpan.Zero;
        }
        if (TryReadSeconds(response, "x-ratelimit-reset-after", out var headerSeconds)) {
            return TimeSpan.FromSeconds(headerSeconds);
        }
        return bodySeconds is >= 0 ? TimeSpan.FromSeconds(bodySeconds.Value) : null;
    }

    public static string? ReadHeader(HttpResponseMessage response, string name) {
        return response.Headers.TryGetValues(name, out var values) ? values.FirstOrDefault() : null;
    }

    public static string? NormalizeDiagnosticToken(string? value) {
        var candidate = value?.Trim();
        if (string.IsNullOrEmpty(candidate) || candidate!.Length > 128) {
            return null;
        }
        foreach (var character in candidate) {
            var isAsciiLetterOrDigit = character is >= 'a' and <= 'z' or
                >= 'A' and <= 'Z' or >= '0' and <= '9';
            if (!isAsciiLetterOrDigit && character is not '-' and not '_' and not '.') {
                return null;
            }
        }
        return candidate;
    }

    private static bool ReadBooleanHeader(HttpResponseMessage response, string name) {
        return bool.TryParse(ReadHeader(response, name), out var value) && value;
    }

    private static bool TryReadSeconds(HttpResponseMessage response, string name, out double seconds) {
        return double.TryParse(
            ReadHeader(response, name),
            NumberStyles.AllowDecimalPoint,
            CultureInfo.InvariantCulture,
            out seconds) && seconds >= 0;
    }

    private static ParsedResponse Parse(string responseBody) {
        try {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object) {
                return ParsedResponse.Invalid;
            }
            return new ParsedResponse {
                IsValid = true,
                MessageId = ReadString(root, "id"),
                ChannelId = ReadString(root, "channel_id"),
                Message = ReadString(root, "message"),
                Code = ReadCode(root),
                RetryAfterSeconds = ReadNumber(root, "retry_after"),
                Global = ReadBoolean(root, "global"),
                Timestamp = ReadTimestamp(root, "timestamp")
            };
        }
        catch (JsonException) {
            return ParsedResponse.Invalid;
        }
    }

    private static string? ReadString(JsonElement root, string name) {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
    }

    private static string? ReadCode(JsonElement root) {
        if (!root.TryGetProperty("code", out var value)) {
            return null;
        }
        return value.ValueKind switch {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null
        };
    }

    private static double? ReadNumber(JsonElement root, string name) {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number &&
            value.TryGetDouble(out var number) && number >= 0
            ? number
            : null;
    }

    private static bool ReadBoolean(JsonElement root, string name) {
        return root.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.True;
    }

    private static DateTimeOffset? ReadTimestamp(JsonElement root, string name) {
        return DateTimeOffset.TryParse(
            ReadString(root, name),
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal,
            out var value)
            ? value
            : null;
    }

    private sealed class ParsedResponse {
        public static ParsedResponse Invalid { get; } = new();
        public bool IsValid { get; set; }
        public string? MessageId { get; set; }
        public string? ChannelId { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
        public double? RetryAfterSeconds { get; set; }
        public bool Global { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }

    internal const MessageCapabilities WebhookMessageCapabilities =
        MessageCapabilities.Read | MessageCapabilities.Update | MessageCapabilities.Delete;

    internal const MessageCapabilities BotMessageCapabilities =
        MessageCapabilities.Reply | MessageCapabilities.Update |
        MessageCapabilities.Delete | MessageCapabilities.React | MessageCapabilities.Read;
}
