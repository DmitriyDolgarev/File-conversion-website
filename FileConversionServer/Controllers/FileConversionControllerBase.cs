using FileConversionServer.Services;
using FileConverterLib.Images;
using FileConverterLib.LibreOffice;
using FileConverterLib.MSOffice;
using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Threading.Channels;

namespace FileConversionServer.Controllers
{
    public abstract class FileConversionControllerBase : ControllerBase
    {
        private readonly ChannelWriter<FileConvertedMessage> fileConverterWriter;

        protected readonly string filesDir = Path.Combine(Directory.GetCurrentDirectory(), "temp");
        protected string CurrentDateTime { get => DateTime.UtcNow.ToString("yyyy.MM.dd-HH.mm.ss.fff"); }

        protected async Task<string> LoadFileToDirAsync(string dirPath, IFormFile file)
        {
            string filePath = Path.Combine(dirPath, file.FileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }
            return filePath;
        }
        protected async Task<List<string>> LoadFilesToDirAsync(string dirPath, IFormFileCollection files)
        {
            var filePaths = new List<string>();

            foreach (var file in files)
            {
                string filePath = await LoadFileToDirAsync(dirPath, file);
                filePaths.Add(filePath);
            }

            return filePaths;
        }
        protected async Task<string> FilesToZip(string dirPath, List<string> filePaths, string zipFileName = "result.zip")
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

        public FileConversionControllerBase(ChannelWriter<FileConvertedMessage> channel) 
        {
            fileConverterWriter = channel;
        }

        protected async Task<IResult> ConvertFiles(IFormFileCollection files, string method, string officeConverter = "")
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
                    return Results.BadRequest("Unknown method");
            }

            // Check office converter
            if (officeConverter.ToLower() != "msoffice" && officeConverter.ToLower() != "libreoffice" && officeConverter.ToLower() != "")
                return Results.BadRequest("Wrong office converter");

            // Check extension
            foreach (var file in files)
            {
                if (!inputExtensions.Contains(Path.GetExtension(file.FileName).ToLower()))
                    return Results.BadRequest("Wrong extension");
            }

            // Load files to request directory
            string requestDir = Path.Combine(filesDir, CurrentDateTime);
            Directory.CreateDirectory(requestDir);
            var filePaths = await LoadFilesToDirAsync(requestDir, files);

            // Convert
            var outputFilePath = "";

            foreach (var filePath in filePaths)
            {
                outputFilePath = Path.ChangeExtension(filePath, outputExtension);
                await Task.Run(() => Convert(filePath, outputFilePath));
            }

            // Put in zip
            if (files.Count > 1)
            {
                outputFilePath = await FilesToZip(requestDir, filePaths.Select(p => Path.ChangeExtension(p, outputExtension)).ToList());
            }

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }

        protected async Task FileConvertedMessageWriteAsync(string dirPath)
        {
            await fileConverterWriter.WriteAsync(new FileConvertedMessage(dirPath));
        }
    }
}
