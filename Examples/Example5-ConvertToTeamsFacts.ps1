Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

Get-ChildItem | Select-Object -First 2 | ConvertTo-TeamsFact

ConvertTo-TeamsFact -InputObject (Get-ChildItem | Select-Object -First 2)