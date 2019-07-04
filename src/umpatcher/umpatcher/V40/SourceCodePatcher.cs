/*
    Copyright (C) 2018 de4dot@gmail.com

    This file is part of umpatcher

    umpatcher is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    umpatcher is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with umpatcher.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.IO;
using System.Linq;
using System.Text;

namespace UnityMonoDllSourceCodePatcher.V40 {
	sealed class SourceCodePatcher {
		readonly SolutionOptionsV40 solutionOptions;

		public SourceCodePatcher(SolutionOptionsV40 solutionOptions) =>
			this.solutionOptions = solutionOptions ?? throw new ArgumentNullException(nameof(solutionOptions));

		public void Patch() {
			Patch_mono_metadata_icall_c();
			Patch_mono_mini_debugger_agent_c();
			Patch_mono_mini_mini_runtime_c();
			Add_mono_mini_dnSpy_c();
			Patch_masm_fixed_props();
			Patch_bdwgc_gc_atomic_ops_h();
			Patch_bdwgc_gcconfig_h();
		}

		void Patch_mono_metadata_icall_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "metadata", "icall.c");
			var textFilePatcher = new TextFilePatcher(filename);
			int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("ves_icall_System_Diagnostics_Debugger_IsAttached_internal")).Single();
			var lines = textFilePatcher.Lines;
			textFilePatcher.Insert(index + 2, "\tif (dnSpy_hideDebugger)");
			textFilePatcher.Insert(index + 3, "\t\treturn 0;");
			textFilePatcher.Insert(index - 1, string.Empty);
			textFilePatcher.Insert(index - 1, "extern gboolean dnSpy_hideDebugger;");
			textFilePatcher.Write();
		}

		static void Verify(string value, string expectedValue) {
			if (value != expectedValue)
				throw new ProgramException($"Line is '{value}' but expected line is '{expectedValue}'");
		}

		void Patch_mono_mini_debugger_agent_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "mini", "debugger-agent.c");
			var textFilePatcher = new TextFilePatcher(filename);

			var lines = textFilePatcher.Lines;

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("agent_config.setpgid = parse_flag (\"setpgid\", arg + 8)")).Single();
				Verify(lines[index + 1].Text, "\t\t} else {");
				textFilePatcher.Insert(index + 1, "\t\t} else if (dnSpy_debugger_agent_parse_options (arg)) {");
			}

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("mono_native_tls_alloc (&debugger_tls_id, NULL);")).Single();
				Verify(lines[index - 1].Text, string.Empty);
				textFilePatcher.Insert(index, string.Empty);
				textFilePatcher.Insert(index, "\tdnSpy_debugger_init_after_agent ();");
			}

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.StartsWith("insert_breakpoint (")).Single();
				index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains("gboolean it_has_sp = FALSE;"), index);
				textFilePatcher.Insert(index + 1, "\tSeqPoint found_sp;");
				index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains("if (it.seq_point.il_offset == bp->il_offset) {"), index);
				Verify(lines[index + 1].Text, "\t\t\tit_has_sp = TRUE;");
				Verify(lines[index + 2].Text, "\t\t\tbreak;");
				index += 2;
				lines.RemoveAt(index);
				textFilePatcher.Insert(index++, "\t\t\tif (!(it.seq_point.flags & MONO_SEQ_POINT_FLAG_NONEMPTY_STACK)) {");
				textFilePatcher.Insert(index++, "\t\t\t\tfound_sp = it.seq_point;");
				textFilePatcher.Insert(index++, "\t\t\t\tbreak;");
				textFilePatcher.Insert(index++, "\t\t\t}");
				textFilePatcher.Insert(index++, "\t\t\tfound_sp = it.seq_point;");
				Verify(lines[index++].Text, "\t\t}");
				Verify(lines[index++].Text, "\t}");
				textFilePatcher.Insert(index++, "\tit.seq_point = found_sp;");
			}

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("case CMD_THREAD_GET_FRAME_INFO: {")).Single();
				index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains("tls = (DebuggerTlsData *)mono_g_hash_table_lookup (thread_to_tls, thread);"), index);
				Verify(lines[index + 1].Text, "\t\tmono_loader_unlock ();");
				Verify(lines[index + 2].Text, "\t\tg_assert (tls);");
				lines[index + 2] = lines[index + 2].Replace("\t\tif (!tls)");
				textFilePatcher.Insert(index + 3, "\t\t\treturn ERR_INVALID_ARGUMENT;");
			}

			textFilePatcher.Write();
		}

		void Patch_mono_mini_mini_runtime_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "mini", "mini-runtime.c");
			var textFilePatcher = new TextFilePatcher(filename);
			int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("CHECKED_MONO_INIT ();")).Single();
			textFilePatcher.Insert(++index, string.Empty);
			textFilePatcher.Insert(++index, "\tdnSpy_debugger_init ();");
			textFilePatcher.Write();
		}

		void Add_mono_mini_dnSpy_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "mini", "dnSpy.c");
			var sb = new StringBuilder();
			sb.Append("﻿#include <mono/metadata/profiler.h>\r\n");
			sb.Append("#include <mono/metadata/mono-debug.h>\r\n");
			sb.Append("#include \"debugger-agent.h\"\r\n");
			sb.Append("#define DNUNITYRT 1\r\n");
			if (solutionOptions.UnityVersion.Major >= 2018) {
				sb.Append("typedef void *MonoLegacyProfiler;\r\n");
				sb.Append("typedef void (*MonoLegacyProfileFunc) (MonoLegacyProfiler *prof);\r\n");
				sb.Append("MONO_API void mono_profiler_install (MonoLegacyProfiler *prof, MonoLegacyProfileFunc callback);\r\n");
				sb.Append("#define DEFINED_LEGACY_PROFILER\r\n");
			}
			sb.Append("#include \"../dnSpyFiles/dnSpy.c\"\r\n");
			File.WriteAllBytes(filename, Encoding.UTF8.GetBytes(sb.ToString()));
		}

		void Patch_masm_fixed_props() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "msvc", "masm.fixed.props");
			var textFilePatcher = new TextFilePatcher(filename);
			textFilePatcher.Replace(line => {
				var text = line.Text;
				text = text.Replace(@"$(VCInstallDir)bin\ml.exe", "ml.exe");
				text = text.Replace(@"$(VCInstallDir)bin\amd64\ml64.exe", "ml64.exe");
				return line.Replace(text);
			});
			textFilePatcher.Write();
		}

		void Patch_bdwgc_gc_atomic_ops_h() {
			if (solutionOptions.UnityVersion.CompareTo(new UnityVersion(2018, 3, 0, "-mbe")) < 0)
				return;
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "external", "bdwgc", "include", "private", "gc_atomic_ops.h");
			var textFilePatcher = new TextFilePatcher(filename);
			textFilePatcher.Replace(line => {
				var text = line.Text;
				text = text.Replace("#elif !defined(NN_PLATFORM_CTR)", "#elif false");
				return line.Replace(text);
			});
			textFilePatcher.Write();
		}

		void Patch_bdwgc_gcconfig_h() {
			if (solutionOptions.UnityVersion.CompareTo(new UnityVersion(2019, 1, 0, "-mbe")) < 0)
				return;
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "external", "bdwgc", "include", "private", "gcconfig.h");
			var textFilePatcher = new TextFilePatcher(filename);
			int index = textFilePatcher.GetIndexOfLine("#define GCCONFIG_H");
			textFilePatcher.Insert(index + 1, "#define GC_DISABLE_INCREMENTAL");
			textFilePatcher.Write();
		}
	}
}
