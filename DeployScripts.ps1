param (
    [string]$scriptProcessor,
    [string]$scriptSrc,
    [string]$outDir,
    [string]$scriptDest
)

#TODO: why not failing on error?
$ErrorActionPreference = "Stop"

Echo ""
Echo "build Dir   = $outDir"
Echo "script Src  = $scriptSrc"
Echo "script Dest = $scriptDest"

$scriptDirs = Get-ChildItem -Path $scriptSrc -Dir

$assembly = [Reflection.Assembly]::LoadFile($scriptProcessor)

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

    Copy-If-Exists $cript.BuiltPath $scriptDest
    
	$config = $cript.SourcePath + ".config"
    Copy-If-Exists $config $scriptDest
    
	$icon = $cript.SourcePath + ".png"
    Copy-If-Exists $icon $scriptDest
}

exit 0
