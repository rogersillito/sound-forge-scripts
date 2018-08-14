param (
    [string]$scriptProcessor,
    [string]$scriptSrc,
    [string]$scriptDest
)
$scripts = Get-ChildItem -Path $scriptSrc -Recurse -Filter *.cs

foreach ($file in $scripts) {
    # get-item $file.FullName | get-member

    Echo "Processing '$($file.name)'"
	  start-process -FilePath $scriptProcessor -ArgumentList """$($file.fullName)""" -Wait

    Echo "Deploying '$($file.name)'"
    copy-item $file.FullName -Destination $scriptDest

    #TODO: configs
    # Echo "Deploying config '$($file.name)'"
    # echo $file.directory
    # echo $file.basename
    # copy-item $file.FullName -Destination $scriptDest
}

exit 0
