using WebApi.Utilities.Formatters;

namespace WebApi.Extensions
{
	public static class IMvcBuilderExtensions
	{
		public static IMvcBuilder AddCustomCsvFormatter(this IMvcBuilder buidler) =>
			buidler.AddMvcOptions(config =>
			config.OutputFormatters
			.Add(new CsvOutputFormatter()));
	}
}
