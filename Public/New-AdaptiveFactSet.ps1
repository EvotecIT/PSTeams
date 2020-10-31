function New-AdaptiveFactSet {
    [cmdletBinding()]
    param(
        [scriptblock] $Facts,
        [ValidateSet('None', 'Small', 'Default', 'Medium', 'Large', 'ExtraLarge', 'Padding')][string] $Spacing,
        [ValidateSet('Stretch', 'Automatic')][string] $Height,
        [switch] $Separator
    )
    if ($Facts) {
        $TeamObject = [ordered] @{
            type    = 'FactSet'
            height  = $Height
            spacing = $Spacing
            facts   = & $Facts
        }
        if ($Separator) {
            $TeamObject['separator'] = $Separator.IsPresent
        }
        Remove-EmptyValue -Hashtable $TeamObject
        $TeamObject
    }
}