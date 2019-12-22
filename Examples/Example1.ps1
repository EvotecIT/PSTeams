Clear-Host
Import-Module PSTeams -Force #-Verbose

$TeamsID = 'https://outlook.office.com/webhook/a5c7c95a....'

$Color = [RGBColors]::Chocolate

$Button1 = New-TeamsButton -Name 'Visit English Evotec Website' -Link "https://evotec.xyz"
$Button2 = New-TeamsButton -Name 'Visit Polish Evotec Website' -Link "https://evotec.pl"

$Fact1 = New-TeamsFact -Name 'Bold' -Value '**Special Bold value**'
$Fact2 = New-TeamsFact -Name 'Italic and Bold' -Value '***Italic and Bold value***'
$Fact3 = New-TeamsFact -Name 'Italic' -Value 'Date with italic *2010-10-10*'
$Fact4 = New-TeamsFact -Name 'Link example' -Value "[Microsoft](https://www.microsoft.com)"
$Fact5 = New-TeamsFact -Name 'Other link example' -Value "[Evotec](https://evotec.xyz) and some **bold** text"
$Fact6 = New-TeamsFact -Name 'This is how list looks like' -Value "
* hello
    * 2010-10-10
* test
    * another
* test
* hello"
$Fact7 = New-TeamsFact -Name 'This is strike through example' -Value "<strike> This is strike-through </strike>"
$Fact8 = New-TeamsFact -Name 'List example with nested list' -Value "
- One value
- Another value
    - Third value
        - Fourth value
"
$Fact9 = New-TeamsFact -Name 'List example with a twist' -Value "
1. First ordered list item
2. Another item
* Unordered sub-list.
1. Actual numbers don't matter, just that it's a number
    1. Ordered sub-list
    2. Another entry
4. And another item.
"

$Fact10 = New-TeamsFact -Name 'Code highlight' -Value "This is ``showing code highlight`` "
$Fact11 = New-TeamsFact -Name '' -Value "

### As you see I've not added Name at all for this one and it merges a bit with Fact 10

This is going to add horizontal line below. While this line is highlighed.

---

And a block quote
> Block quote

# H1
## H2
### H3
#### H4
##### H5
###### H6

"


$Section1 = New-TeamsSection `
    -ActivityTitle "**Przemyslaw Klys**" `
    -ActivitySubtitle "@przemyslawklys - 9/12/2016 at 5:33pm" `
    -ActivityImageLink "https://pbs.twimg.com/profile_images/1017741651584970753/hGsbJo-o_400x400.jpg" `
    -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732" `
    -Buttons $Button1, $Button2 `
    -ActivityDetails $Fact1, $Fact2

$Section2 = New-TeamsSection `
    -ActivityTitle "**Przemyslaw Klys**" `
    -ActivitySubtitle "@przemyslawklys - 9/12/2016 at 5:33pm" `
    -ActivityImageLink "https://pbs.twimg.com/profile_images/1017741651584970753/hGsbJo-o_400x400.jpg" `
    -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732" `
    -Buttons $Button1 `
    -ActivityDetails $Fact3, $Fact4, $Fact5, $Fact6, $Fact7, $Fact8, $Fact9, $Fact10, $Fact11

$Section3 = New-TeamsSection `
    -ActivityTitle "**Przemyslaw Klys**" `
    -ActivitySubtitle "@przemyslawklys - 9/12/2016 at 5:33pm" `
    -ActivityImage Add `
    -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732" `
    -Buttons $Button1 `
    -ActivityDetails $Fact3, $Fact4, $Fact5, $Fact6, $Fact7, $Fact8, $Fact9, $Fact10, $Fact11

Send-TeamsMessage `
    -URI $TeamsID `
    -MessageTitle 'Message Title' `
    -MessageText 'This is text' `
    -Color Chocolate `
    -Sections $Section2, $Section1 `
    -Verbose

Send-TeamsMessage `
    -URI $TeamsID `
    -MessageTitle 'Message Title' `
    -MessageText 'This is text' `
    -Color Chocolate `
    -Sections $Section3 `
    -Verbose