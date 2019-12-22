Send-TeamsMessage -Verbose {
    New-TeamsSection {
        ActivityTitle -Title "**Elon Musk**"
        ActivitySubtitle -Subtitle "@elonmusk - 9/12/2016 at 5:33pm"
        ActivityImageLink -Link "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg"
        ActivityText -Text "Climate change explained in comic book form by xkcd xkcd.com/1732"
    }
    New-TeamsSection {
        ActivityTitle -Title "**Mark Knopfler**"
        ActivitySubtitle -Subtitle "@MarkKnopfler - 9/12/2016 at 1:12pm"
        ActivityImageLink -Link "https://pbs.twimg.com/profile_images/1042367841117384704/YvrqQiBK_400x400.jpg"
        ActivityText -Text "Mark Knopfler features on B.B King's all-star album of Blues greats, released on this day in 2005..."
    }
    New-TeamsSection {
        ActivityTitle -Title "**Elon Musk**"
        ActivitySubtitle -Subtitle "@elonmusk - 9/12/2016 at 5:33pm"
        ActivityImageLink -Link "https://pbs.twimg.com/profile_images/782474226020200448/zDo-gAo0.jpg"
        ActivityText -Text "Climate change explained in comic book form by xkcd xkcd.com/1732"
    }
} -Uri $TeamsID -Color DarkSeaGreen -MessageSummary 'Tweet'