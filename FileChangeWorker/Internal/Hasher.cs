using System;

namespace FileChangeWorker.Internal
{
    internal static class Hasher
    {
        internal enum HashType { Md5, Sha1, Sha256, Sha384, Sha512 }
        
        internal record FileHash
        {
            public string Hash { internal get; init; }

            public override string ToString()
            {
                return Hash;
            }
        }
        
        internal static FileHash GenerateHash(string path, HashType type = HashType.Md5) => 
            new() { Hash = GenerateHashInternal(path, type) };
        
        internal static bool CompareHash(FileHash h1, FileHash h2) => h1.Hash == h2.Hash;
        
        private static string GenerateHashInternal(string path, HashType type = HashType.Md5)
        {
            using var hasher = System.Security.Cryptography.HashAlgorithm.Create(type.ToString().ToUpper());
            using var stream = System.IO.File.OpenRead(path);
            var hash = hasher!.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
