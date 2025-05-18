using System.Collections.Concurrent;
using System.Threading.Channels;

namespace FileConversionServer.Services
{
    public class FilesCleanupService : BackgroundService
    {
        private ILogger<FilesCleanupService> logger;
        private ChannelReader<FileConvertedMessage> fileConvertedReader;

        private PeriodicTimer timer = new PeriodicTimer(TimeSpan.FromMinutes(10));
        private ConcurrentQueue<FileConvertedMessage> foldersToDelete = new();

        public FilesCleanupService(ILogger<FilesCleanupService> logger, ChannelReader<FileConvertedMessage> channelReader)
        {
            this.logger = logger;
            fileConvertedReader = channelReader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var receivingTask = ReceiveMessagesAsync(stoppingToken);
            var cleanupTask = CleanupConvertedFilesFolders(stoppingToken);

            await Task.WhenAll(receivingTask, cleanupTask);
        }

        private async Task ReceiveMessagesAsync(CancellationToken stoppingToken)
        {
            try
            {
                await foreach (var message in fileConvertedReader.ReadAllAsync(stoppingToken))
                {
                    foldersToDelete.Enqueue(message);
                }
            }
            catch(OperationCanceledException e)
            {
            }
        }

        private async Task CleanupConvertedFilesFolders(CancellationToken stoppingToken)
        {
            try
            {
                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    int deletedDirsCounter = 0;
                    while (foldersToDelete.TryDequeue(out FileConvertedMessage message))
                    {
                        Directory.Delete(message.folderName, true);
                        deletedDirsCounter++;
                    }

                    if (deletedDirsCounter > 0)
                        logger.LogInformation($"Deleted {deletedDirsCounter} directories");
                }
            }
            catch(OperationCanceledException e)
            {
            }
            finally
            {
                timer.Dispose();
            }
        }                
    }

    public record FileConvertedMessage(string folderName);
}
