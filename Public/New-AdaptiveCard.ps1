function New-AdaptiveCard {
    <#
    .SYNOPSIS
    An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

    .DESCRIPTION
    An Adaptive Card, containing a free-form body of card elements, and an optional set of actions.

    .PARAMETER Body
    The card elements to show in the primary card region.

    .PARAMETER Action
    The Actions to show in the card's action bar.

    .PARAMETER Uri
    WebHook Uri to send Adaptive Card to. When provided sends Adaptive Card. When not provided JSON is returned.

    .PARAMETER FallBackText
    Text shown when the client doesn't support the version specified (may contain markdown).

    .PARAMETER MinimumHeight
    Specifies the minimum height of the card.

    .PARAMETER Speak
    Specifies what should be spoken for this entire card. This is simple text or SSML fragment.

    .PARAMETER Language
    The 2-letter ISO-639-1 language used in the card. Used to localize any date/time functions.

    .PARAMETER VerticalContentAlignment
    Defines how the content should be aligned vertically within the container. Only relevant for fixed-height cards, or cards with a minHeight specified.

    .PARAMETER BackgroundUrl
    Specifies a background image. Acceptable formats are PNG, JPEG, and GIF

    .PARAMETER BackgroundFillMode
    Controls how background is displayed

    "cover": The background image covers the entire width of the container. Its aspect ratio is preserved. Content may be clipped if the aspect ratio of the image doesn't match the aspect ratio of the container. verticalAlignment is respected (horizontalAlignment is meaningless since it's stretched width). This is the default mode and is the equivalent to the current model.
    "repeatHorizontally": The background image isn't stretched. It is repeated in the x axis as many times as necessary to cover the container's width. verticalAlignment is honored (default is top), horizontalAlignment is ignored.
    "repeatVertically": The background image isn't stretched. It is repeated in the y axis as many times as necessary to cover the container's height. verticalAlignment is ignored, horizontalAlignment is honored (default is left).
    "repeat": The background image isn't stretched. It is repeated first in the x axis then in the y axis as many times as necessary to cover the entire container. Both horizontalAlignment and verticalAlignment are honored (defaults are left and top).

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

    .PARAMETER FullWidth
    Provide ability to make card full width. By default it set to small.

    .PARAMETER AllowImageExpand
    Defines that image may expand

    .PARAMETER ReturnJson
    Defines that JSON should be returned even when Uri is provided. Useful for debugging

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
            New-AdaptiveColumn {
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 <at>Name</at>' -Color Warning
                New-AdaptiveTextBlock -Size 'Medium' -Text 'Test Card Title 1 <at>Zenon Jaskuła</at>' -Color Warning
            }
        }
        New-AdaptiveMention -Text 'Zenon Jaskuła' -UserPrincipalName 'przemyslaw.klys@evotec.test'
        New-AdaptiveMention -Text 'Name' -UserPrincipalName 'przemyslaw.klys@evotec.test'
    } -Verbose -FullWidth

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
        [string] $SelectActionTitle,
        [switch] $FullWidth,
        [switch] $AllowImageExpand,
        [switch] $ReturnJson
    )
    $Mentions = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
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
                            $OutputBody = & $Body
                            foreach ($B in $OutputBody) {
                                if ($B.type -eq 'mention') {
                                    $Mentions.Add($B)
                                } else {
                                    $B
                                }
                            }
                        }
                    )
                    actions   = @(
                        if ($Action) {
                            & $Action
                        }
                    )
                    msteams   = [ordered]@{

                    }
                }
            }
        )
    }
    if ($AllowImageExpand) {
        $Wrapper['attachments'][0]['content']['msteams']['allowExpand'] = $true
    }
    if ($FullWidth) {
        $Wrapper['attachments'][0]['content']['msteams']['width'] = 'Full'
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
    if ($SelectActionUrl) {
        # We help user so the actioon choses itself
        $SelectAction = 'Action.OpenUrl'
    }
    $Wrapper['attachments'][0]['content']['selectAction'] = [ordered] @{
        type  = $SelectAction
        id    = $SelectActionId
        title = $SelectActionTitle
        url   = $SelectActionUrl
    }

    if ($Mentions.Count -gt 0) {
        # this somewhat works, except it doesn't
        $Wrapper['attachments'][0]['content']["msteams"]["entities"] = @(
            foreach ($Mention in $Mentions) {
                $Mention
            }
            <#
                @{
                    "type"      = "mention"
                    "text"      = "<at>przemyslaw.klys</at>"
                    "mentioned" = @{
                        #"id"   = "8:orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d"
                        #"id" = '29:49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                        #"id" = '29:orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                        #"id" = '19:b6b525a2187848ddb257f59e374363bd'
                        #"id" = 'orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                        #"id" = '49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                        #"name" = "przemyslaw.klys"

                        id   = 'przemyslaw.klys@evotec.pl'
                        name = 'Przemysław Kłys'

                    }
                }
            #>
        )
    }
    <#
    #>
    <# this doesn't work, but tested
    $Wrapper["msteams"] = @{
        "entities" = @(
            @{
                "type"      = "mention"
                "text"      = "<at>przemyslaw.klys</at>"
                "mentioned" = @{
                    "id"   = "8:orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d"
                    #"id" = '29:49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                    #"id" = 'orgid:49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                    #"id" = '49f7e27a-ce6c-45ef-9936-6ef3e940583d'
                    "name" = "przemyslaw.klys"
                }
            }
        )
    }
    #>

    Remove-EmptyValue -Hashtable $Wrapper['attachments'][0]['content'] -Recursive -Rerun 1
    $JsonBody = $Wrapper | ConvertTo-JsonLiteral -Depth 20 #ConvertTo-Json -Depth 20
    #$New = $JsonBody | Format-Json -Indentation 4
    # $JsonBody = $Wrapper | ConvertTo-Json -Depth 20
    # If URI is not given we return JSON. This is because it's possible to use nested Adaptive Cards in actions
    if ($Uri) {
        Send-TeamsMessageBody -Uri $URI -Body $JsonBody #-Verbose
        if ($ReturnJson) {
            $JsonBody
        }
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