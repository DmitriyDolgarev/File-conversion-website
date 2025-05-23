using FileConversionServer.Services;
using FileConverterLib.LibreOffice;
using System.Threading.Channels;

namespace FileConversionServer
{
    public class Program
    {
        private static string filesDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            // Сообщение о том, что конвертация завершилась
            var fileConvertedMessageChannel = Channel.CreateUnbounded<FileConvertedMessage>();
            builder.Services.AddSingleton(fileConvertedMessageChannel);
            builder.Services.AddSingleton(fileConvertedMessageChannel.Reader);
            builder.Services.AddSingleton(fileConvertedMessageChannel.Writer);

            // Очистка файлов раз в какое-то время
            builder.Services.AddHostedService<FilesCleanupService>();

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
            app.UseStaticFiles();

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
