using System.Management.Automation;
using System.Runtime.InteropServices;
using System.Security;

namespace MessageX.PowerShell;

/// <summary>Creates an authenticated Slack bot connection without exposing its token.</summary>
[Cmdlet(VerbsCommon.New, "SlackConnection")]
[OutputType(typeof(SlackConnection))]
public sealed class CmdletNewSlackConnection : PSCmdlet {
    /// <summary>Slack bot token stored as a secure string.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public SecureString BotToken { get; set; } = null!;

    /// <summary>Optional Slack or GovSlack API base URI.</summary>
    [Parameter(Mandatory = false)]
    public Uri? ApiBaseUri { get; set; }

    /// <summary>Optional non-secret workspace identifier used in delivery references.</summary>
    [Parameter(Mandatory = false)]
    public string? WorkspaceId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        var pointer = IntPtr.Zero;
        try {
            pointer = Marshal.SecureStringToBSTR(BotToken);
            var token = Marshal.PtrToStringBSTR(pointer);
            WriteObject(SlackConnection.ForBotToken(token ?? string.Empty, ApiBaseUri, WorkspaceId));
        } finally {
            if (pointer != IntPtr.Zero) {
                Marshal.ZeroFreeBSTR(pointer);
            }
        }
    }
}
