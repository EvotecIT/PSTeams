#Get public and private function definition files.
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

Add-Type -Assembly 'System.Drawing'

Export-ModuleMember -Function '*'

<#
As per: https://d-fens.ch/2014/11/26/bug-powershell-scripts-in-scriptstoprocess-attribute-appear-as-loaded-modules/

[string] $ManifestFile = '{0}.psd1' -f (Get-Item $PSCommandPath).BaseName;
$ManifestPathAndFile = Join-Path -Path $PSScriptRoot -ChildPath $ManifestFile;
if( Test-Path -Path $ManifestPathAndFile)
{
  $Manifest = (Get-Content -raw $ManifestPathAndFile) | iex;
  foreach( $ScriptToProcess in $Manifest.ScriptsToProcess)
  {
    $ModuleToRemove = (Get-Item (Join-Path -Path $PSScriptRoot -ChildPath $ScriptToProcess)).BaseName;
    if(Get-Module $ModuleToRemove)
    {
      Remove-Module $ModuleToRemove;
    }
  }
}
#>