using ContactsManager.Core.Domain.IdentityEntities;
using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.ResultFilters;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repositories;
using RepositoryContracts;
using ServiceContracts;
using Services;

namespace CRUDExample
{
	public static class ConfigureServicesExtension
	{
		public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddTransient<ResponseHeaderActionFilter>();

			services.AddControllersWithViews(options =>
			{
				// options.Filters.Add<ResponseHeaderActionFilter>(5);
				var logger = services.BuildServiceProvider()
					.GetRequiredService<ILogger<ResponseHeaderActionFilter>>();
				options.Filters.Add(new ResponseHeaderActionFilter(logger)
				{
					Key = "My-Key-From-Global",
					Value = "My-Value-From-Global",
					Order = 2
				});

				options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
			});

			// add services into IoC container
			services.AddScoped<ICountriesRepository, CountriesRepository>();
			services.AddScoped<IPersonsRepository, PersonsRepository>();
			services.AddScoped<ICountriesService, CountriesService>();

			services.AddScoped<IPersonsGetterService, PersonsGetterServiceWithFewExcelFields>();
			services.AddScoped<PersonsGetterService, PersonsGetterService>();

			services.AddScoped<IPersonsAdderService, PersonsAdderService>();
			services.AddScoped<IPersonsDeleterService, PersonsDeleterService>();
			services.AddScoped<IPersonsUpdaterService, PersonsUpdaterService>();
			services.AddScoped<IPersonsSorterService, PersonsSorterService>();

			services.AddTransient<PersonsListResultFilter>();

			services.AddDbContext<ApplicationDbContext>(options =>
			{
				options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
			});


			// Enable Identity in the project
			services.AddIdentity<ApplicationUser, ApplicationRole>((options) =>
			{
				options.Password.RequiredLength = 5;
				options.Password.RequireNonAlphanumeric = false;
				options.Password.RequireUppercase = false;
				options.Password.RequireLowercase = true;
				options.Password.RequireDigit = false;
				options.Password.RequiredUniqueChars = 3; // Eg: AB12AB
			})
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders()
				.AddUserStore<UserStore<ApplicationUser, ApplicationRole, ApplicationDbContext, Guid>>()
				.AddRoleStore<RoleStore<ApplicationRole, ApplicationDbContext, Guid>>();

			//Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=PersonsDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False;Application Intent=ReadWrite;Multi Subnet Failover=False

			services.AddAuthorization(options =>
			{
				options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();

				options.AddPolicy("NotAuthorized", policy =>
				{
					policy.RequireAssertion(context =>
					{
						return !context.User.Identity.IsAuthenticated;
					});
				});
			});

			services.ConfigureApplicationCookie(options =>
			{
				options.LoginPath = "/Account/Login";
			});

			services.AddHttpLogging(options =>
			{
				options.LoggingFields = HttpLoggingFields.RequestProperties |
				HttpLoggingFields.RequestPropertiesAndHeaders;
			});

			return services;
		}
	}
}
