function New-AdaptiveFactSet {
    [cmdletBinding()]
    param(
        [scriptblock] $Facts,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [switch] $Separator
    )
    if ($Facts) {
        $OutputFacts = & $Facts
        if ($OutputFacts) {
            $TeamObject = [ordered] @{
                type    = 'FactSet'
                height  = $Height
                spacing = $Spacing
                facts   = @($OutputFacts)
            }
            if ($Separator) {
                $TeamObject['separator'] = $Separator.IsPresent
            }
            Remove-EmptyValue -Hashtable $TeamObject
            $TeamObject
        }
    }
}