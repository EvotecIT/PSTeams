param(
    $TeamsID = $Env:TEAMSPESTERID
)

Describe 'New-AdaptiveCard - Should send message properly' {
    It 'Adaptive Card with nested adapted card' {
        $Output = New-AdaptiveCard -Uri $TeamsID {
            New-AdaptiveContainer {
                New-AdaptiveTextBlock -Text 'Publish Adaptive Card schema' -Weight Bolder -Size Medium
                New-AdaptiveColumnSet {
                    New-AdaptiveColumn -Width auto {
                        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
                    }
                    New-AdaptiveColumn -Width stretch {
                        New-AdaptiveTextBlock -Text "Matt Hidinger" -Weight Bolder -Wrap
                        New-AdaptiveTextBlock -Text "Created {{DATE(2017-02-14T06:08:39Z, SHORT)}}" -Subtle -Spacing None -Wrap
                    }
                }
            }
            New-AdaptiveContainer {
                New-AdaptiveTextBlock -Text "Now that we have defined the main rules and features of the format, we need to produce a schema and publish it to GitHub. The schema will be the starting point of our reference documentation." -Wrap
                New-AdaptiveFactSet {
                    New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Card'
                    New-AdaptiveFact -Title 'List:' -Value 'Backlog'
                    New-AdaptiveFact -Title 'Assigned to:' -Value 'Matt Hidinger'
                    New-AdaptiveFact -Title 'Due date:' -Value 'Not set'
                }
            }
        } -Action {
            New-AdaptiveAction -Title 'Set due date' -Type Action.Submit
            New-AdaptiveAction -Title 'Comment'
            New-AdaptiveAction -Title 'Show Nested, but limited Adaptive Card' -Body {
                New-AdaptiveTextBlock -Text 'Publish Adaptive Card schema' -Weight Bolder -Size Medium
                New-AdaptiveColumnSet {
                    New-AdaptiveColumn -Width auto {
                        New-AdaptiveImage -Url "https://pbs.twimg.com/profile_images/3647943215/d7f12830b3c17a5a9e4afcc370e3a37e_400x400.jpeg" -Size Small -Style person
                    }
                    New-AdaptiveColumn -Width stretch {
                        New-AdaptiveTextBlock -Text "Matt Hidinger" -Weight Bolder -Wrap
                        New-AdaptiveTextBlock -Text "Created {{DATE(2017-02-14T06:08:39Z, SHORT)}}" -Subtle -Spacing None -Wrap
                    }
                }
                New-AdaptiveFactSet {
                    New-AdaptiveFact -Title 'Board:' -Value 'Adaptive Card'
                    New-AdaptiveFact -Title 'List:' -Value 'Backlog'
                    New-AdaptiveFact -Title 'Assigned to:' -Value 'Matt Hidinger'
                    New-AdaptiveFact -Title 'Due date:' -Value 'Not set'
                }
            }
        } -ErrorAction Stop
        $Output | Should -Be $null
    } -TestCases @{ TeamsID = $TeamsID }
}