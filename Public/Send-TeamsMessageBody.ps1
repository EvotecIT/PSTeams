function Send-TeamsMessageBody {
    param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string] $Body
    )
    try {
        $Execute = Invoke-RestMethod -URI $URI -Method Post -Body $Body -ContentType 'application/json'
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        Write-Warning "Send-TeamsMessageBody - Failed with error message: $ErrorMessage"
    }
}