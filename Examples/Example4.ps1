param (
    $TeamsID = $Env:TEAMSPESTERID
)

. (Join-Path $PSScriptRoot 'Import-PSTeams.ps1')

# keep in mind for Emoji you may need UTF-8 with BOM

Send-TeamsMessage -Uri $TeamsID -MessageTitle "Foo" -MessageText "Foo ❌ 🐱‍👤 ❤ bar" -Verbose