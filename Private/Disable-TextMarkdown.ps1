function Disable-TextMarkdown() {
    [CmdletBinding()]
    Param(
        $InputJson
    )

    $ReturnBody = [ordered] @{}
    ($InputJson | ConvertFrom-Json) | Foreach-Object {
        $Object = $_
        $Keys = $Object | Get-Member -MemberType NoteProperty
        $Keys | Foreach-Object {
            $Key = $_.Name
            if ($Key -eq 'sections') {
                $ReturnBody.sections = @(
                    $Object.$Key | Foreach-Object {
                        $Section = $_
                        $SectionKeys = ($Section | Get-Member -MemberType NoteProperty).Name
                        Foreach ($Item in $SectionKeys) {
                            Switch($Item -like "*text*") {
                                $true {
                                    $Section."$Item" = Invoke-EscapeMarkdownCharacter -InputString $Section."$Item"
                                }
                            }
                        }
                        $Section
                    }
                )
            } else {
                $ReturnBody.$Key = $Object.$Key
            }
        }
    }
    $ReturnBody | ConvertTo-Json -Depth 10
}