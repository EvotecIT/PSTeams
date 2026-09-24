$binaryAliases = [ordered] @{
    ActivityImage                = 'New-TeamsActivityImage'
    ActivityImageLink            = 'New-TeamsActivityImage'
    ActivitySubtitle             = 'New-TeamsActivitySubtitle'
    ActivityText                 = 'New-TeamsActivityText'
    ActivityTitle                = 'New-TeamsActivityTitle'
    'New-AdaptiveImageGallery'   = 'New-AdaptiveImageSet'
    'New-HeroButton'             = 'New-CardListButton'
    'New-HeroImage'              = 'New-AdaptiveImage'
    'New-TeamsActivityImageLink' = 'New-TeamsActivityImage'
    'New-ThumbnailButton'        = 'New-CardListButton'
    'New-ThumbnailImage'         = 'New-AdaptiveImage'
    TeamsActivityImage           = 'New-TeamsActivityImage'
    TeamsActivityImageLink       = 'New-TeamsActivityImage'
    TeamsActivitySubtitle        = 'New-TeamsActivitySubtitle'
    TeamsActivityText            = 'New-TeamsActivityText'
    TeamsActivityTitle           = 'New-TeamsActivityTitle'
    TeamsBigImage                = 'New-TeamsBigImage'
    TeamsButton                  = 'New-TeamsButton'
    TeamsFact                    = 'New-TeamsFact'
    TeamsImage                   = 'New-TeamsImage'
    TeamsList                    = 'New-TeamsList'
    TeamsListItem                = 'New-TeamsListItem'
    TeamsSection                 = 'New-TeamsSection'
    TeamsMessage                 = 'Send-TeamsMessage'
    TeamsMessageBody             = 'Send-TeamsMessageBody'
}

foreach ($alias in $binaryAliases.GetEnumerator()) {
    Set-Alias -Name $alias.Key -Value $alias.Value -Scope Local
}
