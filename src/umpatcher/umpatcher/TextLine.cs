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

using System.Diagnostics;

namespace UnityMonoDllSourceCodePatcher {
	[DebuggerDisplay("{Text}")]
	readonly struct TextLine {
		public readonly string Text;
		public readonly string NewLine;
		public bool IsRemoved => Text == null;

		public TextLine(string text, string newLine) {
			Text = text;
			NewLine = newLine;
		}

		public TextLine Replace(string text) => new TextLine(text, NewLine);

		public string GetLeadingWhitespace() {
			var text = Text;
			int i;
			for (i = 0; i < text.Length; i++) {
				if (!char.IsWhiteSpace(text[i]))
					break;
			}
			return text.Substring(0, i);
		}
	}
}
