This repo contains all files needed to build mono.dll with debugging support for Unity.

The master branch contains the original files. You have to check out the `dnSpy` branch to build everything. Use VS2017.


# Adding a new unity branch

- Check out the correct branch in the Unity mono repo and make sure there are no modified files left
- Switch to master branch in the dnSpy-Unity-mono repo
- Copy the following from the Unity mono root folder to a new root folder:
	- All root files
	- The following folders: `eglib`, `libgc`, `mono`, `msvc`, `unity`
- Remove `/opcode.def` from `mono/cil/.gitignore`
- Commit
- Switch to dnSpy branch
- Merge master branch into this branch
- Open `msvc\mono.sln` with VS2017 or later
	- Let it convert the following projects:
		- eglib
		- genmdesc
		- libgc
		- libmono
	- Use latest Windows SDK
	- Change toolset to v14.1
- Save and exit VS2017
- Ignore changes made to mono.sln (`git checkout msvc/mono.sln`)
- Open `dnSpy-Unity-mono.sln` and add a new solution folder and the new projects to it
- Fix references since they're referencing the 4.0 projects and make sure each 'new' project reference has `Reference Assembly Output` set to `False` in the `Properties` window
- Remove 'Treat warnings as errors' from `eglib` project in All Configurations and Win32/x64 platforms
- Open `msvc/libmono.vcxproj` and
	- replace `WINVER=0x0500;_WIN32_WINNT=0x0500` with `WINVER=0x0501;_WIN32_WINNT=0x0501`
	- Remove all lines with the string BrowseInformation
- Comment out `#define trunc(x) .....` in `mono/utils/mono-compiler.h`
- Comment out `stdout->_file = fd;` in `unity/unity_utils.c`, it's never called
- Change `libmono` `Output Directory` for all configurations and all platforms to `..\..\builds\$(Configuration)\_UNITY_NAME_\win$(PlatformArchitecture)\` where `_UNITY_NAME_` is the name of the branch
- Change `libmono` `Debug Information Format` and `Generate Debug Info` for configuration `Release_eglib` and for all platforms to No/None
- Update the code. These instructions may be out of date, but you can diff the dnSpy branch with master and ignore all project files to see all changes
	- `mono/metadata/icall.c`: func `ves_icall_System_Diagnostics_Debugger_IsAttached_internal`
```C
+extern gboolean dnSpy_hideDebugger;
+
 static MonoBoolean
 ves_icall_System_Diagnostics_Debugger_IsAttached_internal (void)
 {
+	if (dnSpy_hideDebugger)
+		return 0;
 	return mono_debug_using_mono_debugger () || mono_is_debugger_attached ();
 }
```
	- `mono/mini/debugger-agent.c`: func `mono_debugger_agent_parse_options`
```C
 					agent_config.address = g_strdup_printf ("0.0.0.0:%u", 56000 + (GetCurrentProcessId () % 1000));
 				}
 			}
+		} else if (dnSpy_debugger_agent_parse_options (arg)) {
 		} else {
 			print_usage ();
 			exit (1);
```
	- `mono/mini/mini.c`: func `mini_init`
```C
 
 	MONO_PROBE_VES_INIT_BEGIN ();
 
+	dnSpy_debugger_init ();
+
 #ifdef __linux__
 	if (access ("/proc/self/maps", F_OK) != 0) {
 		g_print ("Mono requires /proc to be mounted.\n");
```
	- Add a new `mono/mini/dnSpy.c` file with this content:
```C
#include "debug-mini.h"
#include "debugger-agent.h"
#include "../dnSpyFiles/dnSpy.c"
```
- Compile it
	- Use configuration `Release_eglib`
	- Use platform `x86` or `x64`
