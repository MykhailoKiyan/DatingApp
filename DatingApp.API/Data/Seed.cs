namespace DatingApp.API.Data {
	using System.Collections.Generic;
	using System.Linq;
	using Newtonsoft.Json;

	using DatingApp.API.Models;

	public class Seed {
		private readonly DataContext context;

		public Seed(DataContext context) => this.context = context;

		public void SeedUsers() {
			if (this.context.Users.Any()) return;
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
