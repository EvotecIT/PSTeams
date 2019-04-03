function Send-TeamsMessageBody {
    param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string] $Body
    )
    try {
        $Execute = Invoke-RestMethod -Uri $uri -Method Post -Body $Body -ContentType 'application/json; charset=UTF-8'
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        Write-Warning "Send-TeamsMessageBody - Failed with error message: $ErrorMessage"
    }
}