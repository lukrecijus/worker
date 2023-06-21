using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FileChangeWorker.Internal
{
    public sealed class FileHashStore
    {
        private readonly object _lockObj = new();
        private ConcurrentDictionary<string, Hasher.FileHash> _fileHashMap;

        internal FileHashStore(string scanPath)
        {
            _fileHashMap = GenerateFileMap(scanPath);
        }

        internal bool Add(string path) => _fileHashMap.TryAdd(path, Hasher.GenerateHash(path));
        internal bool Remove(string path) => _fileHashMap.TryRemove(path, out _);
        internal Hasher.FileHash Set(string path, Hasher.FileHash newHash) => _fileHashMap.AddOrUpdate(
            path,
            newHash,
            (_, _) => newHash);
        internal ICollection<string> Files => _fileHashMap.Keys;
        internal Hasher.FileHash GetHash(string file) => _fileHashMap[file];
        internal void Reinitialize(string scanPath)
        {
            lock (_lockObj) { _fileHashMap = GenerateFileMap(scanPath); }  
        } 

        private static ConcurrentDictionary<string, Hasher.FileHash> GenerateFileMap(string scanPath)
        {
            return new ConcurrentDictionary<string, Hasher.FileHash>(System.IO.Directory
                .GetFiles(scanPath)
                .ToDictionary(
                    path => path,
                    path => Hasher.GenerateHash(path)
                ));
        }
    }
}
