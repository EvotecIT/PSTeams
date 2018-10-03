function Send-TeamsMessageBody {
    param (
        [string] $uri,
        [string] $body
    )
    try {
        $Execute = Invoke-RestMethod -uri $uri -Method Post -body $body -ContentType 'application/json'
    } catch {
        $ErrorMessage = $_.Exception.Message -replace "`n", " " -replace "`r", " "
        Write-Warning "Send-TeamsMessageBody - Failed with error message: $ErrorMessage"
    }
}