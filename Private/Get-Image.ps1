function Get-Image {
    [CmdletBinding()]
    param(
        [string] $PathToImages,
        [string] $FileName,
        [string] $FileExtension
    )
    Write-Verbose "Get-Image - PathToImages $PathToImages FileName $FileName FileExtension $FileExtension"
    #if ($ImageType -ne [ImageType]::None) {
    $ImagePath = [IO.Path]::Combine( $PathToImages, "$($FileName)$FileExtension")
    Write-Verbose "Get-Image - ImagePath $ImagePath"
    if (Test-Path $ImagePath) {
        if ($PSEdition -eq 'Core') {
            $Image = [convert]::ToBase64String((Get-Content $ImagePath -AsByteStream))
        } else {
            $Image = [convert]::ToBase64String((Get-Content $ImagePath -Encoding byte))
        }
        Write-Verbose "Get-Image - Image Type: $($Image.GetType())"
        return "data:image/png;base64,$Image"
    }
    # }
    return ''
}