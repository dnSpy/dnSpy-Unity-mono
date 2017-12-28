extern gboolean dnSpy_hideDebugger = 1;

void
dnSpy_debugger_init ()
{
	char* envVal = NULL;
	if (!envVal)
		envVal = getenv ("DNSPY_UNITY_DBG");
	if (!envVal)
		envVal = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555,defer=y";
	char* argv[] = { envVal };
	mono_jit_parse_options (1, (char**)argv);
	mono_debug_init (MONO_DEBUG_FORMAT_MONO);
}

int
dnSpy_debugger_agent_parse_options (char* arg)
{
	if (strcmp (arg, "no-hide-debugger") == 0) {
		dnSpy_hideDebugger = 0;
		return 1;
	}

	return 0;
}

typedef struct { void* dummy; } DebuggerProfiler;
static DebuggerProfiler dnSpy_dummy_profiler;
static void
dnSpy_runtime_shutdown(MonoProfiler *prof)
{
}

void
dnSpy_debugger_init_after_agent ()
{
	// Prevent the debugger-agent's profiler events from being overwritten
	mono_profiler_install((MonoProfiler*)&dnSpy_dummy_profiler, dnSpy_runtime_shutdown);
}
