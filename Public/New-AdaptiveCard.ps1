function New-AdaptiveCard {
    [cmdletBinding()]
    param(
        [scriptblock] $Content,
        [string] $Uri
    )

    if ($Content) {
        $Wrapper = [ordered]@{
            "type"        = "message"
            "attachments" = @(
                [ordered] @{
                    "contentType" = 'application/vnd.microsoft.card.adaptive'
                    "content"     = [ordered]@{
                        '$schema' = "http://adaptivecards.io/schemas/adaptive-card.json"
                        "type"    = "AdaptiveCard"
                        "version" = "1.2"
                        "body"    = @(
                            & $Content
                        )
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
                        "msteams" = @{
                            "entities" = @(
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
                            )
                        }
                    }
                }
            )
        }
        $Body = $Wrapper | ConvertTo-Json -Depth 20
        Send-TeamsMessageBody -Uri $URI -Body $Body -Verbose
    }
}