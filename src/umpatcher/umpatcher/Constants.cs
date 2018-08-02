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
	static class Constants {
		public const string DnSpyUnityRepo_master_Branch = "master";
		public const string DnSpyUnityRepo_dnSpy_Branch = "dnSpy";
		public const string UnityVersionPrefix = "unity-";
		public const string ReadMeFilename = "README.md";

		public static readonly string[] VersionTableHeaderLines = new string[] {
			"version | git hash",
			"--------|---------",
		};
		public const string VersionTableEndLine = "";

		public const string DefaultWindowsTargetPlatformVersion = "10.0.16299.0";
		public const string DefaultPlatformToolset = "v141";

		public const string GitCleanTreeMessage = "nothing to commit, working tree clean";
	}
}
