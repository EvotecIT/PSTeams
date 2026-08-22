using System.Management.Automation;
using System.Security;

namespace MessageX.PowerShell;

/// <summary>Creates an authenticated Discord bot connection without exposing its token.</summary>
[Cmdlet(VerbsCommon.New, "DiscordConnection")]
[OutputType(typeof(DiscordConnection))]
public sealed class CmdletNewDiscordConnection : PSCmdlet {
    /// <summary>Discord bot token stored as a secure string.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public SecureString BotToken { get; set; } = null!;

    /// <summary>Optional non-secret Discord application identifier.</summary>
    [Parameter(Mandatory = false)]
    public string? ApplicationId { get; set; }

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(SecureStringSupport.Use(
            BotToken,
            token => DiscordConnection.ForBotToken(token, ApplicationId)));
    }
}
