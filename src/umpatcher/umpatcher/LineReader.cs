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
	struct LineReader : IDisposable {
		readonly Stream fileStream;
		readonly List<TextLine> lines;
		readonly Encoding encoding;
		readonly byte[] buffer;
		int bufferIndex;
		int bufferLength;
		const int BUFFER_LEN = 1024;
		byte[] lineBuffer;
		const int LINE_BUFFER_LEN = 256;
		int peekByte;

		LineReader(string filename) {
			fileStream = File.OpenRead(filename);
			encoding = Encoding.UTF8;
			lines = new List<TextLine>();
			buffer = new byte[BUFFER_LEN];
			bufferIndex = 0;
			bufferLength = 0;
			lineBuffer = new byte[LINE_BUFFER_LEN];
			peekByte = -1;
		}

		public static List<TextLine> GetTextLines(string filename) {
			using (var reader = new LineReader(filename)) {
				reader.ReadLines();
				return reader.lines;
			}
		}

		void ReadLines() {
			var lines = this.lines;
			for (;;) {
				bool isEof = ReadLine(out var line);
				lines.Add(line);
				if (isEof)
					break;
			}
		}

		bool ReadLine(out TextLine line) {
			int lineBufferIndex = 0;
			string newLine = string.Empty;
			var lineBuffer = this.lineBuffer;
			bool isEof = false;
			for (;;) {
				var b = ReadByte();
				if (b < 0) {
					isEof = true;
					break;
				}
				if (b == '\n') {
					newLine = "\n";
					break;
				}
				if (b == '\r' && PeekByte() == '\n') {
					ReadByte();
					newLine = "\r\n";
					break;
				}
				if ((uint)lineBufferIndex < (uint)lineBuffer.Length)
					lineBuffer[lineBufferIndex] = (byte)b;
				else {
					Array.Resize(ref lineBuffer, checked(lineBuffer.Length * 2));
					this.lineBuffer = lineBuffer;
					lineBuffer[lineBufferIndex] = (byte)b;
				}
				lineBufferIndex++;
			}

			var text = encoding.GetString(lineBuffer, 0, lineBufferIndex);
			line = new TextLine(text, newLine);
			return isEof;
		}

		int PeekByte() {
			if (peekByte >= 0)
				return peekByte;
			int b = ReadByte();
			peekByte = b;
			return b;
		}

		int ReadByte() {
			if (peekByte >= 0) {
				var res = peekByte;
				peekByte = -1;
				return res;
			}
			if (bufferIndex >= bufferLength) {
				bufferLength = fileStream.Read(buffer, 0, buffer.Length);
				bufferIndex = 0;
				if (bufferLength == 0)
					return -1;
			}
			return buffer[bufferIndex++];
		}

		public void Dispose() => fileStream?.Dispose();
	}
}
