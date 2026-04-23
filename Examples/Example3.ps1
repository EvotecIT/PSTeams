param (
    $TeamsID = $Env:TEAMSPESTERID
)

. (Join-Path $PSScriptRoot 'Import-PSTeams.ps1')

Send-TeamsMessage `
    -URI $TeamsID `
    -Color DodgerBlue `
    -MessageSummary 'Test' `
    -Sections $Section -Verbose -ShowErrors
