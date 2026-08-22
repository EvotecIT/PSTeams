using System.Management.Automation;
using System.Net.Http;

namespace MessageX.PowerShell;

/// <summary>Owns Slack lifecycle clients for one PowerShell pipeline invocation.</summary>
public abstract class SlackLifecycleCmdletBase : MessageHttpCmdletBase {
    private HttpClient? _httpClient;
    private SlackWebApiLifecycleClient? _lifecycleClient;
    private SlackConversationDirectory? _conversationDirectory;

    /// <summary>Authenticated Slack Web API connection.</summary>
    [Parameter(Mandatory = true)]
    public SlackConnection Connection { get; set; } = null!;

    /// <summary>Slack lifecycle client available during cmdlet processing.</summary>
    protected SlackWebApiLifecycleClient LifecycleClient => _lifecycleClient ??
        throw new InvalidOperationException("The Slack lifecycle client is unavailable.");

    /// <summary>Slack conversation directory available during cmdlet processing.</summary>
    protected SlackConversationDirectory ConversationDirectory => _conversationDirectory ??
        throw new InvalidOperationException("The Slack conversation directory is unavailable.");

    /// <inheritdoc />
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        if (UsesDefaultTransport(options)) {
            _lifecycleClient = new SlackWebApiLifecycleClient(Connection);
            _conversationDirectory = new SlackConversationDirectory(Connection);
        } else {
            _httpClient = MessageHttpClientFactory.CreateClient(options);
            _lifecycleClient = new SlackWebApiLifecycleClient(Connection, _httpClient);
            _conversationDirectory = new SlackConversationDirectory(Connection, _httpClient);
        }
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        DisposeClients();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose() {
        DisposeClients();
        base.Dispose();
    }

    private void DisposeClients() {
        _lifecycleClient?.Dispose();
        _conversationDirectory?.Dispose();
        _httpClient?.Dispose();
        _lifecycleClient = null;
        _conversationDirectory = null;
        _httpClient = null;
    }
}
