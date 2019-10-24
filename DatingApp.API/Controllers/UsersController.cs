using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;

namespace DatingApp.API.Controllers {
	[ServiceFilter(typeof(LogUserActivityFilter))]
	[Authorize]
	[Route("/api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase {
		private readonly IDatingRepository repository;
		private readonly IMapper mapper;

		public UsersController(IDatingRepository repository, IMapper mapper) {
			this.repository = repository;
			this.mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers() {
			var users = await this.repository.GetUsers();
			var usersToReturn = this.mapper.Map<IEnumerable<UserForListDto>>(users);
			return Ok(usersToReturn);
		}

		[HttpGet("{id}", Name="GetUser")]
		public async Task<IActionResult> GetUser(int id) {
			var user = await this.repository.GetUser(id);
			var userToReturn = this.mapper.Map<UserForDetailedDto>(user);
			return Ok(userToReturn);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userDto) {
            int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (id != currentUserId) {
				return Unauthorized();
			}

            var user = await this.repository.GetUser(id);
            this.mapper.Map(userDto, user);

            if (await this.repository.SaveAll()) {
                return NoContent();
            }

            throw new Exception($"Updating with user id={id} failed on save!");
		}
	}
}
