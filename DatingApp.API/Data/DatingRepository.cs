using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data {
	public class DatingRepository : IDatingRepository {
		private readonly DataContext context;

		public DatingRepository(DataContext context) {
			this.context = context;
		}

		public void Add<T>(T entity) where T : class {
			this.context.Add(entity);
		}

		public void Delete<T>(T entity) where T : class {
			this.context.Remove(entity);
		}

		public async Task<Like> GetLike(int userId, int recipientId) {
			var like = await this.context.Likes.FirstOrDefaultAsync(l => l.LikerId == userId && l.LikeeId == recipientId);
			return like;
		}

		public async Task<Photo> GetMainPhotoForUser(int userId) {
			return await this.context.Photos
				.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
		}

		public async Task<Message> GetMessage(int id) {
			return await this.context.Messages.FirstOrDefaultAsync(m => m.Id == id);
		}

		public Task<PagedList<Message>> GetMessagesForUser() {
			throw new NotImplementedException();
		}

		public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams) {
			var messages = this.context.Messages
				.Include(u => u.Sender).ThenInclude(p => p.Photos)
				.Include(u => u.Recipient).ThenInclude(p => p.Photos)
				.AsQueryable();
			switch (messageParams.MessageContainer) {
				case "Inbox":
					messages = messages.Where(u => u.RecipientId == messageParams.UserId && !u.RecipientDeleted);
					break;
				case "Outbox":
					messages = messages.Where(u => u.SenderId == messageParams.UserId && !u.SenderDeleted);
					break;
				default:
					messages = messages.Where(u => u.RecipientId == messageParams.UserId && !u.IsRead
						&& !u.RecipientDeleted);
					break;
			}
			messages = messages.OrderByDescending(d => d.MessageSent);
			return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
		}

		public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId) {
			return await this.context.Messages
				.Include(u => u.Sender).ThenInclude(p => p.Photos)
				.Include(u => u.Recipient).ThenInclude(p => p.Photos)
				.Where(m => m.RecipientId == userId && !m.RecipientDeleted && m.SenderId == recipientId
					|| m.RecipientId == recipientId && !m.SenderDeleted && m.SenderId == userId)
				.OrderByDescending(m => m.MessageSent)
				.ToListAsync();
		}

		public async Task<Photo> GetPhoto(int id) {
			return await this.context.Photos
				.FirstAsync(i => i.Id == id);
		}

		public async Task<User> GetUser(int id) {
			return await this.context.Users
				.Include(i => i.Photos)
				.FirstOrDefaultAsync(i => i.Id == id);
		}

		public async Task<PagedList<User>> GetUsers(UserParams userParams) {
			var users = this.context.Users
				.Include(i => i.Photos)
				.Where(user => user.Id != userParams.UserId)
				.Where(user => user.Gender == userParams.Gender);

			if (userParams.Likers) {
				var userLikers = await this.GetUserLikes(userParams.UserId, userParams.Likers);
				users = users.Where(user => userLikers.Contains(user.Id));
			}

			if (userParams.Likees) {
				var userLikees = await this.GetUserLikes(userParams.UserId, userParams.Likers);
				users = users.Where(user => userLikees.Contains(user.Id));
			}

			if (userParams.MinAge.HasValue) {
				var maxDateOfBirth = DateTime.Today.AddYears(-userParams.MinAge.Value);
				users = users.Where(user => user.DateOfBirth <= maxDateOfBirth);
			}

			if (userParams.MaxAge.HasValue) {
				var minDateOfBirth = DateTime.Today.AddYears(-(userParams.MaxAge.Value + 1));
				users = users.Where(user => user.DateOfBirth >= minDateOfBirth);
			}

			if (!string.IsNullOrEmpty(userParams.OrderBy)) {
				switch(userParams.OrderBy){
					case "created":
						users = users.OrderByDescending(user => user.Created);
						break;

					case "lastActive":
					default:
						users = users.OrderByDescending(user => user.LastActive);
						break;
				}
			} else {
				users = users.OrderByDescending(user => user.LastActive);
			}

			return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
		}

		public async Task<bool> SaveAll() {
			return await this.context.SaveChangesAsync() > 0;
		}

		private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers) {
			var user = await this.context.Users
				.Include(u => u.Likers)
				.Include(u => u.Likees)
				.FirstOrDefaultAsync(u => u.Id == id);

			if (likers) {
				return user.Likers
					.Where(u => u.LikeeId == id)
					.Select(u => u.LikerId);
			} else {
				return user.Likees
					.Where(u => u.LikerId == id)
					.Select(u => u.LikeeId);
			}
		}
	}
}
