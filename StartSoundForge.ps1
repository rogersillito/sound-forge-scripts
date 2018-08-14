$Forge10 = "C:\Program Files (x86)\Sony\Sound Forge Pro 10.0\Forge100.exe"
$Forge11 = "C:\Program Files (x86)\MAGIX\Sound Forge Pro 11.0\Forge110.exe"
if ([System.IO.File]::Exists($Forge11)) {
	start-process -FilePath $Forge11
} else {
	start-process -FilePath $Forge10
}
exit 0