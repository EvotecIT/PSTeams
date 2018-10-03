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