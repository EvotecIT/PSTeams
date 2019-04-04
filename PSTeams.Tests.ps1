param (
    $TeamsID = $Env:TEAMSPESTERID
)
$PSVersionTable.PSVersion

$ModuleName = (Get-ChildItem $PSScriptRoot\*.psd1).BaseName
#+$ModuleVersion = (Get-Content -Raw $PSScriptRoot\*.psd1)  | Invoke-Expression | ForEach-Object ModuleVersion

#$Dest = "Builds\$ModuleName-{0}-{1}.zip" -f $ModuleVersion, (Get-Date).ToString("yyyyMMddHHmmss")
#Compress-Archive -Path . -DestinationPath .\$dest

$Pester = (Get-Module -ListAvailable pester)
if ($null -eq $Pester -or ($Pester[0].Version.Major -le 4 -and $Pester[0].Version.Minor -lt 4)) {
    Write-Warning "$ModuleName - Downloading Pester from PSGallery"
    Install-Module -Name Pester -Repository PSGallery -Force -SkipPublisherCheck -Scope CurrentUser
}
if ($null -eq (Get-Module -ListAvailable PSSharedGoods)) {
    Write-Warning "$ModuleName - Downloading PSSharedGoods from PSGallery"
    Install-Module -Name PSSharedGoods -Repository PSGallery -Force -Scope CurrentUser
}

#$result = Invoke-Pester -Script $PSScriptRoot\Tests -PassThru
$result = Invoke-Pester -Script @{ Path = "$($PSScriptRoot)\Tests"; Parameters = @{ TeamsID = $TeamsID }; } -EnableExit

if ($result.FailedCount -gt 0) {
    throw "$($result.FailedCount) tests failed."
}