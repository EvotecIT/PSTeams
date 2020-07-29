function Send-TeamsMessage {
    [alias('TeamsMessage')]
    [CmdletBinding()]
    Param (
        [scriptblock] $SectionsInput,
        [alias("TeamsID", 'Url')][Parameter(Mandatory = $true)][string]$Uri,
        [string]$MessageTitle,
        [string]$MessageText,
        [string]$MessageSummary,
        [string]$Color,
        [switch]$HideOriginalBody,
        [System.Collections.IDictionary[]]$Sections,
        [alias('Supress')][bool] $Suppress = $true,
        [switch] $ShowErrors
    )
    if ($SectionsInput) {
        $Output = & $SectionsInput
    } else {
        $Output = $Sections
    }

    if ($Color -or $Color -ne 'None') {
        try {
            $ThemeColor = ConvertFrom-Color -Color $Color
        } catch {
            $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
            Write-Warning "Send-TeamsMessage - Color conversion for $Color failed. Error message: $ErrorMessage"
            $ThemeColor = $null
        }
    }
    # Write-Verbose "Send-TeamsMessage - Color: $Color ColorConverted: $ThemeColor"
    #Write-Verbose "Send-TeamsMessage - Color: $Color Color HEX $ThemeColor"
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Output `
        -MessageSummary $MessageSummary `
        -HideOriginalBody:$HideOriginalBody.IsPresent
    try {
        $Execute = Invoke-RestMethod -Uri $Uri -Method Post -Body $Body -ContentType 'application/json; charset=UTF-8' -ErrorAction Stop
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        if ($ShowErrors) {
            Write-Error "Couldn't send message. Error $ErrorMessage"
        } else {
            Write-Warning "Send-TeamsMessage - Couldn't send message. Error: $ErrorMessage"
        }
    }
    if ($Execute -like '*failed*' -or $Execute -like '*error*') {
        Write-Warning "Send-TeamsMessage - Couldn't send message. Execute message: $Execute"
    }
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if (-not $Suppress) { return $Body }
}

Register-ArgumentCompleter -CommandName Send-TeamsMessage -ParameterName Color -ScriptBlock { $Script:RGBColors.Keys }