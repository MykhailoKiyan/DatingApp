namespace DatingApp.API.Data {
	using System.Collections.Generic;
	using System.Linq;
	using Microsoft.AspNetCore.Identity;
	using Newtonsoft.Json;

	using DatingApp.API.Models;

    public static class Seed {
		public static void SeedUsers(UserManager<User> manager) {
			if (manager.Users.Any()) return;
			var userData = System.IO.File.ReadAllText(@"Data/UserSeedData.json");
			var users = JsonConvert.DeserializeObject<List<User>>(userData);
			users.ForEach(user => manager.CreateAsync(user, "password").Wait());
		}
	}
}
