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
            await Task.Run(() => PDFConverter.MergePDFs(filePaths.ToArray(), outputFileName));

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
            var outputFileName1 = Path.Combine(requestDir, "file1.pdf");
            var outputFileName2 = Path.Combine(requestDir, "file2.pdf");
            await Task.Run(() => PDFConverter.SplitPDF(filePath, data.PageSplitFrom, outputFileName1, outputFileName2));

            // Put in zip
            var outputFilePath = await FilesToZip(requestDir, new List<string> { outputFileName1, outputFileName2 });

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
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
            await Task.Run(() => PDFConverter.PdfFileToJpgFiles(filePath, outputFilePath, true));

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
            await Task.Run(() => PDFConverter.JpgFilesToPdfFile(filePaths.ToArray(), outputFilePath));

            var outputFileBytes = await System.IO.File.ReadAllBytesAsync(outputFilePath);
            await FileConvertedMessageWriteAsync(requestDir);

            return Results.File(outputFileBytes, "application/octet-stream", Path.GetFileName(outputFilePath));
        }
        
        public class SplitPdfRequestData
        {
            public int PageSplitFrom { get; set; }
            public IFormFile File { get; set; }
        }
    }
}
