using System.Threading;
using System.Threading.Tasks;
using FileChangeWorker.Internal;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FileChangeWorker
{
    public sealed class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<WorkerOptions> _options;
        private readonly Events<Worker> _events;
        private readonly FileHashStore _fileHashStore;
        private readonly FileSync _fileSync;

        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options)
        {
            _logger = logger;
            _options = options;
            _events = new Events<Worker>(logger, this);
            _fileHashStore = new FileHashStore(_options.Value.ScanPath);
            _fileSync = new FileSync(
                _fileHashStore,
                _events,
                _options.Value.ScanPath,
                FileSync.FileSyncStrategy.Performant);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var iterationCounter = 0;
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug("starting iteration: " + iterationCounter);
                _fileSync.SyncFiles();
                await Task.Run(() => Parallel.ForEach(_fileHashStore.Files, (filePath, loopState) =>
                {
                    var newHash = Hasher.GenerateHash(filePath);
                    var currentHash = _fileHashStore.GetHash(filePath);
                    var areHashesEqual = Hasher.CompareHash(newHash, currentHash);
                    if (!areHashesEqual)
                    {
                        var oldHash = currentHash;
                        _events.OnFileChanged(System.IO.Path.GetFileName(filePath));
                        _logger.LogDebug("old hash: " + oldHash);
                        _fileHashStore.Set(filePath, newHash);
                        _logger.LogDebug("new hash: " + newHash);
                        loopState.Stop();
                    }
                }), stoppingToken);
                iterationCounter++;
                await Task.Delay(_options.Value.WaitMs, stoppingToken);
            }
        }
    }
}
