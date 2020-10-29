function New-TeamsActivityImage {
    [CmdletBinding(DefaultParameterSetName = 'Link')]
    [alias('ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage')]
    param(
        [Parameter(ParameterSetName = 'Image')][string][ValidateSet('Add', 'Alert', 'Cancel', 'Check', 'Disable', 'Download', 'Info', 'Minus', 'Question', 'Reload', 'None')] $Image,
        [Parameter(ParameterSetName = 'Link')][string] $Link,

        [Parameter(ParameterSetName = 'Path')]
        [ValidateScript({
            if (-not ($_ | Test-Path)) {
                throw "Path is inaccessible or does not exist"
            }
            if (-not ($_ | Test-Path -PathType Leaf) -or ($_ -notmatch "(\.jpg|\.png)")) {
                throw "Path is not a file or file extension is not supported"
            }
            return $true
        })]
        [System.IO.FileInfo] $Path
    )
    if ($Path) {
        $FilePath = [System.IO.Path]::GetDirectoryName($Path)
        $FileBaseName = [System.IO.Path]::GetFileNameWithoutExtension($Path)
        $FileExtension = [System.IO.Path]::GetExtension($Path)
        @{
            ActivityImageLink = Get-Image -PathToImages $FilePath -FileName $FileBaseName -FileExtension $FileExtension -Verbose
            type              = 'ActivityImage'
        }
    }
    elseif ($Image) {
        if ($Image -ne 'None') {
            $StoredImages = [IO.Path]::Combine($PSScriptRoot, '..', 'Images')
            @{
                ActivityImageLink = Get-Image -PathToImages $StoredImages -FileName $Image -FileExtension '.jpg' # -Verbose
                type              = 'ActivityImage'
            }
        }
    } else {
        @{
            ActivityImageLink = $Link
            Type              = 'ActivityImageLink'
        }
    }
}
