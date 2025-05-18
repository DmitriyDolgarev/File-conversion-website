using Microsoft.AspNetCore.Mvc;

namespace FileConversionServer.Controllers
{
    [ApiController]
    [Route("/api/images")]
    public class ImagesController : FileConversionControllerBase
    {
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
