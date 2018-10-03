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
    Write-Verbose "Get-Image - PathToImages $PathToImages FileName $FileName FileExtension $FileExtension"
    if ($ImageType -ne [ImageType]::None) {
        $ImagePath = [IO.Path]::Combine( $PathToImages, "$($FileName)$FileExtension")
        Write-Verbose "Get-Image - ImagePath $ImagePath"
        if (Test-Path $ImagePath) {
            if ($PSEdition -eq 'Core') {
                $Image = [convert]::ToBase64String((Get-Content $ImagePath -AsByteStream))
            } else {
                $Image = [convert]::ToBase64String((Get-Content $ImagePath -Encoding byte))
            }
            Write-Verbose "Get-Image - Image Type: $($Image.GetType())"
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
        [string] $MessageTitle,
        [string] $ThemeColor,
        [string] $MessageText,
        [hashtable[]] $Sections
    )
    if ($MessageText -ne '') { $Type = 'Text' } else { $Type = 'Summary' }
    $Body = ConvertTo-Json -Depth 6 $([ordered] @{
            title      = "$MessageTitle"
            themeColor = "$ThemeColor"
            $Type      = Repair-Text $($Text)
            sections   = $Sections

        })
    return $Body
}

function New-TeamsSection {
    [CmdletBinding()]
    param (
        [string] $Title,
        [string] $ActivityTitle,
        [string] $ActivitySubtitle ,
        [string] $ActivityImageLink,
        [ImageType] $ActivityImage = [ImageType]::None,
        [string] $ActivityText,
        [hashtable[]]$ActivityDetails,
        [hashtable[]]$Buttons
    )
    if ($ActivityImage -ne [ImageType]::None) {
        $StoredImages = [IO.Path]::Combine("$(Split-Path -Path $PSScriptRoot -Parent)", "Images")
        $ActivityImageLink = Get-Image -PathToImages $StoredImages -FileName $ActivityImage -FileExtension '.jpg' -Verbose
    }

    $Section = [ordered] @{
        title            = $Title
        activityTitle    = "$($ActivityTitle)"
        activitySubtitle = "$($ActivitySubtitle)"
        activityImage    = "$($ActivityImageLink)"
        activityText     = "$($ActivityText)"
        facts            = @(
            $ActivityDetails
        )
        potentialAction  = @(
            $Buttons
        )
    }
    return $Section
}

function New-TeamsFact {
    [CmdletBinding()]
    param (
        [string] $Name,
        [string] $Value
    )
    $Fact = [ordered] @{
        name  = "$Name"
        value = "$Value"
    }
    return $Fact
}

function New-TeamsButton {
    [CmdletBinding()]
    param (
        [string] $Name,
        [string] $Link
    )
    $Button = [ordered] @{
        '@context' = 'http://schema.org'
        '@type'    = 'ViewAction'
        name       = "$Name"
        target     = @("$Link")
    }
    return $Button
}

function Send-TeamsMessage {
    [CmdletBinding()]
    Param (
        [alias("TeamsID")][Parameter(Mandatory = $true)][string]$URI,
        [string]$MessageTitle,
        [string]$MessageText,
        [nullable[System.Drawing.Color]]$Color,
        [hashtable[]]$Sections,
        [bool] $Supress = $true
    )
    $ThemeColor = Convert-FromColor -Color $Color
    $Body = Add-TeamsBody -MessageTitle $MessageTitle `
        -MessageText $MessageText `
        -ThemeColor $ThemeColor `
        -Sections $Sections
    $Execute = Invoke-RestMethod -Uri $uri -Method Post -Body $Body -ContentType 'application/json'
    Write-Verbose "Send-TeamsMessage - Color $Color Color HEX $ThemeColor"
    Write-Verbose "Send-TeamsMessage - Execute $Execute Body $Body"
    if ($Supress) { } else { return $Body }

}

function Send-TeamsMessageBody {
    param (
        [string] $uri,
        [string] $body
    )
    $Execute = Invoke-RestMethod -uri $uri -Method Post -body $body -ContentType 'application/json'
}