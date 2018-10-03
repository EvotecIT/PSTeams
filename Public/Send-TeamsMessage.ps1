function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string]$MessageTitle,
        [string]$MessageText,
        [Object] $Color,
        [hashtable[]]$Sections,
        [bool] $Supress = $true
    )
    try {
        $ThemeColor = Convert-FromColor -Color $Color
    } catch {
        $ThemeColor = $null
    }
    Write-Verbose "Send-TeamsMessage - Color $Color vs $ThemeColor"
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Sections
    $Execute = Invoke-RestMethod -Uri $uri -Method Post -Body $Body -ContentType 'application/json'
    Write-Verbose "Send-TeamsMessage - Color $Color Color HEX $ThemeColor"
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if ($Supress) { } else { return $Body }

}