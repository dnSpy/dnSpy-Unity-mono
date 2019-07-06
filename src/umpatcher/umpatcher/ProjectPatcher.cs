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
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace UnityMonoDllSourceCodePatcher {
	abstract class ProjectPatcher {
		protected readonly SolutionOptions solutionOptions;
		protected readonly ProjectInfo project;
		readonly string oldProjectToolsVersion;
		readonly string newProjectToolsVersion;
		protected readonly TextFilePatcher textFilePatcher;

		protected ProjectPatcher(SolutionOptions? solutionOptions, ProjectInfo? project, string oldProjectToolsVersion, string newProjectToolsVersion) {
			this.solutionOptions = solutionOptions ?? throw new ArgumentNullException(nameof(solutionOptions));
			this.project = project ?? throw new ArgumentNullException(nameof(project));
			this.oldProjectToolsVersion = oldProjectToolsVersion ?? throw new ArgumentNullException(nameof(oldProjectToolsVersion));
			this.newProjectToolsVersion = newProjectToolsVersion ?? throw new ArgumentNullException(nameof(newProjectToolsVersion));
			Debug.Assert(solutionOptions.AllProjects.Any(a => a == project));
			textFilePatcher = new TextFilePatcher(project.Filename);
		}

		protected abstract void PatchCore();

		public void Patch() {
			PatchToolsVersion();
			PatchProjectGuidTags();
			PatchProjectTags();
			AddWindowsTargetPlatformVersion();
			AddPlatformToolset();
			PatchCore();
			textFilePatcher.Write();
		}

		void PatchToolsVersion() {
			if (!TryPatchToolsVersion(0) && !TryPatchToolsVersion(1))
				throw new ProgramException($"Couldn't patch project ToolsVersion attribute, file '{project.Filename}'");
		}

		bool TryPatchToolsVersion(int index) {
			var lines = textFilePatcher.Lines;
			if (index >= lines.Count)
				return false;
			var line = lines[index];
			if (line.IsRemoved || !line.Text.Contains("<Project"))
				return false;
			var newString = line.Text.Replace(oldProjectToolsVersion, newProjectToolsVersion);
			if (newString == line.Text)
				return false;
			lines[index] = line.Replace(newString);
			return true;
		}

		void PatchProjectGuidTags() => textFilePatcher.Replace(line => ReplaceGuids("<ProjectGuid>", line));
		void PatchProjectTags() => textFilePatcher.Replace(line => ReplaceGuids("<Project>", line));

		TextLine ReplaceGuids(string pattern, TextLine line) {
			var text = line.Text;
			if (!text.Contains(pattern))
				return line;

			foreach (var project in solutionOptions.AllProjects) {
				text = text.Replace(project.OldGuidLowerString, project.NewGuidLowerString);
				text = text.Replace(project.OldGuidUpperString, project.NewGuidUpperString);
			}
			return line.Replace(text);
		}

		IEnumerable<(int startTagIndex, int endTagIndex)> GetProjectReferences() => GetTags("ProjectReference");
		IEnumerable<(int startTagIndex, int endTagIndex)> GetPropertyGroups() => GetTags("PropertyGroup");
		protected IEnumerable<(int startTagIndex, int endTagIndex)> GetTags(string tagName) =>
			GetTags(tagName, 0, textFilePatcher.Lines.Count);

		protected IEnumerable<(int startTagIndex, int endTagIndex)> GetTags(string tagName, int startIndex, int endIndex) {
			string tagNamePattern1 = "<" + tagName;
			string tagNamePattern2 = "<" + tagName + ">";
			string tagNamePattern3 = "<" + tagName + " ";
			string endTagPattern = "</" + tagName + ">";
			int index = startIndex;
			for (; ; index++) {
				if (!textFilePatcher.TryGetIndexOfLine(line => line.Text.Contains(tagNamePattern1) && (line.Text.Contains(tagNamePattern2) || line.Text.Contains(tagNamePattern3)), index, out index))
					break;
				var tagStartIndex = index;
				if (tagStartIndex >= endIndex)
					break;
				var firstLine = textFilePatcher.Lines[tagStartIndex];
				if (firstLine.Text.EndsWith("/>"))
					continue;
				if (!textFilePatcher.TryGetIndexOfLine(line => line.Text.Contains(endTagPattern), index + 1, out index))
					throw new ProgramException("Couldn't find the end tag");
				if (index >= endIndex)
					break;
				yield return (tagStartIndex, index);
			}
		}

		protected IEnumerable<(int startTagIndex, int endTagIndex)> GetItemDefinitionGroups(string[] configPlatforms) {
			foreach (var info in GetTags("ItemDefinitionGroup")) {
				var text = textFilePatcher.Lines[info.startTagIndex].Text;
				if (configPlatforms.Any(a => text.Contains(a)))
					yield return info;
			}
		}

		protected void UpdateOrCreateTag(int start, int end, string tagName, string newValue) {
			var lines = textFilePatcher.Lines;
			if ((uint)start > (uint)lines.Count)
				throw new ArgumentOutOfRangeException(nameof(start));
			if ((uint)end > (uint)lines.Count)
				throw new ArgumentOutOfRangeException(nameof(end));
			string tagNamePattern1 = "<" + tagName;
			string tagNamePattern2 = "<" + tagName + ">";
			string endTagPattern = "</" + tagName + ">";
			bool updated = false;
			for (int i = start; i < end; i++) {
				var line = lines[i];
				if (line.IsRemoved)
					continue;
				if (!line.Text.Contains(tagNamePattern1))
					continue;
				if (!line.Text.Contains(tagNamePattern2) || !line.Text.EndsWith(endTagPattern))
					throw new ProgramException("Expected a one line tag");
				updated = true;
				lines[i] = line.Replace(line.GetLeadingWhitespace() + tagNamePattern2 + newValue + endTagPattern);
			}
			if (!updated)
				textFilePatcher.Insert(end, textFilePatcher.GetLeadingWhitespace(end - 1) + tagNamePattern2 + newValue + endTagPattern);
		}

		void AddWindowsTargetPlatformVersion() {
			var info = GetPropertyGroups().FirstOrDefault(a => textFilePatcher.Lines[a.startTagIndex].Text.Contains("Label=\"Globals\""));
			if (info.startTagIndex == 0 && info.endTagIndex == 0)
				throw new ProgramException($"Couldn't find Globals PropertyGroup, file '{textFilePatcher.Filename}'");
			UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "WindowsTargetPlatformVersion", solutionOptions.WindowsTargetPlatformVersion);
		}

		void AddPlatformToolset() {
			var infos = GetPropertyGroups().Where(a => textFilePatcher.Lines[a.startTagIndex].Text.Contains("Label=\"Configuration\""));
			bool updated = false;
			foreach (var info in infos) {
				UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "PlatformToolset", solutionOptions.PlatformToolset);
				updated = true;
			}
			if (!updated)
				throw new ProgramException($"Couldn't find Configuration PropertyGroup, file '{textFilePatcher.Filename}'");
		}

		protected void UpdateTreatWarningAsError(string newValue) {
			Debug.Assert(newValue == "true" || newValue == "false");
			textFilePatcher.Replace(line => {
				var text = line.Text;
				if (!text.Contains("<TreatWarningAsError>"))
					return line;
				return line.Replace(line.GetLeadingWhitespace() + "<TreatWarningAsError>" + newValue + "</TreatWarningAsError>");
			});
		}

		protected void RemoveProjectReference(string projectFilename) {
			var info = GetProjectReferences().FirstOrDefault(a => textFilePatcher.Lines[a.startTagIndex].Text.Contains($"Include=\"{projectFilename}\""));
			if (info.startTagIndex == 0 && info.endTagIndex == 0)
				throw new ProgramException($"Couldn't find {projectFilename} ProjectReference, file '{textFilePatcher.Filename}'");
			textFilePatcher.Lines.RemoveRange(info.startTagIndex, info.endTagIndex - info.startTagIndex + 1);
		}

		protected void AddProjectReference(ProjectInfo project) {
			var info = GetProjectReferences().LastOrDefault();
			if (info.startTagIndex == 0 && info.endTagIndex == 0)
				throw new ProgramException($"Couldn't find any ProjectReference, file '{textFilePatcher.Filename}'");
			if (info.endTagIndex - info.startTagIndex < 2)
				throw new ProgramException("Expected at least 3 lines");
			int index = info.endTagIndex + 1;
			textFilePatcher.Insert(index++, textFilePatcher.Lines[info.startTagIndex].GetLeadingWhitespace() + $"<ProjectReference Include=\"{Path.GetFileName(project.Filename)}\">");
			textFilePatcher.Insert(index++, textFilePatcher.Lines[info.startTagIndex + 1].GetLeadingWhitespace() + $"<Project>{{{project.NewGuidLowerString}}}</Project>");
			textFilePatcher.Insert(index++, textFilePatcher.Lines[info.endTagIndex].Text);
		}

		protected void PatchOutDirs() {
			var newVersion = new UnityVersion(solutionOptions.UnityVersion.Major, solutionOptions.UnityVersion.Minor, solutionOptions.UnityVersion.Build, string.Empty);
			var name = Constants.UnityVersionPrefix + newVersion.ToString();
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

		protected void PatchDebugInformationFormats(string[] releaseConfigsWithNoPdb) {
			foreach (var itemDefInfo in GetItemDefinitionGroups(releaseConfigsWithNoPdb)) {
				foreach (var info in GetTags("ClCompile", itemDefInfo.startTagIndex + 1, itemDefInfo.endTagIndex))
					UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "DebugInformationFormat", "None");
			}
		}

		protected void PatchGenerateDebugInformationTags(string[] releaseConfigsWithNoPdb) {
			foreach (var itemDefInfo in GetItemDefinitionGroups(releaseConfigsWithNoPdb)) {
				foreach (var info in GetTags("Link", itemDefInfo.startTagIndex + 1, itemDefInfo.endTagIndex))
					UpdateOrCreateTag(info.startTagIndex + 1, info.endTagIndex, "GenerateDebugInformation", "false");
			}
		}
	}
}
