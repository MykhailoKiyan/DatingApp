namespace DatingApp.API {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Text;
    using System.Net;
    using AutoMapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.HttpsPolicy;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Diagnostics;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Diagnostics;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
	using Microsoft.AspNetCore.Identity;
	using Microsoft.AspNetCore.Authorization;
	using Microsoft.AspNetCore.Mvc.Authorization;

	using DatingApp.API.Data;
    using DatingApp.API.Helpers;
    using DatingApp.API.Models;

    public class Startup {
		public Startup(IConfiguration configuration, IHostingEnvironment env) {
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.AddJsonFile("appsettings.Security.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
				.AddJsonFile($"appsettings.{env.EnvironmentName}.Security.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables();

			this.Configuration = builder.Build();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services) {
			IdentityBuilder builder = services.AddIdentityCore<User>(options => {
				PasswordOptions po = options.Password;
				po.RequireDigit = false;
				po.RequiredLength = 4;
				po.RequireNonAlphanumeric = false;
				po.RequireUppercase = false;
			});

			builder = new IdentityBuilder(builder.UserType, typeof(Role), services);
			builder.AddEntityFrameworkStores<DataContext>();
			builder.AddRoleValidator<RoleValidator<Role>>();
			builder.AddRoleManager<RoleManager<Role>>();
			builder.AddSignInManager<SignInManager<User>>();
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(option => {
					option.TokenValidationParameters = new TokenValidationParameters {
						ValidateIssuerSigningKey = true,
						IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(
							this.Configuration.GetSection("AppSettings:Token").Value)),
						ValidateIssuer = false,
						ValidateAudience = false
					};
				});

			services.AddAuthorization(options => {
				options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
				options.AddPolicy("RequireModeratorPhotoRole", policy => policy.RequireRole("Admin", "Moderator"));
				options.AddPolicy("VipOnly", policy => policy.RequireRole("VIP"));
			});

			services.AddDbContext<DataContext>(options =>
				options.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"))
					.ConfigureWarnings(w => w.Ignore(CoreEventId.IncludeIgnoredWarning))
				);

			services
				.AddMvc(options => {
					var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
					options.Filters.Add(new AuthorizeFilter(policy));
				})
				.SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
				.AddJsonOptions(o => {
					o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				});

			services.AddCors();

            services.Configure<CloudinarySettings>(this.Configuration.GetSection("CloudinarySettings"));

			services.AddAutoMapper(typeof(Startup).Assembly);

			services.AddScoped<IDatingRepository, DatingRepository>();

			services.AddScoped<LogUserActivityFilter>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env/*, UserManager<User> manager*/) {
			if(env.IsDevelopment()) {
				app.UseDeveloperExceptionPage();
			} else {
				app.UseExceptionHandler(builder => {
					builder.Run(async context => {
						context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

						var error = context.Features.Get<IExceptionHandlerFeature>();

						if (error != null) {
							context.Response.AddApplicationError(error.Error.Message);
							await context.Response.WriteAsync(error.Error.Message);
						}
					});
				});
				// app.UseHsts();
			}

			// app.UseHttpsRedirection();
			//Seed.SeedUsers(manager);
			app.UseCors(x => x
				.AllowAnyOrigin()
				.AllowAnyMethod()
				.AllowAnyHeader());
			app.UseAuthentication();
			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseMvc(roots => {
				roots.MapSpaFallbackRoute(
					name: "spa-fallback",
					defaults: new { controller = "Fallback", action = "Index" }
				);
			});
		}
	}
}
