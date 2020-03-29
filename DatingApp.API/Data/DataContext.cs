namespace DatingApp.API.Data {
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore;

	using DatingApp.API.Models;

	public class DataContext : IdentityDbContext<User, Role, int,
			IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>> {
		public DataContext(DbContextOptions<DataContext> options) : base(options) { }

		public DbSet<Value> Values { get; set; }

		public DbSet<Photo> Photos { get; set; }

		public DbSet<Like> Likes { get; set; }

		public DbSet<Message> Messages { get; set; }

		protected override void OnModelCreating(ModelBuilder modelBuilder) {
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<UserRole>(entity => {
				entity.HasKey(k => new { k.UserId, k.RoleId });
				entity.HasOne(p => p.Role)
					.WithMany(p => p.UserRoles)
					.HasForeignKey(k => k.RoleId)
					.IsRequired();
				entity.HasOne(p => p.User)
					.WithMany(p => p.UserRoles)
					.HasForeignKey(k => k.UserId)
					.IsRequired();
			});

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

			modelBuilder.Entity<Message>()
				.HasOne(u => u.Sender)
				.WithMany(m => m.MessagesSent)
				.OnDelete(DeleteBehavior.Restrict);

			modelBuilder.Entity<Message>()
				.HasOne(u => u.Recipient)
				.WithMany(m => m.MessagesRecived)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
