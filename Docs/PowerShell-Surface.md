# PowerShell surface

`MessageX` is the binary PowerShell module for Teams, Slack, and Discord. Its module shell lives in `Module/MessageX`; compiled cmdlets delegate composition, validation, and delivery to the provider libraries.

| Layer | Responsibility |
| --- | --- |
| `MessageX.Core` | Provider-neutral contracts |
| `MessageX.Teams`, `MessageX.Slack`, `MessageX.Discord` | Typed messages, validation, and provider delivery |
| `MessageX.PowerShell` | PowerShell parameters and pipeline output |
| `Module/MessageX` | Manifest and runtime-specific assembly loading |

See the [generated command reference](Readme.md) for the exported commands and their parameters. Teams commands retain their provider-specific names. MessageX does not provide PSTeams or PSDiscord module aliases or compatibility wrappers; scripts should import `MessageX` explicitly.

## Installed module

```powershell
Import-Module MessageX
Get-Command -Module MessageX
```

Packaged modules load the included assemblies for Windows PowerShell 5.1 (`net472`) or PowerShell on .NET 8/10 (`net8.0`/`net10.0`). They do not require a source checkout or development environment variables.

## Repository examples

Build the cmdlet project from the repository root before running an example:

```powershell
dotnet build Sources/MessageX.PowerShell/MessageX.PowerShell.csproj -c Release
. ./Examples/Import-MessageX.ps1
Get-Command -Module MessageX
```

The example helper imports the repository-relative manifest and explicitly selects Release development binaries for that import. It restores the previous environment settings afterward. This keeps checkout examples tied to the current build even when another MessageX version is installed globally. Use a fresh PowerShell process after rebuilding assemblies that have already been loaded.

The active PowerShell validation suite lives in `Module/Tests`. The module shell does not dot-source legacy public or private function folders.
