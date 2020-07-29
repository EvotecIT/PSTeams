param (
    $TeamsID = $Env:TEAMSPESTERID
)

Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

# keep in mind for Emoji you may need UTF-8 with BOM

Send-TeamsMessage -Uri $TeamsID -MessageTitle "Foo" -MessageText "Foo ❌ 🐱‍👤 ❤ bar" -Verbose