namespace DatingApp.API.Data {
	using DatingApp.API.Models;
	using Newtonsoft.Json;
	using System.Collections.Generic;
	using System.Linq;

	public static class Seed {
		public static void SeedUsers(DataContext context) {
			if (context.Users.Any()) return;
			var userData = System.IO.File.ReadAllText(@"Data/UserSeedData.json");
			var users = JsonConvert.DeserializeObject<List<User>>(userData);
			users.ForEach(user => {
				var passwordHash = AuthRepository.CreatePasswordHash("password");
				user.PasswordHash = passwordHash.passwordHash;
				user.PasswordSalt = passwordHash.passwordSalt;
				user.Username = user.Username.ToUpper();
				context.Users.Add(user);
			});
			context.SaveChanges();
		}
	}
}
