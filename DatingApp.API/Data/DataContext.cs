namespace DatingApp.API.Data {
	using DatingApp.API.Models;
	using Microsoft.EntityFrameworkCore;

	public class DataContext : DbContext {
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }

		public DbSet<Value> Values { get; set; }

		public DbSet<User> Users { get; set; }

		public DbSet<Photo> Photos { get; set; }

		public DbSet<Like> Likes { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			modelBuilder.Entity<Like>()
				.HasKey(key => new { key.LikerId, key.LikeeId });

			modelBuilder.Entity<Like>()
				.HasOne(user => user.Likee)
				.WithMany(user => user.Likers)
				.HasForeignKey(user => user.LikeeId)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Like>()
				.HasOne(user => user.Liker)
				.WithMany(user => user.Likees)
				.HasForeignKey(user => user.LikerId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
