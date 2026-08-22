using System.Globalization;

namespace MessageX.Slack;

internal static class SlackMessageValidator {
    public static void Validate(SlackMessageRequest message) {
        if (message is null) {
            throw new ArgumentNullException(nameof(message));
        }
        if (string.IsNullOrWhiteSpace(message.Text) && message.Blocks.Count == 0) {
            throw new ArgumentException("A Slack message requires text, Block Kit blocks, or both.", nameof(message));
        }
        if (message.Text?.Length > 40000) {
            throw new ArgumentException("Slack message text cannot exceed 40000 characters.", nameof(message));
        }
        if (message.Blocks.Count > 50) {
            throw new ArgumentException("Slack messages cannot contain more than 50 Block Kit blocks.", nameof(message));
        }
        if (!string.IsNullOrWhiteSpace(message.ThreadTimestamp) && !IsTimestamp(message.ThreadTimestamp!)) {
            throw new ArgumentException("Slack thread timestamps must use Slack's numeric timestamp format.", nameof(message));
        }
        if (message.ReplyBroadcast && string.IsNullOrWhiteSpace(message.ThreadTimestamp)) {
            throw new ArgumentException("Slack reply broadcasts require a parent thread timestamp.", nameof(message));
        }

        foreach (var block in message.Blocks) {
            ValidateBlock(block);
        }
    }

    public static DateTimeOffset? ParseTimestamp(string? timestamp) {
        if (!IsTimestamp(timestamp) ||
            !decimal.TryParse(timestamp, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var seconds)) {
            return null;
        }

        try {
            var ticks = checked((long)(seconds * TimeSpan.TicksPerSecond));
            return DateTimeOffset.FromUnixTimeSeconds(0).AddTicks(ticks);
        } catch (Exception exception) when (exception is ArgumentOutOfRangeException or OverflowException) {
            return null;
        }
    }

    private static void ValidateBlock(SlackBlock block) {
        if (block is null) {
            throw new ArgumentException("Slack Block Kit collections cannot contain null blocks.", nameof(block));
        }
        if (block.BlockId?.Length > 255) {
            throw new ArgumentException("Slack block identifiers cannot exceed 255 characters.", nameof(block));
        }

        if (block is SlackSectionBlock section) {
            if (section.Text is null && section.Fields.Count == 0) {
                throw new ArgumentException("A Slack section requires text or fields.", nameof(block));
            }
            if (section.Text is not null) {
                ValidateText(section.Text, 3000, "Slack section text");
            }
            if (section.Fields.Count > 10) {
                throw new ArgumentException("A Slack section cannot contain more than 10 fields.", nameof(block));
            }
            foreach (var field in section.Fields) {
                ValidateText(field, 2000, "Slack section field text");
            }
        }
    }

    private static void ValidateText(SlackTextObject text, int maximumLength, string label) {
        if (text.Style is not SlackTextStyle.PlainText and not SlackTextStyle.Markdown) {
            throw new ArgumentException("Slack text objects must use a supported text style.", nameof(text));
        }
        if (string.IsNullOrEmpty(text.Text) || text.Text.Length > maximumLength) {
            throw new ArgumentException($"{label} must contain between 1 and {maximumLength} characters.", nameof(text));
        }
        if (text.Style == SlackTextStyle.Markdown && text.Emoji is not null) {
            throw new ArgumentException("Slack mrkdwn text cannot set the plain-text emoji option.", nameof(text));
        }
        if (text.Style == SlackTextStyle.PlainText && text.Verbatim is not null) {
            throw new ArgumentException("Slack plain text cannot set the mrkdwn verbatim option.", nameof(text));
        }
    }

    private static bool IsTimestamp(string? timestamp) {
        if (string.IsNullOrWhiteSpace(timestamp) || timestamp!.Length > 32) {
            return false;
        }

        var decimalPointSeen = false;
        foreach (var character in timestamp) {
            if (character == '.' && !decimalPointSeen) {
                decimalPointSeen = true;
                continue;
            }
            if (character is < '0' or > '9') {
                return false;
            }
        }
        return decimalPointSeen && timestamp[0] != '.' && timestamp[timestamp.Length - 1] != '.';
    }
}
