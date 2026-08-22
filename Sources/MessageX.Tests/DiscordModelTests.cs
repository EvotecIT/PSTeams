using System.Text.Json;
using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordModelTests {
    private static readonly Uri WebhookUri = new(
        "https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456");

    [Fact]
    public void WebhookTargetsRejectCredentialExfiltrationUrisAndHideSecret() {
        Assert.Throws<ArgumentException>(() => DiscordMessageTarget.ForIncomingWebhook(
            new Uri("http://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456")));
        Assert.Throws<ArgumentException>(() => DiscordMessageTarget.ForIncomingWebhook(
            new Uri("https://attacker.example/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456")));
        Assert.Throws<ArgumentException>(() => DiscordMessageTarget.ForIncomingWebhook(
            new Uri("https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456?wait=true")));

        var target = DiscordMessageTarget.ForIncomingWebhook(WebhookUri, "123456789012345679");
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz", target.ToString(), StringComparison.Ordinal);
        Assert.Equal("123456789012345679", target.ThreadId);
        Assert.DoesNotContain("WebhookUri", typeof(DiscordMessageTarget).GetProperties()
            .Where(property => property.GetMethod?.IsPublic == true)
            .Select(property => property.Name));
    }

    [Fact]
    public void BotTargetsPreserveChannelThreadAndDirectCoordinates() {
        var channel = DiscordMessageTarget.ForChannel("123456789012345678", "223456789012345678");
        var thread = DiscordMessageTarget.ForThread("323456789012345678", "223456789012345678");
        var direct = DiscordMessageTarget.ForDirectMessage("423456789012345678");

        Assert.Equal(DiscordDeliveryMethod.BotChannel, channel.DeliveryMethod);
        Assert.Null(channel.ThreadId);
        Assert.Equal(
            MessageCapabilities.Send | MessageCapabilities.Reply | MessageCapabilities.UploadFile,
            channel.Capabilities);
        Assert.Equal(DiscordDeliveryMethod.BotThread, thread.DeliveryMethod);
        Assert.Equal(thread.ChannelId, thread.ThreadId);
        Assert.Equal(DiscordDeliveryMethod.BotDirectMessage, direct.DeliveryMethod);
        Assert.Equal("423456789012345678", direct.UserId);
    }

    [Fact]
    public void BotConnectionDoesNotExposeCredential() {
        var connection = DiscordConnection.ForBotToken(
            "discord-super-secret-token-value",
            "523456789012345678");

        Assert.DoesNotContain("secret", connection.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("BotToken", typeof(DiscordConnection).GetProperties()
            .Where(property => property.GetMethod?.IsPublic == true)
            .Select(property => property.Name));
    }

    [Fact]
    public void RendererDefaultsToNoMentionsAndBuildsReplyAndEmbedPayload() {
        var message = new DiscordMessageRequest {
            Content = "Build <@123456789012345678> completed",
            Nonce = "build-42",
            EnforceNonce = true,
            ReplyToMessageId = " 623456789012345678 "
        };
        var embed = new DiscordEmbed {
            Title = "Release",
            Description = "Ready",
            Color = 0x336699,
            Footer = new DiscordEmbedFooter { Text = "MessageX" }
        };
        embed.Fields.Add(new DiscordEmbedField { Name = "Build", Value = "42", Inline = true });
        message.Embeds.Add(embed);

        var json = DiscordJsonSerializer.Serialize(
            message,
            DiscordMessageTarget.ForChannel("123456789012345678"));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Empty(root.GetProperty("allowed_mentions").GetProperty("parse").EnumerateArray());
        Assert.False(root.GetProperty("allowed_mentions").GetProperty("replied_user").GetBoolean());
        Assert.Equal("build-42", root.GetProperty("nonce").GetString());
        Assert.True(root.GetProperty("enforce_nonce").GetBoolean());
        Assert.Equal("623456789012345678", root.GetProperty("message_reference").GetProperty("message_id").GetString());
        Assert.Equal("Release", root.GetProperty("embeds")[0].GetProperty("title").GetString());
        Assert.True(root.GetProperty("embeds")[0].GetProperty("fields")[0].GetProperty("inline").GetBoolean());
    }

    [Fact]
    public void ExplicitMentionPolicyAndWebhookIdentityAreProviderNative() {
        var mentions = new DiscordAllowedMentions { RepliedUser = true };
        mentions.UserIds.Add("123456789012345678");
        var message = new DiscordMessageRequest {
            Content = "Hello",
            AllowedMentions = mentions,
            WebhookUsername = "MessageX",
            WebhookAvatarUrl = new Uri("https://example.com/avatar.png")
        };

        var json = DiscordJsonSerializer.Serialize(message, DiscordMessageTarget.ForIncomingWebhook(WebhookUri));
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("123456789012345678", root.GetProperty("allowed_mentions").GetProperty("users")[0].GetString());
        Assert.Equal("MessageX", root.GetProperty("username").GetString());
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            message,
            DiscordMessageTarget.ForChannel("123456789012345678")));
    }

    [Fact]
    public void RendererRejectsProviderLimitsAndWebhookReplies() {
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            new DiscordMessageRequest(),
            DiscordMessageTarget.ForChannel("123456789012345678")));
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            new DiscordMessageRequest { Content = new string('x', 2001) },
            DiscordMessageTarget.ForChannel("123456789012345678")));
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            new DiscordMessageRequest { Content = "reply", ReplyToMessageId = "623456789012345678" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri)));
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            new DiscordMessageRequest { Content = "webhook", Nonce = "not-supported" },
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri)));
    }

    [Fact]
    public void AttachmentJsonContainsMetadataButNeverFileBytes() {
        var message = new DiscordMessageRequest { Content = "report" };
        message.Attachments.Add(DiscordAttachment.FromBytes(
            "report.txt",
            System.Text.Encoding.UTF8.GetBytes("highly-sensitive-content"),
            "Build report",
            "text/plain",
            isSpoiler: true));
        message.Embeds.Add(new DiscordEmbed {
            Image = new DiscordEmbedMedia { Url = new Uri("attachment://report.txt") }
        });

        var json = DiscordJsonSerializer.Serialize(message, DiscordMessageTarget.ForChannel("123456789012345678"));
        using var document = JsonDocument.Parse(json);
        var attachment = document.RootElement.GetProperty("attachments")[0];

        Assert.Contains("SPOILER_report.txt", json, StringComparison.Ordinal);
        Assert.Equal("SPOILER_report.txt", attachment.GetProperty("filename").GetString());
        Assert.False(attachment.TryGetProperty("is_spoiler", out _));
        Assert.Equal("attachment://SPOILER_report.txt", document.RootElement.GetProperty("embeds")[0]
            .GetProperty("image").GetProperty("url").GetString());
        Assert.DoesNotContain("highly-sensitive-content", json, StringComparison.Ordinal);
    }

    [Fact]
    public void AttachmentFilesAreRejectedBeforeOversizedContentIsRead() {
        var path = Path.GetTempFileName();
        try {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Write, FileShare.None)) {
                stream.SetLength(DiscordMessageValidator.MaximumAttachmentBytes + 1L);
            }

            Assert.Throws<ArgumentException>(() => DiscordAttachment.FromFile(path));
        } finally {
            File.Delete(path);
        }
    }

    [Fact]
    public void RendererRejectsAmbiguousOrMissingAttachmentReferences() {
        var duplicate = new DiscordMessageRequest { Content = "report" };
        duplicate.Attachments.Add(DiscordAttachment.FromBytes("report.txt", new byte[] { 1 }, isSpoiler: true));
        duplicate.Attachments.Add(DiscordAttachment.FromBytes("SPOILER_report.txt", new byte[] { 2 }));
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            duplicate,
            DiscordMessageTarget.ForChannel("123456789012345678")));

        var missing = new DiscordMessageRequest { Content = "report" };
        missing.Embeds.Add(new DiscordEmbed {
            Image = new DiscordEmbedMedia { Url = new Uri("attachment://missing.png") }
        });
        Assert.Throws<ArgumentException>(() => DiscordJsonSerializer.Serialize(
            missing,
            DiscordMessageTarget.ForChannel("123456789012345678")));
    }
}
