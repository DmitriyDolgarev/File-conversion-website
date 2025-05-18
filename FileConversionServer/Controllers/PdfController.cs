using FileConversionServer.Services;
using FileConverterLib.PDF;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;

namespace FileConversionServer.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : FileConversionControllerBase
    {
        public PdfController(ChannelWriter<FileConvertedMessage> channel) : base(channel)
        {
        }

        [HttpPost("merge")]
        public async Task<IResult> Merge(IFormFileCollection files)
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
            PdfConverter.MergePdfFiles(filePaths.ToArray(), outputFileName);

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFileName);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
        }

        [HttpPost("split")]
        public async Task<IResult> Split([FromForm] SplitPdfRequestData data)
        {
            // Check extension
            if (Path.GetExtension(data.File.FileName).ToLower() != ".pdf")
                return Results.BadRequest();

            // Load files to request directory
            string requestDir = Path.Combine(filesDir, CurrentDateTime);
            Directory.CreateDirectory(requestDir);
            var filePath = await LoadFileToDirAsync(requestDir, data.File);

            // Split
            var outputFileName = Path.Combine(requestDir, "result.pdf");
            PdfConverter.SplitPdfFile(filePath, data.SplitString, outputFileName);

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFileName);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFileName));
        }

        [HttpPost("pdfToJpg")]
        public async Task<IResult> PfgToJpg(IFormFile file)
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
            PdfConverter.PdfFileToJpgFiles(filePath, outputFilePath, true);

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }

        [HttpPost("jpgToPdf")]
        public async Task<IResult> JpgToPdf(IFormFileCollection files)
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
            PdfConverter.JpgFilesToPdfFile(filePaths.ToArray(), outputFilePath);

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }
        
        public class SplitPdfRequestData
        {
            public string SplitString { get; set; }
            public IFormFile File { get; set; }
        }
    }
}
