function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [scriptblock] $SectionsInput,
        [alias("TeamsID", 'Url')][Parameter(Mandatory = $true)][string]$Uri,
        [string]$MessageTitle,
        [string]$MessageText,
        [string]$MessageSummary,
        [RGBColors] $Color,
        [System.Collections.IDictionary[]]$Sections,
        [bool] $Supress = $true,
        [switch] $ShowErrors
    )
    if (-not $MessageText -and -not $MessageTitle -and -not $MessageSummary) {
        Write-Warning 'Send-TeamsMessage - MessageText or MessageTitle or MessageSummary is required.'
        return
    }

    if ($SectionsInput) {
        $Output = & $SectionsInput
    } else {
        $Output = $Sections
    }

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
    Write-Verbose "Send-TeamsMessage - Color: $Color Color HEX $ThemeColor"
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Output `
        -MessageSummary $MessageSummary
    try {
        $Execute = Invoke-RestMethod -Uri $Uri -Method Post -Body $Body -ContentType 'application/json; charset=UTF-8'
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        if ($ShowErrors) {
            Write-Error "Couldn't send message. Error $ErrorMessage"
        } else {
            Write-Warning "Send-TeamsMessage - Couldn't send message. Error: $ErrorMessage"
        }
    }
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if (-not $Supress) { return $Body }
}