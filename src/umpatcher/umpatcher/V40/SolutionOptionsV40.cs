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
using System.Collections.Generic;
using System.IO;

namespace UnityMonoDllSourceCodePatcher.V40 {
	enum ProjectFilesKind {
		V2017,
		V2018,
	}

	sealed class SolutionOptionsV40 : SolutionOptions {
		public readonly ProjectInfo? BuildInitProject;
		public readonly ProjectInfo? EglibProject;
		public readonly ProjectInfo? GenmdescProject;
		public readonly ProjectInfo? LibgcbdwgcProject;
		public readonly ProjectInfo? LibmonoProject;
		public readonly ProjectInfo? LibmonoDynamicProject;
		public readonly ProjectInfo? LibmonoruntimeProject;
		public readonly ProjectInfo? LibmonoStaticProject;
		public readonly ProjectInfo? LibmonoutilsProject;

		public override IEnumerable<ProjectInfo> AllProjects {
			get {
				if (BuildInitProject != null) yield return BuildInitProject;
				if (EglibProject != null) yield return EglibProject;
				if (GenmdescProject != null) yield return GenmdescProject;
				if (LibgcbdwgcProject != null) yield return LibgcbdwgcProject;
				if (LibmonoProject != null) yield return LibmonoProject;
				if (LibmonoDynamicProject != null) yield return LibmonoDynamicProject;
				if (LibmonoruntimeProject != null) yield return LibmonoruntimeProject;
				if (LibmonoStaticProject != null) yield return LibmonoStaticProject;
				if (LibmonoutilsProject != null) yield return LibmonoutilsProject;
			}
		}

		public override string[] SolutionConfigurations => ConstantsV40.SolutionConfigurations;
		public override (string archName, string configName)[] SolutionPlatforms => ConstantsV40.SolutionPlatforms;
		public override string[] SolutionBuildInfos => ConstantsV40.SolutionBuildInfos;

		public SolutionOptionsV40(string solutionDir, string versionPath, string unityVersion, string windowsTargetPlatformVersion, string platformToolset, ProjectFilesKind projectFilesKind)
			: base(solutionDir, versionPath, unityVersion, windowsTargetPlatformVersion, platformToolset, ConstantsV40.SolutionFilenameFormatString) {
			var msvcPath = Path.Combine(versionPath, "msvc");
			switch (projectFilesKind) {
			case ProjectFilesKind.V2017:
				BuildInitProject = new ProjectInfo(ConstantsV40.OldGuid_build_init, Path.Combine(msvcPath, "build-init.vcxproj"));
				EglibProject = new ProjectInfo(ConstantsV40.OldGuid_eglib, Path.Combine(msvcPath, "eglib.vcxproj"));
				GenmdescProject = new ProjectInfo(ConstantsV40.OldGuid_genmdesc, Path.Combine(msvcPath, "genmdesc.vcxproj"));
				LibgcbdwgcProject = new ProjectInfo(ConstantsV40.OldGuid_libgcbdwgc, Path.Combine(msvcPath, "libgcbdwgc.vcxproj"));
				LibmonoProject = new ProjectInfo(ConstantsV40.OldGuid_libmono, Path.Combine(msvcPath, "libmono.vcxproj"));
				LibmonoruntimeProject = new ProjectInfo(ConstantsV40.OldGuid_libmonoruntime, Path.Combine(msvcPath, "libmonoruntime.vcxproj"));
				LibmonoStaticProject = new ProjectInfo(ConstantsV40.OldGuid_libmono_static, Path.Combine(msvcPath, "libmono-static.vcxproj"));
				LibmonoutilsProject = new ProjectInfo(ConstantsV40.OldGuid_libmonoutils, Path.Combine(msvcPath, "libmonoutils.vcxproj"));
				break;

			case ProjectFilesKind.V2018:
				BuildInitProject = new ProjectInfo(ConstantsV40.OldGuid_build_init, Path.Combine(msvcPath, "build-init.vcxproj"));
				EglibProject = new ProjectInfo(ConstantsV40.OldGuid_eglib, Path.Combine(msvcPath, "eglib.vcxproj"));
				GenmdescProject = new ProjectInfo(ConstantsV40.OldGuid_genmdesc, Path.Combine(msvcPath, "genmdesc.vcxproj"));
				LibgcbdwgcProject = new ProjectInfo(ConstantsV40.OldGuid_libgcbdwgc, Path.Combine(msvcPath, "libgcbdwgc.vcxproj"));
				LibmonoDynamicProject = new ProjectInfo(ConstantsV40.OldGuid_libmono_dynamic, Path.Combine(msvcPath, "libmono-dynamic.vcxproj"));
				break;

			default:
				throw new InvalidOperationException();
			}
		}
	}
}
