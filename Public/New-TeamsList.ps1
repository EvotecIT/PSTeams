function New-TeamsList {
    [CmdletBinding()]
    param(
        [scriptblock] $List,
        [string] $Name
    )

    if ($List) {
        $Output = & $List
        [Array] $Fact = foreach ($_ in $Output) {
            if ($_.Numbered) {
                $Type = '1. '
            } else {
                $Type = "- "
            }
            if ($_.Type -eq 'ListItem') {
                "`t" * $_.Level + $Type + $_.Text
            }
        }
        [string] $Value = $Fact -join "`r" #[System.Environment]::NewLine

        New-TeamsFact -Name $Name -Value $Value
    }
}