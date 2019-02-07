function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string]$MessageTitle,
        [string]$MessageText,
        [RGBColors] $Color,
        [hashtable[]]$Sections,
        [bool] $Supress = $true
    )
    if ($null -ne $Color) {
        try {
            $ThemeColor = ConvertFrom-Color -Color $Color
        } catch {
            $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
            Write-Warning "Send-TeamsMessage - Color conversion for $Color failed. Error message: $ErrorMessage"
            $ThemeColor = $null
        }
    }
    Write-Verbose "Send-TeamsMessage - Color: $Color ColorConverted: $ThemeColor"
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Sections
    $Execute = Invoke-RestMethod -Uri $uri -Method Post -Body $Body -ContentType 'application/json'
    Write-Verbose "Send-TeamsMessage - Color $Color Color HEX $ThemeColor"
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if ($Supress) { } else { return $Body }

}