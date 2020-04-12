param (
    $TeamsID = $Env:TEAMSPESTERID
)
#Requires -Modules Pester
Import-Module $PSScriptRoot\..\PSTeams.psd1 -Force #-Verbose

Describe "Conversion of different objects to Teams fact" {
    Context "PSObject" {
        It "Convert PSObject to Teams facts" {
            $Output = New-Object -TypeName PSObject -Property ([ordered]@{
                    Application = 'Microsoft Teams'
                    Developer   = 'Microsoft Corporation'
                }
            ) | ConvertTo-TeamsFact
            $Output | Should -HaveCount 2
            $Output | Should -BeOfType 'System.Collections.Specialized.OrderedDictionary'
            $Output[0].type | Should -Be 'fact'
        }
    }
    Context "Unordered dictionary" {
        It "Convert unordered dictionary to Teams facts" {
            $Output = @{
                Application = 'Microsoft Teams'
                Developer   = 'Microsoft Corporation'
            } | ConvertTo-TeamsFact
            
            $Output | Should -HaveCount 2
            $Output | Should -BeOfType 'System.Collections.Specialized.OrderedDictionary'
            $Output[0].type | Should -Be 'fact'
        }
    }
    Context "Ordered dictionary" {
        It "Convert ordered dictionary to Teams facts" {
            $Output = [ordered]@{
                Application = 'Microsoft Teams'
                Developer   = 'Microsoft Corporation'
            } | ConvertTo-TeamsFact
            
            $Output | Should -HaveCount 2
            $Output | Should -BeOfType 'System.Collections.Specialized.OrderedDictionary'
            $Output[0].type | Should -Be 'fact'
        }
    }
    Context "String" {
        It "Throw an error when a plain string is passed in" {
            { 'My Plain String' | ConvertTo-TeamsFact } | Should Throw
        }
    }
}