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

namespace UnityMonoDllSourceCodePatcher {
	sealed class SolutionOptions {
		public readonly Guid SolutionDirGuid;

		public string SolutionDir { get; }
		public string SolutionFilename { get; }
		public string UnityVersionDir { get; }

		public readonly ProjectInfo EglibProject;
		public readonly ProjectInfo GenmdescProject;
		public readonly ProjectInfo LibgcProject;
		public readonly ProjectInfo LibmonoProject;
		public IEnumerable<ProjectInfo> AllProjects {
			get {
				yield return EglibProject;
				yield return GenmdescProject;
				yield return LibgcProject;
				yield return LibmonoProject;
			}
		}

		public readonly string WindowsTargetPlatformVersion;
		public readonly string PlatformToolset;

		public SolutionOptions(string solutionDir, string versionPath, string windowsTargetPlatformVersion, string platformToolset) {
			SolutionDirGuid = Guid.NewGuid();
			SolutionDir = solutionDir ?? throw new ArgumentNullException(nameof(solutionDir));
			SolutionFilename = Path.Combine(solutionDir, Constants.SolutionFilename);
			UnityVersionDir = versionPath ?? throw new ArgumentNullException(nameof(versionPath));
			var msvcPath = Path.Combine(versionPath, "msvc");
			EglibProject = new ProjectInfo(Constants.OldGuid_eglib, Path.Combine(msvcPath, "eglib.vcxproj"));
			GenmdescProject = new ProjectInfo(Constants.OldGuid_genmdesc, Path.Combine(msvcPath, "genmdesc.vcxproj"));
			LibgcProject = new ProjectInfo(Constants.OldGuid_libgc, Path.Combine(msvcPath, "libgc.vcxproj"));
			LibmonoProject = new ProjectInfo(Constants.OldGuid_libmono, Path.Combine(msvcPath, "libmono.vcxproj"));
			WindowsTargetPlatformVersion = windowsTargetPlatformVersion ?? throw new ArgumentNullException(nameof(windowsTargetPlatformVersion));
			PlatformToolset = platformToolset ?? throw new ArgumentNullException(nameof(platformToolset));
		}
	}
}
