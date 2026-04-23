using System.Text.Json;
using TeamsX;

namespace TeamsX.Tests;

public class GraphMessageRendererTests {
    [Fact]
    public void RenderHtmlMessageIncludesTitleTextSectionsAndLinks() {
        var request = new TeamsMessageRequest {
            Title = "Build failed",
            Text = "Pipeline 42 stopped."
        };
        request.Sections.Add(new TeamsMessageSection {
            ActivityTitle = "Release pipeline",
            ActivitySubtitle = "Run 42",
            ActivityText = "Deployment stopped after test failures."
        });
        request.Sections[0].Facts.Add(new TeamsMessageFact { Name = "Status", Value = "Failed" });
        request.Sections[0].Buttons.Add(new TeamsMessageButton {
            Name = "Open build",
            Link = "https://example.test/build/42",
            ButtonType = TeamsMessageButtonType.OpenUri
        });

        var json = GraphMessageRenderer.Render(request, TeamsDeliveryMethod.GraphChannelMessage);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        Assert.Equal("Build failed", root.GetProperty("subject").GetString());
        var body = root.GetProperty("body");
        Assert.Equal("html", body.GetProperty("contentType").GetString());

        var html = body.GetProperty("content").GetString();
        Assert.Contains("<strong>Build failed</strong>", html);
        Assert.Contains("Pipeline 42 stopped.", html);
        Assert.Contains("Release pipeline", html);
        Assert.Contains("Status", html);
        Assert.Contains("https://example.test/build/42", html);
    }

    [Fact]
    public void RenderAdaptiveCardMessageCreatesGraphAttachmentPayload() {
        var request = new TeamsMessageRequest {
            Summary = "Build summary",
            Text = "Pipeline 42 stopped.",
            AdaptiveCard = new TeamsAdaptiveCard()
        };
        request.AdaptiveCard.Body.Add(new TeamsAdaptiveTextBlock {
            Text = "Build failed"
        });
        request.AdaptiveCard.Actions.Add(new TeamsAdaptiveOpenUrlAction {
            Title = "Open build",
            Url = "https://example.test/build/42"
        });

        var json = GraphMessageRenderer.Render(request, TeamsDeliveryMethod.GraphChatMessage);
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;

        if (root.TryGetProperty("subject", out var subject)) {
            Assert.Equal(JsonValueKind.Null, subject.ValueKind);
        }
        var body = root.GetProperty("body");
        var content = body.GetProperty("content").GetString();
        Assert.Contains("<attachment id=\"", content);

        var attachment = root.GetProperty("attachments")[0];
        Assert.Equal("application/vnd.microsoft.card.adaptive", attachment.GetProperty("contentType").GetString());
        Assert.Contains("\"type\":\"AdaptiveCard\"", attachment.GetProperty("content").GetString());
    }

    [Fact]
    public void RenderAdaptiveCardMessageRejectsUnsupportedActions() {
        var request = new TeamsMessageRequest {
            AdaptiveCard = new TeamsAdaptiveCard()
        };
        request.AdaptiveCard.Actions.Add(new TeamsAdaptiveToggleVisibilityAction {
            Title = "Toggle details"
        });

        var action = () => GraphMessageRenderer.Render(request, TeamsDeliveryMethod.GraphChatMessage);

        Assert.Throws<NotSupportedException>(action);
    }

    [Fact]
    public void RenderHtmlMessageUsesSummaryWhenSectionsRenderEmptyFragments() {
        var request = new TeamsMessageRequest {
            Summary = "Fallback summary"
        };
        request.Sections.Add(new TeamsMessageSection {
            StartGroup = true
        });

        var json = GraphMessageRenderer.Render(request, TeamsDeliveryMethod.GraphChatMessage);
        using var document = JsonDocument.Parse(json);

        var body = document.RootElement.GetProperty("body");
        var html = body.GetProperty("content").GetString();

        Assert.Contains("Fallback summary", html);
    }
}
