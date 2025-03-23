using FileConverterLib.Images;
using FileConverterLib.PDF;
using FileConverterLib.LibreOffice;

using System.IO.Compression;
using FileConverterLib.MSOffice;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Antiforgery;

namespace FileConversionServer
{
    public class Program
    {
        private const string sofficePath = @"C:\Program Files\LibreOffice\program";
        private static string filesDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        private static string CurrentDateTime { get => DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss.fff"); }

        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
             builder.Services.AddAuthorization();
            
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            //builder.Services.AddAntiforgery(options => options.HeaderName = "X-XSRF-TOKEN");

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
            app.UseAuthorization(); // можно выключить
            //app.UseAntiforgery();

            LibreOfficeConverter.sofficePath = sofficePath;

            // Create files dir if not exist
            if (!Directory.Exists(filesDir))
                Directory.CreateDirectory(filesDir);

            // Clear files dir
            foreach(var dir in Directory.GetDirectories(filesDir))
                Directory.Delete(dir, true);

            //app.MapGet("api/antiforgery/token", (IAntiforgery forgeryService, HttpContext context) =>
            //{
            //    var tokens = forgeryService.GetAndStoreTokens(context);
            //    var xsrfToken = tokens.RequestToken!;
            //    return TypedResults.Content(xsrfToken, "text/plain");
            //});

            // Images
            app.MapPost("/api/images/pngToJpg", async (IFormFileCollection files) => 
            {
                return await ConvertFiles(files, "PngToJpg");
            }).WithTags("Images");
            app.MapPost("/api/images/jpgToPng", async (IFormFileCollection files) => 
            {
                return await ConvertFiles(files, "JpgToPng");
            }).DisableAntiforgery().WithTags("Images");

            // PDF
            app.MapPost("/api/pdf/merge", async (IFormFileCollection files) =>
            {
                // Check extension
                foreach (var file in files)
                {
                    if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                        return Results.BadRequest();
                }

                // Load files to request directory
                string requestDir = Path.Combine(filesDir, CurrentDateTime);
                Directory.CreateDirectory(requestDir);
                var filePaths = await LoadFilesToDirAsync(requestDir, files);

                // Merge
                var outputFileName = Path.Combine(requestDir, "result.pdf");
                await Task.Run(() => PDFConverter.MergePDFs(filePaths.ToArray(), outputFileName));

                var outputFileBytes = await File.ReadAllBytesAsync(outputFileName);
                return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFileName));

            }).DisableAntiforgery().WithTags("PDF");
            app.MapPost("/api/pdf/split", async ([FromForm] SplitPdfRequestData data) =>
            {
                // Check extension
                if (Path.GetExtension(data.File.FileName).ToLower() != ".pdf")
                    return Results.BadRequest();

                // Load files to request directory
                string requestDir = Path.Combine(filesDir, CurrentDateTime);
                Directory.CreateDirectory(requestDir);
                var filePath = await LoadFileToDirAsync(requestDir, data.File);

                // Split
                var outputFileName1 = Path.Combine(requestDir, "file1.pdf");
                var outputFileName2 = Path.Combine(requestDir, "file2.pdf");
                await Task.Run(() => PDFConverter.SplitPDF(filePath, data.PageSplitFrom, outputFileName1, outputFileName2));

                // Put in zip
                var outputFilePath = await FilesToZip(requestDir, new List<string> { outputFileName1, outputFileName2 });

                var outputFileBytes = await File.ReadAllBytesAsync(outputFilePath);
                return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
            }).DisableAntiforgery().WithTags("PDF");
            app.MapPost("/api/pdf/pdfToJpg", async (IFormFile file) =>
            {
                // Check extension
                if (Path.GetExtension(file.FileName).ToLower() != ".pdf")
                    return Results.BadRequest();

                // Load files to request directory
                string requestDir = Path.Combine(filesDir, CurrentDateTime);
                Directory.CreateDirectory(requestDir);
                var filePath = await LoadFileToDirAsync(requestDir, file);

                // Convert
                var outputFilePath = Path.Combine(requestDir, "result.zip");
                await Task.Run(() => PDFConverter.PdfFileToJpgFiles(filePath, outputFilePath, true));

                var outputFileBytes = await File.ReadAllBytesAsync(outputFilePath);
                return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
            }).DisableAntiforgery().WithTags("PDF");
            app.MapPost("/api/pdf/jpgToPdf", async (IFormFileCollection files) =>
            {
                // Check extension
                foreach (var file in files)
                {
                    if (Path.GetExtension(file.FileName).ToLower() != ".jpg" && Path.GetExtension(file.FileName).ToLower() != ".jpeg")
                        return Results.BadRequest();
                }

                // Load files to request directory
                string requestDir = Path.Combine(filesDir, CurrentDateTime);
                Directory.CreateDirectory(requestDir);
                var filePaths = await LoadFilesToDirAsync(requestDir, files);

                // Convert
                var outputFilePath = Path.Combine(requestDir, "result.pdf");
                await Task.Run(() => PDFConverter.JpgFilesToPdfFile(filePaths.ToArray(), outputFilePath));

                var outputFileBytes = await File.ReadAllBytesAsync(outputFilePath);
                return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
            }).DisableAntiforgery().WithTags("PDF");
            
