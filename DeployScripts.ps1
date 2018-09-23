param (
    [string]$scriptProcessorDir,
    [string]$scriptSrc,
    [string]$outDir,
    [string]$scriptDest
)

function Load-Assembly($filePath) {
	while (!(Test-Path $filePath)) { 
		echo "Waiting for $filePath..."
		Start-Sleep 1 
	}
	[Reflection.Assembly]::LoadFile($filePath) | Out-Null
}

function Copy-If-Exists
{
    if([System.IO.File]::Exists($args[0])){
		Echo "Deploying '$($args[0])'"
		copy-item $args[0] -Destination $args[1]
    }
}

$retCode = 0
$processingErrors = 0
try
{
	Echo ""
	Echo "build Dir            = $outDir"
	Echo "script Src           = $scriptSrc"
	Echo "script Dest          = $scriptDest"
	Echo "script Processor Dir = $scriptProcessorDir"

	$scriptDirs = Get-ChildItem -Path $scriptSrc -Dir

	Load-Assembly "$scriptProcessorDir\ScriptFileProcessor.dll"

	foreach ($scriptDir in $scriptDirs) {

		Echo ""
		Echo "Processing Script Dir: '$($scriptDir)'"
		$sp = New-Object ScriptFileProcessor.ScriptProcessor
		$script = $sp.BuildEntryPointScript($scriptDir.FullName, $outDir)
		Echo ""
		if (-Not $script.Success) { 
			Echo $script.error
			$processingErrors++
			continue
		}

		Copy-If-Exists $script.BuiltPath $scriptDest
		
		$config = $script.SourcePath + ".config"
		Copy-If-Exists $config "$scriptDest\$($script.BuiltFilename).config"
		
		$icon = $script.SourcePath + ".png"
		Copy-If-Exists $icon "$scriptDest\$($script.BuiltFilename).png"
		Echo ""
	}
}
catch
{
	Write-Error $_.Exception.Message
	$retCode = 1
}
if ($processingErrors -ne 0)
{
	$retCode = 1
}
exit $retCode
