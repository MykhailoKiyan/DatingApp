using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

		public async Task<IEnumerable<User>> GetUsers() {
			return await this.context.Users
				.Include(i => i.Photos)
				.ToListAsync();
		}

		public async Task<bool> SaveAll() {
			return await this.context.SaveChangesAsync() > 0;
		}
	}
}
