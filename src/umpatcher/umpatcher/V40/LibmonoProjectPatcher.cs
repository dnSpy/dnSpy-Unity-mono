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
	sealed class LibmonoProjectPatcher : ProjectPatcherV40 {
		readonly ProjectInfo libgcbdwgcProject;

		public LibmonoProjectPatcher(SolutionOptionsV40? solutionOptions)
			: base(solutionOptions, solutionOptions?.LibmonoProject) {
			libgcbdwgcProject = solutionOptions!.LibgcbdwgcProject ?? throw new InvalidOperationException();
		}

		protected override void PatchCore() {
			PatchOutDirs();
			PatchDebugInformationFormats(ConstantsV40.ReleaseConfigsWithNoPdb);
			PatchGenerateDebugInformationTags(ConstantsV40.ReleaseConfigsWithNoPdb);
			AddProjectReference(libgcbdwgcProject);
			RemoveProjectReference("libgc.vcxproj");
			RemoveProjectReference("libgcmonosgen.vcxproj");
			PatchSolutionDir();
		}
	}
}
