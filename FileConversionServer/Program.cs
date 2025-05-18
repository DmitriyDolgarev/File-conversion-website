using FileConverterLib.LibreOffice;

namespace FileConversionServer
{
    public class Program
    {
        private const string sofficePath = @"C:\Program Files\LibreOffice\program";
        private static string filesDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "OpenAPI V1");
                });
            }

            app.UseHttpsRedirection();
            app.MapControllers();

            LibreOfficeConverter.sofficePath = sofficePath;

            // Create files dir if not exist
            if (!Directory.Exists(filesDir))
                Directory.CreateDirectory(filesDir);

            // Clear files dir
            foreach (var dir in Directory.GetDirectories(filesDir))
                Directory.Delete(dir, true);

            app.Run();
        }
    }
}
