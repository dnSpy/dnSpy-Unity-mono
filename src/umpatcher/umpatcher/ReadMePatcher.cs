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

namespace UnityMonoDllSourceCodePatcher {
	sealed class ReadMePatcher {
		readonly string unityVersion;
		readonly string unityGitHash;
		readonly TextFilePatcher textFilePatcher;

		public ReadMePatcher(string unityVersion, string unityGitHash, string readmeFilename) {
			this.unityVersion = unityVersion ?? throw new ArgumentNullException(nameof(unityVersion));
			this.unityGitHash = unityGitHash ?? throw new ArgumentNullException(nameof(unityGitHash));
			textFilePatcher = new TextFilePatcher(readmeFilename);
		}

		public void Patch() {
			int startIndex = GetStartOfVersionTable();
			int endIndex = GetEndOfVersionTable(startIndex);
			textFilePatcher.Insert(endIndex, $"{unityVersion} | {unityGitHash}");
			textFilePatcher.Lines.Sort(startIndex, endIndex - startIndex + 1, VersionTableComparer.Instance);
			textFilePatcher.Write();
		}

		int GetStartOfVersionTable() {
			if (textFilePatcher.TryGetIndexOfLines(Constants.VersionTableHeaderLines, out int index))
				return index + Constants.VersionTableHeaderLines.Length;
			throw new ProgramException("Couldn't find the start of the version table in README.md");
		}

		int GetEndOfVersionTable(int startIndex) {
			if (textFilePatcher.TryGetIndexOfLine(Constants.VersionTableEndLine, startIndex, out int index))
				return index;
			throw new ProgramException("Couldn't find the end of the version table in README.md");
		}
	}

	sealed class VersionTableComparer : IComparer<TextLine> {
		public static readonly VersionTableComparer Instance = new VersionTableComparer();
		VersionTableComparer() { }

		public int Compare(TextLine x, TextLine y) {
			var vx = GetVersion(x);
			var vy = GetVersion(y);
			return vx.CompareTo(vy);
		}

		static UnityVersion GetVersion(TextLine line) {
			int index = line.Text.IndexOf(' ');
			if (index <= 0 || !UnityVersion.TryParse(line.Text.Substring(0, index), out var version))
				throw new ProgramException("Invalid version table in README");
			return version;
		}
	}
}
