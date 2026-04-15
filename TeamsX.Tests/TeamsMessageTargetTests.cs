using TeamsX;

namespace TeamsX.Tests;

public class TeamsMessageTargetTests {
    [Fact]
    public void ForIncomingWebhookCreatesWebhookTarget() {
        var uri = new Uri("https://example.test/webhook");

        var target = TeamsMessageTarget.ForIncomingWebhook(uri, "alerts");

        Assert.Equal(TeamsDeliveryMethod.IncomingWebhook, target.DeliveryMethod);
        Assert.Equal(uri, target.TargetUri);
        Assert.Equal("alerts", target.DisplayName);
    }

    [Fact]
    public void ForIncomingWebhookRequiresAbsoluteUri() {
        var uri = new Uri("/relative", UriKind.Relative);

        var action = () => TeamsMessageTarget.ForIncomingWebhook(uri);

        Assert.Throws<ArgumentException>(action);
    }
}
