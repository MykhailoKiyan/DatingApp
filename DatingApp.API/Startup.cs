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

    using DatingApp.API.Data;
    using DatingApp.API.Helpers;

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
			services.AddDbContext<DataContext>(i =>
				i
					.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"))
					.ConfigureWarnings(w => w.Ignore(CoreEventId.IncludeIgnoredWarning))
				);

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
				.AddJsonOptions(o => {
					o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				});

			services.AddCors();

            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

			services.AddAutoMapper(typeof(Startup).Assembly);

			services.AddTransient<Seed>();

			services.AddScoped<IAuthRepository, AuthRepository>();

			services.AddScoped<IDatingRepository, DatingRepository>();

			services.AddScoped<LogUserActivityFilter>();

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
		}

		public void ConfigureDevelopmentServices(IServiceCollection services) {
			services.AddDbContext<DataContext>(i =>
				i
					.UseSqlServer(this.Configuration.GetConnectionString("DefaultConnection"))
					.ConfigureWarnings(w => w.Ignore(CoreEventId.IncludeIgnoredWarning))
				);

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
				.AddJsonOptions(o => {
					o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
				});

			services.AddCors();

            services.Configure<CloudinarySettings>(Configuration.GetSection("CloudinarySettings"));

			services.AddAutoMapper(typeof(Startup).Assembly);

			services.AddTransient<Seed>();

			services.AddScoped<IAuthRepository, AuthRepository>();

			services.AddScoped<IDatingRepository, DatingRepository>();

			services.AddScoped<LogUserActivityFilter>();

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
		}


		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, Seed seeder) {
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
			seeder.SeedUsers();
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
