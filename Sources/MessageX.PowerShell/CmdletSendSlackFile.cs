using System.IO;
using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Uploads a file through Slack's external upload workflow.</summary>
/// <example>
/// <summary>Share a build log in a Slack channel</summary>
/// <code>Send-SlackFile -Path .\build.log -ConversationId C0123456789 -Connection $connection -InitialComment 'Build output'</code>
/// </example>
[Cmdlet(VerbsCommunications.Send, "SlackFile", SupportsShouldProcess = true)]
[OutputType(typeof(SlackFileUploadResult))]
public sealed class CmdletSendSlackFile : MessageHttpCmdletBase {
    private SlackExternalFileUploadClient? _client;

    /// <summary>File path to upload.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
    [Alias("FullName")]
    public string Path { get; set; } = string.Empty;

    /// <summary>Authenticated Slack Web API connection with files:write permission.</summary>
    [Parameter(Mandatory = true)]
    public SlackConnection Connection { get; set; } = null!;

    /// <summary>Optional Slack channel, direct-message, or multiparty-message identifier.</summary>
    [Parameter]
    public string? ConversationId { get; set; }

    /// <summary>Optional parent message timestamp for a threaded file share.</summary>
    [Parameter]
    public string? ThreadTimestamp { get; set; }

    /// <summary>Optional provider-visible title.</summary>
    [Parameter]
    public string? Title { get; set; }

    /// <summary>Optional message text introducing the file.</summary>
    [Parameter]
    public string? InitialComment { get; set; }

    /// <summary>Optional screen-reader description for an image.</summary>
    [Parameter]
    public string? AlternativeText { get; set; }

    /// <summary>Optional Slack snippet syntax identifier.</summary>
    [Parameter]
    public string? SnippetType { get; set; }

    /// <summary>Returns the typed upload result.</summary>
    [Parameter]
    public SwitchParameter PassThru { get; set; }

    /// <inheritdoc />
    protected override Task BeginProcessingAsync() {
        var options = CreateTransportOptions();
        _client = UsesDefaultTransport(options)
            ? new SlackExternalFileUploadClient(Connection)
            : new SlackExternalFileUploadClient(Connection, options);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    protected override async Task ProcessRecordAsync() {
        var resolvedPath = SessionState.Path.GetUnresolvedProviderPathFromPSPath(Path);
        var file = new FileInfo(resolvedPath);
        if (!file.Exists) {
            ThrowTerminatingError(new ErrorRecord(
                new FileNotFoundException("Slack upload file was not found.", resolvedPath),
                "SlackFileNotFound",
                ErrorCategory.ObjectNotFound,
                resolvedPath));
            return;
        }

        var destination = string.IsNullOrWhiteSpace(ConversationId)
            ? "Slack private files"
            : $"Slack conversation {ConversationId}";
        if (!ShouldProcess(destination, $"Upload {file.Name}")) {
            return;
        }

        using var stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = await Client.UploadAsync(new SlackFileUploadRequest {
            Content = stream,
            Length = stream.Length,
            FileName = file.Name,
            Title = Title,
            AlternativeText = AlternativeText,
            SnippetType = SnippetType,
            ConversationId = ConversationId,
            ThreadTimestamp = ThreadTimestamp,
            InitialComment = InitialComment
        }, CancelToken).ConfigureAwait(false);
        if (!result.IsSuccess) {
            WriteError(SlackPowerShellDeliverySupport.CreateFileUploadFailureError(result, "Send-SlackFile"));
        }
        if (PassThru) {
            WriteObject(result);
        }
    }

    /// <inheritdoc />
    protected override Task EndProcessingAsync() {
        DisposeClient();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public override void Dispose() {
        DisposeClient();
        base.Dispose();
    }

    private SlackExternalFileUploadClient Client => _client ??
        throw new InvalidOperationException("The Slack file upload client is unavailable.");

    private void DisposeClient() {
        _client?.Dispose();
        _client = null;
    }
}
