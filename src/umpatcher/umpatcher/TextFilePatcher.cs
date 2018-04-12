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
using System.Text;

namespace UnityMonoDllSourceCodePatcher {
	readonly struct TextFilePatcher {
		readonly string filename;
		public readonly List<TextLine> Lines;
		public string NewLine => Lines.Count == 0 ? Environment.NewLine : Lines[0].NewLine;
		public string Filename => filename;

		public TextFilePatcher(string filename) {
			if (filename == null)
				throw new ArgumentNullException(nameof(filename));
			this.filename = FileUtils.GetExistingFile(filename);
			Lines = LineReader.GetTextLines(filename);
		}

		public static bool RemoveLines(string filename, Func<TextLine, bool> shouldRemove) {
			if (shouldRemove == null)
				throw new ArgumentNullException(nameof(shouldRemove));
			var patcher = new TextFilePatcher(filename);
			bool removedLines = patcher.RemoveLines(shouldRemove);
			if (removedLines)
				patcher.Write();
			return removedLines;
		}

		public bool RemoveLines(Func<TextLine, bool> shouldRemove) {
			var lines = Lines;
			bool removedAtLeastOneLine = false;
			for (int i = 0; i < lines.Count; i++) {
				var line = lines[i];
				if (line.IsRemoved)
					continue;
				if (shouldRemove(line)) {
					lines[i] = default;
					removedAtLeastOneLine = true;
				}
			}
			return removedAtLeastOneLine;
		}

		public int GetIndexOfLines(string[] lines) =>
			GetIndexOfLines(lines, 0);

		public int GetIndexOfLines(string[] lines, int start) =>
			GetIndexOfLines(lines, start, Lines.Count - start);

		public int GetIndexOfLines(string[] lines, int start, int length) {
			if (TryGetIndexOfLines(lines, start, length, out int index))
				return index;
			throw new ProgramException($"Couldn't find lines '\"{string.Join("\" -- \"", lines)}\"' in '{filename}', lines [{start + 1}-{start + 1 + length})");
		}

		public bool TryGetIndexOfLines(string[] lines, out int index) =>
			TryGetIndexOfLines(lines, 0, out index);

		public bool TryGetIndexOfLines(string[] lines, int start, out int index) =>
			TryGetIndexOfLines(lines, start, Lines.Count - start, out index);

		public bool TryGetIndexOfLines(string[] lines, int start, int length, out int index) {
			if (lines == null)
				throw new ArgumentNullException(nameof(lines));
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			var allLines = Lines;
			if (length < 0 || start + length > allLines.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			int end = start + length;
			for (int i = start; i + lines.Length - 1 < end; i++) {
				bool ok = true;
				for (int j = 0; j < lines.Length; j++) {
					if (allLines[i + j].Text != lines[j]) {
						ok = false;
						break;
					}
				}
				if (ok) {
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public int GetIndexOfLine(string text) =>
			GetIndexOfLine(text, 0);

		public int GetIndexOfLine(string text, int start) =>
			GetIndexOfLine(text, start, Lines.Count - start);

		public int GetIndexOfLine(string text, int start, int length) {
			if (TryGetIndexOfLine(text, start, length, out int index))
				return index;
			throw new ProgramException($"Couldn't find line '{text}' in '{filename}', lines [{start + 1}-{start + 1 + length})");
		}

		public bool TryGetIndexOfLine(string text, out int index) =>
			TryGetIndexOfLine(text, 0, out index);

		public bool TryGetIndexOfLine(string text, int start, out int index) =>
			TryGetIndexOfLine(text, start, Lines.Count - start, out index);

		public bool TryGetIndexOfLine(string text, int start, int length, out int index) {
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			var lines = Lines;
			if (length < 0 || start + length > lines.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			int end = start + length;
			for (int i = start; i < end; i++) {
				if (lines[i].Text == text) {
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public int GetIndexOfLine(Func<TextLine, bool> callback) =>
			GetIndexOfLine(callback, 0);

		public int GetIndexOfLine(Func<TextLine, bool> callback, int start) =>
			GetIndexOfLine(callback, start, Lines.Count - start);

		public int GetIndexOfLine(Func<TextLine, bool> callback, int start, int length) {
			if (TryGetIndexOfLine(callback, start, length, out int index))
				return index;
			throw new ProgramException($"Couldn't find a line in '{filename}', lines [{start + 1}-{start + 1 + length})");
		}

		public bool TryGetIndexOfLine(Func<TextLine, bool> callback, out int index) =>
			TryGetIndexOfLine(callback, 0, out index);

		public bool TryGetIndexOfLine(Func<TextLine, bool> callback, int start, out int index) =>
			TryGetIndexOfLine(callback, start, Lines.Count - start, out index);

		public bool TryGetIndexOfLine(Func<TextLine, bool> callback, int start, int length, out int index) {
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			var lines = Lines;
			if (length < 0 || start + length > lines.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			int end = start + length;
			for (int i = start; i < end; i++) {
				var line = lines[i];
				if (line.IsRemoved)
					continue;
				if (callback(line)) {
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public int GetLastIndexOfLine(string text) =>
			GetLastIndexOfLine(text, 0);

		public int GetLastIndexOfLine(string text, int start) =>
			GetLastIndexOfLine(text, start, Lines.Count - start);

		public int GetLastIndexOfLine(string text, int start, int length) {
			if (TryGetLastIndexOfLine(text, start, length, out int index))
				return index;
			throw new ProgramException($"Couldn't find line '{text}' in '{filename}', lines [{start + 1}-{start + 1 + length})");
		}

		public bool TryGetLastIndexOfLine(string text, out int index) =>
			TryGetLastIndexOfLine(text, 0, out index);

		public bool TryGetLastIndexOfLine(string text, int start, out int index) =>
			TryGetLastIndexOfLine(text, start, Lines.Count - start, out index);

		public bool TryGetLastIndexOfLine(string text, int start, int length, out int index) {
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			var lines = Lines;
			if (length < 0 || start + length > lines.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			int end = start + length;
			for (int i = end - 1; i >= start; i--) {
				if (lines[i].Text == text) {
					index = i;
					return true;
				}
			}

			index = -1;
			return false;
		}

		public IEnumerable<int> GetIndexesOfLine(Func<TextLine, bool> callback) =>
			GetIndexesOfLine(callback, 0);

		public IEnumerable<int> GetIndexesOfLine(Func<TextLine, bool> callback, int start) =>
			GetIndexesOfLine(callback, start, Lines.Count - start);

		public IEnumerable<int> GetIndexesOfLine(Func<TextLine, bool> callback, int start, int length) {
			if (start < 0)
				throw new ArgumentOutOfRangeException(nameof(start));
			var lines = Lines;
			if (length < 0 || start + length > lines.Count)
				throw new ArgumentOutOfRangeException(nameof(length));
			int end = start + length;
			for (int i = start; i < end; i++) {
				var line = lines[i];
				if (line.IsRemoved)
					continue;
				if (callback(line))
					yield return i;
			}
		}

		public void Insert(int index, string text) {
			if (text == null)
				throw new ArgumentNullException(nameof(text));
			Lines.Insert(index, new TextLine(text, NewLine));
		}

		public void Replace(Func<TextLine, TextLine> callback) {
			var lines = Lines;
			for (int i = 0; i < lines.Count; i++) {
				var line = lines[i];
				if (line.IsRemoved)
					continue;
				lines[i] = callback(line);
			}
		}

		public string GetLeadingWhitespace(int index) {
			var lines = Lines;
			while (index >= 0) {
				var line = lines[index--];
				if (!line.IsRemoved)
					return line.GetLeadingWhitespace();
			}
			return string.Empty;
		}

		public void Write() {
			var encoding = Encoding.UTF8;
			using (var stream = File.Create(filename)) {
				foreach (var line in Lines) {
					if (line.IsRemoved)
						continue;
					var s = line.Text;
					if (s.Length != 0) {
						var bytes = encoding.GetBytes(s);
						stream.Write(bytes, 0, bytes.Length);
					}
					if (line.NewLine == "\n")
						stream.Write(newLineLF, 0, 1);
					else if (line.NewLine == "\r\n")
						stream.Write(newLineCRLF, 0, 2);
					else if (line.NewLine != string.Empty)
						throw new ProgramException("Unknown new line");
				}
			}
		}
		static readonly byte[] newLineLF = new byte[1] { (byte)'\n' };
		static readonly byte[] newLineCRLF = new byte[2] { (byte)'\r', (byte)'\n' };
	}
}
