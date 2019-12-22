function New-TeamsActivityImage {
    [CmdletBinding()]
    [alias('ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage')]
    param(
        [string][ValidateSet('Alert', 'Cancel', 'Disable', 'Download', 'Minus', 'Check', 'Add', 'None')] $Image,
        [string] $Link
    )
    if ($Image) {
        if ($Image -ne 'None') {
            $StoredImages = [IO.Path]::Combine("$(Split-Path -Path $PSScriptRoot -Parent)", "Images")
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