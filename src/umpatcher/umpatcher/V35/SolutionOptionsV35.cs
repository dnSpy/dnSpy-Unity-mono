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

using System.Collections.Generic;
using System.IO;

namespace UnityMonoDllSourceCodePatcher.V35 {
	sealed class SolutionOptionsV35 : SolutionOptions {
		public readonly ProjectInfo EglibProject;
		public readonly ProjectInfo GenmdescProject;
		public readonly ProjectInfo LibgcProject;
		public readonly ProjectInfo LibmonoProject;

		public override IEnumerable<ProjectInfo> AllProjects {
			get {
				yield return EglibProject;
				yield return GenmdescProject;
				yield return LibgcProject;
				yield return LibmonoProject;
			}
		}

		public override string[] SolutionConfigurations => ConstantsV35.SolutionConfigurations;
		public override (string archName, string configName)[] SolutionPlatforms => ConstantsV35.SolutionPlatforms;
		public override string[] SolutionBuildInfos => ConstantsV35.SolutionBuildInfos;

		public SolutionOptionsV35(string solutionDir, string versionPath, string unityVersion, string windowsTargetPlatformVersion, string platformToolset)
			: base(solutionDir, versionPath, unityVersion, windowsTargetPlatformVersion, platformToolset, ConstantsV35.SolutionFilenameFormatString) {
			var msvcPath = Path.Combine(versionPath, "msvc");
			EglibProject = new ProjectInfo(ConstantsV35.OldGuid_eglib, Path.Combine(msvcPath, "eglib.vcxproj"));
			GenmdescProject = new ProjectInfo(ConstantsV35.OldGuid_genmdesc, Path.Combine(msvcPath, "genmdesc.vcxproj"));
			LibgcProject = new ProjectInfo(ConstantsV35.OldGuid_libgc, Path.Combine(msvcPath, "libgc.vcxproj"));
			LibmonoProject = new ProjectInfo(ConstantsV35.OldGuid_libmono, Path.Combine(msvcPath, "libmono.vcxproj"));
		}
	}
}
