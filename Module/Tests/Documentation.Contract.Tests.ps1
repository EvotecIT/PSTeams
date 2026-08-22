Describe 'Generated command documentation contract' {
    BeforeAll {
        Get-Module PSTeams, MessageX.PowerShell | Remove-Module -Force -ErrorAction SilentlyContinue
        Import-Module "$PSScriptRoot\..\PSTeams\PSTeams.psd1" -Force

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
            Get-Command -Module PSTeams |
                Where-Object CommandType -in @('Cmdlet', 'Function') |
                Select-Object -ExpandProperty Name -Unique
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
}
