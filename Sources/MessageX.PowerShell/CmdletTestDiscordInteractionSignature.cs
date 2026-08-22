using System.Management.Automation;

namespace MessageX.PowerShell;

/// <summary>
/// Verifies a Discord interaction signature and bounds its age. The hosting service must separately
/// reject duplicate signatures because an age window alone does not prevent replay.
/// </summary>
[Cmdlet(VerbsDiagnostic.Test, "DiscordInteractionSignature")]
[OutputType(typeof(bool))]
public sealed class CmdletTestDiscordInteractionSignature : PSCmdlet {
    /// <summary>Discord application public key encoded as hexadecimal.</summary>
    [Parameter(Mandatory = true, Position = 0)]
    public string PublicKey { get; set; } = string.Empty;

    /// <summary>Ed25519 request signature from the X-Signature-Ed25519 header.</summary>
    [Parameter(Mandatory = true, Position = 1)]
    public string Signature { get; set; } = string.Empty;

    /// <summary>Unix timestamp text from the X-Signature-Timestamp header.</summary>
    [Parameter(Mandatory = true, Position = 2)]
    public string Timestamp { get; set; } = string.Empty;

    /// <summary>Exact raw request body bytes.</summary>
    [Parameter(Mandatory = true, Position = 3)]
    public byte[] Body { get; set; } = Array.Empty<byte>();

    /// <summary>Maximum accepted clock skew and request age in seconds.</summary>
    [Parameter(Mandatory = false)]
    [ValidateRange(1, 3600)]
    public int MaximumAgeSeconds { get; set; } = 300;

    /// <inheritdoc />
    protected override void ProcessRecord() {
        WriteObject(DiscordInteractionVerifier.VerifyRecent(
            PublicKey,
            Signature,
            Timestamp,
            Body,
            DateTimeOffset.UtcNow,
            TimeSpan.FromSeconds(MaximumAgeSeconds)));
    }
}
