namespace MessageX.Tests;

public sealed class CoreContractTests {
    [Fact]
    public void MessageReference_RequiresProviderIdentifier() {
        Assert.Throws<ArgumentException>(() => new MessageReference(" "));
    }

    [Fact]
    public void MessageReference_CarriesOnlyExplicitSafeCoordinates() {
        var reference = new MessageReference(MessageProviders.Teams, "activity-42") {
            InstallationId = "installation-1",
            ScopeId = "tenant-1",
            ConversationId = "conversation-1",
            ThreadId = "thread-1",
            CorrelationId = "correlation-1",
            Capabilities = MessageCapabilities.Reply | MessageCapabilities.Update | MessageCapabilities.Delete
        };

        Assert.Equal("activity-42", reference.MessageId);
        Assert.True(reference.Capabilities.HasFlag(MessageCapabilities.Reply));
        Assert.False(reference.Capabilities.HasFlag(MessageCapabilities.React));
    }

    [Fact]
    public void MessageReference_ExposesCapabilitiesThroughSharedContract() {
        IProviderCapabilities capabilities = new MessageReference(MessageProviders.Teams) {
            Capabilities = MessageCapabilities.Reply | MessageCapabilities.Update
        };

        Assert.Equal(MessageCapabilities.Reply | MessageCapabilities.Update, capabilities.Capabilities);
    }

    [Fact]
    public void TeamsWebhookTarget_AdvertisesSendThroughSharedContract() {
        IProviderCapabilities capabilities = TeamsMessageTarget.ForWorkflowWebhook(
            new Uri("https://example.test/workflows/secret-token"));

        Assert.Equal(MessageCapabilities.Send, capabilities.Capabilities);
    }

    [Fact]
    public void UnsupportedTeamsTarget_AdvertisesNoCapabilities() {
        IProviderCapabilities capabilities = new TeamsMessageTarget {
            DeliveryMethod = (TeamsDeliveryMethod)999,
            TargetUri = new Uri("https://example.test/unsupported")
        };

        Assert.Equal(MessageCapabilities.None, capabilities.Capabilities);
    }

    [Fact]
    public void TeamsDeliveryResult_UsesSharedProviderContract() {
        MessageDeliveryResult result = new TeamsDeliveryResult {
            DeliveryMethod = TeamsDeliveryMethod.WorkflowWebhook,
            IsSuccessStatusCode = true,
            StatusCode = 202
        };

        Assert.Equal(MessageProviders.Teams, result.Provider);
        Assert.True(result.IsSuccess);
        Assert.Equal(202, result.StatusCode);
    }
}
