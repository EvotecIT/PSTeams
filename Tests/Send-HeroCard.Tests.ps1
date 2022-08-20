param(
    $TeamsID = $Env:TEAMSPESTERID
)

Describe 'New-HeroCard - Should send message properly' {
    It 'Should not throw error' {
        $Output = New-HeroCard -Title 'Seattle Center Monorail' -SubTitle 'Seattle Center Monorail' -Text "The Seattle Center Monorail is an elevated train line between Seattle Center (near the Space Needle) and downtown Seattle. It was built for the 1962 World's Fair. Its original two trains, completed in 1961, are still in service." {
            New-HeroImage -Url 'https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg'
            New-HeroButton -Type openUrl -Title 'Official website' -Value 'https://www.seattlemonorail.com'
            New-HeroButton -Type openUrl -Title 'Wikipeda page' -Value 'https://www.seattlemonorail.com'
            New-HeroButton -Type imBack -Title 'Evotec page' -Value 'https://www.evotec.xyz'
        } -Uri $TeamsID -ErrorAction Stop
        $Output | Should -Be $null
    } -TestCases @{ TeamsID = $TeamsID }
}