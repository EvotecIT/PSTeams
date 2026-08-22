namespace MessageX.Discord;

internal static class DiscordReaction {
    public static string Normalize(string? reaction) {
        var normalized = reaction?.Trim();
        if (string.IsNullOrEmpty(normalized) || normalized!.Length > 100 ||
            normalized.Any(character => char.IsWhiteSpace(character) || char.IsControl(character))) {
            throw InvalidReaction(nameof(reaction));
        }

        var colon = normalized.LastIndexOf(':');
        if (colon >= 0) {
            ValidateCustomEmoji(normalized, colon, nameof(reaction));
        }
        else if (!IsUnicodeEmojiSequence(normalized)) {
            throw InvalidReaction(nameof(reaction));
        }
        return normalized;
    }

    private static void ValidateCustomEmoji(string value, int colon, string parameterName) {
        var name = value.Substring(0, colon);
        if (value.IndexOf(':') != colon || name.Length is < 2 or > 32 ||
            name.Any(character => !(character is >= 'a' and <= 'z' or
                >= 'A' and <= 'Z' or >= '0' and <= '9' or '_')) ||
            !DiscordSnowflake.TryNormalize(value.Substring(colon + 1), out _)) {
            throw new ArgumentException(
                "Discord custom reactions must use the name:id format.",
                parameterName);
        }
    }

    private static bool IsUnicodeEmojiSequence(string value) {
        var hasEmojiBase = false;
        var hasKeycap = false;
        var pendingKeycapBase = false;
        var previousWasJoiner = false;
        var previousBaseWasRegionalIndicator = false;
        var regionalIndicatorCount = 0;

        for (var index = 0; index < value.Length;) {
            int codePoint;
            if (char.IsHighSurrogate(value[index])) {
                if (index + 1 >= value.Length || !char.IsLowSurrogate(value[index + 1])) {
                    return false;
                }
                codePoint = char.ConvertToUtf32(value[index], value[index + 1]);
                index += 2;
            }
            else {
                if (char.IsLowSurrogate(value[index])) {
                    return false;
                }
                codePoint = value[index++];
            }

            if (pendingKeycapBase) {
                if (codePoint is 0xFE0E or 0xFE0F) {
                    continue;
                }
                if (codePoint != 0x20E3) {
                    return false;
                }
                pendingKeycapBase = false;
                hasKeycap = true;
                previousWasJoiner = false;
                continue;
            }

            if (previousWasJoiner && !IsEmojiBase(codePoint)) {
                return false;
            }
            if (IsEmojiComponent(codePoint)) {
                if (!hasEmojiBase) {
                    return false;
                }
                previousWasJoiner = false;
                continue;
            }
            if (IsEmojiBase(codePoint)) {
                var isRegionalIndicator = codePoint is >= 0x1F1E6 and <= 0x1F1FF;
                if (hasKeycap || hasEmojiBase && !previousWasJoiner &&
                    !(previousBaseWasRegionalIndicator && isRegionalIndicator && regionalIndicatorCount == 1)) {
                    return false;
                }
                hasEmojiBase = true;
                previousBaseWasRegionalIndicator = isRegionalIndicator;
                regionalIndicatorCount = isRegionalIndicator ? regionalIndicatorCount + 1 : 0;
                previousWasJoiner = false;
                continue;
            }
            if (codePoint is '#' or '*' or >= '0' and <= '9') {
                if (hasEmojiBase || hasKeycap || previousWasJoiner) {
                    return false;
                }
                pendingKeycapBase = true;
                previousWasJoiner = false;
                continue;
            }
            if (codePoint == 0x200D) {
                if ((!hasEmojiBase && !hasKeycap) || previousWasJoiner) {
                    return false;
                }
                previousWasJoiner = true;
                continue;
            }
            return false;
        }

        return !previousWasJoiner && !pendingKeycapBase && (hasEmojiBase || hasKeycap);
    }

    private static bool IsEmojiBase(int codePoint) {
        var low = 0;
        var high = EmojiRanges.Length - 1;
        while (low <= high) {
            var middle = low + (high - low) / 2;
            var range = EmojiRanges[middle];
            if (codePoint < range.Start) {
                high = middle - 1;
            }
            else if (codePoint > range.End) {
                low = middle + 1;
            }
            else {
                return true;
            }
        }
        return false;
    }

    private static bool IsEmojiComponent(int codePoint) =>
        codePoint is 0xFE0E or 0xFE0F or >= 0x1F3FB and <= 0x1F3FF or
        >= 0xE0020 and <= 0xE007F;

    private static ArgumentException InvalidReaction(string parameterName) => new(
        "A valid Discord Unicode emoji or custom emoji coordinate is required.",
        parameterName);

