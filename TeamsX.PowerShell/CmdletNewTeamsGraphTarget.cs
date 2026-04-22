using System.Management.Automation;
using System.Security;
using TeamsX;

namespace TeamsX.PowerShell;

[Cmdlet(VerbsCommon.New, "TeamsGraphTarget")]
[OutputType(typeof(TeamsMessageTarget))]
public sealed class CmdletNewTeamsGraphTarget : PSCmdlet {
    [Parameter(Mandatory = true, ParameterSetName = "ChannelToken")]
    [Parameter(Mandatory = true, ParameterSetName = "ChannelSecure")]
    [Parameter(Mandatory = true, ParameterSetName = "ChannelVariable")]
    public string TeamId { get; set; } = null!;

    [Parameter(Mandatory = true, ParameterSetName = "ChannelToken")]
    [Parameter(Mandatory = true, ParameterSetName = "ChannelSecure")]
    [Parameter(Mandatory = true, ParameterSetName = "ChannelVariable")]
    public string ChannelId { get; set; } = null!;

    [Parameter(Mandatory = true, ParameterSetName = "ChatToken")]
    [Parameter(Mandatory = true, ParameterSetName = "ChatSecure")]
    [Parameter(Mandatory = true, ParameterSetName = "ChatVariable")]
    public string ChatId { get; set; } = null!;

    [Alias("Token")]
    [Parameter(Mandatory = true, ParameterSetName = "ChannelToken")]
    [Parameter(Mandatory = true, ParameterSetName = "ChatToken")]
    public string AccessToken { get; set; } = null!;

    [Parameter(Mandatory = true, ParameterSetName = "ChannelSecure")]
    [Parameter(Mandatory = true, ParameterSetName = "ChatSecure")]
    public SecureString SecureAccessToken { get; set; } = null!;

    [Parameter(Mandatory = true, ParameterSetName = "ChannelVariable")]
    [Parameter(Mandatory = true, ParameterSetName = "ChatVariable")]
    public string AccessTokenVariableName { get; set; } = null!;

    [Parameter(Mandatory = false, ParameterSetName = "ChannelToken")]
    [Parameter(Mandatory = false, ParameterSetName = "ChannelSecure")]
    [Parameter(Mandatory = false, ParameterSetName = "ChannelVariable")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatToken")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatSecure")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatVariable")]
    public string? DisplayName { get; set; }

    [Parameter(Mandatory = false, ParameterSetName = "ChannelToken")]
    [Parameter(Mandatory = false, ParameterSetName = "ChannelSecure")]
    [Parameter(Mandatory = false, ParameterSetName = "ChannelVariable")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatToken")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatSecure")]
    [Parameter(Mandatory = false, ParameterSetName = "ChatVariable")]
    public Uri? GraphBaseUri { get; set; }

    protected override void ProcessRecord() {
        var accessTokenProvider = ResolveAccessTokenProvider();

        var target = ParameterSetName switch {
            "ChannelToken" => TeamsMessageTarget.ForGraphChannelMessage(TeamId, ChannelId, AccessToken, DisplayName, GraphBaseUri),
            "ChannelSecure" => TeamsMessageTarget.ForGraphChannelMessage(TeamId, ChannelId, accessTokenProvider!, DisplayName, GraphBaseUri),
            "ChannelVariable" => TeamsMessageTarget.ForGraphChannelMessage(TeamId, ChannelId, accessTokenProvider!, DisplayName, GraphBaseUri),
            "ChatToken" => TeamsMessageTarget.ForGraphChatMessage(ChatId, AccessToken, DisplayName, GraphBaseUri),
            "ChatSecure" => TeamsMessageTarget.ForGraphChatMessage(ChatId, accessTokenProvider!, DisplayName, GraphBaseUri),
            "ChatVariable" => TeamsMessageTarget.ForGraphChatMessage(ChatId, accessTokenProvider!, DisplayName, GraphBaseUri),
            _ => throw new InvalidOperationException($"Unsupported parameter set '{ParameterSetName}'.")
        };

        WriteObject(target);
    }

    private Func<CancellationToken, Task<string>>? ResolveAccessTokenProvider() {
        return ParameterSetName switch {
            "ChannelSecure" or "ChatSecure" => _ => Task.FromResult(TeamsPowerShellGraphTokenSupport.ConvertToUnsecureString(SecureAccessToken)),
            "ChannelVariable" or "ChatVariable" => _ => Task.FromResult(TeamsPowerShellGraphTokenSupport.ReadEnvironmentVariable(AccessTokenVariableName)),
            _ => null
        };
    }
}
