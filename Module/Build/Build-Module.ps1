Import-Module PSPublishModule -Force -ErrorAction Stop

Build-Module -ModuleName 'TeamsX' {
    $Manifest = [ordered] @{
        ModuleVersion        = '0.1.X'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = '2ce3429c-d55a-4d79-97d2-a4ac17549936'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'TeamsX is a binary-first PowerShell module for composing and sending Microsoft Teams messages using typed C# cmdlets over the TeamsX .NET library.'
        Tags                 = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'PowerShell', 'Windows', 'MacOS', 'Linux')
        IconUri              = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
        ProjectUri           = 'https://github.com/EvotecIT/PSTeams'
        PowerShellVersion    = '5.1'
    }
    New-ConfigurationManifest @Manifest

    $configurationFormat = [ordered] @{
        RemoveComments                              = $false
        PlaceOpenBraceEnable                        = $true
        PlaceOpenBraceOnSameLine                    = $true
        PlaceOpenBraceNewLineAfter                  = $true
        PlaceOpenBraceIgnoreOneLineBlock            = $false
        PlaceCloseBraceEnable                       = $true
        PlaceCloseBraceNewLineAfter                 = $false
        PlaceCloseBraceIgnoreOneLineBlock           = $false
        PlaceCloseBraceNoEmptyLineBefore            = $true
        UseConsistentIndentationEnable              = $true
        UseConsistentIndentationKind                = 'space'
        UseConsistentIndentationPipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
        UseConsistentIndentationIndentationSize     = 4
        UseConsistentWhitespaceEnable               = $true
        UseConsistentWhitespaceCheckInnerBrace      = $true
        UseConsistentWhitespaceCheckOpenBrace       = $true
        UseConsistentWhitespaceCheckOpenParen       = $true
        UseConsistentWhitespaceCheckOperator        = $true
        UseConsistentWhitespaceCheckPipe            = $true
        UseConsistentWhitespaceCheckSeparator       = $true
        AlignAssignmentStatementEnable              = $true
        AlignAssignmentStatementCheckHashtable      = $true
        UseCorrectCasingEnable                      = $true
    }

    New-ConfigurationFormat -ApplyTo 'OnMergePSM1', 'OnMergePSD1' -Sort None @configurationFormat
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'DefaultPSM1' -EnableFormatting -Sort None
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'OnMergePSD1' -PSD1Style 'Minimal'

    New-ConfigurationDocumentation -Enable:$false -StartClean -UpdateWhenNew -PathReadme 'Docs\Readme.md' -Path 'Docs'
    New-ConfigurationImportModule -ImportSelf -ImportRequiredModules

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $true
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        CertificateThumbprint             = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
        NETProjectPath                    = "$PSScriptRoot\..\..\TeamsX.PowerShell"
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'TeamsX.PowerShell'
        NETProjectName                    = 'TeamsX.PowerShell'
        NETBinaryModule                   = 'TeamsX.PowerShell.dll'
        NETConfiguration                  = 'Release'
        # PSPublishModule maps all modern .NET targets into Lib\Core. Keep the packaged
        # module on net8.0 for PowerShell compatibility while the projects themselves
        # still build and test on net10.0.
        NETFramework                      = 'net472', 'net8.0'
        DotSourceLibraries                = $true
        NETSearchClass                    = 'TeamsX.PowerShell.CmdletSendTeamsMessage'
        NETBinaryModuleDocumentation      = $true
        RefreshPSD1Only                   = $false
    }

    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot\..\Artefacts\Unpacked" -RequiredModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot\..\Artefacts\Packed" -IncludeTagName -ArtefactName "TeamsX-PowerShellModule.<TagModuleVersionWithPreRelease>.zip" -ID 'ToGitHub'

    # global options for publishing to github/psgallery
    #New-ConfigurationPublish -Type PowerShellGallery -FilePath 'C:\Support\Important\PowerShellGalleryAPI.txt' -Enabled:$true
    #New-ConfigurationPublish -Type GitHub -FilePath 'C:\Support\Important\GitHubAPI.txt' -UserName 'EvotecIT' -Enabled:$true -ID 'ToGitHub' -OverwriteTagName 'TeamsX-PowerShellModule.<TagModuleVersionWithPreRelease>'
}
