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

using System.IO;

namespace UnityMonoDllSourceCodePatcher.V35 {
	sealed class LibmonoProjectPatcher : ProjectPatcher {
		public LibmonoProjectPatcher(SolutionOptions solutionOptions)
			: base(solutionOptions, solutionOptions?.LibmonoProject) {
		}

		protected override void PatchCore() {
			PatchOutDirs();
			PatchPreprocessorDefinitions();
			RemoveBrowseInformationTags();
			PatchDebugInformationFormats();
			PatchGenerateDebugInformationTags();
			AddSourceFiles();
		}

		void PatchOutDirs() {
			var name = Path.GetFileName(solutionOptions.UnityVersionDir);
			var newValue = @"..\..\builds\$(Configuration)\" + name + @"\win$(PlatformArchitecture)\";
			textFilePatcher.Replace(line => {
				if (!line.Text.Contains("<OutDir"))
					return line;
				const string PATTERN = "\">";
				int index = line.Text.IndexOf(PATTERN);
				if (!line.Text.Contains("<OutDir Condition") || !line.Text.EndsWith("</OutDir>") || index < 0)
					throw new ProgramException("Unexpected tag content");
				var first = line.Text.Substring(0, index + PATTERN.Length);
				var newText = first + newValue + "</OutDir>";
				return line.Replace(newText);
			});
		}

		void PatchDebugInformationFormats() {
			foreach (var itemDefInfo in GetItemDefinitionGroups(ConstantsV35.ReleaseConfigsWithNoPdb)) {
				foreach (var info in GetTags("ClCompile", itemDefInfo.startTagIndex + 1, itemDefInfo.endTagIndex))
					UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "DebugInformationFormat", "None");
			}
		}

		void PatchGenerateDebugInformationTags() {
			foreach (var itemDefInfo in GetItemDefinitionGroups(ConstantsV35.ReleaseConfigsWithNoPdb)) {
				foreach (var info in GetTags("Link", itemDefInfo.startTagIndex + 1, itemDefInfo.endTagIndex))
					UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "GenerateDebugInformation", "false");
			}
		}

		void AddSourceFiles() {
			int index = textFilePatcher.GetIndexOfLine(line => line.Text.Contains(@"<ClCompile Include=""..\mono\utils\dlmalloc.c"""));
			var indent = textFilePatcher.GetLeadingWhitespace(index);
			textFilePatcher.Insert(index, indent + @"<ClCompile Include=""..\mono\mini\dnSpy.c"" />");
		}
	}
}
