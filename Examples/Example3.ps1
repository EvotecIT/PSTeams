param (
    $TeamsID = $Env:TEAMSPESTERID
)

Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

Send-TeamsMessage `
    -URI $TeamsID `
    -Color DodgerBlue `
    -MessageSummary 'Test' `
    -Sections $Section -Verbose -ShowErrors
