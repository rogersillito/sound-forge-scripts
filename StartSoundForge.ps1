param (
    [string]$forgeExe = "C:\Program Files (x86)\MAGIX\Sound Forge Pro 11.0\Forge110.exe"
)
Echo Running $forgeExe
start-process -FilePath """$forgeExe"""
exit 0