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
	abstract class SolutionOptions {
		public readonly Guid SolutionDirGuid;
		public readonly string SolutionFilename;
		public readonly string UnityVersionDir;
		public readonly string WindowsTargetPlatformVersion;
		public readonly string PlatformToolset;
		public readonly UnityVersion UnityVersion;

		public abstract IEnumerable<ProjectInfo> AllProjects { get; }
		public abstract string[] SolutionConfigurations { get; }
		public abstract (string archName, string configName)[] SolutionPlatforms { get; }
		public abstract string[] SolutionBuildInfos { get; }

		protected SolutionOptions(string solutionDir, string versionPath, string unityVersion, string windowsTargetPlatformVersion, string platformToolset, string solutionFilenameFormatString) {
			SolutionDirGuid = Guid.NewGuid();
			if (!UnityVersion.TryParse(unityVersion, out UnityVersion))
				throw new InvalidOperationException($"Invalid version: {unityVersion}");
			SolutionFilename = Path.Combine(solutionDir, string.Format(solutionFilenameFormatString, UnityVersion.Major));
			UnityVersionDir = versionPath ?? throw new ArgumentNullException(nameof(versionPath));
			WindowsTargetPlatformVersion = windowsTargetPlatformVersion ?? throw new ArgumentNullException(nameof(windowsTargetPlatformVersion));
			PlatformToolset = platformToolset ?? throw new ArgumentNullException(nameof(platformToolset));
		}
	}
}
