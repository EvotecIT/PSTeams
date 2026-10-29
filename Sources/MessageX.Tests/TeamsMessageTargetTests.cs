using MessageX.Teams;

namespace MessageX.Tests;

public class TeamsMessageTargetTests {
    [Fact]
    public void ForWorkflowWebhookCreatesWebhookTarget() {
        var uri = new Uri("https://example.test/webhook");

        var target = TeamsMessageTarget.ForWorkflowWebhook(uri, "alerts");

        Assert.Equal(TeamsDeliveryMethod.WorkflowWebhook, target.DeliveryMethod);
        Assert.Equal(uri, target.TargetUri);
        Assert.Equal("alerts", target.DisplayName);
    }

    [Fact]
    public void ForWorkflowWebhookRequiresAbsoluteUri() {
        var uri = new Uri("/relative", UriKind.Relative);

        var action = () => TeamsMessageTarget.ForWorkflowWebhook(uri);

        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void ForWorkflowWebhookRequiresHttps() {
        var uri = new Uri("http://example.test/webhook");

        var action = () => TeamsMessageTarget.ForWorkflowWebhook(uri);

        var exception = Assert.Throws<ArgumentException>(action);
        Assert.Contains("HTTPS", exception.Message);
    }

    [Fact]
    public void ForWorkflowWebhookRecordsConfiguredDestinationWithoutAddingConversationCapabilities() {
        var target = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"),
            "Release channel",
            TeamsWorkflowDestinationKind.Channel);

        Assert.Equal(TeamsWorkflowDestinationKind.Channel, target.WorkflowDestination);
        Assert.Equal(MessageCapabilities.Send, target.Capabilities);
        Assert.False(target.Capabilities.HasFlag(MessageCapabilities.Reply));
    }

    [Fact]
    public void WebhookCredentialRequiresExplicitAccessAndIsNotRendered() {
        var uri = new Uri("https://example.test/workflows/secret-token");
        var target = TeamsMessageTarget.ForWorkflowWebhook(uri);
        var publicProperties = typeof(TeamsMessageTarget).GetProperties()
            .Where(property => property.GetMethod?.IsPublic == true)
            .Select(property => property.Name)
            .ToArray();

        Assert.DoesNotContain("TargetUri", publicProperties);
        Assert.Equal(uri, target.GetWebhookUri());
        Assert.DoesNotContain("secret-token", target.ToString(), StringComparison.Ordinal);
    }
}
