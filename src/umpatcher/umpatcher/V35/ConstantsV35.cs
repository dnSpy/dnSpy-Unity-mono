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

namespace UnityMonoDllSourceCodePatcher.V35 {
	static class ConstantsV35 {
		public static readonly string[] UnityFoldersToCopy = new string[] {
			"eglib",
			"libgc",
			"mono",
			"msvc",
			"unity",
		};

		public const string SolutionFilenameFormatString = "dnSpy-Unity-mono-v{0}.x.sln";
		public static readonly string[] SolutionConfigurations = new string[] {
			"Debug_eglib",
			"Debug",
			"Release_eglib_xarm",
			"Release_eglib",
			"Release",
		};
		public static readonly (string archName, string configName)[] SolutionPlatforms = new(string, string)[] {
			("x64", "x64"),
			("x86", "Win32"),
		};
		public static readonly string[] SolutionBuildInfos = new string[] {
			"ActiveCfg",
			"Build.0",
		};

		public static readonly string[] ReleaseConfigsWithNoPdb = new string[] {
			"Release_eglib|Win32",
			"Release_eglib|x64",
		};

		public const string OldProjectToolsVersion = "ToolsVersion=\"4.0\"";
		public const string NewProjectToolsVersion = "ToolsVersion=\"15.0\"";

		public static readonly Guid OldGuid_eglib = new Guid("158073ED-99AE-4196-9EDC-DDB2344F8466");
		public static readonly Guid OldGuid_genmdesc = new Guid("B7098DFA-31E6-4006-8A15-1C9A4E925149");
		public static readonly Guid OldGuid_libgc = new Guid("EB56000B-C80B-4E8B-908D-D84D31B517D3");
		public static readonly Guid OldGuid_libmono = new Guid("CB0D9E92-293C-439C-9AC7-C5F59B6E0771");

		public const string OldWinVer = "0x0500";
		public const string NewWinVer = "0x0501";
	}
}
