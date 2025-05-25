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
            if (!IsCorrectExtension(files, [".pdf"]))
                return Results.BadRequest("Wrong extension");

            var inputBytes = await FormFileCollectionToBytesAsync(files);
            var outputBytes = await PdfConverter.MergePdfBytesAsync(inputBytes);

            return Results.File(outputBytes, "application/octet-stream", "result.pdf");
        }

        [HttpPost("split")]
        public async Task<IResult> Split([FromForm] SplitPdfRequestData data)
        {
            if (!IsCorrectExtension(data.File, [".pdf"]))
                return Results.BadRequest("Wrong extension");

            var inputBytes = await FormFileToBytesAsync(data.File);
            try
            {
                var outputBytes = await PdfConverter.SplitPdfBytesAsync(inputBytes, data.SplitString);
                return Results.File(outputBytes, "application/octet-stream", "result.pdf");
            }
            catch(ArgumentException e)
            {
                return Results.BadRequest(e.Message);
            }
        }

        [HttpPost("pdfToJpg")]
        public async Task<IResult> PdfToJpg(IFormFile file)
        {
            if (!IsCorrectExtension(file, [".pdf"]))
                return Results.BadRequest("Wrong extension");

            var inputBytes = await FormFileToBytesAsync(file);
            var outputBytes = await PdfConverter.PdfBytesToJpgBytesZipAsync(inputBytes);

            return Results.File(outputBytes, "application/octet-stream", "result.zip");
        }

        [HttpPost("jpgToPdf")]
        public async Task<IResult> JpgToPdf(IFormFileCollection files)
        {
            if (!IsCorrectExtension(files, [".jpg", ".jpeg"]))
                return Results.BadRequest("Wrong extension");

            var inputBytes = await FormFileCollectionToBytesAsync(files);
            var outputBytes = await PdfConverter.JpgBytesToPdfBytesAsync(inputBytes);

            return Results.File(outputBytes, "application/octet-stream", "result.pdf");
        }
        
        public class SplitPdfRequestData
        {
            public string SplitString { get; set; }
            public IFormFile File { get; set; }
        }
    }
}
