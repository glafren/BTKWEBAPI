using AspNetCoreRateLimit;
using Entities.DataTransferObjects;
using Entities.Models;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Presentation.ActionFilters;
using Presentation.Controllers;
using Repositories.Contracts;
using Repositories.EFCore;
using Services;
using Services.Contracts;
using System.IO.Enumeration;
using System.Text;

namespace WebApi.Extensions
{
	public static class ServicesExtensions
	{
		public static void ConfigureSqlContext(this IServiceCollection services, IConfiguration configuration)
		{
			services.AddDbContext<RepositoryContext>(options =>
				options.UseSqlServer(configuration.GetConnectionString("sqlConnection"))); // IoC DbContext Tanımı yapıldı.
		}

		public static void ConfigureRepositoryManager(this IServiceCollection services)
		{
			services.AddScoped<IRepositoryManager, RepositoyManager>();
		}

		public static void ConfigureServiceManager(this IServiceCollection services)
		{
			services.AddScoped<IServiceManager, ServiceManager>();
		}

		public static void ConfigureLoggerService(this IServiceCollection services) =>
			services.AddSingleton<ILoggerService, LoggerManager>();

		public static void ConfigureActionFilters(this IServiceCollection services)
		{
			services.AddScoped<ValidationFilterAttribute>(); // IoC
			services.AddSingleton<LogFilterAttribute>();
			services.AddScoped<ValidateMediaTypeAttribute>();
		}

		public static void ConfigureCros(this IServiceCollection services)
		{
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy", buidler =>
					buidler.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.WithExposedHeaders("X-Pagination")
				);
			}
			);
		}

		public static void ConfigureDataShaper(this IServiceCollection services)
		{
			services.AddScoped<IDataShaper<BookDto>, DataShaper<BookDto>>();
		}

		public static void AddCustomMediaTypes(this IServiceCollection services)
		{
			services.Configure<MvcOptions>(config =>
			{
				var systemTextJsonOutputFormatter = config
				.OutputFormatters
				.OfType<SystemTextJsonOutputFormatter>()?.FirstOrDefault();

				if (systemTextJsonOutputFormatter is not null)
				{
					systemTextJsonOutputFormatter.SupportedMediaTypes
					.Add("application/vnd.btkakademi.hateoas+json");

					systemTextJsonOutputFormatter.SupportedMediaTypes
					.Add("application/vnd.btkakademi.apiroot+json");
				}

				var xmlOutputFormatter = config
				.OutputFormatters
				.OfType<XmlDataContractSerializerOutputFormatter>()?.FirstOrDefault();

				if (xmlOutputFormatter is not null)
				{
					xmlOutputFormatter.SupportedMediaTypes
					.Add("application/vnd.btkakademi.hateoas+xml");

					xmlOutputFormatter.SupportedMediaTypes
					.Add("application/vnd.btkakademi.apiroot+xml");
				}
			});
		}

		public static void ConfigureVersioning(this IServiceCollection services)
		{
			services.AddApiVersioning(opt =>
			{
				opt.ReportApiVersions = true; //headera version bilgisi ekler
				opt.AssumeDefaultVersionWhenUnspecified = true; // kullanıcı version bilgisi talep etmezse default ile dönüş yap
				opt.DefaultApiVersion = new ApiVersion(1, 0); // büyük değişiklikler 1 küçük değişiklikler 0
				opt.ApiVersionReader = new HeaderApiVersionReader("api-version");

				opt.Conventions.Controller<BooksController>() // versionları controller içerisinde attribute ile belirtmek yerine burada belirttik.
					.HasApiVersion(new ApiVersion(1, 0));

				opt.Conventions.Controller<BooksV2Controller>()
					.HasDeprecatedApiVersion(new ApiVersion(2, 0));
			});
		}
		public static void ConfigureResponseCaching(this IServiceCollection services) =>
			services.AddResponseCaching();

		public static void ConfigureHttpCacheHeaders(this IServiceCollection services) =>
			services.AddHttpCacheHeaders(expirationOpt =>
			{
				expirationOpt.MaxAge = 90;
				expirationOpt.CacheLocation = CacheLocation.Public;
			},
			validationOpt =>
			{
				validationOpt.MustRevalidate = false;
			}); // marvin cache header

		public static void ConfigureRateLimitingOptions(this IServiceCollection services)
		{
			var rateLimitRules = new List<RateLimitRule>()
			{
				new RateLimitRule()
				{
					Endpoint  = "*",
					Limit=60, // 1 dk içerisinde 60  istek atılabilir.
					Period = "1m"
				}
			};

			services.Configure<IpRateLimitOptions>(opt =>
			{
				opt.GeneralRules = rateLimitRules;
			});
			services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
			services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
			services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
			services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
		}

		public static void ConfigureIdentity(this IServiceCollection services)
		{
			var builder = services.AddIdentity<User, IdentityRole>(opts =>
			{
				opts.Password.RequireDigit = true; // şifrede rawkam olsun mu 
				opts.Password.RequireLowercase = false;
				opts.Password.RequireUppercase = false;
				opts.Password.RequireNonAlphanumeric = false; // şifrede özel karakter zorunlu değil
				opts.Password.RequiredLength = 6;

				opts.User.RequireUniqueEmail = true; // 1 e mail 1 kez kullanılsın

			})
			.AddEntityFrameworkStores<RepositoryContext>()
			.AddDefaultTokenProviders(); // json web token için
		}

		public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
		{
			var jwtSettings = configuration.GetSection("JwtSettings");
			var secretKey = jwtSettings["secretkey"];

			services.AddAuthentication(opt =>
			{
				opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
			}).AddJwtBearer(options =>
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = true,
					ValidateAudience = true,
					ValidateLifetime = true,
					ValidateIssuerSigningKey = true,
					ValidIssuer = jwtSettings["validIssuer"],
					ValidAudience = jwtSettings["validAudience"],
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
				}
			);
		}
		
		public static void ConfigureSwagger(this IServiceCollection services)
		{
			services.AddSwaggerGen(s => 
			{
				s.SwaggerDoc("v1", new OpenApiInfo 
				{ 
					Title = "BTK Akademi", 
					Version = "v1",
					Description = "BTK Akademi ASP.NEt Core Web API",
					TermsOfService = new Uri("https://btkakademi.gov.tr/"),
					Contact = new OpenApiContact
					{
						Name = "Metin Yeni",
						Email ="metinyeni33@gmail.com",
						Url = new Uri("https://github.com/glafren")
					}
				});
				s.SwaggerDoc("v2", new OpenApiInfo { Title = "BTK Akademi", Version = "v2" });

				s.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					In = ParameterLocation.Header,
					Description = "Place to add JWT with Bearer",
					Name = "Authorization",
					Type = SecuritySchemeType.ApiKey,
					Scheme = "Bearer"
				});

				s.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference
							{
								Type = ReferenceType.SecurityScheme,
								Id= "Bearer"
							},
							Name = "Bearer"
						},
						new List<string>()
					}
				});
			});
		}

		public static void RegisterRepositories(this IServiceCollection services)
		{
			services.AddScoped<IBookRepository, BookRepository>();
			services.AddScoped<ICategoryRepository, CategoryRepository>();
		}
		public static void RegisterServices(this IServiceCollection services)
		{
			services.AddScoped<IBookService, BookManager>();
			services.AddScoped<ICategoryService, CategoryManager>();
			services.AddScoped<IAuthenticationService, AuthenticationManager>();
		}

	}
}
