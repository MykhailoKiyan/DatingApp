using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Utilities.ExtensionMethods {
	public static class Comparers {
		public static bool IsEqual(this byte[] target, byte[] source) {
			if (target == null || source == null)
				return false;

			if (target == source)
				return true;

			if (target.Length != source.Length)
				return false;

			for (var i = 0; i < target.Length; i++)
				if (target[i] != source[i])
					return false;

			return true;
		}
	}
}
