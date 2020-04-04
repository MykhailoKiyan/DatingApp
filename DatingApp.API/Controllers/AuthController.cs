namespace DatingApp.API.Controllers {
	using System;
	using System.Text;
	using System.Collections.Generic;
	using System.Linq;
	using System.Security.Claims;
	using System.IdentityModel.Tokens.Jwt;
	using System.Threading.Tasks;
	using Microsoft.AspNetCore.Http;
	using Microsoft.AspNetCore.Mvc;
	using Microsoft.Extensions.Configuration;
	using Microsoft.IdentityModel.Tokens;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Identity;
	using AutoMapper;

	using DatingApp.API.Data;
	using DatingApp.API.Models;
	using DatingApp.API.Dtos;
	using DatingApp.API.Utilities.ExtensionMethods;

    [Route("api/[controller]")]
	[ApiController]
	[AllowAnonymous]
	public class AuthController : ControllerBase {
		private readonly IConfiguration configuration;
		private readonly IMapper mapper;
		private readonly UserManager<User> userManager;
		private readonly SignInManager<User> signInManager;

		public AuthController(
				IConfiguration configuration,
				IMapper mapper,
				UserManager<User> userManager,
				SignInManager<User> signInManager
		) {
			this.configuration = configuration;
			this.mapper = mapper;
			this.userManager = userManager;
			this.signInManager = signInManager;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(UserForRegisterDto userDto) {
			userDto.Username = userDto.Username.GetСorrectUsername();
			var userToCreate = this.mapper.Map<User>(userDto);
			var result = await this.userManager.CreateAsync(userToCreate, userDto.Password);
			if (!result.Succeeded) return this.BadRequest(result.Errors);
			var userToReturn = this.mapper.Map<UserForDetailedDto>(userToCreate);
			return this.CreatedAtRoute("GetUser", new { Controller="Users", id=userToCreate.Id }, userToReturn);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(UserForLoginDto userDto) {
			userDto.Username = userDto.Username.GetСorrectUsername();
			var user = await this.userManager.FindByNameAsync(userDto.Username);
			var result = await this.signInManager.CheckPasswordSignInAsync(user, userDto.Password, false);
			if (!result.Succeeded) return this.Unauthorized();
			var userForReturn = this.mapper.Map<UserForListDto>(user);
			return this.Ok(new { token = await this.GenerateJwtToken(user), user = userForReturn });
		}

		private async Task<string> GenerateJwtToken(User user) {
			var claims = new List<Claim> {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.UserName)
			};
			var roles = await this.userManager.GetRolesAsync(user);
			foreach (var role in roles) {
				claims.Add(new Claim(ClaimTypes.Role, role));
			}

			var tokenSecret = this.configuration.GetSection("AppSettings:Token").Value;
			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecret));

			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var tokenDescription = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.Now.AddDays(1),
				SigningCredentials = credentials
			};

			var tokenHandler = new JwtSecurityTokenHandler();

			var token = tokenHandler.CreateToken(tokenDescription);

			return tokenHandler.WriteToken(token);
		}
	}
}