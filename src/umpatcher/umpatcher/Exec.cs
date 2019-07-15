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

using System.Diagnostics;

namespace UnityMonoDllSourceCodePatcher {
	static class Exec {
		public static int Run(string workingDir, string filename, string args) =>
			RunCore(workingDir, filename, args, redirectOutput: false, out _, out _);

		public static int Run(string workingDir, string filename, string args, out string standardOutput, out string standardError) =>
			RunCore(workingDir, filename, args, redirectOutput: true, out standardOutput, out standardError);

		static int RunCore(string workingDir, string filename, string args, bool redirectOutput, out string standardOutput, out string standardError) {
			var options = new ProcessStartInfo(filename, args);
			options.WorkingDirectory = workingDir;
			options.CreateNoWindow = true;
			options.UseShellExecute = false;
			if (redirectOutput) {
				options.RedirectStandardOutput = true;
				options.RedirectStandardError = true;
			}
			using (var process = Process.Start(options)) {
				process.WaitForExit();
				if (redirectOutput) {
					standardOutput = process.StandardOutput.ReadToEnd();
					standardError = process.StandardError.ReadToEnd();
				}
				else {
					standardOutput = string.Empty;
					standardError = string.Empty;
				}
				return process.ExitCode;
			}
		}
	}
}
