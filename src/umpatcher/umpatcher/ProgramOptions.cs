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

namespace UnityMonoDllSourceCodePatcher {
	enum ProgramCommand {
		Unknown,
		Timestamp,
		Patch,
	}

	sealed class ProgramOptions {
		public ProgramCommand ProgramCommand;
		public readonly TimestampOptions TimestampOptions = new TimestampOptions();
		public readonly PatchOptions PatchOptions = new PatchOptions();
	}

	sealed class TimestampOptions {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public string ExecutableFilename;
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}

	sealed class PatchOptions {
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public string UnityVersion;
		public string UnityGitHash;
		public string UnityRepoPath;
		public string DnSpyUnityMonoRepoPath;
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
		public string? GitExePath;
		public string? WindowsTargetPlatformVersion;
		public string? PlatformToolset;
	}
}
