Add-Type -TypeDefinition @"
   public enum ImageType
   {
    Alert,
    Cancel,
    Disable,
    Download,
    Minus,
    Check,
    Add,
    None
   }
"@

Add-Type -TypeDefinition @"
   public enum TeamsType
   {
    Text,
    Summary
   }
"@

<#
enum MessageType {
    Alert
    Cancel
    Disable
    Download
    Minus
    Check
    Add
    None
}
#>