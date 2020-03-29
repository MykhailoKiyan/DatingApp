using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using DatingApp.API.Models;
using DatingApp.API.Data;

namespace DatingApp.API {
	public class Program {
		public static void Main(string[] args) {
			var host = CreateWebHostBuilder(args).Build();
			using (var scope = host.Services.CreateScope()) {
				var services = scope.ServiceProvider;
				try {
					var userManager = services.GetRequiredService<UserManager<User>>();
					Seed.SeedUsers(userManager);
				} catch (Exception ex) {
					var logger = services.GetRequiredService<ILogger<Program>>();
					logger.LogError(ex, "A fatal error in the Main method.");
					throw;
				}

			}

			host.Run();
		}

		public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
			WebHost.CreateDefaultBuilder(args)
				.UseStartup<Startup>();
	}
}
