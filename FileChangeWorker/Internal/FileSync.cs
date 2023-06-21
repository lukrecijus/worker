using System;
using System.Linq;

namespace FileChangeWorker.Internal
{
    public class FileSync
    {
        private readonly object _lockObj = new();
        private readonly FileHashStore _store;
        private readonly Events<Worker> _events;
        private readonly string _scanPath;
        private FileSyncStrategy _strategy = FileSyncStrategy.Default;

        internal enum FileSyncStrategy
        {
            Default,
            Performant
        }

        internal FileSync(
            FileHashStore store,
            Events<Worker> events,
            string scanPath,
            FileSyncStrategy strategy)
        {
            _store = store;
            _events = events;
            _scanPath = scanPath;
            SetStrategy(strategy);
        }
        
        internal void SyncFiles()
        {
            lock (_lockObj)
            {
                switch (_strategy)
                {
                    case FileSyncStrategy.Default:
                        SyncFilesDefault();
                        break;
                    case FileSyncStrategy.Performant:
                        SyncFilesPerformant();
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void SetStrategy(FileSyncStrategy strategy)
        {
            lock (_lockObj)
            {
                if (Enum.IsDefined(strategy))
                {
                    _strategy = strategy;
                }
            }
        }
        
        private void SyncFilesDefault()
        {
            var files = _store.Files;
            var deletedPaths = files
                .Except(System.IO.Directory.GetFiles(_scanPath))
                .ToList();
            foreach (var path in deletedPaths)
            {
                _store.Remove(path);
            }
            var newPaths = System.IO.Directory
                .GetFiles(_scanPath)
                .Except(files)
                .ToList();
            foreach (var path in newPaths)
            {
                _events.OnNewFileFound(System.IO.Path.GetFileName(path));
                _store.Add(path);
            }
        }
        
        private void SyncFilesPerformant()
        {
            var knownPaths = _store.Files;
            var currentPaths = System.IO.Directory.GetFiles(_scanPath);
            
            foreach (var path in currentPaths.Union(knownPaths))
            {
                if (!knownPaths.Contains(path))
                {
                    _events.OnNewFileFound(System.IO.Path.GetFileName(path));
                    _store.Add(path);
                    continue;
                }

                if (!currentPaths.Contains(path))
                {
                    _store.Remove(path);
                }
            }
        }
    }
}
