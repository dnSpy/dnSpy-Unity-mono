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
	sealed class PatcherV40 : Patcher {
		readonly SolutionOptionsV40 solutionOptions;

		public PatcherV40(string unityVersion, string unityGitHash, string unityRepoPath, string dnSpyUnityMonoRepoPath, string gitExePath, string windowsTargetPlatformVersion, string platformToolset)
			: base(unityVersion, unityGitHash, unityRepoPath, dnSpyUnityMonoRepoPath, gitExePath) {
			solutionOptions = new SolutionOptionsV40(dnSpyRepo.RepoPath, dnSpyVersionPath, unityVersion, windowsTargetPlatformVersion, platformToolset, GetProjectFilesKind(unityVersion));
		}

		static ProjectFilesKind GetProjectFilesKind(string unityVersion) {
			if (!UnityVersion.TryParse(unityVersion, out var version))
				throw new InvalidOperationException($"Invalid unity version: {unityVersion}");
			if (version.Major >= 2018)
				return ProjectFilesKind.V2018;
			if (version.Major == 2017)
				return ProjectFilesKind.V2017;
			// 5.5-mbe and 5.6-mbe exist too, but they weren't part of the official 5.5 and 5.6 releases, probably only betas
			throw new Exception($"Unknown version: {version}");
		}

		protected override string[] Submodules => ConstantsV40.Submodules;
		protected override string[] UnityFoldersToCopy {
			get {
				if (solutionOptions.UnityVersion.Major >= 2018)
					return ConstantsV40.UnityFoldersToCopy_2018;
				if (solutionOptions.UnityVersion.Major == 2017)
					return ConstantsV40.UnityFoldersToCopy_2017;
				throw new InvalidOperationException($"Unknown version: {solutionOptions.UnityVersion}");
			}
		}

		protected override void PatchOriginalFilesCore() {
			new SolutionPatcher(solutionOptions).Patch();
			if (solutionOptions.BuildInitProject != null) new BuildInitProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.EglibProject != null) new EglibProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.GenmdescProject != null) new GenmdescProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibgcbdwgcProject != null) new LibgcbdwgcProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibmonoProject != null) new LibmonoProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibmonoDynamicProject != null) new LibmonoDynamicProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibmonoruntimeProject != null) new LibmonoruntimeProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibmonoStaticProject != null) new LibmonoStaticProjectPatcher(solutionOptions).Patch();
			if (solutionOptions.LibmonoutilsProject != null) new LibmonoutilsProjectPatcher(solutionOptions).Patch();
			new SourceCodePatcher(solutionOptions).Patch();
		}
	}
}
