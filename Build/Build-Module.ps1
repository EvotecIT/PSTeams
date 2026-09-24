param(
    [ValidateSet('Manifest', 'Documentation', 'Build', 'Publish')]
    [string] $ConfigurationGateMode = 'Build',

    [bool] $SignModule = $false,

    [string] $PowerShellGalleryApiKeyPath = 'C:\Support\Important\PowerShellGalleryAPI.txt',

    [string] $GitHubApiKeyPath = 'C:\Support\Important\GitHubAPI.txt'
)

Import-Module PSPublishModule -Force -ErrorAction Stop

Build-Module -ModuleName 'MessageX' -Path 'Module' -SkipInstall {
    $manifest = [ordered] @{
        ModuleVersion        = '0.1.0'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = 'd62b583e-c92b-4ac1-b83d-b7d710e48cb5'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'Compose and deliver Teams, Slack, and Discord messages with typed MessageX libraries and compiled PowerShell cmdlets.'
        Tags                 = @('Teams', 'Slack', 'Discord', 'Microsoft', 'MSTeams', 'Notifications', 'Webhook', 'PowerShell', 'Windows', 'MacOS', 'Linux')
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
        NETProjectPath             = '..\..\Sources\MessageX.PowerShell\MessageX.PowerShell.csproj'
        NETProjectName             = 'MessageX.PowerShell'
        NETBinaryModule            = 'MessageX.PowerShell.dll'
        NETConfiguration           = 'Release'
        NETFramework               = 'net472', 'net8.0', 'net10.0'
        NETDevelopmentBinaries     = $true
        NETDevelopmentBinariesMode = 'Environment'
        NETDevelopmentSourceBootstrapperMode = 'ReplaceSingleFile'
        DotSourceLibraries         = $true
    }
    New-ConfigurationBuild @build

    New-ConfigurationArtefact -Type Unpacked -Enable -Path '..\..\Artefacts\Unpacked' -ModulesPath '..\..\Artefacts\Unpacked\Modules'
    New-ConfigurationArtefact -Type Packed -Enable -Path '..\..\Artefacts\Packed' -ModulesPath '..\..\Artefacts\Packed\Modules' -IncludeTagName -ArtefactName 'MessageX-PowerShellModule.<TagModuleVersionWithPreRelease>.zip' -ID 'ToGitHub'

    New-ConfigurationPublish -Type PowerShellGallery -FilePath $PowerShellGalleryApiKeyPath -Enabled:$false
    New-ConfigurationPublish -Type GitHub -FilePath $GitHubApiKeyPath -UserName 'EvotecIT' -RepositoryName 'PSTeams' -Enabled:$false -ID 'ToGitHub'
    New-ConfigurationGate -Mode $ConfigurationGateMode
} -ExitCode
