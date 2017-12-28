This repo contains all files needed to build mono.dll with debugging support for Unity.

The master branch contains the original files. You have to check out the `dnSpy` branch to build everything. Use VS2017.

# Known issues

> mono.dll sometimes crashes in `mono_unwind_frame`

Workaround: Compile a debug build (`Debug_eglib`) instead of a release build (`Release_eglib`)

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
- Open `dnSpy-Unity-mono.sln` and add a new solution folder and the new projects to it
- Retarget the solution to latest Windows SDK and toolset v141
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
	- `mono/mini/debugger-agent.c`: func `mono_debugger_agent_init`
```C
 	mono_profiler_install_jit_end (jit_end);
 	mono_profiler_install_method_invoke (start_runtime_invoke, end_runtime_invoke);
 
+	dnSpy_debugger_init_after_agent ();
+
 	debugger_tls_id = TlsAlloc ();
 
 	thread_to_tls = mono_g_hash_table_new (NULL, NULL);
```
	- `mono/mini/debugger-agent.c`: func `thread_commands`
```C
 		mono_loader_lock ();
 		tls = mono_g_hash_table_lookup (thread_to_tls, thread);
 		mono_loader_unlock ();
-		g_assert (tls);
+		if (!tls)
+			return ERR_INVALID_ARGUMENT;
 
 		compute_frame_info (thread, tls);
 
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

# Commit hashes

A few versions have the same hash.

version | git hash
--------|---------
4.0.0 | 7ec3cabdff850179420727c42bc73a91d1a97a29
4.0.1 | 7ec3cabdff850179420727c42bc73a91d1a97a29
4.1.0 | 6232bc3853ea9881fd87db0198486304f0619274
4.1.2 | 6232bc3853ea9881fd87db0198486304f0619274
4.1.3 | 98d922bd3445bf022be90dae4ab9e65b588279c3
4.1.4 | 499f8bdf30282419ff830e00a36aab185da7fca8
4.1.5 | 499f8bdf30282419ff830e00a36aab185da7fca8
4.2.0 | 21f2c5856b2b57fee7c0bdaf9b10f90386b15f30
4.2.1 | ac09c9bb53471083b961dbb95ec747176bad6aa9
4.2.2 | ac09c9bb53471083b961dbb95ec747176bad6aa9
4.3.0 | affd2dfa7e152ff89edda6daa6a1eaba9d36635e
4.3.1 | 593baca940f9238b693b6843882442664e38a5f2
4.3.2 | 593baca940f9238b693b6843882442664e38a5f2
4.3.3 | 663508eae2c9bac2d1d51fafd2de173fb83c329f
4.3.4 | 663508eae2c9bac2d1d51fafd2de173fb83c329f
4.5.0 | 1d6076a7fbb17a89eb13446ac4dad1470d4c2ed8
4.5.1 | 1d6076a7fbb17a89eb13446ac4dad1470d4c2ed8
4.5.2 | 6d301e8bf32e50808f0101a12b03c976d8546b71
4.5.3 | 2b39f03a86e603dd01daf8684e28280863f1dab8
4.5.4 | 74242f8238bf81055e7a9179a2ad207ef0be4b39
4.5.5 | b6c89ce33f404463751003e1328edf8a3d8e6fd3
4.6.0 | b6c89ce33f404463751003e1328edf8a3d8e6fd3
4.6.1 | 2f808416774125014e03f4a85b8b7df7a26c3db8
4.6.2 | f5af3c697aa14eaefc0ecec39846dfeb8298db3e
4.6.3 | 4ccc9d766b5e37b86e2db4cf8a7251166706b451
4.6.4 | 9cdfd37df707daa3ab0d19bff2ce93aa82ef6bd7
4.6.5 | 94fc25748d3d63feec85ff86683fb1361b2381c4
4.6.6 | afb14df41b50125acd930a226bc9f0a5d5b29496
4.6.7 | afb14df41b50125acd930a226bc9f0a5d5b29496
4.6.8 | f24711c7235afa235d8db647c22c85675da1b75c
4.6.9 | f24711c7235afa235d8db647c22c85675da1b75c
4.7.0 | dce5affd7506ddbe1f8449a73d9d52b3ede01e42
4.7.1 | dce5affd7506ddbe1f8449a73d9d52b3ede01e42
4.7.2 | 6288f2b6a22d5f48feff5ae279844e3387672edb
5.0.0 | bfc0b170d55d50c6f3f08941e21e73cce7e47692
5.0.1 | eacf7dc705f07d62981216402c7242d6e084863f
5.0.2 | 46da7397c4ae32539e44ab4c64362158a7f8d9c5
5.0.3 | 944a1294616548560d220be060a7599013371b85
5.0.4 | 5f17675341de3d3c9e4027241c7c3d971c4ef9c1
5.1.0 | e7470728f0d77c0e44aabf9209d136ed9c17cb38
5.1.1 | 8cc67d6e880e365631872e4e8ede273fe6dd1b96
5.1.2 | 2c6d1ad3510d57e1653888a72cc19510fbf7fd6a
5.1.3 | adce0585e3ae5dd014e70e3197f50f4fcf3905a7
5.1.4 | adce0585e3ae5dd014e70e3197f50f4fcf3905a7
5.1.5 | ecec8aba4e735e96f886fb7bbd736c569d8998fd
5.2.0 | 990d4243b192298e079b94996f9afe06da43f5c5
5.2.1 | 990d4243b192298e079b94996f9afe06da43f5c5
5.2.2 | dd13669b78d04117a74a68f6b57286ac7aa0f11e
5.2.3 | dd13669b78d04117a74a68f6b57286ac7aa0f11e
5.2.4 | 07337a5ad920d0d2349677a125f921225e423bdb
5.2.5 | d52ec8fb6c9c8d7777dc0da9f4769160918c4cf4
5.3.0 | 12ad0fb3b08d260dce20d3d906643026b51c0a42
5.3.1 | 12ad0fb3b08d260dce20d3d906643026b51c0a42
5.3.2 | e67f70e197cdc120b3f6b78e2c4a9b2abf8073be
5.3.3 | a0c32dbacb1594464bee45638839452f7d49eedc
5.3.4 | da080ba6db629d00c52bbc1146b1fca21d22ec7f
5.3.5 | 32dd5015852131c334fd9be915c7eb47b9c7ce9d
5.3.6 | 465a117be0859b9de7e7f54d7a2cb49a83816242
5.3.7 | ab5d69a9ea45530e92b457c7792e64963c68d7e8
5.3.8 | 12ac9d2d802867d06c066b5b330e50df94bf1f13
5.4.0 | 737162df810edbe681553b1161a4e85c4815eff1
5.4.1 | 1932a0c940aa9906a5ef417d7032484070535462
5.4.2 | a10ee4dd3735084f7b7e9864fe01ba690e536b96
5.4.3 | aa8a6e7afc2f4fe63921df4fe8a18cfd0a441d19
5.4.4 | aa8a6e7afc2f4fe63921df4fe8a18cfd0a441d19
5.4.5 | 98743ee2d770ba0c461940806d1b582c654eb0f3
5.4.6 | 98743ee2d770ba0c461940806d1b582c654eb0f3
5.5.0 | 942d467726dc8b8c25576ae91c46ac488f113f0e
5.5.1 | 942d467726dc8b8c25576ae91c46ac488f113f0e
5.5.2 | e63de00f88941237b4021991fb16203094865a69
5.5.3 | 98d65b1b3de869688b83c0a5e0c966fe9925d29b
5.5.4 | 1257261cb4342a4e57691462d8ae961729026c3b
5.5.5 | 5a670fd418c3fbde85226e9573976cec730561c1
5.6.0 | 78505a00c8a9668fcf64ef0e3f1e5d3cade4b0b7
5.6.1 | 3ce3097320f2a4ecc486477434a99ec2327933ae
5.6.2 | a4d8fea68a59fe5d297740c271a60a7f2dc827c2
5.6.3 | 6a3c0a37e1fad3caf8b6f61e891c67b340d18d20
5.6.4 | 7c1507f591db29dc9f22fa401e9bfb224361ab03
2017.1.0 | 284c883072cb5fdf21ed9ca4dbe8e7e6d3e7d145
2017.1.1 | 8c37e8d3077124bc33a12df74a71b0575cd9a05f
2017.1.2 | 3ca35a90d3c4c41f3abe53ed518d46732fe8a79e
2017.2.0 | 2fea43d2755abd9aace913a596dd4f7d011014c5
2017.2.1 | 540e1297eefe581a588a80173fd41ae6b8c1cc78
2017.3.0 | 94a03ac32cab0d7c1d663fd5ee415ecf8c707ca3

To find the commit hash, get the PE header timestamp from `mono.dll` and find the closest commit (should be a merge) from the correct branch, eg. `unity-2017.1`. `gitk` or other UI can be used, or `git log --merges`.
