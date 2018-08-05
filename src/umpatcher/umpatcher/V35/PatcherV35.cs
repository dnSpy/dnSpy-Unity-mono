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
	sealed class PatcherV35 : Patcher {
		readonly SolutionOptionsV35 solutionOptions;

		public PatcherV35(string unityVersion, string unityGitHash, string unityRepoPath, string dnSpyUnityMonoRepoPath, string gitExePath, string windowsTargetPlatformVersion, string platformToolset)
			: base(unityVersion, unityGitHash, unityRepoPath, dnSpyUnityMonoRepoPath, gitExePath) {
			solutionOptions = new SolutionOptionsV35(dnSpyRepo.RepoPath, dnSpyVersionPath, unityVersion, windowsTargetPlatformVersion, platformToolset);
		}

		protected override string[] Submodules => Array.Empty<string>();
		protected override string[] UnityFoldersToCopy => ConstantsV35.UnityFoldersToCopy;

		protected override void PatchOriginalFilesCore() {
			new SolutionPatcher(solutionOptions).Patch();
			new EglibProjectPatcher(solutionOptions).Patch();
			new GenmdescProjectPatcher(solutionOptions).Patch();
			new LibgcProjectPatcher(solutionOptions).Patch();
			new LibmonoProjectPatcher(solutionOptions).Patch();
			new SourceCodePatcher(solutionOptions).Patch();
		}
	}
}
