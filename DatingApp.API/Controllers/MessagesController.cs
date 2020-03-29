using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;

using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;

namespace DatingApp.API.Controllers {
	[ServiceFilter(typeof(LogUserActivityFilter))]
	[Route("api/users/{userId}/[controller]")]
	[ApiController]
	public class MessagesController : ControllerBase {

		private readonly IDatingRepository repository;

		private readonly IMapper mapper;

		public MessagesController(
				IDatingRepository repository,
				IMapper mapper) {
			this.repository = repository;
			this.mapper = mapper;
		}

		[HttpGet("{id}", Name = "GetMessage")]
		public async Task<IActionResult> GetMessage(int userId, int id) {
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) return this.Unauthorized();
			var messageFromRepository = await this.repository.GetMessage(id);
			if (messageFromRepository == null) return this.NotFound();
			return this.Ok(messageFromRepository);
		}

		[HttpGet]
		public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery] MessageParams messageParams) {
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) return this.Unauthorized();
			messageParams.UserId = userId;
			var messagesFromRepo = await this.repository.GetMessagesForUser(messageParams);
			var messages = this.mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);
			this.Response.AddPagination(messagesFromRepo.CurrentPage, messagesFromRepo.PageSize,
				messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
			return this.Ok(messages);
		}

		[HttpGet("thread/{recipientId}")]
		public async Task<IActionResult> GetMessageThread(int userId, int recipientId) {
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) return this.Unauthorized();
			var messagesFromRepository = await this.repository.GetMessageThread(userId, recipientId);
			var messageThread = this.mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepository);
			return this.Ok(messageThread);
		}

		[HttpPost]
		public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto) {
			var sender = await this.repository.GetUser(userId);
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (sender.Id != currentUserId) return this.Unauthorized();
			messageForCreationDto.SenderId = userId;
			var recipient = await this.repository.GetUser(messageForCreationDto.RecipientId);
			if (recipient == null) return this.BadRequest("Could not find user");
			var message = this.mapper.Map<Message>(messageForCreationDto);
			this.repository.Add(message);
			if (await this.repository.SaveAll()) {
				var messageToReturn = this.mapper.Map<MessageToReturnDto>(message);
				return this.CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
			}
			throw new Exception("Creating the message failed on save");
		}

		[HttpPost("{id}")]
		public async Task<IActionResult> DeleteMessage(int id, int userId) {
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) return this.Unauthorized();
			var messageFromRepository = await this.repository.GetMessage(id);
			if (messageFromRepository.SenderId == userId)
				messageFromRepository.SenderDeleted = true;
			else if (messageFromRepository.RecipientId == userId)
				messageFromRepository.RecipientDeleted = true;

			if (messageFromRepository.SenderDeleted && messageFromRepository.RecipientDeleted)
				this.repository.Delete(messageFromRepository);

			if (await this.repository.SaveAll()) return this.NoContent();

			throw new Exception("Error deleting the message");
		}

		[HttpPost("{id}/read")]
		public async Task<IActionResult> MarkMessageAsRead(int userId, int id) {
			int currentUserId = int.Parse(this.User.FindFirst(ClaimTypes.NameIdentifier).Value);
			if (userId != currentUserId) return this.Unauthorized();
			var message = await this.repository.GetMessage(id);
			if (message.RecipientId != userId) return this.Unauthorized();
			message.IsRead = true;
			message.DateRead = DateTime.Now;
			await this.repository.SaveAll();
			return this.NoContent();
		}
	}}