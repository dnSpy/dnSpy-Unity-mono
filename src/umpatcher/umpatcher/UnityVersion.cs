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
using System.Text.RegularExpressions;

namespace UnityMonoDllSourceCodePatcher {
	readonly struct UnityVersion : IComparable<UnityVersion> {
		public readonly uint Major;
		public readonly uint Minor;
		public readonly uint Build;
		public readonly string Extra;

		public UnityVersion(uint major, uint minor, uint build, string extra) {
			Major = major;
			Minor = minor;
			Build = build;
			Extra = extra ?? string.Empty;
		}

		public static bool TryParse(string value, out UnityVersion version) {
			version = default;
			if (string.IsNullOrEmpty(value))
				return false;
			var m = Regex.Match(value, @"^(\d+)\.(\d+)\.(\d+)(-[\-.\w]*)?$");
			if (!m.Success)
				return false;
			if (m.Groups.Count != 5)
				return false;
			if (!uint.TryParse(m.Groups[1].Value, out uint major))
				return false;
			if (!uint.TryParse(m.Groups[2].Value, out uint minor))
				return false;
			if (!uint.TryParse(m.Groups[3].Value, out uint build))
				return false;

			version = new UnityVersion(major, minor, build, m.Groups[4].Value);
			return true;
		}

		public int CompareTo(UnityVersion other) {
			int c = Major.CompareTo(other.Major);
			if (c != 0)
				return c;
			c = Minor.CompareTo(other.Minor);
			if (c != 0)
				return c;
			c = Build.CompareTo(other.Build);
			if (c != 0)
				return c;
			return StringComparer.OrdinalIgnoreCase.Compare(Extra ?? string.Empty, other.Extra ?? string.Empty);
		}

		public override string ToString() => $"{Major}.{Minor}.{Build}{Extra}";
	}
}
