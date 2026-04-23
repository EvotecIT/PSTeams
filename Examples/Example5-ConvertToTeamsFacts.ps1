. (Join-Path $PSScriptRoot 'Import-PSTeams.ps1')

Get-ChildItem | Select-Object -First 2 | ConvertTo-TeamsFact

ConvertTo-TeamsFact -InputObject (Get-ChildItem | Select-Object -First 2)