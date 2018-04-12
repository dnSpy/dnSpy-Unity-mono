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

using System.Collections.Generic;
using System.IO;

namespace UnityMonoDllSourceCodePatcher {
	static class FileUtils {
		public static string GetExistingFile(string filename) {
			if (!File.Exists(filename))
				throw new ProgramException($"File '{filename}' doesn't exist");
			return filename;
		}

		public static void CopyFilesFromTo(string sourceDir, string destinationDir) {
			Directory.CreateDirectory(destinationDir);
			foreach (var sourceFile in Directory.GetFiles(sourceDir)) {
				var destFile = Path.Combine(destinationDir, Path.GetFileName(sourceFile));
				File.Copy(sourceFile, destFile, overwrite: false);
			}
		}

		public static void CopyDirectoryFromTo(string sourceDir, string destinationDir) {
			foreach (var info in GetAllFilesRecursively(sourceDir, destinationDir))
				File.Copy(info.sourceFile, info.destFile, overwrite: false);
		}

		static IEnumerable<(string sourceFile, string destFile)> GetAllFilesRecursively(string sourceDir, string destinationDir) {
			Directory.CreateDirectory(destinationDir);
			foreach (var sourceFile in Directory.GetFiles(sourceDir)) {
				var destFile = Path.Combine(destinationDir, Path.GetFileName(sourceFile));
				yield return (sourceFile, destFile);
			}

			foreach (var subSourceDir in Directory.GetDirectories(sourceDir)) {
				var subDestinationDir = Path.Combine(destinationDir, Path.GetFileName(subSourceDir));
				foreach (var info in GetAllFilesRecursively(subSourceDir, subDestinationDir))
					yield return info;
			}
		}

		public static bool TryGetPortableExecutableTimestamp(string filename, out uint timestamp) {
			timestamp = 0;
			if (!File.Exists(filename))
				return false;
			using (var f = File.OpenRead(filename)) {
				var r = new BinaryReader(f);
				if (r.ReadUInt16() != 0x5A4D)
					return false;
				f.Position = 0x3C;
				f.Position = r.ReadUInt32();
				if (r.ReadUInt32() != 0x4550)
					return false;
				f.Position += 4;
				timestamp = r.ReadUInt32();
				return true;
			}
		}
	}
}
