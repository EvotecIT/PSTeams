function Send-TeamsMessageBody {
    [alias('TeamsMessageBody')]
    [CmdletBinding()]
    param (
        [alias("TeamsID", 'Url')][Parameter(Mandatory = $true)][string]$Uri,
        [string] $Body,
        [bool] $Supress = $true,
        [switch] $Wrap,
        [Uri] $Proxy
    )
    if ($Wrap) {
        $TemporaryBody = ConvertFrom-Json -InputObject $Body
        $Wrapper = [ordered]@{
            "type"        = "message"
            "attachments" = @(
                $TemporaryBody
            )
        }
        $Body = $Wrapper | ConvertTo-Json -Depth 20
    }
    Write-Verbose "Send-TeamsMessage - Body $Body"
    try {
        $Params = @{
            Uri         = $Uri
            Method      = 'Post'
            Body        = $Body
            ContentType = 'application/json; charset=UTF-8'
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
            Write-Warning "Send-TeamsMessageBody - Couldn't send message. Error: $ErrorMessage"
        }
    }
    Write-Verbose "Send-TeamsMessageBody - Execute $Execute"
    if ($Execute -like '*failed*' -or $Execute -like '*error*') {
        if ($PSBoundParameters.ErrorAction -eq 'Stop') {
            Write-Error "Send-TeamsMessageBody - Couldn't send message. Execute message: $Execute"
        } else {
            Write-Warning "Send-TeamsMessageBody - Couldn't send message. Execute message: $Execute"
        }
    }
    if (-not $Supress) { return $Body }
}