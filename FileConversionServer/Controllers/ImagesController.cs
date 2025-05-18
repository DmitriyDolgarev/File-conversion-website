using FileConversionServer.Services;
using FileConverterLib.Images;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;
using System.Threading.Channels;

namespace FileConversionServer.Controllers
{
    [ApiController]
    [Route("/api/images")]
    public class ImagesController : FileConversionControllerBase
    {
        public ImagesController(ChannelWriter<FileConvertedMessage> channel) : base(channel)
        {
        }

        [HttpPost("pngToJpg")]
        public async Task<IResult> PngToJpg(IFormFileCollection files)
        {
            return await ConvertImages(files, ImageConverter.PngBytesToJpgBytesAsync, [".png"], ".jpg");
        }

        [HttpPost("jpgToPng")]
        public async Task<IResult> JpgToPng(IFormFileCollection files)
        {
            return await ConvertImages(files, ImageConverter.JpgBytesToPngBytesAsync, [".jpeg", ".jpg"], ".png");
        }

        private async Task<IResult> ConvertImages(IFormFileCollection files, Func<byte[], Task<byte[]>> Converter, IEnumerable<string> inputExtensions, string outputExtension)
        {
            // Check extension
            if(!IsCorrectExtension(files, inputExtensions))
                return Results.BadRequest("Wrong extension");

            var inputBytes = await FormFileCollectionToBytesAsync(files);

            var outputBytes = new List<byte[]>();
            var outputFileNames = new List<string>();

            // Output file names
            foreach (var file in files)
            {
                outputFileNames.Add(Path.ChangeExtension(file.FileName, outputExtension));
            }

            // Convert
            foreach (var input in inputBytes)
            {
                var output = await Converter(input);
                outputBytes.Add(output);
            }

            if (files.Count > 1) // Zip, many files
            {
                var zipBytes = await BytesToZipAsync(outputBytes, outputFileNames);
                return Results.File(zipBytes, "application/octet-stream", "result.zip");
            }
            else // Single file
            {
                return Results.File(outputBytes[0], "application/octet-stream", outputFileNames[0]);
            }
        }
    }
}
