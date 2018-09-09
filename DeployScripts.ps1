param (
    [string]$scriptProcessorDir,
    [string]$scriptSrc,
    [string]$outDir,
    [string]$scriptDest
)

#TODO: why not failing build on error?
$ErrorActionPreference = "Stop"

Echo ""
Echo "build Dir            = $outDir"
Echo "script Src           = $scriptSrc"
Echo "script Dest          = $scriptDest"
Echo "script Processor Dir = $scriptProcessorDir"

$scriptDirs = Get-ChildItem -Path $scriptSrc -Dir

[Reflection.Assembly]::LoadFile("$scriptProcessorDir\SoundForgeScriptsLib.dll") | Out-Null
[Reflection.Assembly]::LoadFile("$scriptProcessorDir\ScriptFileProcessor.dll") | Out-Null

function Copy-If-Exists
{
    if([System.IO.File]::Exists($args[0])){
		Echo "Deploying '$($args[0])'"
		copy-item $args[0] -Destination $args[1]
    }
}

foreach ($scriptDir in $scriptDirs) {

	Echo ""
    Echo "Processing Script Dir: '$($scriptDir)'"
	$sp = New-Object ScriptFileProcessor.ScriptProcessor
	$script = $sp.BuildEntryPointScript($scriptDir.FullName, $outDir)
	if (-Not $script.Success) { 
		Echo $script.error
		continue
	}

    Copy-If-Exists $cript.BuiltPath $scriptDest
    
	$config = $cript.SourcePath + ".config"
    Copy-If-Exists $config $scriptDest
    
	$icon = $cript.SourcePath + ".png"
    Copy-If-Exists $icon $scriptDest
}

exit 0
