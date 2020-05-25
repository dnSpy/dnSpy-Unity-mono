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
set mono2017Path=Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\Data\Mono\EmbedRuntime\mono.dll
set mono2017BleedingEdgePath=Editor\Data\PlaybackEngines\windowsstandalonesupport\Variations\win64_nondevelopment_mono\Data\MonoBleedingEdge\EmbedRuntime\mono-2.0-bdwgc.dll

rem Trim file path to unity version, then extract to folder named after unity version
for %%f in (%1\UnitySetup*.exe) do (
    for /f "tokens=2 delims=-" %%a in ("%%f") do (
        for /f "tokens=1 delims=f" %%b in ("%%a") do (
            for /f "tokens=1 delims=." %%c in ("%%b") do (
                if "%%c" == "2017" call :ExtractOld %%f %2\%%b %3
                if not "%%c" == "2017" call :ExtractLater %%f %2\%%b %3
            )
        )
    
    )
)
goto :eof

:ExtractOld
if [%3] == [] 7z.exe e %1 %mono2017Path% -o%2

if [%3] == [mbe] 7z.exe e %1 %mono2017BleedingEdgePath% -o%2

if [%3] == [both] 7z.exe e %1 %mono2017Path% %mono2017BleedingEdgePath% -o%2
exit /B 0

:ExtractLater
if [%3] == [] 7z.exe e %1 %monoPath% -o%2

if [%3] == [mbe] 7z.exe e %1 %monoBleedingEdgePath% -o%2

if [%3] == [both] 7z.exe e %1 %monoPath% %monoBleedingEdgePath% -o%2
exit /B 0

rem Explain to user how to use this script
rem mbe is optional, script will search for mono-bdwgc if mbe is passed
:Usage
echo Usage: %0 ^<look_directory^> ^<output_directory^> ^[mbe/both^]
exit /B 0
