@echo off
setlocal

rem Check if user knows what they're doing
if [%1] == [] goto Usage

if [%2] == [] goto Usage

if not [%3] == [] (
    if not [%3] == [mbe] (
        if not [%3] == [both] goto Usage
    )
)

rem Adjust mono dll to extract based on third argument
set monoPath=Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\Mono\EmbedRuntime\mono.dll
set monoBleedingEdgePath=Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll

rem Trim file path to unity version, then extract to folder named after unity version
for %%f in (%1\UnitySetup*.exe) do (
    for /f "tokens=2 delims=-" %%a in ("%%f") do (
        for /f "tokens=1 delims=f" %%b in ("%%a") do (
            rem We were not told to extract mono-bdwgc so we're just extracting the normal one
            if [%3] == [] 7z.exe -o%2\%%b e %%f %monoPath%
        
            rem Just extract mono-bdwgc if told to
            if [%3] == [mbe] z.exe -o%2\%%b e %%f %monoBleedingEdgePath%
                
            rem Otherwise the only other option is both so extract both
            if [%3] == [both] 7z.exe -o%2\%%b e %%f %monoBleedingEdgePath% %monoPath%
        )
    
    )
)
goto :eof

rem Explain to user how to use this script
rem mbe is optional, script will search for mono-bdwgc if mbe is passed
:Usage
echo Usage: %0 ^<look_directory^> ^<output_directory^> ^[mbe/both^]
exit /B 1
