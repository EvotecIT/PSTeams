function ConvertFrom-Color {
    [alias('Convert-FromColor')]
    [CmdletBinding()]
    param (
        [RGBColors] $Color,
        [switch] $AsDecimal
    )

    $Value = $Script:RGBColors."$Color"
    Write-Verbose "Convert-FromColor - Color Name: $Color Value: $Value"
    foreach ($arg in $Value) {
        $hexval = $hexval + [Convert]::ToString($arg, 16).ToUpper()
    }
    if ($AsDecimal) {
        return  [Convert]::ToInt64($hexval,16)
    } else {
        return "#$($hexval)" # .Substring(2)
    }
}