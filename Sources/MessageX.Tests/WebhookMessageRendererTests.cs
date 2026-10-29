using System.Text.Json;
using MessageX.Teams;

namespace MessageX.Tests;

public class WebhookMessageRendererTests {
    [Fact]
    public void TextMessageUsesWorkflowAdaptiveAttachment() {
        var json = WebhookMessageRenderer.Render(new TeamsMessageRequest {
            Title = "Build failed", Text = "Pipeline_42"
        });
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        Assert.Equal("message", root.GetProperty("type").GetString());
        Assert.Equal("Build failed", root.GetProperty("summary").GetString());
        var attachment = Assert.Single(root.GetProperty("attachments").EnumerateArray());
        Assert.Equal("application/vnd.microsoft.card.adaptive", attachment.GetProperty("contentType").GetString());
        var card = attachment.GetProperty("content");
        Assert.Equal("AdaptiveCard", card.GetProperty("type").GetString());
        Assert.Equal("Pipeline_42", card.GetProperty("fallbackText").GetString());
        Assert.Equal("Pipeline_42", card.GetProperty("body")[1].GetProperty("text").GetString());
        Assert.False(root.TryGetProperty("sections", out _));
        Assert.False(root.TryGetProperty("potentialAction", out _));
    }

    [Fact]
    public void ReportLinkUsesOpenUrlAndRetainsFallback() {
        var card = new TeamsAdaptiveCard { FallbackText = "Incident inc-42: build failed. https://example.test/report/42" };
        card.Body.Add(new TeamsAdaptiveFactSet { Facts = {
            new TeamsAdaptiveFact { Title = "Incident ID", Value = "inc-42" }
        } });
        card.Actions.Add(new TeamsAdaptiveOpenUrlAction { Title = "Open report", Url = "https://example.test/report/42" });
        var json = WebhookMessageRenderer.Render(new TeamsMessageRequest { AdaptiveCard = card, Summary = "Build failed" });
        using var document = JsonDocument.Parse(json);
        var content = document.RootElement.GetProperty("attachments")[0].GetProperty("content");
        Assert.Contains("inc-42", content.GetProperty("fallbackText").GetString());
        Assert.Equal("Action.OpenUrl", content.GetProperty("actions")[0].GetProperty("type").GetString());
        Assert.Equal("https://example.test/report/42", content.GetProperty("actions")[0].GetProperty("url").GetString());
    }

    [Fact]
    public void NestedOpenUrlActionIsValidated() {
        var card = new TeamsAdaptiveCard();
        card.Body.Add(new TeamsAdaptiveContainer { Items = {
            new TeamsAdaptiveActionSet { Actions = {
                new TeamsAdaptiveOpenUrlAction { Url = "http://example.test/insecure" }
            } }
        } });
        Assert.Throws<ArgumentException>(() => WebhookMessageRenderer.Render(new TeamsMessageRequest { AdaptiveCard = card }));
    }

    [Fact]
    public void EmptyMessageIsRejected() {
        Assert.Throws<ArgumentException>(() => WebhookMessageRenderer.Render(new TeamsMessageRequest()));
    }
}
