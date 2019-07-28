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

namespace UnityMonoDllSourceCodePatcher {
	sealed class SolutionPatcher {
		readonly SolutionOptions solutionOptions;
		readonly string solutionDir;
		readonly TextFilePatcher textFilePatcher;
		readonly Guid unityVersionDirGuid;
		static readonly Guid solutionDirGuid = new Guid("2150E333-8FDC-42A3-9474-1A3956D46DE8");
		static readonly Guid cppProjectGuid = new Guid("8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942");

		public SolutionPatcher(SolutionOptions solutionOptions) {
			this.solutionOptions = solutionOptions ?? throw new ArgumentNullException(nameof(solutionOptions));
			solutionDir = Path.GetDirectoryName(solutionOptions.SolutionFilename)!;
			textFilePatcher = new TextFilePatcher(solutionOptions.SolutionFilename);
			unityVersionDirGuid = Guid.NewGuid();
		}

		static string ToString(Guid guid) => guid.ToString("B").ToUpperInvariant();

		public void Patch() {
			AddProjects();
			AddProjectConfigurationPlatforms();
			AddNestedProjects();
			textFilePatcher.Write();
		}

		void AddProjects() {
			int index = textFilePatcher.GetIndexOfLine("Global");
			AddProjectDir(ref index);
			foreach (var project in solutionOptions.AllProjects)
				AddProject(ref index, project);
		}

		void AddProjectDir(ref int index) {
			var dirName = Path.GetFileName(solutionOptions.UnityVersionDir);
			AddProjectInfo(ref index, solutionDirGuid, unityVersionDirGuid, dirName, dirName);
		}

		void AddProject(ref int index, ProjectInfo project) {
			var name = Path.GetFileNameWithoutExtension(project.Filename);
			var projectRelativePath = GetProjectRelativePath(project);
			AddProjectInfo(ref index, cppProjectGuid, project.Guid, name, projectRelativePath);
		}

		string GetProjectRelativePath(ProjectInfo project) {
			if (!project.Filename.StartsWith(solutionDir, StringComparison.OrdinalIgnoreCase))
				throw new ProgramException("Invalid solution dir");
			var relName = project.Filename.Substring(solutionDir.Length + 1);
			return relName.Replace('/', '\\');
		}

		void AddProjectInfo(ref int index, Guid typeGuid, Guid projectGuid, string name, string projectRelativePath) {
			var typeGuidString = ToString(typeGuid);
			var projectGuidString = ToString(projectGuid);
			textFilePatcher.Insert(index++, $"Project(\"{typeGuidString}\") = \"{name}\", \"{projectRelativePath}\", \"{projectGuidString}\"");
			textFilePatcher.Insert(index++, "EndProject");
		}

		void AddProjectConfigurationPlatforms() {
			int index = textFilePatcher.GetIndexOfLine("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution");
			index = textFilePatcher.GetIndexOfLine("\tEndGlobalSection", index);
			foreach (var project in solutionOptions.AllProjects) {
				var projectGuidString = ToString(project.Guid);
				foreach (var config in solutionOptions.SolutionConfigurations) {
					foreach (var info in solutionOptions.SolutionPlatforms) {
						foreach (var buildInfo in solutionOptions.SolutionBuildInfos)
							textFilePatcher.Insert(index++, $"\t\t{projectGuidString}.{config}|{info.archName}.{buildInfo} = {config}|{info.configName}");
					}
				}
			}
		}

		void AddNestedProjects() {
			int index = textFilePatcher.GetIndexOfLine("\tGlobalSection(NestedProjects) = preSolution");
			index = textFilePatcher.GetIndexOfLine("\tEndGlobalSection", index);
			foreach (var project in solutionOptions.AllProjects) {
				var projectGuidString = ToString(project.Guid);
				var unityVersionDirGuidString = ToString(unityVersionDirGuid);
				textFilePatcher.Insert(index++, $"\t\t{projectGuidString} = {unityVersionDirGuidString}");
			}
		}
	}
}
