function New-TeamsActivityImage {
    [CmdletBinding(DefaultParameterSetName = 'Link')]
    [alias('ActivityImageLink', 'TeamsActivityImageLink', 'New-TeamsActivityImageLink', 'ActivityImage', 'TeamsActivityImage')]
    param(
        [Parameter(ParameterSetName = 'Image')][string][ValidateSet('Add', 'Alert', 'Cancel', 'Check', 'Disable', 'Download', 'Info', 'Minus', 'Question', 'Reload', 'None')] $Image,
        [Parameter(ParameterSetName = 'Link')][string] $Link
    )
    if ($Image) {
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