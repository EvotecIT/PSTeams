function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string]$MessageTitle,
        [string]$MessageText,
        [nullable[System.Drawing.Color]]$Color,
        [hashtable[]]$Sections,
        [bool] $Supress = $true
    )
    $ThemeColor = Convert-FromColor -Color $Color
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Sections
    $Execute = Invoke-RestMethod -Uri $uri -Method Post -Body $Body -ContentType 'application/json'
    Write-Verbose "Send-TeamsMessage - Color $Color Color HEX $ThemeColor"
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if ($Supress) { } else { return $Body }

}