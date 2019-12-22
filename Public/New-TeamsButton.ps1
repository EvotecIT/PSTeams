function New-TeamsButton {
    [alias('TeamsButton')]
    [CmdletBinding()]
    param (
        [alias('ButtonName')][Parameter(Mandatory)][ValidateNotNull()][ValidateNotNullOrEmpty()][string] $Name,
        [alias('TargetUri', 'Uri', 'Url')][Parameter(Mandatory)][ValidateNotNull()][ValidateNotNullOrEmpty()][string] $Link,
        [alias('ButtonType')][string][ValidateSet('ViewAction', 'TextInput', 'DateInput', 'HttpPost', 'OpenUri')] $Type = 'ViewAction'
    )
    if ($Type -eq 'ViewAction') {
        $Button = [ordered] @{
            '@context' = 'http://schema.org'
            '@type'    = 'ViewAction'
            name       = "$Name"
            target     = @("$Link")
            type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
        }
    } elseif ($Type -eq 'TextInput') {
        $Button = [ordered] @{
            #'@context' = 'http://schema.org'
            '@type'    = 'ActionCard'
            'Name'     = $Name
            'Inputs'   = @(
                @{
                    '@type'       = 'TextInput'
                    'id'          = 'Comment'
                    'isMultiLine' = $true
                    'title'       = 'Enter Your Text Input Here'
                }
            )
            actions    = @(
                @{
                    '@type'  = 'HttpPOST'
                    'Name'   = 'OK'
                    'target' = $Link
                }
            )
            type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
        }
    } elseif ($Type -eq 'DateInput') {
        $Button = [ordered] @{
            '@type'  = 'ActionCard'
            'Name'   = $Name
            'Inputs' = @(
                @{
                    '@type' = 'DateInput'
                    'id'    = 'dueDate'
                }
            )
            actions  = @(
                @{
                    '@type'  = 'HttpPOST'
                    'Name'   = 'OK'
                    'target' = $Link
                }
            )
            type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
        }
    } elseif ($Type -eq 'HttpPost') {
        $Button = [ordered] @{
            'name'   = $Name
            '@type'  = 'HttpPOST'
            'Target' = $Link
            type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
        }
    } elseif ($Type -eq 'OpenUri') {
        $Button = [ordered] @{
            'name'    = $Name
            '@type'   = 'OpenURI'
            'Targets' = @(
                @{
                    'os'  = 'default'
                    'uri' = $Link
                }
            )
            type       = 'button' # this is only needed for module to process this correctly. JSON doesn't care
        }
    }
    return $Button
}