param (
    [string]$scriptProcessor,
    [string]$scriptSrc,
    [string]$scriptDest
)
Echo ""
Echo "script Src  = $scriptSrc"
Echo "script Dest = $scriptDest"

$scripts = Get-ChildItem -Path $scriptSrc -Recurse -Filter *.cs

function Copy-If-Exists
{
    if([System.IO.File]::Exists($args[0])){
		Echo "Deploying '$($args[0])'"
		copy-item $args[0] -Destination $args[1]
    }
}

foreach ($file in $scripts) {
	$icon = $file.fullName + ".png"
	$config = $file.fullName + ".config"

	Echo ""
    Echo "Processing '$($file.name)'"
	start-process -FilePath $scriptProcessor -ArgumentList """$($file.fullName)""" -Wait

    Copy-If-Exists $file.fullName $scriptDest
    
    Copy-If-Exists $config $scriptDest
    
    Copy-If-Exists $icon $scriptDest
}

exit 0
