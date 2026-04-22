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

    [Fact]
    public void ForGraphChannelMessageCreatesGraphTarget() {
        var target = TeamsMessageTarget.ForGraphChannelMessage(
            "team-1",
            "channel-1",
            "token-1",
            "alerts",
            new Uri("https://graph.example.test/"));

        Assert.Equal(TeamsDeliveryMethod.GraphChannelMessage, target.DeliveryMethod);
        Assert.Equal("https://graph.example.test/v1.0/teams/team-1/channels/channel-1/messages", target.TargetUri.ToString());
        Assert.Equal("token-1", target.AccessToken);
        Assert.Equal("alerts", target.DisplayName);
    }

    [Fact]
    public void ForGraphChatMessageCreatesGraphTarget() {
        var target = TeamsMessageTarget.ForGraphChatMessage(
            "19:testchat@thread.v2",
            "token-1",
            "ops-chat",
            new Uri("https://graph.example.test/"));

        Assert.Equal(TeamsDeliveryMethod.GraphChatMessage, target.DeliveryMethod);
        Assert.Equal("https://graph.example.test/v1.0/chats/19%3Atestchat%40thread.v2/messages", target.TargetUri.ToString());
        Assert.Equal("token-1", target.AccessToken);
        Assert.Equal("ops-chat", target.DisplayName);
    }

    [Fact]
    public void ForGraphChatMessageSupportsDynamicAccessTokenProvider() {
        var target = TeamsMessageTarget.ForGraphChatMessage(
            "19:testchat@thread.v2",
            _ => Task.FromResult("token-2"),
            "ops-chat",
            new Uri("https://graph.example.test/"));

        Assert.Equal(TeamsDeliveryMethod.GraphChatMessage, target.DeliveryMethod);
        Assert.True(target.HasDynamicAccessToken);
        Assert.Null(target.AccessToken);
    }
}
