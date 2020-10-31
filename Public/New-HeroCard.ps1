function New-HeroCard {
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
                    Write-Warning "New-HeroCard - Herd Card support only up to 6 buttons."
                }
            } else {
                $Images.Add($E)
            }
        }

        $Wrapper = @{
            contentType = "application/vnd.microsoft.card.hero"
            content     = @{
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
        Send-TeamsMessageBody -Uri $URI -Body $Body -Verbose -Wrap
    } else {
        $Body
    }
}