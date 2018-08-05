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

namespace UnityMonoDllSourceCodePatcher.V35 {
	sealed class SourceCodePatcher {
		readonly SolutionOptionsV35 solutionOptions;

		public SourceCodePatcher(SolutionOptionsV35 solutionOptions) =>
			this.solutionOptions = solutionOptions ?? throw new ArgumentNullException(nameof(solutionOptions));

		public void Patch() {
			Patch_mono_utils_mono_compiler_h();
			Patch_unity_unity_utils_c();
			Patch_mono_metadata_icall_c();
			Patch_mono_mini_debugger_agent_c();
			Patch_mono_mini_mini_c();
			Add_mono_mini_dnSpy_c();
		}

		void Patch_mono_utils_mono_compiler_h() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "utils", "mono-compiler.h");
			var textFilePatcher = new TextFilePatcher(filename);
			textFilePatcher.Replace(line => {
				if (!line.Text.StartsWith("#define trunc(x)"))
					return line;
				return line.Replace("//" + line.Text);
			});
			textFilePatcher.Write();
		}

		void Patch_unity_unity_utils_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "unity", "unity_utils.c");
			var textFilePatcher = new TextFilePatcher(filename);
			textFilePatcher.Replace(line => {
				if (!line.Text.Contains("stdout->_file = fd;"))
					return line;
				return line.Replace(line.GetLeadingWhitespace() + "//" + line.Text.TrimStart());
			});
			textFilePatcher.Write();
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
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("agent_config.address = g_strdup_printf")).Single();
				Verify(lines[index + 1].Text, "\t\t\t\t}");
				Verify(lines[index + 2].Text, "\t\t\t}");
				textFilePatcher.Insert(index + 3, "\t\t} else if (dnSpy_debugger_agent_parse_options (arg)) {");
			}

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("mono_profiler_install_method_invoke (start_runtime_invoke, end_runtime_invoke);")).Single();
				Verify(lines[index + 1].Text, string.Empty);
				textFilePatcher.Insert(index + 2, "\tdnSpy_debugger_init_after_agent ();");
				textFilePatcher.Insert(index + 3, string.Empty);
			}

			{
				int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("case CMD_THREAD_GET_FRAME_INFO: {")).Single();
				index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains("tls = mono_g_hash_table_lookup (thread_to_tls, thread);"), index);
				Verify(lines[index + 1].Text, "\t\tmono_loader_unlock ();");
				Verify(lines[index + 2].Text, "\t\tg_assert (tls);");
				lines[index + 2] = lines[index + 2].Replace("\t\tif (!tls)");
				textFilePatcher.Insert(index + 3, "\t\t\treturn ERR_INVALID_ARGUMENT;");
			}

			textFilePatcher.Write();
		}

		void Patch_mono_mini_mini_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "mini", "mini.c");
			var textFilePatcher = new TextFilePatcher(filename);
			int index = textFilePatcher.GetIndexesOfLine(line => line.Text.Contains("MONO_PROBE_VES_INIT_BEGIN")).Single();
			textFilePatcher.Insert(++index, string.Empty);
			textFilePatcher.Insert(++index, "\tdnSpy_debugger_init ();");
			textFilePatcher.Write();
		}

		void Add_mono_mini_dnSpy_c() {
			var filename = Path.Combine(solutionOptions.UnityVersionDir, "mono", "mini", "dnSpy.c");
			var sourceCode =
				"#include \"debug-mini.h\"\r\n" +
				"#include \"debugger-agent.h\"\r\n" +
				"#include \"../dnSpyFiles/dnSpy.c\"\r\n";
			File.WriteAllBytes(filename, Encoding.UTF8.GetBytes(sourceCode));
		}
	}
}
