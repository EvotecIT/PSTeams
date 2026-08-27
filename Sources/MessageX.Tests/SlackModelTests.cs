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

        Assert.Equal(MessageCapabilities.Send | MessageCapabilities.Reply, target.Capabilities);
        Assert.DoesNotContain("secret", target.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ConversationTargetsAcceptProviderIdsAndRejectDisplayNames() {
        var target = SlackMessageTarget.ForConversation("C0123456789", "Release alerts");

        Assert.Equal("C0123456789", target.ConversationId);
        Assert.Equal(
            MessageCapabilities.Send |
            MessageCapabilities.Reply |
            MessageCapabilities.Update |
            MessageCapabilities.Delete |
            MessageCapabilities.React |
            MessageCapabilities.UploadFile,
            target.Capabilities);
        foreach (var providerId in new[] {
            "C0123456789",
            "G0123456789",
            "D0123456789",
            "U0123456789",
            "W0123456789",
            "Cprovider-evolved-α"
        }) {
            Assert.Equal(providerId, SlackMessageTarget.ForConversation(providerId).ConversationId);
        }
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("release alerts"));
        Assert.Throws<ArgumentException>(() => SlackMessageTarget.ForConversation("C release alerts"));
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
        Assert.Equal(
            MessageCapabilities.Send |
            MessageCapabilities.Reply |
            MessageCapabilities.Update |
            MessageCapabilities.Delete |
            MessageCapabilities.React |
            MessageCapabilities.UploadFile |
            MessageCapabilities.ResolveConversation,
            connection.Capabilities);
        Assert.Throws<ArgumentException>(() => SlackConnection.ForBotToken(
            "xoxb-secret-token",
            new Uri("https://attacker.example/api/")));
        Assert.Throws<ArgumentException>(() => SlackConnection.ForBotToken(
            "xoxb-secret-token",
            new Uri("https://slack.com/redirect/")));
    }

    [Theory]
    [InlineData("xoxb-long-lived-bot-token")]
    [InlineData("xoxe.xoxb-rotating-bot-access-token")]
    public void BotConnectionAcceptsCurrentBotAccessTokenForms(string token) {
        var connection = SlackConnection.ForBotToken(token);

        Assert.DoesNotContain(token, connection.ToString(), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("xoxe-refresh-token")]
    [InlineData("xoxe.xoxp-rotating-user-access-token")]
    [InlineData("xoxp-user-token")]
    [InlineData("xapp-app-token")]
    public void BotConnectionRejectsNonBotSlackTokens(string token) {
        Assert.Throws<ArgumentException>(() => SlackConnection.ForBotToken(token));
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
    public void RendererSupportsHeadersContextButtonsAndAccessories() {
        var message = new SlackMessageRequest { Text = "Build approval" };
        message.Blocks.Add(new SlackHeaderBlock { Text = SlackTextObject.Plain("Build approval") });
        message.Blocks.Add(new SlackSectionBlock {
            Text = SlackTextObject.Markdown("Build *42* is ready"),
            Accessory = new SlackButtonElement {
                Text = SlackTextObject.Plain("Open"),
                ActionId = "open-build",
                Url = new Uri("https://example.com/build/42")
            }
        });
        message.Blocks.Add(new SlackActionsBlock {
            Elements = {
                new SlackButtonElement {
                    Text = SlackTextObject.Plain("Approve"),
                    ActionId = "approve-build",
                    Value = "42",
                    Style = SlackButtonStyle.Primary
                },
                new SlackButtonElement {
                    Text = SlackTextObject.Plain("Reject"),
                    ActionId = "reject-build",
                    Value = "42",
                    Style = SlackButtonStyle.Danger
                }
            }
        });
        message.Blocks.Add(new SlackContextBlock {
            Elements = {
                SlackTextObject.Markdown("Requested by *CI*")
            }
        });

        var json = SlackJsonSerializer.Serialize(
            message,
            SlackMessageTarget.ForConversation("C0123456789"));
        using var document = JsonDocument.Parse(json);
        var blocks = document.RootElement.GetProperty("blocks");

        Assert.Equal("header", blocks[0].GetProperty("type").GetString());
        Assert.Equal("button", blocks[1].GetProperty("accessory").GetProperty("type").GetString());
        Assert.Equal("primary", blocks[2].GetProperty("elements")[0].GetProperty("style").GetString());
        Assert.Equal("danger", blocks[2].GetProperty("elements")[1].GetProperty("style").GetString());
        Assert.Equal("mrkdwn", blocks[3].GetProperty("elements")[0].GetProperty("type").GetString());
    }

    [Fact]
    public void MessageRendererRejectsModalInputBlocks() {
        var message = new SlackMessageRequest { Text = "fallback" };
        message.Blocks.Add(new SlackInputBlock {
            Label = SlackTextObject.Plain("Reason"),
            Element = new SlackPlainTextInputElement { ActionId = "reason" }
        });

        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(
            message,
            SlackMessageTarget.ForConversation("C0123456789")));
    }

    [Fact]
    public void ModalRendererRequiresSubmitForInputsAndRejectsButtonsAsInputElements() {
        var missingSubmit = new SlackModalView {
            CallbackId = "approval",
            Title = SlackTextObject.Plain("Approval"),
            Blocks = {
                new SlackInputBlock {
                    Label = SlackTextObject.Plain("Reason"),
                    Element = new SlackPlainTextInputElement { ActionId = "reason" }
                }
            }
        };
        Assert.Throws<ArgumentException>(() => SlackModalRenderer.RenderOpen("trigger", missingSubmit));

        var buttonInput = new SlackModalView {
            CallbackId = "approval",
            Title = SlackTextObject.Plain("Approval"),
            Submit = SlackTextObject.Plain("Submit"),
            Blocks = {
                new SlackInputBlock {
                    Label = SlackTextObject.Plain("Decision"),
                    Element = new SlackButtonElement {
                        Text = SlackTextObject.Plain("Approve"),
                        ActionId = "approve"
                    }
                }
            }
        };
        Assert.Throws<ArgumentException>(() => SlackModalRenderer.RenderOpen("trigger", buttonInput));
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

        var invalidStyle = new SlackMessageRequest { Text = "fallback" };
        invalidStyle.Blocks.Add(new SlackSectionBlock {
            Text = new SlackTextObject { Style = (SlackTextStyle)42, Text = "unsupported" }
        });
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(invalidStyle, target));

        var tooManyBlocks = new SlackMessageRequest { Text = "fallback" };
        for (var index = 0; index < 51; index++) {
            tooManyBlocks.Blocks.Add(new SlackDividerBlock());
        }
        Assert.Throws<ArgumentException>(() => SlackJsonSerializer.Serialize(tooManyBlocks, target));
    }
}
