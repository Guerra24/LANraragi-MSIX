[xml]$xmlDoc = Get-Content "./LANraragi/Package.appxmanifest"

Set-Location "./LANraragi/AppPackages/"
Rename-Item $(Get-ChildItem *LANraragi* -Directory) "$(Get-Location)/LANraragi"

Set-Location "./LANraragi"
Remove-Item -Path "./Install.ps1","./Add-AppDevPackage.ps1"
Remove-Item -Path "./Add-AppDevPackage.resources" –recurse
