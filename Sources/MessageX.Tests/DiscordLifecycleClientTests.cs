using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using MessageX.Discord;

namespace MessageX.Tests;

public sealed class DiscordLifecycleClientTests {
    private static readonly Uri WebhookUri = new(
        "https://discord.com/api/webhooks/123456789012345678/abcdefghijklmnopqrstuvwxyz123456");

    [Fact]
    public async Task BotUpdateUsesPersistedCoordinatesAndManagedCapabilities() {
        using var handler = new QueueHandler(Response(
            HttpStatusCode.OK,
            "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\"}"));
        using var client = CreateBotClient(handler);

        var result = await client.UpdateAsync(
            new DiscordMessageRequest { Content = "Updated" },
            BotReference(),
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        var request = Assert.Single(handler.Requests);
        Assert.Equal("PATCH", request.Method.Method);
        Assert.Equal(
            "https://discord.com/api/v10/channels/123456789012345678/messages/623456789012345678",
            request.Uri.AbsoluteUri);
        Assert.Equal("Bot", request.AuthorizationScheme);
        Assert.Equal("discord-super-secret-token-value", request.AuthorizationParameter);
        using var payload = JsonDocument.Parse(request.Body!);
        Assert.Equal("Updated", payload.RootElement.GetProperty("content").GetString());
        Assert.Empty(payload.RootElement.GetProperty("embeds").EnumerateArray());
        Assert.Equal(ManagedBotCapabilities, result.Reference?.Capabilities);
    }

    [Fact]
    public async Task BotUpdateClearsOmittedContentAndPreservesCapabilities() {
        using var handler = new QueueHandler(Response(
            HttpStatusCode.OK,
            "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\"}"));
        using var client = CreateBotClient(handler);
        var reference = BotReference();
        reference.Capabilities = MessageCapabilities.Update | MessageCapabilities.Read;
        var message = new DiscordMessageRequest();
        message.Embeds.Add(new DiscordEmbed { Description = "Replacement" });

        var result = await client.UpdateAsync(message, reference, TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(reference.Capabilities, result.Reference?.Capabilities);
        using var payload = JsonDocument.Parse(Assert.Single(handler.Requests).Body!);
        Assert.Equal(JsonValueKind.Null, payload.RootElement.GetProperty("content").ValueKind);
        Assert.Single(payload.RootElement.GetProperty("embeds").EnumerateArray());
    }

    [Fact]
    public async Task BotDeleteAndReactionAcceptEmptySuccessBodies() {
        using var handler = new QueueHandler(
            Response(HttpStatusCode.NoContent, string.Empty),
            Response(HttpStatusCode.NoContent, string.Empty));
        using var client = CreateBotClient(handler);

        var reaction = await client.AddReactionAsync(
            BotReference(),
            "party_parrot:723456789012345678",
            TestContext.Current.CancellationToken);
        var deletion = await client.DeleteAsync(
            BotReference(),
            TestContext.Current.CancellationToken);

        Assert.True(reaction.IsSuccess);
        Assert.Equal(ManagedBotCapabilities, reaction.Reference?.Capabilities);
        Assert.Contains(
            "/reactions/party_parrot%3A723456789012345678/@me",
            handler.Requests[0].Uri.AbsoluteUri,
            StringComparison.Ordinal);
        Assert.Equal(HttpMethod.Put, handler.Requests[0].Method);
        Assert.True(deletion.IsSuccess);
        Assert.Equal(MessageCapabilities.None, deletion.Reference?.Capabilities);
        Assert.Equal(HttpMethod.Delete, handler.Requests[1].Method);
    }

    [Fact]
    public async Task BotReactionPreservesSourceCapabilitiesAndNormalizesCoordinates() {
        using var handler = new QueueHandler(Response(HttpStatusCode.NoContent, string.Empty));
        using var client = CreateBotClient(handler);
        var reference = new MessageReference(MessageProviders.Discord, " 623456789012345678 ") {
            ConversationId = " 123456789012345678 ",
            Capabilities = MessageCapabilities.React
        };

        var result = await client.AddReactionAsync(
            reference,
            "eyes",
            TestContext.Current.CancellationToken);

        Assert.True(result.IsSuccess);
        Assert.Equal(MessageCapabilities.React, result.Reference?.Capabilities);
        Assert.Equal("623456789012345678", result.Reference?.MessageId);
        Assert.Equal("123456789012345678", result.Reference?.ConversationId);
    }

    [Fact]
    public async Task BotRetrievesExactReferencedMessage() {
        using var handler = new QueueHandler(Response(
            HttpStatusCode.OK,
            "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\",\"content\":\"Current\"}"));
        using var client = CreateBotClient(handler);

        var result = await client.GetAsync(BotReference(), TestContext.Current.CancellationToken);

        Assert.Equal("Current", result.Content);
        Assert.Equal(ManagedBotCapabilities, result.Reference.Capabilities);
        Assert.Equal(HttpMethod.Get, Assert.Single(handler.Requests).Method);
        Assert.Equal("Bot", handler.Requests[0].AuthorizationScheme);
    }

    [Fact]
    public async Task BotRetrievalMatchesNormalizedReferenceCoordinates() {
        using var handler = new QueueHandler(Response(
            HttpStatusCode.OK,
            "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345678\",\"content\":\"Current\"}"));
        using var client = CreateBotClient(handler);
        var reference = BotReference();
        reference.MessageId = " 623456789012345678 ";
        reference.ConversationId = " 123456789012345678 ";

        var result = await client.GetAsync(reference, TestContext.Current.CancellationToken);

        Assert.Equal("623456789012345678", result.Reference.MessageId);
        Assert.Equal("123456789012345678", result.Reference.ConversationId);
    }

    [Fact]
    public async Task BotUpdateRejectsAttachmentsAndSendOnlyOptions() {
        using var client = CreateBotClient(new QueueHandler(Response(HttpStatusCode.OK, "{}")));
        var attachment = new DiscordMessageRequest { Content = "Updated" };
        attachment.Attachments.Add(DiscordAttachment.FromBytes("new.txt", Encoding.UTF8.GetBytes("new")));

        await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateAsync(
            attachment,
            BotReference(),
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.UpdateAsync(
            new DiscordMessageRequest { Content = "Updated", ReplyToMessageId = "823456789012345678" },
            BotReference(),
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => client.AddReactionAsync(
            BotReference(),
            "bad:name:723456789012345678",
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task UpdateRejectsMismatchedProviderCoordinatesAsTransient() {
        using var handler = new QueueHandler(Response(
            HttpStatusCode.OK,
            "{\"id\":\"723456789012345678\",\"channel_id\":\"123456789012345678\"}"));
        using var client = CreateBotClient(handler);

        var result = await client.UpdateAsync(
            new DiscordMessageRequest { Content = "Updated" },
            BotReference(),
            TestContext.Current.CancellationToken);

        Assert.False(result.IsSuccess);
        Assert.Equal("invalid_response", result.ProviderCode);
        Assert.Equal(MessageErrorKind.Transient, result.ErrorKind);
        Assert.Null(result.Reference);
    }

    [Fact]
    public async Task WebhookRetrievesUpdatesAndDeletesOwnedMessage() {
        const string message = "{\"id\":\"623456789012345678\",\"channel_id\":\"123456789012345679\",\"content\":\"Current\",\"timestamp\":\"2026-08-21T08:00:00Z\"}";
        using var handler = new QueueHandler(
            Response(HttpStatusCode.OK, message),
            Response(HttpStatusCode.OK, message.Replace("Current", "Updated")),
            Response(HttpStatusCode.NoContent, string.Empty));
        var target = DiscordMessageTarget.ForIncomingWebhook(WebhookUri, "123456789012345679");
        using var client = new DiscordWebhookLifecycleClient(
            target,
            new HttpClient(handler),
            disposeHttpClient: true);
        var reference = WebhookReference();
        reference.ScopeId = "guild-scope";

        var retrieved = await client.GetAsync(reference, TestContext.Current.CancellationToken);
        var updated = await client.UpdateAsync(
            new DiscordMessageRequest { Content = "Updated" },
            reference,
            TestContext.Current.CancellationToken);
        var deleted = await client.DeleteAsync(reference, TestContext.Current.CancellationToken);

        Assert.Equal("Current", retrieved.Content);
        Assert.Equal(WebhookCapabilities, retrieved.Reference.Capabilities);
        Assert.True(updated.IsSuccess);
        Assert.Equal(WebhookCapabilities, updated.Reference?.Capabilities);
        Assert.Equal("guild-scope", updated.Reference?.ScopeId);
        Assert.True(deleted.IsSuccess);
        Assert.Equal(MessageCapabilities.None, deleted.Reference?.Capabilities);
        Assert.All(handler.Requests, item => {
            Assert.Contains("/messages/623456789012345678", item.Uri.AbsoluteUri, StringComparison.Ordinal);
            Assert.Contains("thread_id=123456789012345679", item.Uri.Query, StringComparison.Ordinal);
            Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz", item.Body ?? string.Empty, StringComparison.Ordinal);
        });
        Assert.Equal(new[] { "GET", "PATCH", "DELETE" }, handler.Requests.Select(item => item.Method.Method));
    }

    [Fact]
    public async Task LifecycleReferencesFailClosedForProviderCapabilityAndWebhookThread() {
        using var bot = CreateBotClient(new QueueHandler(Response(HttpStatusCode.NoContent, string.Empty)));
        var wrongProvider = BotReference();
        wrongProvider = new MessageReference(MessageProviders.Slack, wrongProvider.MessageId) {
            ConversationId = wrongProvider.ConversationId,
            Capabilities = MessageCapabilities.Delete
        };
        var sendOnly = BotReference();
        sendOnly.Capabilities = MessageCapabilities.Send;
        using var webhook = new DiscordWebhookLifecycleClient(
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri, "123456789012345679"),
            new HttpClient(new QueueHandler(Response(HttpStatusCode.NoContent, string.Empty))),
            disposeHttpClient: true);
        var wrongThread = WebhookReference();
        wrongThread.ThreadId = "223456789012345679";

        await Assert.ThrowsAsync<ArgumentException>(() => bot.DeleteAsync(
            wrongProvider,
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<InvalidOperationException>(() => bot.DeleteAsync(
            sendOnly,
            TestContext.Current.CancellationToken));
        await Assert.ThrowsAsync<ArgumentException>(() => webhook.DeleteAsync(
            wrongThread,
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task WebhookReadErrorsAreClassifiedWithoutRetainingCredentialMaterial() {
        using var client = new DiscordWebhookLifecycleClient(
            DiscordMessageTarget.ForIncomingWebhook(WebhookUri, "123456789012345679"),
            new HttpClient(new QueueHandler(Response(
                HttpStatusCode.NotFound,
                "{\"code\":10008,\"message\":\"abcdefghijklmnopqrstuvwxyz123456\"}"))),
            disposeHttpClient: true);

        var exception = await Assert.ThrowsAsync<MessageDeliveryException>(() => client.GetAsync(
            WebhookReference(),
            TestContext.Current.CancellationToken));

        Assert.Equal(MessageErrorKind.NotFound, exception.Kind);
        Assert.DoesNotContain("abcdefghijklmnopqrstuvwxyz", exception.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Null(exception.InnerException);
    }

    private static DiscordBotLifecycleClient CreateBotClient(HttpMessageHandler handler) => new(
        DiscordConnection.ForBotToken("discord-super-secret-token-value"),
        new HttpClient(handler),
        disposeHttpClient: true);

    private static MessageReference BotReference() => new(MessageProviders.Discord, "623456789012345678") {
        ScopeId = "223456789012345678",
        ConversationId = "123456789012345678",
        Capabilities = ManagedBotCapabilities
    };

    private static MessageReference WebhookReference() => new(MessageProviders.Discord, "623456789012345678") {
        ConversationId = "123456789012345679",
        ThreadId = "123456789012345679",
        Capabilities = WebhookCapabilities
    };

    private static HttpResponseMessage Response(HttpStatusCode statusCode, string body) => new(statusCode) {
        Content = new StringContent(body, Encoding.UTF8, "application/json")
    };

    private const MessageCapabilities ManagedBotCapabilities =
        MessageCapabilities.Reply | MessageCapabilities.Update |
        MessageCapabilities.Delete | MessageCapabilities.React | MessageCapabilities.Read;

    private const MessageCapabilities WebhookCapabilities =
        MessageCapabilities.Read | MessageCapabilities.Update | MessageCapabilities.Delete;

    private sealed class QueueHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;

        public QueueHandler(params HttpResponseMessage[] responses) {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        public List<RecordedRequest> Requests { get; } = new();

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) {
            Requests.Add(new RecordedRequest(
                request.Method,
                request.RequestUri!,
                request.Headers.Authorization?.Scheme,
                request.Headers.Authorization?.Parameter,
                request.Content is null ? null : await request.Content.ReadAsStringAsync(cancellationToken)));
            return _responses.Dequeue();
        }
    }

    private sealed record RecordedRequest(
        HttpMethod Method,
        Uri Uri,
        string? AuthorizationScheme,
        string? AuthorizationParameter,
        string? Body);
}
