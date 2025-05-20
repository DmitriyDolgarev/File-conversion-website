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

        public FileConversionControllerBase(ChannelWriter<FileConvertedMessage> channel)
        {
            fileConverterWriter = channel;
        }

        // Load files from request
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
        
        // Files to zip
        protected async Task<string> FilesToZipAsync(string dirPath, List<string> filePaths, string zipFileName = "result.zip")
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
        protected async Task<byte[]> BytesToZipAsync(IEnumerable<byte[]> filesBytes, List<string> filesNames)
        {
            using (var stream = new MemoryStream())
            {
                using (var archive = new ZipArchive(stream, ZipArchiveMode.Create))
                {
                    int i = 0;
                    foreach (var fileBytes in filesBytes)
                    {
                        var archiveEntry = archive.CreateEntry(filesNames[i]);

                        using (var zipStream = archiveEntry.Open())
                        {
                            await zipStream.WriteAsync(fileBytes);
                        }
                        i++;
                    }
                }

                return stream.ToArray();
            }
        }

        // Send message to channel
        protected async Task FileConvertedMessageWriteAsync(string dirPath)
        {
            await fileConverterWriter.WriteAsync(new FileConvertedMessage(dirPath));
        }

        // Extension check
        protected bool IsCorrectExtension(IFormFileCollection files, IEnumerable<string> possibleExtensions)
        {
            foreach (var file in files)
            {
                if (!IsCorrectExtension(file, possibleExtensions))
                    return false;
            }
            return true;
        }
        protected bool IsCorrectExtension(IFormFile file, IEnumerable<string> possibleExtensions)
        {
            return possibleExtensions.Contains(Path.GetExtension(file.FileName).ToLower());
        }

        // IFormFile(s) to bytes
        protected async Task<byte[]> FormFileToBytesAsync(IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);

            return stream.ToArray();
        }
        protected async Task<IEnumerable<byte[]>> FormFileCollectionToBytesAsync(IFormFileCollection files)
        {
            var tasks = files.Select(file => FormFileToBytesAsync(file));
            var filesBytes = await Task.WhenAll(tasks);

            return filesBytes;
        }
    }
}