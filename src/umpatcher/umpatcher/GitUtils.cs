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
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UnityMonoDllSourceCodePatcher {
	static class GitUtils {
		static string GitExeName => RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "git.exe" : "git";

		public static string? FindGit() {
			if (TryFindGitFromPathEnvVar(out var gitPath))
				return gitPath;
			if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
				if (TryFindFromPath_ProgramFiles_Windows(Environment.SpecialFolder.ProgramFiles, out gitPath))
					return gitPath;
				if (TryFindFromPath_ProgramFiles_Windows(Environment.SpecialFolder.ProgramFilesX86, out gitPath))
					return gitPath;
			}

			return null;
		}

		static bool TryFindGitFromPathEnvVar([NotNullWhenTrue] out string? gitPath) {
			gitPath = null;
			var pathEnvVar = Environment.GetEnvironmentVariable("PATH");
			if (pathEnvVar == null)
				return false;
			foreach (var path in pathEnvVar.Split(new char[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries)) {
				if (TryFindFromPath(path.Trim(), out gitPath))
					return true;
			}

			return false;
		}

		static bool TryFindFromPath_ProgramFiles_Windows(Environment.SpecialFolder folder, [NotNullWhenTrue] out string? gitPath) {
			Debug.Assert(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
			gitPath = null;
			var path = Environment.GetFolderPath(folder);
			if (!Directory.Exists(path))
				return false;
			path = Path.Combine(path, "Git", "bin");
			return TryFindFromPath(path, out gitPath);
		}

		static bool TryFindFromPath(string path, [NotNullWhenTrue] out string? gitPath) {
			gitPath = null;
			if (!Directory.Exists(path))
				return false;
			try {
				var exePath = Path.Combine(path, GitExeName);
				if (File.Exists(exePath)) {
					gitPath = exePath;
					return true;
				}

				return false;
			}
			catch {
				return false;
			}
		}

		public static bool IsValidGitHash(string s) {
			if (s == null)
				return false;
			if (s.Length != 40)
				return false;
			foreach (var c in s) {
				if (!char.IsDigit(c) && !(('a' <= c && c <= 'f') || ('A' <= c && c <= 'F')))
					return false;
			}
			return true;
		}

		public static bool IsGitRepo(string path) {
			try {
				if (!Directory.Exists(path))
					return false;
				var gitDir = Path.Combine(path, ".git");
				if (!Directory.Exists(gitDir) && !File.Exists(gitDir))
					return false;

				return true;
			}
			catch {
				return false;
			}
		}
	}
}
