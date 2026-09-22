# Build Sources/MessageX.PowerShell/MessageX.PowerShell.csproj in Release before running checkout examples.
$previousDevelopmentMode = $env:MESSAGEX_USE_DEVELOPMENT_BINARIES
$previousConfiguration = $env:MESSAGEX_DEVELOPMENT_CONFIGURATION
try {
    $env:MESSAGEX_USE_DEVELOPMENT_BINARIES = 'true'
    $env:MESSAGEX_DEVELOPMENT_CONFIGURATION = 'Release'
    Import-Module (Join-Path $PSScriptRoot '../Module/MessageX/MessageX.psd1') -Force -ErrorAction Stop
} finally {
    $env:MESSAGEX_USE_DEVELOPMENT_BINARIES = $previousDevelopmentMode
    $env:MESSAGEX_DEVELOPMENT_CONFIGURATION = $previousConfiguration
}
