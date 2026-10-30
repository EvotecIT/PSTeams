Describe 'Generated command documentation contract' {
    BeforeAll {
        Get-Module MessageX, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\MessageX\MessageX.psd1" -Force

        $docsCandidates = @(
            [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\Docs'))
            [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\Docs'))
        )
        $script:DocsPath = @($docsCandidates | Where-Object { Test-Path -LiteralPath $_ -PathType Container })[0]
        if (-not $script:DocsPath) {
            throw "Unable to locate generated command documentation from '$PSScriptRoot'."
        }
        $script:DocumentedCommands = @(
            Get-ChildItem -LiteralPath $script:DocsPath -Filter '*.md' -File |
                Where-Object {
                    Get-Content -LiteralPath $_.FullName -TotalCount 8 |
                        Where-Object { $_ -match '^external help file:' }
                } |
                Select-Object -ExpandProperty BaseName
        )
        $script:ExportedCommands = @(
            Get-Command -Module MessageX |
                Where-Object CommandType -in @('Cmdlet', 'Function') |
                Select-Object -ExpandProperty Name -Unique
        )
        $helpPath = Join-Path $PSScriptRoot '..\MessageX\en-US\MessageX-help.xml'
        [xml] $externalHelp = Get-Content -LiteralPath $helpPath -Raw
        $script:ExternalHelp = $externalHelp
        $script:ExternalHelpCommands = @(
            $externalHelp.SelectNodes(
                "/*[local-name()='helpItems']/*[local-name()='command']/*[local-name()='details']/*[local-name()='name']") |
                ForEach-Object InnerText
        )
    }

    It 'contains no unfinished parameter-description placeholders' {
        $placeholders = @(
            Get-ChildItem -LiteralPath $script:DocsPath -Filter '*.md' -File |
                Select-String -Pattern '\{\{ Fill .+ Description \}\}'
        )

        $placeholders | Should -BeNullOrEmpty
    }

    It 'documents every exported command and only existing commands' {
        $missingDocs = @($script:ExportedCommands | Where-Object { $_ -notin $script:DocumentedCommands })
        $staleDocs = @($script:DocumentedCommands | Where-Object { $_ -notin $script:ExportedCommands })

        $missingDocs | Should -BeNullOrEmpty
        $staleDocs | Should -BeNullOrEmpty
    }

    It 'includes external help for every exported command and only existing commands' {
        $missingHelp = @($script:ExportedCommands | Where-Object { $_ -notin $script:ExternalHelpCommands })
        $staleHelp = @($script:ExternalHelpCommands | Where-Object { $_ -notin $script:ExportedCommands })

        $missingHelp | Should -BeNullOrEmpty
        $staleHelp | Should -BeNullOrEmpty
    }

    It 'shows the installed script-block builder contract in external help' {
        $builderParameters = @{
            'New-TeamsAdaptiveActionSet' = @('Actions')
            'New-TeamsAdaptiveCard' = @('Actions', 'Body')
            'New-TeamsAdaptiveColumn' = @('Items')
            'New-TeamsAdaptiveColumnSet' = @('Columns')
            'New-TeamsAdaptiveContainer' = @('Items')
            'New-TeamsAdaptiveFactSet' = @('Facts')
            'New-TeamsAdaptiveImageSet' = @('Images')
        }

        foreach ($commandName in $builderParameters.Keys) {
            $command = Get-Command -Name $commandName -Module MessageX
            $helpCommand = $script:ExternalHelp.SelectSingleNode(
                "/*[local-name()='helpItems']/*[local-name()='command'][*[local-name()='details']/*[local-name()='name' and text()='$commandName']]")
            $helpCommand | Should -Not -BeNullOrEmpty

            foreach ($parameterName in $builderParameters[$commandName]) {
                $command.Parameters[$parameterName].ParameterType.Name | Should -Be 'ScriptBlock'
                $helpParameter = $helpCommand.SelectSingleNode(
                    "*[local-name()='parameters']/*[local-name()='parameter'][*[local-name()='name' and text()='$parameterName']]")
                $helpParameter | Should -Not -BeNullOrEmpty
                $helpParameter.SelectSingleNode("*[local-name()='type']/*[local-name()='name']").InnerText |
                    Should -Be 'ScriptBlock'
            }
        }

        (Get-Help New-TeamsAdaptiveCard -Examples | Out-String) | Should -Match 'New-TeamsAdaptiveCard -Body \{'
    }
}
