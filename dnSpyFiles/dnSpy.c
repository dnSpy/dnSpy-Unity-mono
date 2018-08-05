// 0 = .NET 2.0-3.5 Unity
// 1 = .NET 4.0+ Unity
#ifndef DNUNITYRT
#define DNUNITYRT 0
#endif

#ifndef DEFINED_LEGACY_PROFILER
#define MonoLegacyProfiler MonoProfiler
#endif

// dnSpy doesn't know which version this is so it will set both of these environment variables.
// Older dnSpy versions will only set the first one.
#define ENV_VAR_NAME_V0 "DNSPY_UNITY_DBG"
#define ENV_VAR_NAME_V1 "DNSPY_UNITY_DBG2"
#if DNUNITYRT == 0
#define ENV_VAR_NAME ENV_VAR_NAME_V0
#elif DNUNITYRT == 1
#define ENV_VAR_NAME ENV_VAR_NAME_V1
#else
#error Invalid DNUNITYRT value
#endif

extern gboolean dnSpy_hideDebugger = 1;

void
dnSpy_debugger_init ()
{
	gboolean fixDefer = FALSE;
	char* envVal = getenv (ENV_VAR_NAME);

#if DNUNITYRT != 0
	if (!envVal) {
		envVal = getenv (ENV_VAR_NAME_V0);
		fixDefer = TRUE;
	}
#endif

	if (!envVal) {
		envVal = "--debugger-agent=transport=dt_socket,server=y,address=127.0.0.1:55555,defer=y";
		fixDefer = TRUE;
	}

#if DNUNITYRT != 0

#define defer_y		"defer=y"
#define suspend_n	"suspend=n"

	const char* s = strstr (envVal, defer_y);
	if (s && fixDefer) {
		int envVal_len = strlen (envVal);
		int defer_y_len = strlen (defer_y);
		int suspend_n_len = strlen (suspend_n);
		int newStr_len = envVal_len - defer_y_len + suspend_n_len;
		char* newStr = (char*)malloc (newStr_len + 1);
		memcpy (newStr, envVal, s - envVal);
		memcpy (newStr + (s - envVal), suspend_n, suspend_n_len);
		memcpy (newStr + (s - envVal) + suspend_n_len, s + defer_y_len, envVal_len - (s + defer_y_len - envVal));
		newStr [newStr_len] = 0;
		envVal = newStr;
	}
#endif

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
dnSpy_runtime_shutdown (MonoLegacyProfiler *prof)
{
}

void
dnSpy_debugger_init_after_agent ()
{
	// Prevent the debugger-agent's profiler events from being overwritten
	mono_profiler_install ((MonoLegacyProfiler*)&dnSpy_dummy_profiler, dnSpy_runtime_shutdown);
}
