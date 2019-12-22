Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

$TeamsID = 'https://outlook.office.com/webhook/a5c7c'

Send-TeamsMessage -Verbose {
    New-TeamsSection -ActivityTitle "**Elon Musk**" -ActivitySubtitle "@elonmusk - 9/12/2016 at 5:33pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg" -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732" {
        New-TeamsButton -Name 'View Link' -Link 'https://evotec.xyz' -Type 'ViewAction'
        #New-TeamsButton -Name 'Input Text' -Type 'TextInput' -Link 'https://evotec.xyz'
        New-TeamsButton -Type OpenURI -Name 'Open Link' -Link 'https://evotec.xyz'
        New-TeamsButton -Type TextInput -Name 'Leave a Comment' -Link 'https://evotec.xyz'
        New-TeamsButton -Type DateInput -Name 'Choose a Date' -Link 'https://evotec.xyz'
        New-TeamsButton -Type HttpPost -Name 'Post Link' -Link 'https://evotec.xyz'
        New-TeamsButton -Type OpenURI -Name 'Open Link' -Link 'https://evotec.xyz'
        New-TeamsImage -Link "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
        New-TeamsImage -Link "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
        New-TeamsImage -Link "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
        New-TeamsBigImage -Link "https://evotec.pl/wp-content/uploads//2015/05/Logo-evotec-012.png"
        New-TeamsBigImage -Link "https://upload.wikimedia.org/wikipedia/commons/thumb/4/49/Seattle_monorail01_2008-02-25.jpg/1024px-Seattle_monorail01_2008-02-25.jpg"
    } -Text 'This is long text that will be added below Big Images. If there would be no big image... it would be just this text.'

    #New-TeamsSection -ActivityTitle "**Mark Knopfler**" -ActivitySubtitle "@MarkKnopfler - 9/12/2016 at 1:12pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/1042367841117384704/YvrqQiBK_400x400.jpg" -ActivityText "Mark Knopfler features on B.B King's all-star album of Blues greats, released on this day in 2005..."
    #New-TeamsSection -StartGroup -ActivityTitle "**Elon Musk**" -ActivitySubtitle "@elonmusk - 9/12/2016 at 5:33pm" -ActivityImageLink "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg" -ActivityText "Climate change explained in comic book form by xkcd xkcd.com/1732"
} -Uri $TeamsID -Color DarkSeaGreen -MessageSummary 'Tweet'