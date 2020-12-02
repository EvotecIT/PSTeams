﻿function Add-TeamsBody {
    [CmdletBinding()]
    param (
        [string] $MessageTitle,
        [string] $ThemeColor,
        [string] $MessageText,
        [string] $MessageSummary,
        [System.Collections.IDictionary[]] $Sections,
        [switch] $HideOriginalBody,
        [switch] $DisableTextMarkdown
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
    if ($HideOriginalBody.IsPresent) {
        $Body.hideOriginalBody = $HideOriginalBody.IsPresent
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
    if ($DisableTextMarkdown.IsPresent) {
        $Body = Disable-TextMarkdown -InputObject $Body
    }

    return $Body | ConvertTo-Json -Depth 6
}