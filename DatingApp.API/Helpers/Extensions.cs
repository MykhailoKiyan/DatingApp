using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers {
	public static class Extensions {
		public static void AddApplicationError(this HttpResponse response, string message) {
			response.Headers.Add("Application-Error", message);
			response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
			response.Headers.Add("Access-Control-Allow-Origin", "*");
		}

		public static int CalculateAge(this DateTime data) {
			var diff = DateTime.Today.Year - data.Year;
			if (data.AddYears(diff) > DateTime.Today) diff--;
			return diff;
		}
	}
}
