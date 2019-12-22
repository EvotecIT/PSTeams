function Send-TeamsMessageBody {
    [alias('TeamsMessageBody')]
    [CmdletBinding()]
    param (
        [alias("TeamsID", 'Url')][Parameter(Mandatory = $true)][string]$Uri,
        [string] $Body,
        [bool] $Supress = $true
    )
    try {
        $Execute = Invoke-RestMethod -Uri $Uri -Method Post -Body $Body -ContentType 'application/json; charset=UTF-8'
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        Write-Warning "Send-TeamsMessageBody - Failed with error message: $ErrorMessage"
    }
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if (-not $Supress) { return $Body }
}