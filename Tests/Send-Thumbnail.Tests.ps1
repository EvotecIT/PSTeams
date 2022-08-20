param(
    $TeamsID = $Env:TEAMSPESTERID
)

Describe 'New-ThumbnailCard - Should send message properly' {
    It 'Should not throw error' {
        $Output = New-ThumbnailCard -Title 'Bender' -SubTitle "tale of a robot who dared to love" -Text "Bender Bending Rodríguez is a main character in the animated television series Futurama. He was created by series creators Matt Groening and David X. Cohen, and is voiced by John DiMaggio" {
            New-ThumbnailImage -Url 'https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png' -AltText "Bender Rodríguez"
            New-ThumbnailButton -Type imBack -Title 'Thumbs Up' -Value 'I like it' -Image "http://moopz.com/assets_c/2012/06/emoji-thumbs-up-150-thumb-autox125-140616.jpg"
            New-ThumbnailButton -Type openUrl -Title 'Thumbs Down' -Value 'https://evotec.xyz'
            New-ThumbnailButton -Type openUrl -Title 'I feel luck' -Value 'https://www.bing.com/images/search?q=bender&qpvt=bender&qpvt=bender&qpvt=bender&FORM=IGRE'
        } -Uri $TeamsID -ErrorAction Stop
        $Output | Should -Be $null
    } -TestCases @{ TeamsID = $TeamsID }
}