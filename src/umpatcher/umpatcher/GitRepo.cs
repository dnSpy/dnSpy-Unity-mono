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

namespace UnityMonoDllSourceCodePatcher {
	sealed class GitRepo {
		public string RepoPath => repoPath;

		readonly string gitPath;
		readonly string repoPath;

		public GitRepo(string gitPath, string repoPath) {
			this.gitPath = gitPath ?? throw new ArgumentNullException(nameof(gitPath));
			this.repoPath = repoPath ?? throw new ArgumentNullException(nameof(repoPath));
		}

		void ThrowError(string message) =>
			throw new ProgramException($"{message} (Repo: {repoPath})");

		public void ThrowIfTreeNotClean() {
			int result = Exec.Run(repoPath, gitPath, "status", out var standardOutput, out _);
			if (result != 0)
				ThrowError($"Git status failed with error code {result}");
			if (!standardOutput.Contains(Constants.GitCleanTreeMessage))
				ThrowError("Git working tree is not clean. Check in the modified files.");
		}

		public void SubmoduleInit() {
			int result = Exec.Run(repoPath, gitPath, "submodule init");
			if (result != 0)
				ThrowError($"Git submodule init failed with error code {result}");
		}

		public void SubmoduleUpdate(string path) {
			int result = Exec.Run(repoPath, gitPath, $"submodule update {path}");
			if (result != 0)
				ThrowError($"Git submodule update {path} failed with error code {result}");
		}

		public void CheckOut(string hashOrBranchName) {
			int result = Exec.Run(repoPath, gitPath, $"checkout {hashOrBranchName}");
			if (result != 0)
				ThrowError($"Git checkout {hashOrBranchName} failed with error code {result}");
		}

		void AddAll() {
			int result = Exec.Run(repoPath, gitPath, $"add .");
			if (result != 0)
				ThrowError($"Git add failed with error code {result}");
		}

		void Commit(string commitMessage) {
			int result = Exec.Run(repoPath, gitPath, $"commit -m \"{commitMessage}\"");
			if (result != 0)
				ThrowError($"Git commit failed with error code {result}");
		}

		public void CommitAllFiles(string commitMessage) {
			AddAll();
			Commit(commitMessage);
		}

		public void Merge(string hashOrBranchName) {
			int result = Exec.Run(repoPath, gitPath, $"merge --no-edit \"{hashOrBranchName}\"");
			if (result != 0)
				ThrowError($"Git commit failed with error code {result}");
		}
	}
}
