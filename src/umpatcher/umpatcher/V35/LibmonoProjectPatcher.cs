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


namespace UnityMonoDllSourceCodePatcher.V35 {
	sealed class LibmonoProjectPatcher : ProjectPatcherV35 {
		public LibmonoProjectPatcher(SolutionOptionsV35? solutionOptions)
			: base(solutionOptions, solutionOptions?.LibmonoProject) {
		}

		protected override void PatchCore() {
			PatchOutDirs();
			PatchPreprocessorDefinitions();
			RemoveBrowseInformationTags();
			PatchDebugInformationFormats(ConstantsV35.ReleaseConfigsWithNoPdb);
			PatchGenerateDebugInformationTags(ConstantsV35.ReleaseConfigsWithNoPdb);
			AddSourceFiles();
		}

		void AddSourceFiles() {
			int index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains(@"<ClCompile Include=""..\mono\utils\dlmalloc.c"""));
			var indent = textFilePatcher.GetLeadingWhitespace(index);
			textFilePatcher.Insert(index, indent + @"<ClCompile Include=""..\mono\mini\dnSpy.c"" />");
		}
	}
}
