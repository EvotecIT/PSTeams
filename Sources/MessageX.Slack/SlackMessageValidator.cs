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
            ValidateBlock(block, allowInput: false);
        }
    }

    internal static void ValidateModal(SlackModalView view) {
        if (view is null) {
            throw new ArgumentNullException(nameof(view));
        }
        ValidateIdentifier(view.CallbackId, 255, "Slack modal callback identifiers");
        ValidatePlainText(view.Title, 24, "Slack modal titles");
        if (view.Submit is not null) {
            ValidatePlainText(view.Submit, 24, "Slack modal submit labels");
        }
        if (view.Close is not null) {
            ValidatePlainText(view.Close, 24, "Slack modal close labels");
        }
        if (view.Blocks.Count is < 1 or > 100) {
            throw new ArgumentException("Slack modal views require one to one hundred blocks.", nameof(view));
        }
        if (view.Submit is null && view.Blocks.Any(static block => block is SlackInputBlock)) {
            throw new ArgumentException("Slack modal views containing input blocks require a submit label.", nameof(view));
        }
        foreach (var block in view.Blocks) {
            ValidateBlock(block, allowInput: true);
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

    private static void ValidateBlock(SlackBlock block, bool allowInput) {
        if (block is null) {
            throw new ArgumentException("Slack Block Kit collections cannot contain null blocks.", nameof(block));
        }
        if (block.BlockId?.Length > 255 || block.BlockId?.Any(char.IsControl) == true) {
            throw new ArgumentException("Slack block identifiers cannot exceed 255 non-control characters.", nameof(block));
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
            if (section.Accessory is not null) {
                ValidateElement(section.Accessory, allowTextInput: false);
            }
            return;
        }
        if (block is SlackActionsBlock actions) {
            if (actions.Elements.Count is < 1 or > 25) {
                throw new ArgumentException("Slack action blocks require one to twenty-five elements.", nameof(block));
            }
            foreach (var element in actions.Elements) {
                ValidateElement(element, allowTextInput: false);
            }
            return;
        }
        if (block is SlackHeaderBlock header) {
            ValidatePlainText(header.Text, 150, "Slack header text");
            return;
        }
        if (block is SlackContextBlock context) {
            if (context.Elements.Count is < 1 or > 10) {
                throw new ArgumentException("Slack context blocks require one to ten elements.", nameof(block));
            }
            foreach (var text in context.Elements) {
                ValidateText(text, 2000, "Slack context text");
            }
            return;
        }
        if (block is SlackInputBlock input) {
            if (!allowInput) {
                throw new ArgumentException("Slack input blocks are supported only inside modal views.", nameof(block));
            }
            ValidatePlainText(input.Label, 2000, "Slack input labels");
            if (input.Hint is not null) {
                ValidatePlainText(input.Hint, 2000, "Slack input hints");
            }
            ValidateElement(input.Element, allowTextInput: true);
            return;
        }
        if (block is not SlackDividerBlock) {
            throw new ArgumentException($"Unsupported Slack block type '{block.GetType().Name}'.", nameof(block));
        }
    }

    private static void ValidateElement(SlackBlockElement element, bool allowTextInput) {
        if (element is null) {
            throw new ArgumentException("Slack element collections cannot contain null values.", nameof(element));
        }
        if (element is SlackButtonElement button) {
            if (allowTextInput) {
                throw new ArgumentException("Slack input blocks require an input-capable element.", nameof(element));
            }
            ValidatePlainText(button.Text, 75, "Slack button text");
            ValidateIdentifier(button.ActionId, 255, "Slack button action identifiers");
            if (button.Value?.Length > 2000 || button.Value?.Any(char.IsControl) == true ||
                button.AccessibilityLabel?.Length > 75 || button.AccessibilityLabel?.Any(char.IsControl) == true) {
                throw new ArgumentException("Slack button value or accessibility label exceeds provider limits.", nameof(element));
            }
            if (button.Url is not null && (!button.Url.IsAbsoluteUri ||
                !string.Equals(button.Url.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))) {
                throw new ArgumentException("Slack button URLs must be absolute HTTPS URIs.", nameof(element));
            }
            if (button.Style is < SlackButtonStyle.Default or > SlackButtonStyle.Danger) {
                throw new ArgumentException("Slack button style is invalid.", nameof(element));
            }
            return;
        }
        if (element is SlackPlainTextInputElement input && allowTextInput) {
            ValidateIdentifier(input.ActionId, 255, "Slack input action identifiers");
            if (input.InitialValue?.Length > 3000 || input.MinimumLength is < 0 or > 3000 ||
                input.MaximumLength is < 1 or > 3000 || input.MinimumLength is not null &&
                input.MaximumLength is not null && input.MinimumLength > input.MaximumLength) {
                throw new ArgumentException("Slack plain-text input length settings are invalid.", nameof(element));
            }
            if (input.Placeholder is not null) {
                ValidatePlainText(input.Placeholder, 150, "Slack input placeholders");
            }
            return;
        }
        throw new ArgumentException($"Unsupported Slack element type '{element.GetType().Name}'.", nameof(element));
    }

    private static void ValidatePlainText(SlackTextObject text, int maximumLength, string label) {
        if (text is null || text.Style != SlackTextStyle.PlainText) {
            throw new ArgumentException($"{label} must use a plain-text object.", nameof(text));
        }
        ValidateText(text, maximumLength, label);
    }

    private static void ValidateIdentifier(string? value, int maximumLength, string label) {
        if (string.IsNullOrWhiteSpace(value) || value!.Length > maximumLength || value.Any(char.IsControl)) {
            throw new ArgumentException($"{label} must contain bounded non-control text.", nameof(value));
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
