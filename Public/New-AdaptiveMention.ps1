function New-AdaptiveMention {
    <#
    .SYNOPSIS
    Allows Adaptive Card to mention specific person in the notification.

    .DESCRIPTION
    Allows Adaptive Card to mention specific person in the notification.
    Currently Adaptive Card in incoming webhook notifications allows you to define a mention property, but it doesn't notify the user on notification.
    It's supposed to be enabled in future by Microsoft.

    .PARAMETER Text
    Provide the text to be mentioned by using <at>person</at> tag. You can provide tags or skip them. Should work either way.

    .PARAMETER UserPrincipalName
    Provide the user principal name of the person to be mentioned.

    .PARAMETER Name
    Provide the name of the person to be mentioned. This is optional.

    .EXAMPLE
    New-AdaptiveMention -Text '<at>przemyslaw.klys</at>' -UserPrincipalName 'przemyslaw.klys@evotec.test'

    .EXAMPLE
    New-AdaptiveMention -Text 'przemyslaw.klys' -UserPrincipalName 'przemyslaw.klys@evotec.test' -Name 'Przemysław Kłys'

    .EXAMPLE
    New-AdaptiveCard -Uri $URI -VerticalContentAlignment center {
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
    More information here: https://github.com/EvotecIT/PSTeams/issues/17

    #>
    [cmdletBinding()]
    param(
        [parameter(Mandatory)][string] $Text,
        [parameter(Mandatory)][string] $UserPrincipalName,
        [string] $Name
    )

    $Mention = [ordered] @{
        "type"      = "mention"
        "text"      = if ($Text -like "*<at>*") { $Text } else { "<at>$Text</at>" }
        "mentioned" = @{
            id   = $UserPrincipalName
            name = $Name
        }
    }
    if ($Name) {
        $Mention.mentioned.name = $Name
    }
    $Mention
}