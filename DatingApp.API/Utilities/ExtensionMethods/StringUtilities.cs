using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Utilities.ExtensionMethods {
	public static class StringUtilities {
		public static string GetСorrectUsername(this string username) {
			return username.Trim().ToUpper();
		}

	}
}
