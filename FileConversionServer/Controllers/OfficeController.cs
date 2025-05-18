using Microsoft.AspNetCore.Mvc;

namespace FileConversionServer.Controllers
{
    [ApiController]
    [Route("/api/office")]
    public class OfficeController : FileConversionControllerBase
    {
        [HttpPost("wordToPdf")]
        public async Task<IResult> WordToPdf(string officeConverter, IFormFileCollection files)
        {
            return await ConvertFiles(files, "wordToPdf", officeConverter);
        }

        [HttpPost("pdfToWord")]
        public async Task<IResult> PdfToWord(string officeConverter, IFormFileCollection files)
        {
            return await ConvertFiles(files, "pdfToWord", officeConverter);
        }

        [HttpPost("pptxToPdf")]
        public async Task<IResult> PptxToWord(string officeConverter, IFormFileCollection files)
        {
            return await ConvertFiles(files, "pptxToPdf", officeConverter);
        }
    }
}
