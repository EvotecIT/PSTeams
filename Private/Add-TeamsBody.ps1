function Add-TeamsBody {
    [CmdletBinding()]
    param (
        [string] $MessageTitle,
        [string] $ThemeColor,
        [string] $MessageText,
        [string] $MessageSummary,
        [System.Collections.IDictionary[]] $Sections
    )

    $Body = [ordered] @{
        sections   = $Sections
    }
    if ($ThemeColor) {
        $body.themeColor = $ThemeColor
    }
    if ($MessageTitle) {
        $Body.title = $MessageTitle
    }
    if ($MessageSummary -ne '') {
        $Body.summary = $MessageSummary
    } else {
        if ($MessageTitle -ne '') {
            $Body.summary = $MessageTitle
        } elseif ($MessageText -ne '') {
            $Body.summary = $MessageText
        }
    }
    if ($MessageText -ne '') {
        $Body.text = $MessageText
    }
    return $Body | ConvertTo-Json -Depth 6
}