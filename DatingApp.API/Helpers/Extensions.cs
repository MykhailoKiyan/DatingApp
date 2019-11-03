using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers {
	public static class Extensions {
		public static void AddApplicationError(this HttpResponse response, string message) {
			response.Headers.Add("Application-Error", message);
			response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
			response.Headers.Add("Access-Control-Allow-Origin", "*");
		}

		public static void AddPagination(this HttpResponse response,
				int currentPage, int itemsPerPage, int totalItems, int totalPages) {
			var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
			var serializerSettings = new JsonSerializerSettings();
			serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
			var paginationWebResponseHeaderValue = JsonConvert.SerializeObject(paginationHeader, serializerSettings);
			response?.Headers.Add("Pagination", paginationWebResponseHeaderValue);
			response?.Headers.Add("Access-Control-Expose-Headers", "Pagination");
		}

		public static int CalculateAge(this DateTime data) {
			var diff = DateTime.Today.Year - data.Year;
			if (data.AddYears(diff) > DateTime.Today) diff--;
			return diff;
		}
	}
}
