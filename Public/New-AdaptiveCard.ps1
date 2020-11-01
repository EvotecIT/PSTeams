function New-AdaptiveCard {
    <#
    .SYNOPSIS
    Short description

    .DESCRIPTION
    Long description

    .PARAMETER Content
    Parameter description

    .PARAMETER Action
    Parameter description

    .PARAMETER Uri
    Parameter description

    .PARAMETER FallBackText
    Parameter description

    .PARAMETER MinimumHeight
    Specifies the minimum height of the card.

    .PARAMETER Speak
    Specifies what should be spoken for this entire card. This is simple text or SSML fragment.

    .PARAMETER Language
    The 2-letter ISO-639-1 language used in the card. Used to localize any date/time functions.

    .PARAMETER VerticalContentAlignment
    .EXAMPLE
    An example

    .NOTES
    General notes
    #>
    [cmdletBinding()]
    param(
        [scriptblock] $Content,
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
        [ValidateSet('top', 'center', 'bottom')][string] $BackgroundVerticalAlignment
    )
    $Wrapper = [ordered]@{
        "type"        = "message"
        "attachments" = @(
            [ordered] @{
                "contentType" = 'application/vnd.microsoft.card.adaptive'
                "content"     = [ordered]@{
                    '$schema' = "http://adaptivecards.io/schemas/adaptive-card.json"
                    type      = "AdaptiveCard"
                    version   = "1.2"
                    body      = @(
                        if ($Content) {
                            & $Content
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
    Remove-EmptyValue -Hashtable $Wrapper['attachments'][0]['content'] -Recursive -Rerun 2
    $Body = $Wrapper | ConvertTo-Json -Depth 20
    # If URI is not given we return JSON. This is because it's possible to use nested Adaptive Cards in actions
    if ($Uri) {
        Send-TeamsMessageBody -Uri $URI -Body $Body #-Verbose
    } else {
        $Body
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