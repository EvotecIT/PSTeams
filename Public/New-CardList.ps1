function New-CardList {
    [cmdletBinding()]
    param(
        [Parameter(Mandatory)][scriptblock] $Content,
        [string] $Title,
        [string] $Uri
    )
    if ($Content) {
        $Buttons = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $Items = [System.Collections.Generic.List[System.Collections.Specialized.OrderedDictionary]]::new()
        $ExecutedContent = & $Content
        foreach ($E in $ExecutedContent) {
            if ($E.Value) {
                $Buttons.Add($E)
            } else {
                $Items.Add($E)
            }
        }

        $Wrapper = @{
            contentType = "application/vnd.microsoft.teams.card.list"
            content     = @{
                title   = $Title
                items   = @(
                    $Items
                )
                buttons = @(
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