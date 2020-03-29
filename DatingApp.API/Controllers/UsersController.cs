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
using DatingApp.API.Models;

namespace DatingApp.API.Controllers {
	[ServiceFilter(typeof(LogUserActivityFilter))]
	[Route("api/[controller]")]
	[ApiController]
	public class UsersController : ControllerBase {
		private readonly IDatingRepository repository;
		private readonly IMapper mapper;

		public UsersController(IDatingRepository repository, IMapper mapper) {
			this.repository = repository;
			this.mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> GetUsers([FromQuery]UserParams userParams) {
			var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			var currentUser = await this.repository.GetUser(currentUserId);
			userParams.UserId = currentUserId;
			if (string.IsNullOrEmpty(userParams.Gender)) {
				userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
			}


			var users = await this.repository.GetUsers(userParams);
			var usersToReturn = this.mapper.Map<IEnumerable<UserForListDto>>(users);
			this.Response.AddPagination(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
			return this.Ok(usersToReturn);
		}

		[HttpGet("{id}", Name="GetUser")]
		public async Task<IActionResult> GetUser(int id) {
			var user = await this.repository.GetUser(id);
			var userToReturn = this.mapper.Map<UserForDetailedDto>(user);
			return this.Ok(userToReturn);
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto userDto) {
			int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (id != currentUserId) {
				return this.Unauthorized();
			}

            var user = await this.repository.GetUser(id);
            this.mapper.Map(userDto, user);

            if (await this.repository.SaveAll()) {
                return this.NoContent();
            }

            throw new Exception($"Updating with user id={id} failed on save!");
		}

		[HttpPost("{id}/Like/{recipientId}")]
		public async Task<IActionResult> LikeUser(int id, int recipientId) {
			int currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (currentUserId != id) {
				return this.Unauthorized();
			}

			var like = await this.repository.GetLike(id, recipientId);
			if (like != null) {
				return this.BadRequest("You already like this user");
			}

			var recipient = await this.repository.GetUser(recipientId);
			if (recipient == null) {
				return this.NotFound();
			}

			like = new Like { LikerId = id, LikeeId = recipientId };
			this.repository.Add(like);
			if (await this.repository.SaveAll()) {
				return this.Ok();
			} else {
				return this.BadRequest("Failed to like user");
			}
		}
	}
}
