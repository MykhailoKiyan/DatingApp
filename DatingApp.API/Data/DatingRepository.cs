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

		public async Task<Photo> GetMainPhotoForUser(int userId) {
			return await this.context.Photos
				.FirstOrDefaultAsync(p => p.UserId == userId && p.IsMain);
		}

		public async Task<Photo> GetPhoto(int id) {
			return await this.context.Photos
				.FirstAsync(i => i.Id == id);
		}

		public async Task<User> GetUser(int id) {
			return await this.context.Users
				.Include(i => i.Photos)
				.FirstAsync(i => i.Id == id);
		}

		public async Task<PagedList<User>> GetUsers(UserParams userParams) {
			var users = this.context.Users
				.Include(i => i.Photos)
				.Where(user => user.Id != userParams.UserId)
				.Where(user => user.Gender == userParams.Gender);

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
	}
}
