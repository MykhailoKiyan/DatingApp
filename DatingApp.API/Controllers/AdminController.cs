namespace DatingApp.API.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;

	using DatingApp.API.Data;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
	[Route("api/[controller]")]
	public class AdminController : ControllerBase {
		private readonly DataContext context;

		public AdminController(DataContext context) {
			this.context = context;
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpGet("usersWithRoles")]
		public async Task<IActionResult> GetUsersWithRoles() {
			var users = await context.Users.OrderBy(user => user.UserName).Select(user => new {
				user.Id,
				user.UserName,
				Roles = (from userRole in user.UserRoles
						 join role in context.Roles on userRole.RoleId equals role.Id
						 select role.Name).ToList()
			}).ToListAsync();

			return this.Ok(users);
		}

		[Authorize(Policy = "RequireModeratorPhotoRole")]
		[HttpGet("photosForModeration")]
		public IActionResult GetPhotosForModeration() {
			return this.Ok("Only moderators and admins can see this respons");
		}
	}
}
