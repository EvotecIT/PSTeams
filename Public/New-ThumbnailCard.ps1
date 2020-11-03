function New-ThumbnailCard {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][scriptblock] $Content,
        [string] $Title,
        [string] $SubTitle,
        [string] $Text,
        [string] $Uri
    )
    if ($Content) {
        $Buttons = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $Images = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $ExecutedContent = & $Content
        foreach ($E in $ExecutedContent) {
            if ($E.Value) {
                if ($Buttons.Count -lt 6) {
                    $Buttons.Add($E)
                } else {
                    Write-Warning "New-ThumbnailCard - Thumbnail Card support only up to 6 buttons."
                }
            } else {
                if ($Images.Count -lt 1) {
                    $Images.Add($E)
                } else {
                    Write-Warning "New-ThumbnailCard - Thumbnail Card support only 1 image."
                }
            }
        }

        $Wrapper = [ordered]@{
            contentType = "application/vnd.microsoft.card.thumbnail"
            content     = [ordered]@{
                title    = $Title
                subTitle = $SubTitle
                text     = $Text
                images   = @(
                    $Images
                )
                buttons  = @(
                    $Buttons
                )
            }
        }
    }
    $Body = $Wrapper | ConvertTo-Json -Depth 20
    if ($Uri) {
        Send-TeamsMessageBody -Uri $URI -Body $Body -Wrap
    } else {
        $Body
    }
}