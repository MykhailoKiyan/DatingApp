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
	using AutoMapper;

	using DatingApp.API.Data;
	using DatingApp.API.Models;
	using DatingApp.API.Dtos;
	using DatingApp.API.Utilities.ExtensionMethods;

    [Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase {

		private readonly IAuthRepository repository;

		private readonly IConfiguration configuration;
		private readonly IMapper mapper;

		public AuthController(IAuthRepository repository, IConfiguration configuration, IMapper mapper) {
			this.repository = repository;
			this.configuration = configuration;
			this.mapper = mapper;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register(UserForRegisterDto userDto) {
			userDto.Username = userDto.Username.GetСorrectUsername();

			if (await this.repository.UserExists(userDto.Username))
				return this.BadRequest("Username already exists");

			var userToCreate = this.mapper.Map<User>(userDto);

			await this.repository.Register(userToCreate, userDto.Password);

			var userToReturn = this.mapper.Map<UserForDetailedDto>(userToCreate);

			return this.CreatedAtRoute("GetUser", new { Controller="Users", id=userToCreate.Id }, userToReturn);
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login(UserForLoginDto userDto) {
			userDto.Username = userDto.Username.GetСorrectUsername();

			var user = await this.repository.Login(userDto.Username, userDto.Password);

			if (user == null)
				return this.Unauthorized();

			var claims = new[] {
				new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
				new Claim(ClaimTypes.Name, user.Username)
			};

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
				this.configuration.GetSection("AppSettings:Token").Value));

			var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

			var tokenDescription = new SecurityTokenDescriptor {
				Subject = new ClaimsIdentity(claims),
				Expires = DateTime.Now.AddDays(1),
				SigningCredentials = credentials
			};

			var tokenHandler = new JwtSecurityTokenHandler();

			var token = tokenHandler.CreateToken(tokenDescription);

			var userForReturn = this.mapper.Map<UserForListDto>(user);

			return this.Ok(new {
				token = tokenHandler.WriteToken(token),
				user = userForReturn
			});
		}
	}
}