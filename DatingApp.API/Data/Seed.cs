namespace DatingApp.API.Data {
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.AspNetCore.Identity;
	using Newtonsoft.Json;

	using DatingApp.API.Models;
	using DatingApp.API.Utilities.ExtensionMethods;

	public static class Seed {
		public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager) {
			if (userManager.Users.Any()) return;
			var userData = System.IO.File.ReadAllText(@"Data/UserSeedData.json");

			// Create some roles
			var roles = new List<Role> {
				new Role { Name = "Member" },
				new Role { Name = "Admin" },
				new Role { Name = "Moderator" },
				new Role { Name = "VIP" }
			};
			roles.ForEach(role => roleManager.CreateAsync(role).Wait());

			// Create some users
			var users = JsonConvert.DeserializeObject<List<User>>(userData);
			users.ForEach(user => {
				user.UserName = user.UserName.GetСorrectUsername();
				userManager.CreateAsync(user, "password").Wait();
				userManager.AddToRoleAsync(user, "Member").Wait();
			});

			// Create admine user
			var adminUser = new User { UserName = "Admin".GetСorrectUsername() };
			var adminUserCreateResult = userManager.CreateAsync(adminUser, "password").Result;
			if (adminUserCreateResult.Succeeded) {
				var admin = userManager.FindByNameAsync(adminUser.UserName).Result;
				userManager.AddToRolesAsync(admin, new[] { "Admin", "Member" }).Wait();
			}
		}
	}
}
