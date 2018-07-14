function Repair-Text {
    [CmdletBinding()]
    param(
        [string] $Text
    )
    if ($Text -ne $null) {
        $Text = $Text.ToString().Replace('"', '\"').Replace('\', '\\').Replace("`n", '\n\n').Replace("`r", '').Replace("`t", '\t')
        $Text = [System.Text.RegularExpressions.Regex]::Unescape($($Text))
    }
    if ($Text -eq '') { $Text = ' ' }
    return $Text
}
function Get-Image {
    [CmdletBinding()]
    param(
        [string] $PathToImages,
        [string] $FileName,
        [string] $FileExtension
    )
    if ($ImageType -ne [ImageType]::None) {
        $ImagePath = "$PathToImages\$($MessageType)$FileExtension"
        if (Test-Path $ImagePath) {
            $Image = [convert]::ToBase64String((Get-Content $ImagePath -Encoding byte))
            return "data:image/png;base64,$Image"
        }
    }
    return ''
}
function Convert-FromColor {
    [CmdletBinding()]
    param(
        [nullable[System.Drawing.Color]]$Color
    )
    if ($Color -ne $null) {
        $Value = $Color.R, $Color.G, $Color.B
        foreach ($arg in $Value) {
            $hexval = $hexval + [Convert]::ToString($arg, 16).ToUpper()
        }
        return "#$($hexval)" # .Substring(2)
    } else { '' }
}

function Add-TeamsMessageButtons {
    param(
        $buttons
    )
    $PotentialAction = @()
    foreach ($button in $buttons) {
        $PotentialAction += @{
            '@context' = 'http://schema.org'
            '@type'    = 'ViewAction'
            name       = $($button.Name)
            target     = @("$($button.Value)")
        }
    }
    return $PotentialAction
}
function Add-TeamsSection {
    param(
        $Sections
    )
    $PreparedSections = @()
    foreach ($section in $Sections) {
        $PreparedSections += $section
    }
    return $PreparedSections
}

function Add-TeamsBody {
    param (
        $MessageTitle,
        $ThemeColor,
        $Text,
        $Sections
    )

    $Body = ConvertTo-Json -Depth 6 @{
        title             = $MessageTitle
        themeColor        = $ThemeColor
        $([string] $Type)	= Repair-Text $($Text)
        sections          = $Sections

    }
    return $Body
}

function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [TeamsType]$Type = [TeamsType]::Summary,
        [string]$Text,
        [string]$MessageTitle,
        [string]$ActivityTitle,
        [string]$ActivitySubtitle,
        [array]$details = $null,
        [string]$detailTitle,
        [nullable[System.Drawing.Color]]$Color,
        [array]$Buttons = $null,
        [Parameter(Mandatory = $true)][string]$URI,
        [ImageType]$ImageType = [ImageType]::None,
        [switch]$ImageLink,
        [bool] $Supress = $true
    )
    [bool] $UseImage = $false


    $StoredImages = "$(Split-Path -Path $PSScriptRoot -Parent)\Images"
    $Image = Get-Image -PathToImages $StoredImages -FileName $ImageType -FileExtension '.jpg'
    $ThemeColor = Convert-FromColor -Color $Color
    Write-Verbose "Send-TeamsMessage - Color $Color"
    Write-Verbose "Send-TeamsMessage - Color HEX $ThemeColor"

    $PotentialAction = Add-TeamsMessageButtons $Buttons

    $Section1 = @{
        activityTitle    = $ActivityTitle
        activitySubtitle = $ActivitySubtitle
        activityImage    = $Image
    }
    $Section2 = @{
        title           = $detailTitle
        facts           = $details
        potentialAction = @(
            $PotentialAction
        )
    }
    $Sections = Add-TeamsSection $Section1, $Section2
    $Body = Add-TeamsBody -MessageTitle $MessageTitle -ThemeColor $ThemeColor -Text $Text -Sections $Sections
    $Execute = Invoke-RestMethod -uri $uri -Method Post -body $Body -ContentType 'application/json'
    Write-Verbose "Send-TeamChannelMessage - Body $Body"
    Write-Verbose "Send-TeamChannelMessage - Execute $Execute"
    if ($Supress) { } else { return $Body }

}

function Send-TeamsMessageBody {
    param (
        [string] $uri,
        [string] $body
    )
    $Execute = Invoke-RestMethod -uri $uri -Method Post -body $body -ContentType 'application/json'
}