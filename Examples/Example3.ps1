param (
    $TeamsID = $Env:TEAMSPESTERID
)

. (Join-Path $PSScriptRoot 'Import-MessageX.ps1')

Send-TeamsMessage `
    -URI $TeamsID `
    -Color DodgerBlue `
    -MessageSummary 'Test' `
    -Sections $Section -Verbose -ShowErrors
