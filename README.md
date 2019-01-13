# sound-forge-scripts

Scripts for automation of Sound Forge, along with a build pipeline to allow script development in the slightly more forgiving environment of Visual Studio.

* Vinyl Rip 1 - Find Tracks (applies temporary, heavy noise reduction - then finds music tracks, creating regions for them)
* Vinyl Rip 2 - Adjust Tracks (allows regions found in Vinyl Rip 1 to be edited/auditioned, add/delete tracks, and adjust track fades)
* Vinyl Rip 3 - Find Tracks (extract found tracks to be extracted with fade in/out as per Vinyl Rip 1 & 2)

The SoundForgeScriptsLib project (which compiles to a dll) can use modern C# features (6.0), while SoundForgeScripts use C# 2.0 as required by Sound Forge.

# ScriptFileProcessor conventions

When the solution is built, each subdirectory of `SoundForgeScripts.Scripts` gets processed by `ScriptProcessor.BuildEntryPointScript`.  This results in the following:

* The class that inherits from `SoundForgeScriptsLib.EntryPoints.EntryPointBase` is taken to be the main script file:
	* If the entry point class is decorated with `SoundForgeScriptsLib.ScriptNameAttribute`, the value of that attribute is used for the output script name.
	* When no `ScriptNameAttribute`, the original entry point class file name is used for the output script name.
* All code files within the script subdirectory have their namespaces removed (NB: ensure unique names within the context of a script subdirectory!)
* Content of all code files within the script subdirectory are combined below the entry point file into the output script.

The output script for each script subdirectory is written to the build directory of the SoundForgeScripts project.


# building/debugging

* Output scripts are copied into Sound Forge's script menu directory.
* The DebugWithDllCopy build configuration also pushes the dll for the SoundForgeScriptsLib into the script menu directory (and closes/restarts sound forge to pickup the dll change).
* Setting the environment variable FORGE_DIR will override the default location of Sound Forge's script menu directory for build purposes.
* Setting the environment variable FORGE_EXE will override the default location of the Sound Forge executable used to restart after dll changes.