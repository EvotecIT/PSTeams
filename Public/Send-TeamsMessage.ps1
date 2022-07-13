function Send-TeamsMessage {
    [alias('TeamsMessage')]
    [CmdletBinding()]
    Param (
        [scriptblock] $SectionsInput,
        [alias("TeamsID", 'Url')][Parameter(Mandatory)][string]$Uri,
        [string]$MessageTitle,
        [string]$MessageText,
        [string]$MessageSummary,
        [string]$Color,
        [switch]$HideOriginalBody,
        [Uri]$Proxy,
        [System.Collections.IDictionary[]]$Sections,
        [alias('Supress')][bool] $Suppress = $true
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
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Output `
        -MessageSummary $MessageSummary `
        -HideOriginalBody:$HideOriginalBody.IsPresent
    Write-Verbose "Send-TeamsMessage - Body $Body"
    try {
        $Params = @{
            Uri         = $Uri
            Method      = 'Post'
            Body        = $Body
            ContentType = 'application/json; charset=UTF-8'
            ErrorAction = 'Stop'
        }
        if ($Proxy) {
            $Params.Proxy = $Proxy
        }
        $Execute = Invoke-RestMethod @Params
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error "Couldn't send message. Error $ErrorMessage"
        } else {
            Write-Warning "Send-TeamsMessage - Couldn't send message. Error: $ErrorMessage"
        }
    }
    Write-Verbose "Send-TeamsMessage - Execute $Execute"
    if ($Execute -like '*failed*' -or $Execute -like '*error*') {
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error "Send-TeamsMessage - Couldn't send message. Execute message: $Execute"
        } else {
            Write-Warning "Send-TeamsMessage - Couldn't send message. Execute message: $Execute"
        }
    }
    if (-not $Suppress) { return $Body }
}

Register-ArgumentCompleter -CommandName Send-TeamsMessage -ParameterName Color -ScriptBlock { $Script:RGBColors.Keys }