using System.Text.Json;
using MessageX.Slack;

namespace MessageX.Tests;

public sealed class SlackModelTests {
    [Fact]
    public void IncomingWebhookTargetRejectsInsecureUrisAndHidesSecretFromLabels() {
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForIncomingWebhook(
            new Uri("http://hooks.slack.com/services/secret")));

        var target = SlackMessageTarget.ForIncomingWebhook(
            new Uri("https://hooks.slack.com/services/T000/B000/secret"));

        Assert.Equal(MessageCapabilities.Send, target.Capabilities);
        Assert.DoesNotContain("secret", target.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConversationTargetsAcceptProviderIdsAndRejectDisplayNames() {
        var target = SlackMessageTarget.ForConversation("C0123456789", "Release alerts");

        Assert.Equal("C0123456789", target.ConversationId);
        Assert.Equal(MessageCapabilities.Send | MessageCapabilities.Reply, target.Capabilities);
        foreach (var providerId in new[] {
            "C0123456789",
            "G0123456789",
            "D0123456789",
            "U0123456789",
            "W0123456789"
        }) {
            Assert.Equal(providerId, SlackMessageTarget.ForConversation(providerId).ConversationId);
        }
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("release alerts"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("general"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("release-alerts"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("#release-alerts"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("T0123456789"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("c0123456789"));
    }

    [Fact]
    public void BotConnectionDoesNotExposeTokenAndRejectsCredentialExfiltrationEndpoints() {
        var connection = SlackConnection.ForBotToken("xoxb-secret-token", workspaceId: "T0123");

        Assert.DoesNotContain("secret", connection.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal(MessageCapabilities.Send | MessageCapabilities.Reply, connection.Capabilities);
        Assert.Throws<ArgumentException>(() => SlackConnection.ForBotToken(
            "xoxb-secret-token",
            new Uri("https://attacker.example/api/")));
        Assert.Throws<ArgumentException>(() => SlackConnection.ForBotToken(
            "xoxb-secret-token",
            new Uri("https://slack.com/redirect/")));
    }

    [Fact]
    public void RendererProducesProviderNativeBlockKitAndThreadPayload() {
        var message = new SlackMessageRequest {
            Text = "Build failed",
            ThreadTimestamp = "1712345678.123456",
            ReplyBroadcast = true,
            UnfurlLinks = false
        };
        var section = new SlackSectionBlock {
            BlockId = "summary",
            Text = SlackTextObject.Markdown("*Build failed*")
        };
        section.Fields.Add(SlackTextObject.Plain("Pipeline 42"));
        message.Blocks.Add(section);
        message.Blocks.Add(new SlackDividerBlock());
        var target = SlackMessageTarget.ForConversation("C0123456789");

        var json = SlackJsonSerializer.Serialize(message, target);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("C0123456789", root.GetProperty("channel").GetString());
        Assert.Equal("1712345678.123456", root.GetProperty("thread_ts").GetString());
        Assert.True(root.GetProperty("reply_broadcast").GetBoolean());
        Assert.False(root.GetProperty("unfurl_links").GetBoolean());
        Assert.Equal("section", root.GetProperty("blocks")[0].GetProperty("type").GetString());
        Assert.Equal("mrkdwn", root.GetProperty("blocks")[0].GetProperty("text").GetProperty("type").GetString());
        Assert.False(root.GetProperty("blocks")[0].GetProperty("text").TryGetProperty("emoji", out _));
        Assert.False(root.GetProperty("blocks")[0].GetProperty("text").TryGetProperty("verbatim", out _));
        Assert.False(root.GetProperty("blocks")[1].TryGetProperty("block_id", out _));
        Assert.False(json.Contains("WebhookUri", StringComparison.Ordinal));
    }

    [Fact]
    public void WebhookCredentialIsNotAVisibleTargetProperty() {
        var publicProperties = typeof(SlackMessageTarget).GetProperties()
            .Where(property => property.GetMethod?.IsPublic == true)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("WebhookUri", publicProperties);
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForIncomingWebhook(
            new Uri("relative/webhook", UriKind.Relative)));
    }

    [Fact]
    public void RendererRejectsInvalidSlackContracts() {
        var target = SlackMessageTarget.ForConversation("C0123456789");
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(new SlackMessageRequest(), target));
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(
            new SlackMessageRequest { Text = "hello", ReplyBroadcast = true },
            target));
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(
            new SlackMessageRequest { Text = "hello", ThreadTimestamp = "1712345678" },
            target));

        var tooManyBlocks = new SlackMessageRequest { Text = "fallback" };
        for (var index = 0; index < 51; index++) {
            tooManyBlocks.Blocks.Add(new SlackDividerBlock());
        }
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(tooManyBlocks, target));
    }
}
