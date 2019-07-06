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

namespace UnityMonoDllSourceCodePatcher {
	sealed class CommandLineParserException : Exception {
		public CommandLineParserException() : this("Invalid command line arguments") { }
		public CommandLineParserException(string message) : base(message) { }
	}

	readonly struct CommandLineParser {
		readonly ProgramOptions options;

		public CommandLineParser(ProgramOptions options) =>
			this.options = options ?? throw new ArgumentNullException(nameof(options));

		public void Parse(string[] args) {
			if (args.Length == 0)
				throw new CommandLineParserException();

			if (args.Length == 2 && args[0] == "--timestamp") {
				options.ProgramCommand = ProgramCommand.Timestamp;
				options.TimestampOptions.ExecutableFilename = GetFile(args[1]);
			}
			else if (args.Length >= 4) {
				options.ProgramCommand = ProgramCommand.Patch;

				options.PatchOptions.UnityVersion = GetUnityVersion(args[0]);
				options.PatchOptions.UnityGitHash = GetGitHash(args[1]);
				options.PatchOptions.UnityRepoPath = GetGitRepo(args[2]);
				options.PatchOptions.DnSpyUnityMonoRepoPath = GetGitRepo(args[3]);
				for (int i = 4; i < args.Length; i++) {
					var arg = args[i];
					var nextArg = i + 1 < args.Length ? args[i + 1] : null;
					switch (arg) {
					case "--git":
						if (nextArg == null)
							throw new CommandLineParserException("Missing path to git executable");
						options.PatchOptions.GitExePath = GetFile(nextArg);
						i++;
						break;

					case "--winpver":
						if (nextArg == null)
							throw new CommandLineParserException("Missing WindowsTargetPlatformVersion value");
						options.PatchOptions.WindowsTargetPlatformVersion = nextArg;
						i++;
						break;

					case "--toolset":
						if (nextArg == null)
							throw new CommandLineParserException("Missing PlatformToolset value");
						options.PatchOptions.PlatformToolset = nextArg;
						i++;
						break;

					default:
						throw new CommandLineParserException($"Unknown option '{arg}'");
					}
				}
			}
			else
				throw new CommandLineParserException();
		}

		static string GetUnityVersion(string version) {
			if (!UnityVersion.TryParse(version, out _))
				throw new CommandLineParserException($"Invalid unity version: '{version}'");
			return version;
		}

		static string GetGitHash(string hash) {
			if (!GitUtils.IsValidGitHash(hash))
				throw new CommandLineParserException("Invalid git hash (must be 40 chars long and only hex chars)");
			return hash;
		}

		static string GetGitRepo(string path) {
			if (!GitUtils.IsGitRepo(path))
				throw new CommandLineParserException($"Not a git repo: '{path}'");
			return Path.GetFullPath(path);
		}

		static string GetFile(string filename) {
			if (filename == null)
				throw new CommandLineParserException();
			return Path.GetFullPath(FileUtils.GetExistingFile(filename));
		}

		public static void PrintUsage() {
			var message = $@"
Patches Unity Mono source code

Usage:

    umpatcher <unity-version> <unity-git-hash> <unity-repo-path> <dnSpy-Unity-mono-repo-path> [--git PATH] [--winpver VER] [--toolset VER]
    umpatcher --timestamp <mono.dll>

    --git PATH               Path to git executable if umpatcher can't find it in the usual locations
    --winpver VER            <WindowsTargetPlatformVersion> version, default is {Constants.DefaultWindowsTargetPlatformVersion}
    --toolset VER            <PlatformToolset> version, default is {Constants.DefaultPlatformToolset}

Examples:

    umpatcher 2017.1.2 0123abcd ""C:\repos\Unity"" ""C:\repos\dnSpy-Unity-mono""

Copies code, patches it and checks it in.

    umpatcher --timestamp ""C:\some\path\mono.dll""

Shows the timestamp stored in the PE header.
";
			message = message.Trim();
			Console.WriteLine(message);
		}
	}
}
