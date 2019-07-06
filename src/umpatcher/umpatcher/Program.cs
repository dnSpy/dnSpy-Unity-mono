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

namespace UnityMonoDllSourceCodePatcher {
	enum ExitCodes {
		OK = 0,
		BadCommandLine = 1,
		Exception = 2,
		OtherError = 3,
	}

	sealed class ProgramException : Exception {
		public ProgramException(string message) : base(message) { }
	}

	sealed class Program {
		static int Main(string[] args) {
			try {
				new Program().Start(args);
				return (int)ExitCodes.OK;
			}
			catch (CommandLineParserException ex) {
				CommandLineParser.PrintUsage();
				Console.WriteLine();
				Console.WriteLine(ex.Message);
				return (int)ExitCodes.BadCommandLine;
			}
			catch (ProgramException ex) {
				Console.WriteLine(ex.Message);
				return (int)ExitCodes.OtherError;
			}
			catch (Exception ex) {
				Console.WriteLine(ex.ToString());
				return (int)ExitCodes.Exception;
			}
		}

		void Start(string[] args) {
			var options = new ProgramOptions();
			new CommandLineParser(options).Parse(args);

			switch (options.ProgramCommand) {
			case ProgramCommand.Unknown:
			default:
				throw new ProgramException($"Unknown enum value: {options.ProgramCommand}");

			case ProgramCommand.Timestamp:
				ShowTimestamp(options.TimestampOptions);
				break;

			case ProgramCommand.Patch:
				Patch(options.PatchOptions);
				break;
			}
		}

		void ShowTimestamp(TimestampOptions options) {
			if (!FileUtils.TryGetPortableExecutableTimestamp(options.ExecutableFilename, out var timestamp))
				throw new ProgramException($"Not a PE file: '{options.ExecutableFilename}'");
			var timeUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(timestamp);
			Console.WriteLine($"UTC  : {timeUtc}");
			Console.WriteLine($"LOCAL: {timeUtc.ToLocalTime()}");
		}

		void Patch(PatchOptions options) {
			if (options.GitExePath == null)
				options.GitExePath = GitUtils.FindGit();
			if (options.GitExePath == null)
				throw new ProgramException("Could not find git executable in the usual places. Use the --git option");
			if (options.WindowsTargetPlatformVersion == null)
				options.WindowsTargetPlatformVersion = Constants.DefaultWindowsTargetPlatformVersion;
			if (options.PlatformToolset == null)
				options.PlatformToolset = Constants.DefaultPlatformToolset;

			Patcher patcher;
			switch (GetPatcherKind(options.UnityVersion)) {
			default:
			case PatcherKind.Unknown:
				throw new ProgramException("Invalid version number");

			case PatcherKind.V35:
				patcher = new V35.PatcherV35(options.UnityVersion, options.UnityGitHash, options.UnityRepoPath, options.DnSpyUnityMonoRepoPath, options.GitExePath, options.WindowsTargetPlatformVersion, options.PlatformToolset);
				break;

			case PatcherKind.V40:
				patcher = new V40.PatcherV40(options.UnityVersion, options.UnityGitHash, options.UnityRepoPath, options.DnSpyUnityMonoRepoPath, options.GitExePath, options.WindowsTargetPlatformVersion, options.PlatformToolset);
				break;
			}
			patcher.Patch();
		}

		static PatcherKind GetPatcherKind(string unityVersion) {
			if (!UnityVersion.TryParse(unityVersion, out var version))
				throw new ProgramException("Invalid version number");
			if (version.Extra == string.Empty)
				return PatcherKind.V35;
			if (version.Extra == "-mbe")
				return PatcherKind.V40;
			return PatcherKind.Unknown;
		}

		enum PatcherKind {
			Unknown,
			V35,
			V40,
		}
	}
}
