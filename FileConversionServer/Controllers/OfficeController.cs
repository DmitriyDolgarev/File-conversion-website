using FileConversionServer.Services;
using FileConverterLib.LibreOffice;
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
            return await ConvertFiles(files, "wordToPdf", officeConverter);
        }

        [HttpPost("pdfToWord")]
        public async Task<IResult> PdfToWord(IFormFileCollection files)
        {
            return await ConvertFiles(files, "pdfToWord", officeConverter);
        }

        [HttpPost("pptxToPdf")]
        public async Task<IResult> PptxToWord(IFormFileCollection files)
        {
            return await ConvertFiles(files, "pptxToPdf", officeConverter);
        }
    }
}
