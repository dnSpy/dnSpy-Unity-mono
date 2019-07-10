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

namespace UnityMonoDllSourceCodePatcher.V40 {
	static class ConstantsV40 {
		public static readonly string[] Submodules = new string[] {
			"external/bdwgc",
		};

		public static readonly string[] UnityFoldersToCopy_2017 = new string[] {
			"eglib",
			"external/bdwgc",
			"mono",
			"msvc",
		};

		public static readonly string[] UnityFoldersToCopy_2018 = new string[] {
			"external/bdwgc",
			"mono",
			"msvc",
		};

		public const string SolutionFilenameFormatString = "dnSpy-Unity-mono-v{0}.x-V40.sln";
		public static readonly string[] SolutionConfigurations = new string[] {
			"Debug",
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
			"Release|Win32",
			"Release|x64",
		};

		public const string OldProjectToolsVersion = "ToolsVersion=\"14.0\"";
		public const string NewProjectToolsVersion = "ToolsVersion=\"15.0\"";

		public static readonly Guid OldGuid_build_init = new Guid("92AE7622-5F58-4234-9A26-9EC71876B3F4");
		public static readonly Guid OldGuid_eglib = new Guid("158073ED-99AE-4196-9EDC-DDB2344F8466");
		public static readonly Guid OldGuid_genmdesc = new Guid("B7098DFA-31E6-4006-8A15-1C9A4E925149");
		public static readonly Guid OldGuid_libgcbdwgc = new Guid("CF169633-14AF-4DB8-BEF9-26A6C8FE4C90");
		public static readonly Guid OldGuid_libmono = new Guid("CB0D9E92-293C-439C-9AC7-C5F59B6E0771");
		public static readonly Guid OldGuid_libmono_dynamic = new Guid("675F4175-FFB1-480D-AD36-F397578844D4");
		public static readonly Guid OldGuid_libmonoruntime = new Guid("C36612BD-22D3-4B95-85E2-7FDC4FC5D739");
		public static readonly Guid OldGuid_libmono_static = new Guid("CB0D9E92-293C-439C-9AC7-C5F59B6E0772");
		public static readonly Guid OldGuid_libmonoutils = new Guid("8FC2B0C8-51AD-49DF-851F-5D01A77A75E4");
	}
}
