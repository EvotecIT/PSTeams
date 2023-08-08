New-AdaptiveCard -Uri $Env:TEAMSPESTERID {
    New-AdaptiveContainer {
        New-AdaptiveTextBlock -Text 'Publish Adaptive Card schema' -Weight Bolder -Size Medium
        New-AdaptiveColumnSet {
            New-AdaptiveColumn -Width auto {
                New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
            }
            New-AdaptiveColumn -Width stretch {
                New-AdaptiveTextBlock -Text "Przemysław Kłys <at>Przemysław Kłys</at>" -Weight Bolder -Wrap
                New-AdaptiveTextBlock -Text "Created {{DATE(2017-02-14T06:08:39Z, SHORT)}}" -Subtle -Spacing None -Wrap
            }
        }
    }
    New-AdaptiveContainer {
        New-AdaptiveTextBlock -Text "Now that we have defined the main `n`n rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation." -Wrap
        New-AdaptiveFactSet {
            New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Card'
            New-AdaptiveFact -Title 'List:' -Value 'Backlog'
            New-AdaptiveFact -Title 'Assigned to:' -Value 'Matt Hidinger'
            New-AdaptiveFact -Title 'Due date:' -Value 'Not set'
        }
    }
    # we need to tell PSTeams to match <at> tags to the user's profile using UPN or AAD ID
    New-AdaptiveMention -Text 'Przemysław Kłys' -UserPrincipalName 'przemyslaw.klys@domain.pl'
} -FullWidth -Verbose