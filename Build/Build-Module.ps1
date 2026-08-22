param(
    [ValidateSet('Manifest', 'Documentation', 'Build', 'Publish')]
    [string] $ConfigurationGateMode = 'Build',

    [bool] $SignModule = $false,

    [string] $PowerShellGalleryApiKeyPath = 'C:\Support\Important\PowerShellGalleryAPI.txt',

    [string] $GitHubApiKeyPath = 'C:\Support\Important\GitHubAPI.txt'
)

Import-Module PSPublishModule -Force -ErrorAction Stop

Build-Module -ModuleName 'PSTeams' -Path 'Module' {
    $manifest = [ordered] @{
        ModuleVersion        = '2.4.X'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = 'a46c3b0b-5687-4d62-89c5-753ae01e0926'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'PSTeams provides typed Microsoft Teams message composition and delivery through the reusable TeamsX library and compiled PowerShell cmdlets.'
        Tags                 = @('Teams', 'Microsoft', 'MSTeams', 'Notifications', 'Webhook', 'PowerShell', 'Windows', 'MacOS', 'Linux')
        IconUri              = 'https://statics.teams.microsoft.com/evergreen-assets/apps/teamscmdlets_largeimage.png'
        ProjectUri           = 'https://github.com/EvotecIT/PSTeams'
        PowerShellVersion    = '5.1'
    }
    New-ConfigurationManifest @manifest

    New-ConfigurationDocumentation -Enable -PathReadme '..\..\Docs\Readme.md' -Path '..\..\Docs'
    New-ConfigurationImportModule -ImportSelf -ImportRequiredModules

    $build = @{
        Enable                     = $true
        SignModule                 = $SignModule
        MergeModuleOnBuild         = $true
        CertificateThumbprint      = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
        NETProjectPath             = '..\..\TeamsX.PowerShell\TeamsX.PowerShell.csproj'
        NETProjectName             = 'TeamsX.PowerShell'
        NETBinaryModule            = 'TeamsX.PowerShell.dll'
        NETConfiguration           = 'Release'
        NETFramework               = 'net472', 'net8.0', 'net10.0'
        DotSourceLibraries         = $true
    }
    New-ConfigurationBuild @build

    New-ConfigurationArtefact -Type Unpacked -Enable -Path '..\..\Artefacts\Unpacked' -ModulesPath '..\..\Artefacts\Unpacked\Modules'
    New-ConfigurationArtefact -Type Packed -Enable -Path '..\..\Artefacts\Packed' -ModulesPath '..\..\Artefacts\Packed\Modules' -IncludeTagName -ArtefactName 'PSTeams-PowerShellModule.<TagModuleVersionWithPreRelease>.zip' -ID 'ToGitHub'

    New-ConfigurationPublish -Type PowerShellGallery -FilePath $PowerShellGalleryApiKeyPath -Enabled:$false
    New-ConfigurationPublish -Type GitHub -FilePath $GitHubApiKeyPath -UserName 'EvotecIT' -RepositoryName 'PSTeams' -Enabled:$false -ID 'ToGitHub' -OverwriteTagName 'PSTeams-PowerShellModule.<TagModuleVersionWithPreRelease>'
    New-ConfigurationGate -Mode $ConfigurationGateMode
} -ExitCode
