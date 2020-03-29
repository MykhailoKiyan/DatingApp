namespace DatingApp.API.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;

    using Microsoft.AspNetCore.Identity;

    public class UserRole : IdentityUserRole<int> {
		public Role Role { get; set; }

		public User User { get; set; }
	}
}
