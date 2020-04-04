namespace DatingApp.API.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.EntityFrameworkCore;

	using DatingApp.API.Data;
	using DatingApp.API.Dtos;
	using DatingApp.API.Models;

	[ApiController]
	[Route("api/[controller]")]
	public class AdminController : ControllerBase {
		private readonly DataContext context;
		private readonly UserManager<User> userManager;

		public AdminController(DataContext context, UserManager<User> userManager) {
			this.context = context;
			this.userManager = userManager;
		}

		[Authorize(Policy = "RequireAdminRole")]
		[HttpGet("usersWithRoles")]
		public async Task<IActionResult> GetUsersWithRoles() {
			var users = await this.context.Users.OrderBy(user => user.UserName).Select(user => new {
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

		[Authorize(Policy = "RequireAdminRole")]
		[HttpPost("editRoles/{userName}")]
		public async Task<IActionResult> EditRoles(string userName, RoleEditDto roleEditDto) {
			var user = await this.userManager.FindByNameAsync(userName);
			var userRoles = await this.userManager.GetRolesAsync(user);
			var selectedRoles = roleEditDto.RoleNames ?? Array.Empty<string>();
			var identityResult = await this.userManager.AddToRolesAsync(user, selectedRoles.Except(userRoles));
			if (!identityResult.Succeeded) return this. BadRequest("Failed to add to roles.");
			identityResult = await this.userManager.RemoveFromRolesAsync(user, userRoles.Except(selectedRoles));
			if (!identityResult.Succeeded) return this.BadRequest("Failed to remove the roles.");
			var roles = await this.userManager.GetRolesAsync(user);
			return this.Ok(roles);
		}
	}
}
