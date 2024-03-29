﻿#Get public and private function definition files.
$Public = @( Get-ChildItem -Path $PSScriptRoot\Public\*.ps1 -ErrorAction SilentlyContinue )
$Private = @( Get-ChildItem -Path $PSScriptRoot\Private\*.ps1 -ErrorAction SilentlyContinue )
$Enums = @( Get-ChildItem -Path $PSScriptRoot\Enums\*.ps1 -ErrorAction SilentlyContinue )

#Dot source the files
Foreach ($import in @($Public + $Private + $Enums)) {
    Try {
        . $import.fullname
    } Catch {
        Write-Error -Message "Failed to import function $($import.fullname): $_"

    }
}

Export-ModuleMember -Function * -Alias *