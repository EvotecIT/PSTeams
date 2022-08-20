Describe "Conversion of different objects to Teams fact" {
    Context "Object types: PSObject, unordered dictionary, ordered dictionary and string" {
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
        It "Convert unordered dictionary to Teams facts" {
            $Output = @{
                Application = 'Microsoft Teams'
                Developer   = 'Microsoft Corporation'
            } | ConvertTo-TeamsFact

            $Output | Should -HaveCount 2
            $Output | Should -BeOfType 'System.Collections.Specialized.OrderedDictionary'
            $Output[0].type | Should -Be 'fact'
        }
        It "Convert ordered dictionary to Teams facts" {
            $Output = [ordered]@{
                Application = 'Microsoft Teams'
                Developer   = 'Microsoft Corporation'
            } | ConvertTo-TeamsFact

            $Output | Should -HaveCount 2
            $Output | Should -BeOfType 'System.Collections.Specialized.OrderedDictionary'
            $Output[0].type | Should -Be 'fact'
        }
        It "Throw an error when a plain string is passed in" {
            { 'My Plain String' | ConvertTo-TeamsFact } | Should -Throw
        }
    }
}