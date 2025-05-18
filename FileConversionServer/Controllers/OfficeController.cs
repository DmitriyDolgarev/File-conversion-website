using FileConversionServer.Services;
using FileConverterLib.LibreOffice;
using FileConverterLib.MSOffice;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace FileConversionServer.Controllers
{
    [ApiController]
    [Route("/api/office")]
    public class OfficeController : FileConversionControllerBase
    {
        private readonly string officeConverter;

        public OfficeController(ChannelWriter<FileConvertedMessage> channel, IConfiguration config) : base(channel)
        {
            LibreOfficeConverter.sofficePath = config.GetValue<string>("SofficePath")!;
            officeConverter = config.GetValue<string>("OfficeConverter")!;
        }

        [HttpPost("wordToPdf")]
        public async Task<IResult> WordToPdf(IFormFileCollection files)
        {
            if (officeConverter.ToLower() == "msoffice")
                return await ConvertOffice(files, MSOfficeConverter.DocxFileToPdfFileAsync, [".docx"], ".pdf");
            else if (officeConverter.ToLower() == "libreoffice")
                return await ConvertOffice(files, LibreOfficeConverter.DocxFileToPdfFileAsync, [".docx"], ".pdf");
            else
                return Results.BadRequest("Wrong office converter");
        }

        [HttpPost("pdfToWord")]
        public async Task<IResult> PdfToWord(IFormFileCollection files)
        {
            if (officeConverter.ToLower() == "msoffice")
                return await ConvertOffice(files, MSOfficeConverter.PdfFileToDocxFileAsync, [".pdf"], ".docx");
            else if (officeConverter.ToLower() == "libreoffice")
                return await ConvertOffice(files, LibreOfficeConverter.PdfFileToDocxFileAsync, [".pdf"], ".docx");
            else
                return Results.BadRequest("Wrong office converter");
        }

        [HttpPost("pptxToPdf")]
        public async Task<IResult> PptxToWord(IFormFileCollection files)
        {
            if (officeConverter.ToLower() == "msoffice")
                return await ConvertOffice(files, MSOfficeConverter.PptxFileToPdfFileAsync, [".pptx"], ".pdf");
            else if (officeConverter.ToLower() == "libreoffice")
                return await ConvertOffice(files, LibreOfficeConverter.PptxFileToPdfFileAsync, [".pptx"], ".pdf");
            else
                return Results.BadRequest("Wrong office converter");
        }

        private async Task<IResult> ConvertOffice(IFormFileCollection files, Func<string, string, Task> Converter, IEnumerable<string> inputExtensions, string outputExtension)
        {
            // Check extension
            if (!IsCorrectExtension(files, inputExtensions))
                return Results.BadRequest("Wrong extension");

            // Load files to request directory
            string requestDir = Path.Combine(filesDir, CurrentDateTime);
            Directory.CreateDirectory(requestDir);
            var filePaths = await LoadFilesToDirAsync(requestDir, files);

            // Convert
            var outputFilePath = "";

            foreach (var filePath in filePaths)
            {
                outputFilePath = Path.ChangeExtension(filePath, outputExtension);
                await Converter(filePath, outputFilePath);
            }

            // Zip
            if (files.Count > 1)
            {
                outputFilePath = await FilesToZipAsync(requestDir, filePaths.Select(p => Path.ChangeExtension(p, outputExtension)).ToList());
            }

            // Read converted file
            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }
    }
}
