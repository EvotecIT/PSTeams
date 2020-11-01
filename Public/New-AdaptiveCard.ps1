function New-AdaptiveCard {
    <#
    .SYNOPSIS
    An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

    .DESCRIPTION
    An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

    .PARAMETER Body
    The card elements to show in the primary card region.

    .PARAMETER Action
    The Actions to show in the card’s action bar.

    .PARAMETER Uri
    WebHook Uri to send Adaptive Card to. When provided sends Adaptive Card. When not provided JSON is returned.

    .PARAMETER FallBackText
    Text shown when the client doesn’t support the version specified (may contain markdown).

    .PARAMETER MinimumHeight
    Specifies the minimum height of the card.

    .PARAMETER Speak
    Specifies what should be spoken for this entire card. This is simple text or SSML fragment.

    .PARAMETER Language
    The 2-letter ISO-639-1 language used in the card. Used to localize any date/time functions.

    .PARAMETER VerticalContentAlignment
    Defines how the content should be aligned vertically within the container. Only relevant for fixed-height cards, or cards with a minHeight specified.

    .PARAMETER BackgroundUrl
    Specifies the background image of the card.

    .PARAMETER BackgroundFillMode
    Controls how background is displayed

    .PARAMETER BackgroundHorizontalAlignment
    Controls how background is aligned horizontally

    .PARAMETER BackgroundVerticalAlignment
    Controls how background is aligned vertically

    .PARAMETER SelectAction
    An Action that will be invoked when the card is tapped or selected.

    .PARAMETER SelectActionId
    Provide ID for Select Action

    .PARAMETER SelectActionUrl
    Provide URL to open when using SelectAction with Action.OpenUrl

    .PARAMETER SelectActionTitle
    Provide Title for Select Action

    .EXAMPLE
    New-AdaptiveCard -Uri $Env:TEAMSPESTERID -VerticalContentAlignment center {
        New-AdaptiveTextBlock -Size ExtraLarge -Weight Bolder -Text 'Test' -Color Attention -HorizontalAlignment Center
        New-AdaptiveColumnSet {
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Dark
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Light
            }
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Warning
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1' -Color Good
            }
        }
    } -SelectAction Action.OpenUrl -SelectActionUrl 'https://evotec.xyz' -Verbose

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [scriptblock] $Body,
        [scriptblock] $Action,
        [string] $Uri,
        [string] $FallBackText,
        [int] $MinimumHeight,
        [string] $Speak,
        [string] $Language,
        [ValidateSet('top', 'center', 'bottom')][string] $VerticalContentAlignment,

        [string] $BackgroundUrl,
        [ValidateSet('Cover', 'RepeatHorizontally', 'RepeatVertically', 'Repeat')][string] $BackgroundFillMode,
        [ValidateSet('left', 'center', 'right')][string] $BackgroundHorizontalAlignment,
        [ValidateSet('top', 'center', 'bottom')][string] $BackgroundVerticalAlignment,

        [ValidateSet('Action.Submit', 'Action.OpenUrl', 'Action.ToggleVisibility')][string] $SelectAction,
        [string] $SelectActionId,
        [string] $SelectActionUrl,
        [string] $SelectActionTitle
    )
    $Wrapper = [ordered]@{
        "type"        = "message"
        "attachments" = @(
            [ordered] @{
                "contentType" = 'application/vnd.microsoft.card.adaptive'
                "content"     = [ordered]@{
                    '$schema' = "http://adaptivecards.io/schemas/adaptive-card.json"
                    type      = "AdaptiveCard"
                    version   = "1.2" # Currently maximum supported is 1.2 for Teams, available is 1.3
                    body      = @(
                        if ($Body) {
                            & $Body
                        }
                    )
                    actions   = @(
                        if ($Action) {
                            & $Action
                        }
                    )
                }
            }
        )
    }
    if ($MinimumHeight) {
        $Wrapper['attachments'][0]['content']['minHeight'] = "$($MinimumHeight)px"
    }
    # if ($FallBackText) {
    $Wrapper['attachments'][0]['content']['fallbackText'] = $FallBackText
    #  }
    #  if ($Language) {
    $Wrapper['attachments'][0]['content']['lang'] = $Language
    # }
    # if ($Speak) {
    $Wrapper['attachments'][0]['content']['speak'] = $Speak
    # }
    # if ($VerticalContentAlignment) {
    $Wrapper['attachments'][0]['content']['verticalContentAlignment'] = $VerticalContentAlignment
    #}
    #if ($BackgroundUrl) {
    $Wrapper['attachments'][0]['content']['backgroundImage'] = [ordered] @{
        "fillMode"            = $BackgroundFillMode
        "horizontalAlignment" = $BackgroundHorizontalAlignment
        "verticalAlignment"   = $BackgroundVerticalAlignment
        "url"                 = $BackgroundUrl
    }
    #}
    $Wrapper['attachments'][0]['content']['selectAction'] = [ordered] @{
        type  = $SelectAction
        id    = $SelectActionId
        title = $SelectActionTitle
        url   = $SelectActionUrl
    }
    Remove-EmptyValue -Hashtable $Wrapper['attachments'][0]['content'] -Recursive -Rerun 2
    $JsonBody = $Wrapper | ConvertTo-Json -Depth 20
    # If URI is not given we return JSON. This is because it's possible to use nested Adaptive Cards in actions
    if ($Uri) {
        Send-TeamsMessageBody -Uri $URI -Body $JsonBody #-Verbose
    } else {
        $JsonBody
    }
}

<#
    "channelId" = @{
        "entities" = @(
            @{
                "type"      = "mention"
                "text"      = "<at>Name</at>"
                "mentioned" = @{
                    "id"   = "29:124124124124"
                    "name" = "Mungo"
                }
            }
        )
    }
#>


#"msteams" = @{
#"entities" = @(
<#
                                @{
                                    "type"      = "mention"
                                    "text"      = "<at>przemyslawklys</at>"
                                    "mentioned" = @{
                                        "id"   = "8:orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d"
                                        "name" = "przemyslawklys"
                                    }
                                }
                                #>
# Azure ID: 49f7e27a-ce6c-45ef-9936-6ef3e940583d
# AD GUID: d425e1e4-d6b3-4e58-bb24-f96c995fd3a0
<#
                                @{
                                    "type"      = "mention"
                                    "text"      = "<at>Przemysław</at>"
                                    "mentioned" = @{
                                        "id"   = "29:124124124124"
                                        "name" = "Mungo"
                                    }
                                }
                                #>
#)
#}