function Invoke-EscapeMarkdownCharacter() {
    [CmdletBinding()]
    Param(
        [char[]]$MarkdownChar = @('`','*','_','{','}','[',']','(',')','#','+','-','.','!'),

        [Parameter(Mandatory)]
        [System.String] $InputString
    )

    Process {
        $MarkdownChar | Foreach-Object {
            $EscapeMe = $_.ToString()
            $InputString = $InputString.ToString().Replace($EscapeMe,"\$EscapeMe")
        }
        $InputString
    }
}