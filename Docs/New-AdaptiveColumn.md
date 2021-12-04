---
external help file: PSTeams-help.xml
Module Name: PSTeams
online version:
schema: 2.0.0
---

# New-AdaptiveColumn

## SYNOPSIS
{{ Fill in the Synopsis }}

## SYNTAX

```
New-AdaptiveColumn [[-Items] <ScriptBlock>] [[-Spacing] <String>] [[-Height] <String>] [[-Width] <String>]
 [[-WidthInWeight] <Int32>] [[-WidthInPixels] <Int32>] [[-MinimumHeight] <Int32>]
 [[-HorizontalAlignment] <String>] [[-VerticalContentAlignment] <String>] [[-Style] <String>] [-Hidden]
 [-Separator] [[-SelectAction] <String>] [[-SelectActionId] <String>] [[-SelectActionUrl] <String>]
 [[-SelectActionTitle] <String>] [[-SelectActionTargetElement] <String[]>] [<CommonParameters>]
```

## DESCRIPTION
{{ Fill in the Description }}

## EXAMPLES

### Example 1
```powershell
PS C:\> {{ Add example code here }}
```

{{ Add example description here }}

## PARAMETERS

### -Height
{{ Fill Height Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Stretch, Automatic

Required: False
Position: 2
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Hidden
{{ Fill Hidden Description }}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -HorizontalAlignment
{{ Fill HorizontalAlignment Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Left, Center, Right

Required: False
Position: 7
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Items
{{ Fill Items Description }}

```yaml
Type: ScriptBlock
Parameter Sets: (All)
Aliases:

Required: False
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MinimumHeight
{{ Fill MinimumHeight Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 6
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectAction
{{ Fill SelectAction Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Action.Submit, Action.OpenUrl, Action.ToggleVisibility

Required: False
Position: 10
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionId
{{ Fill SelectActionId Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 11
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionTargetElement
{{ Fill SelectActionTargetElement Description }}

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: 14
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionTitle
{{ Fill SelectActionTitle Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 13
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SelectActionUrl
{{ Fill SelectActionUrl Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: 12
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Separator
{{ Fill Separator Description }}

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Spacing
{{ Fill Spacing Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: None, Small, Default, Medium, Large, ExtraLarge, Padding

Required: False
Position: 1
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Style
{{ Fill Style Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Accent, Default, Emphasis, Good, Warning, Attention

Required: False
Position: 9
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -VerticalContentAlignment
{{ Fill VerticalContentAlignment Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Top, Center, Bottom

Required: False
Position: 8
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Width
{{ Fill Width Description }}

```yaml
Type: String
Parameter Sets: (All)
Aliases:
Accepted values: Stretch, Auto, Weighted

Required: False
Position: 3
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WidthInPixels
{{ Fill WidthInPixels Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 5
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -WidthInWeight
{{ Fill WidthInWeight Description }}

```yaml
Type: Int32
Parameter Sets: (All)
Aliases:

Required: False
Position: 4
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### System.Object
## NOTES

## RELATED LINKS
