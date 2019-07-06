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
using System.IO;

namespace UnityMonoDllSourceCodePatcher.V40 {
	sealed class LibmonoDynamicProjectPatcher : ProjectPatcherV40 {
		readonly ProjectInfo libgcbdwgcProject;

		public LibmonoDynamicProjectPatcher(SolutionOptionsV40? solutionOptions)
			: base(solutionOptions, solutionOptions?.LibmonoDynamicProject) {
			libgcbdwgcProject = solutionOptions!.LibgcbdwgcProject ?? throw new InvalidOperationException();
		}

		protected override void PatchCore() {
			PatchOutDirs();
			PatchDebugInformationFormats(ConstantsV40.ReleaseConfigsWithNoPdb);
			PatchGenerateDebugInformationTags(ConstantsV40.ReleaseConfigsWithNoPdb);
			AddSourceFiles();
			AddProjectReference(libgcbdwgcProject);
			RemoveProjectReference("libgc.vcxproj");
			PatchSolutionDir();
		}

		void AddSourceFiles() {
			var textFilePatcher = new TextFilePatcher(Path.Combine(solutionOptions.UnityVersionDir, "msvc", "libmini-common.targets"));
			int index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains(@"<ClCompile Include=""$(MonoSourceLocation)\mono\mini\debugger-agent.c"""));
			var indent = textFilePatcher.GetLeadingWhitespace(index);
			textFilePatcher.Insert(index + 1, indent + @"<ClCompile Include=""$(MonoSourceLocation)\mono\mini\dnSpy.c"" />");
			textFilePatcher.Write();
		}
	}
}
