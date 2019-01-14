function ConvertFrom-Color {
    [alias('Convert-FromColor')]
    [CmdletBinding()]
    param (
        [RGBColors] $Color,
        [switch] $AsDecimal
    )
    $Value = $Script:RGBColors."$Color"
    $HexValue = Convert-Color -RGB $Value
    Write-Verbose "Convert-FromColor - Color Name: $Color Value: $Value HexValue: $HexValue"
    <#
    [string] $HexVal = ''
    foreach ($arg in $Value) {
        $hexval = $hexval + [Convert]::ToString($arg, 16).ToUpper()
    }
    #>
    if ($AsDecimal) {
        return  [Convert]::ToInt64($HexValue, 16)
    } else {
        return "#$($HexValue)"
    }
}