            // Office
            app.MapPost("/api/office/wordToPdf", async (string officeConverter, IFormFileCollection files) =>
            {
                await ConvertFiles(files, "wordToPdf", officeConverter);
            }).DisableAntiforgery().WithTags("Office");
            app.MapPost("/api/office/pdfToWord", async (string officeConverter, IFormFileCollection files) =>
            {
                await ConvertFiles(files, "pdfToWord", officeConverter);
            }).DisableAntiforgery().WithTags("Office");
            app.MapPost("/api/office/pptxToPdf", async (string officeConverter, IFormFileCollection files) =>
            {
                await ConvertFiles(files, "pptxToPdf", officeConverter);
            }).DisableAntiforgery().WithTags("Office");

            app.Run();
        }

        private static async Task<string> LoadFileToDirAsync(string dirPath, IFormFile file)
        {
            string filePath = Path.Combine(dirPath, file.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return filePath;
        }
        private static async Task<List<string>> LoadFilesToDirAsync(string dirPath, IFormFileCollection files)
        {
            var filePaths = new List<string>();

            foreach (var file in files)
            {
                string filePath = await LoadFileToDirAsync(dirPath, file);
                filePaths.Add(filePath);
            }

            return filePaths;
        }
        private static async Task<string> FilesToZip(string dirPath, List<string> filePaths, string zipFileName = "result.zip")
        {
            var outputFilePath = Path.Combine(dirPath, zipFileName);

            using (var fs = new FileStream(outputFilePath, FileMode.OpenOrCreate))
            {
                using (var zip = new ZipArchive(fs, ZipArchiveMode.Create))
                {
                    await Task.Run(() =>
                    {
                        foreach (var filePath in filePaths)
                        {
                            zip.CreateEntryFromFile(filePath, Path.GetFileName(filePath));
                        }
                    });
                }
            }

            return outputFilePath;
        }
        private static async Task<IResult> ConvertFiles(IFormFileCollection files, string method, string officeConverter="")
        {
            // Choose converter
            Action<string, string>? Convert = null;
            var inputExtensions = new List<string>();
            var outputExtension = "";

            switch (method)
            {
                case "JpgToPng":
                    Convert = ImageConverter.JpgFileToPngFile;
                    inputExtensions.Add(".jpg");
                    inputExtensions.Add(".jpeg");
                    outputExtension = ".png";
                    break;
                case "PngToJpg":
                    Convert = ImageConverter.PngFileToJpgFile;
                    inputExtensions.Add(".png");
                    outputExtension = ".jpg";
                    break;
                case "wordToPdf":
                    if (officeConverter.ToLower() == "msoffice")
                        Convert = MSOfficeConverter.DocxFileToPdfFile;
                    else if (officeConverter.ToLower() == "libreoffice")
                        Convert = LibreOfficeConverter.DocxFileToPdfFile;
                    inputExtensions.Add(".docx");
                    inputExtensions.Add(".doc");
                    outputExtension = ".pdf";
                    break;
                case "pdfToWord":
                    if (officeConverter.ToLower() == "msoffice")
                        Convert = MSOfficeConverter.PdfFileToDocxFile;
                    else if (officeConverter.ToLower() == "libreoffice")
                        Convert = LibreOfficeConverter.PdfFileToDocxFile;
                    inputExtensions.Add(".pdf");
                    outputExtension = ".docx";
                    break;
                case "pptxToPdf":
                    if (officeConverter.ToLower() == "msoffice")
                        Convert = MSOfficeConverter.PptxFileToPdfFile;
                    else if (officeConverter.ToLower() == "libreoffice")
                        Convert = LibreOfficeConverter.PptxFileToPdfFile;
                    inputExtensions.Add(".pptx");
                    inputExtensions.Add(".ppt");
                    outputExtension = ".pdf";
                    break;
                default:
                    return Results.BadRequest();
            }

            // Check office converter
            if (officeConverter.ToLower() != "msoffice" && officeConverter.ToLower() != "libreoffice" && officeConverter.ToLower() != "")
                return Results.BadRequest();

            // Check extension
            foreach (var file in files)
            {
                if (!inputExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                    return Results.BadRequest();
            }

            // Load files to request directory
            string requestDir = Path.Combine(filesDir, CurrentDateTime);
            Directory.CreateDirectory(requestDir);
            var filePaths = await LoadFilesToDirAsync(requestDir, files);

            // Convert
            var outputFilePath = "";
            foreach(var filePath in filePaths)
            {
                outputFilePath = Path.ChangeExtension(filePath, outputExtension);
                await Task.Run(() => Convert(filePath, outputFilePath));
            }

            // Put in zip
            if (files.Count > 1)
            {
                outputFilePath = await FilesToZip(requestDir, filePaths.Select(p => Path.ChangeExtension(p, outputExtension)).ToList());
            }

            var outputFileBytes = await File.ReadAllBytesAsync(outputFilePath);
            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }
    }
    public class SplitPdfRequestData
    {
        public int PageSplitFrom { get; set; }
        public IFormFile File { get; set; }

        public SplitPdfRequestData(int pageSplitFrom, IFormFile pdfFile)
        {
            PageSplitFrom = pageSplitFrom;
            File = pdfFile;
        }
    }
}
