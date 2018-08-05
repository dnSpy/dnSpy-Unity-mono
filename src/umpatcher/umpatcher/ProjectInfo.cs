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
	sealed class ProjectInfo {
		public readonly Guid OldGuid;
		public readonly string OldGuidLowerString;
		public readonly string OldGuidUpperString;
		public readonly Guid Guid;
		public readonly string NewGuidLowerString;
		public readonly string NewGuidUpperString;
		public readonly string Filename;

		public ProjectInfo(Guid oldGuid, string filename) {
			OldGuid = oldGuid;
			OldGuidLowerString = oldGuid.ToString();
			OldGuidUpperString = OldGuidLowerString.ToUpperInvariant();
			Guid = Guid.NewGuid();
			NewGuidLowerString = Guid.ToString();
			NewGuidUpperString = NewGuidLowerString.ToUpperInvariant();
			Filename = filename;
		}
	}
}
