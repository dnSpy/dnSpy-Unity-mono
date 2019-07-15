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
	abstract class Patcher {
		readonly string unityVersion;
		readonly string unityGitHash;
		readonly GitRepo unityRepo;
		protected readonly GitRepo dnSpyRepo;
		protected readonly string dnSpyVersionPath;

		protected Patcher(string unityVersion, string unityGitHash, string unityRepoPath, string dnSpyUnityMonoRepoPath, string gitExePath) {
			this.unityVersion = unityVersion ?? throw new ArgumentNullException(nameof(unityVersion));
			this.unityGitHash = unityGitHash ?? throw new ArgumentNullException(nameof(unityGitHash));
			unityRepo = new GitRepo(gitExePath, unityRepoPath);
			dnSpyRepo = new GitRepo(gitExePath, dnSpyUnityMonoRepoPath);
			dnSpyVersionPath = Path.Combine(dnSpyUnityMonoRepoPath, Constants.UnityVersionPrefix + unityVersion);
		}

		void Log(string message) => Console.WriteLine(message);

		public void Patch() {
			if (Directory.Exists(dnSpyVersionPath))
				throw new ProgramException($"Directory {dnSpyVersionPath} already exists");
			CopyOriginalUnityFiles();
			UpdateReadMe();
			MergeMasterIntoDnSpy();
			PatchOriginalFiles();
		}

		protected abstract string[] Submodules { get; }
		protected abstract string[] UnityFoldersToCopy { get; }

		void CopyOriginalUnityFiles() {
			Log($"Copying files from {unityRepo.RepoPath} to {dnSpyVersionPath}");
			unityRepo.ThrowIfTreeNotClean();
			dnSpyRepo.ThrowIfTreeNotClean();
			unityRepo.CheckOut(unityGitHash);
			dnSpyRepo.CheckOut(Constants.DnSpyUnityRepo_master_Branch);
			dnSpyRepo.ThrowIfTreeNotClean();

			var submodules = Submodules;
			if (submodules.Length != 0) {
				unityRepo.SubmoduleInit();
				foreach (var relPath in submodules)
					unityRepo.SubmoduleUpdate(relPath);
			}
			unityRepo.ThrowIfTreeNotClean();

			FileUtils.CopyFilesFromTo(unityRepo.RepoPath, dnSpyVersionPath);
			foreach (var dir in UnityFoldersToCopy) {
				var sourceDir = PathCombine(unityRepo.RepoPath, dir);
				var destinationDir = PathCombine(dnSpyVersionPath, dir);
				FileUtils.CopyDirectoryFromTo(sourceDir, destinationDir);
			}

			var gitignore = Path.Combine(dnSpyVersionPath, "mono", "cil", ".gitignore");
			if (!TextFilePatcher.RemoveLines(gitignore, line => line.Text == "/opcode.def"))
				throw new ProgramException("Couldn't remove /opcode.def from .gitignore");

			Log($"Committing copied files");
			dnSpyRepo.CommitAllFiles($"Add Unity files ({Path.GetFileName(dnSpyVersionPath)}), commit hash {unityGitHash}");
		}

		static string PathCombine(string path1, string path2) {
			var list = new List<string>();
			list.Add(path1);
			list.AddRange(path2.Split('/'));
			return Path.Combine(list.ToArray());
		}

		void UpdateReadMe() {
			Log($"Updating README's version table with new hash");
			dnSpyRepo.ThrowIfTreeNotClean();
			var readmeFilename = Path.Combine(dnSpyRepo.RepoPath, Constants.ReadMeFilename);
			var patcher = new ReadMePatcher(unityVersion, unityGitHash, readmeFilename);
			patcher.Patch();
			Log($"Committing updated README");
			dnSpyRepo.CommitAllFiles($"Update README ({Path.GetFileName(dnSpyVersionPath)})");
		}

		void MergeMasterIntoDnSpy() {
			Log($"Merging {Constants.DnSpyUnityRepo_master_Branch} into {Constants.DnSpyUnityRepo_dnSpy_Branch}");
			dnSpyRepo.CheckOut(Constants.DnSpyUnityRepo_dnSpy_Branch);
			dnSpyRepo.Merge(Constants.DnSpyUnityRepo_master_Branch);
		}

		void PatchOriginalFiles() {
			Log($"Patching solution, projects and source code files");
			dnSpyRepo.ThrowIfTreeNotClean();
			dnSpyRepo.CheckOut(Constants.DnSpyUnityRepo_dnSpy_Branch);
			dnSpyRepo.ThrowIfTreeNotClean();

			PatchOriginalFilesCore();

			Log($"Committing patched files");
			dnSpyRepo.CommitAllFiles($"Patch Unity files ({Path.GetFileName(dnSpyVersionPath)})");
		}

		protected abstract void PatchOriginalFilesCore();
	}
}
