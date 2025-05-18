using FileConversionServer.Services;
using Microsoft.AspNetCore.Mvc;
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
            return await ConvertFiles(files, "PngToJpg");
        }

        [HttpPost("jpgToPng")]
        public async Task<IResult> JpgToPng(IFormFileCollection files)
        {
            return await ConvertFiles(files, "JpgToPng");
        }
    }
}
