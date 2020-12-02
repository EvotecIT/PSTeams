function Disable-TextMarkdown() {
    [CmdletBinding()]
    Param(
        $InputObject
    )

    $ReturnBody = [ordered] @{}
    $InputObject | Foreach-Object {
        $Object = $_
        $Keys = $Object.Keys
        $Keys | Foreach-Object {
            $Key = $_
            if ($Key -eq 'sections') {
                $ReturnBody.sections = @(
                    $Object.$Key | Foreach-Object {
                        $Section = $_
                        $SectionKeys = $Section.Keys
                        Foreach ($Item in $SectionKeys.Clone()) {
                            Switch($Item.ToLower().Contains('text')) {
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
    $ReturnBody
}