function Add-TeamsSection {
    param(
        $Sections
    )
    $PreparedSections = @()
    foreach ($section in $Sections) {
        $PreparedSections += $section
    }
    return $PreparedSections
}