. (Join-Path $PSScriptRoot 'Import-MessageX.ps1')

Send-TeamsMessage -URI $TeamsID -MessageText "This text will show up"