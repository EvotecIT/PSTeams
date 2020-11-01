function New-AdaptiveContainer {
    [cmdletBinding()]
    param(
        [scriptblock] $Items
    )
    if ($Items) {
        $TeamObject = [ordered] @{
            "type"  = "Container"
            "items" = @(
                & $Items
            )
            #"horizontalAlignment" = $HorizontalAlignment
            #"style"               = $Style
            #"height"              = $Height
            #"spacing"             = $Spacing
        }
        Remove-EmptyValue -Hashtable $TeamObject
        $TeamObject
    }
}