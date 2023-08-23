using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using Presentation.ActionFilters;
using Repositories.EFCore;
using Services;
using Services.Contracts;
using WebApi.Extensions;

namespace WebApi
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			LogManager.LoadConfiguration(String.Concat(Directory.GetCurrentDirectory(), "/nlog.config"));

			// Add services to the container.

			builder.Services.AddControllers(config =>
			{
				config.RespectBrowserAcceptHeader = true; // i�erik pazarl���nda a��k hale getirdik.
				config.ReturnHttpNotAcceptable = true; // kabul edilmeyen istek tipiyle kar��la�ma
				config.CacheProfiles.Add("5mins", new CacheProfile() { Duration = 300 }); // cacheleme profili olu�uturduk
			})
			.AddXmlDataContractSerializerFormatters() // xml format�nda ��k�� verebilnmek/istekleri kar��layabilmek i�in
			.AddCustomCsvFormatter()
			.AddApplicationPart(typeof(Presentation.AssemblyReference).Assembly)
			.AddNewtonsoftJson(opt => 
				opt.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			);

			builder.Services.AddScoped<ValidationFilterAttribute>(); // IoC

			builder.Services.Configure<ApiBehaviorOptions>(options =>
			{
				options.SuppressModelStateInvalidFilter = true;
			});

			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.ConfigureSwagger();
			builder.Services.ConfigureSqlContext(builder.Configuration);
			builder.Services.ConfigureRepositoryManager();
			builder.Services.ConfigureServiceManager();
			builder.Services.ConfigureLoggerService();
			builder.Services.AddAutoMapper(typeof(Program));
			builder.Services.ConfigureActionFilters();
			builder.Services.ConfigureCros();
			builder.Services.ConfigureDataShaper();
			builder.Services.AddCustomMediaTypes();
			builder.Services.AddScoped<IBookLinks, BookLinks>();
			builder.Services.ConfigureVersioning();
			builder.Services.ConfigureResponseCaching();
			builder.Services.ConfigureHttpCacheHeaders();
			builder.Services.AddMemoryCache(); // rate limit i�in
			builder.Services.ConfigureRateLimitingOptions(); // rate limit i�in
			builder.Services.AddHttpContextAccessor(); // rate limit i�in
			builder.Services.ConfigureIdentity();
			builder.Services.ConfigureJWT(builder.Configuration); // kullan�c� ad� �ifre middleware metot i�erisinde aktfile�itir.

			builder.Services.RegisterRepositories();
			builder.Services.RegisterServices();

			var app = builder.Build();

			var logger = app.Services.GetRequiredService<ILoggerService>(); //Hata Y�netimi
			app.ConfigureExceptionHandler(logger); //Hata Y�netimi

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI(s => 
				{
					s.SwaggerEndpoint("/swagger/v1/swagger.json", "BTK Akademi v1");
					s.SwaggerEndpoint("/swagger/v2/swagger.json", "BTK Akademi v2");
				});
			}
			if (app.Environment.IsProduction()) //Hata Y�netimi
			{
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			app.UseIpRateLimiting(); // rate limit i�in
			app.UseCors("CorsPolicy");
			app.UseResponseCaching(); // caching i�lemi i�in Corstan sonra �a��r�lmas� �nerilir.
			app.UseHttpCacheHeaders();

			app.UseAuthentication(); // �nce kullanc�� ad� �ifre ile do�rulama 
			app.UseAuthorization(); // sonra yetkilendirme

			app.MapControllers();

			app.Run();
		}
	}
}