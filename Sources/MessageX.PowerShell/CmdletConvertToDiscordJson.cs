using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>Serializes a typed Discord message using the exact provider payload contract.</summary>
[Cmdlet(VerbsData.ConvertTo, "DiscordJson")]
[OutputType(typeof(string))]
public sealed class CmdletConvertToDiscordJson : PSCmdlet {
    /// <summary>Discord message to serialize.</summary>
    [Parameter(Mandatory = true, Position = 0, ValueFromPipeline = true)]
    public DiscordMessageRequest Message { get; set; } = null!;

    /// <summary>Discord target whose transport determines the payload envelope.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public DiscordMessageTarget Target { get; set; } = null!;

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordJsonSerializer.Serialize(Message, Target));
    }
}