    // Unicode 17.0 Emoji property, excluding ASCII keycap bases handled by the sequence validator.
    private static readonly EmojiRange[] EmojiRanges = {
        new(0xA9, 0xA9), new(0xAE, 0xAE), new(0x203C, 0x203C), new(0x2049, 0x2049),
        new(0x2122, 0x2122), new(0x2139, 0x2139), new(0x2194, 0x2199), new(0x21A9, 0x21AA),
        new(0x231A, 0x231B), new(0x2328, 0x2328), new(0x23CF, 0x23CF), new(0x23E9, 0x23F3),
        new(0x23F8, 0x23FA), new(0x24C2, 0x24C2), new(0x25AA, 0x25AB), new(0x25B6, 0x25B6),
        new(0x25C0, 0x25C0), new(0x25FB, 0x25FE), new(0x2600, 0x2604), new(0x260E, 0x260E),
        new(0x2611, 0x2611), new(0x2614, 0x2615), new(0x2618, 0x2618), new(0x261D, 0x261D),
        new(0x2620, 0x2620), new(0x2622, 0x2623), new(0x2626, 0x2626), new(0x262A, 0x262A),
        new(0x262E, 0x262F), new(0x2638, 0x263A), new(0x2640, 0x2640), new(0x2642, 0x2642),
        new(0x2648, 0x2653), new(0x265F, 0x2660), new(0x2663, 0x2663), new(0x2665, 0x2666),
        new(0x2668, 0x2668), new(0x267B, 0x267B), new(0x267E, 0x267F), new(0x2692, 0x2697),
        new(0x2699, 0x2699), new(0x269B, 0x269C), new(0x26A0, 0x26A1), new(0x26A7, 0x26A7),
        new(0x26AA, 0x26AB), new(0x26B0, 0x26B1), new(0x26BD, 0x26BE), new(0x26C4, 0x26C5),
        new(0x26C8, 0x26C8), new(0x26CE, 0x26CF), new(0x26D1, 0x26D1), new(0x26D3, 0x26D4),
        new(0x26E9, 0x26EA), new(0x26F0, 0x26F5), new(0x26F7, 0x26FA), new(0x26FD, 0x26FD),
        new(0x2702, 0x2702), new(0x2705, 0x2705), new(0x2708, 0x270D), new(0x270F, 0x270F),
        new(0x2712, 0x2712), new(0x2714, 0x2714), new(0x2716, 0x2716), new(0x271D, 0x271D),
        new(0x2721, 0x2721), new(0x2728, 0x2728), new(0x2733, 0x2734), new(0x2744, 0x2744),
        new(0x2747, 0x2747), new(0x274C, 0x274C), new(0x274E, 0x274E), new(0x2753, 0x2755),
        new(0x2757, 0x2757), new(0x2763, 0x2764), new(0x2795, 0x2797), new(0x27A1, 0x27A1),
        new(0x27B0, 0x27B0), new(0x27BF, 0x27BF), new(0x2934, 0x2935), new(0x2B05, 0x2B07),
        new(0x2B1B, 0x2B1C), new(0x2B50, 0x2B50), new(0x2B55, 0x2B55), new(0x3030, 0x3030),
        new(0x303D, 0x303D), new(0x3297, 0x3297), new(0x3299, 0x3299), new(0x1F004, 0x1F004),
        new(0x1F0CF, 0x1F0CF), new(0x1F170, 0x1F171), new(0x1F17E, 0x1F17F), new(0x1F18E, 0x1F18E),
        new(0x1F191, 0x1F19A), new(0x1F1E6, 0x1F1FF), new(0x1F201, 0x1F202), new(0x1F21A, 0x1F21A),
        new(0x1F22F, 0x1F22F), new(0x1F232, 0x1F23A), new(0x1F250, 0x1F251), new(0x1F300, 0x1F321),
        new(0x1F324, 0x1F393), new(0x1F396, 0x1F397), new(0x1F399, 0x1F39B), new(0x1F39E, 0x1F3F0),
        new(0x1F3F3, 0x1F3F5), new(0x1F3F7, 0x1F4FD), new(0x1F4FF, 0x1F53D), new(0x1F549, 0x1F54E),
        new(0x1F550, 0x1F567), new(0x1F56F, 0x1F570), new(0x1F573, 0x1F57A), new(0x1F587, 0x1F587),
        new(0x1F58A, 0x1F58D), new(0x1F590, 0x1F590), new(0x1F595, 0x1F596), new(0x1F5A4, 0x1F5A5),
        new(0x1F5A8, 0x1F5A8), new(0x1F5B1, 0x1F5B2), new(0x1F5BC, 0x1F5BC), new(0x1F5C2, 0x1F5C4),
        new(0x1F5D1, 0x1F5D3), new(0x1F5DC, 0x1F5DE), new(0x1F5E1, 0x1F5E1), new(0x1F5E3, 0x1F5E3),
        new(0x1F5E8, 0x1F5E8), new(0x1F5EF, 0x1F5EF), new(0x1F5F3, 0x1F5F3), new(0x1F5FA, 0x1F64F),
        new(0x1F680, 0x1F6C5), new(0x1F6CB, 0x1F6D2), new(0x1F6D5, 0x1F6D8), new(0x1F6DC, 0x1F6E5),
        new(0x1F6E9, 0x1F6E9), new(0x1F6EB, 0x1F6EC), new(0x1F6F0, 0x1F6F0), new(0x1F6F3, 0x1F6FC),
        new(0x1F7E0, 0x1F7EB), new(0x1F7F0, 0x1F7F0), new(0x1F90C, 0x1F93A), new(0x1F93C, 0x1F945),
        new(0x1F947, 0x1F9FF), new(0x1FA70, 0x1FA7C), new(0x1FA80, 0x1FA8A), new(0x1FA8E, 0x1FAC6),
        new(0x1FAC8, 0x1FAC8), new(0x1FACD, 0x1FADC), new(0x1FADF, 0x1FAEA), new(0x1FAEF, 0x1FAF8)
    };

    private readonly struct EmojiRange {
        public EmojiRange(int start, int end) {
            Start = start;
            End = end;
        }

        public int Start { get; }

        public int End { get; }
    }
}
