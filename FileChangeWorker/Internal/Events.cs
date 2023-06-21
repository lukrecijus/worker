using System;
using Microsoft.Extensions.Logging;

namespace FileChangeWorker.Internal
{
    internal sealed class Events<T> where T : class
    {
        private readonly T _sender;
        private static event EventHandler<OnFileChangedEventArgs> FileChanged;
        private static event EventHandler<OnFileChangedEventArgs> NewFileFound;

        internal Events(ILogger<T> logger, T sender)
        {
            _sender = sender;
            FileChanged += (_, args) =>
            {
                logger.LogInformation("file changed: " + args.FileName);
            };
            NewFileFound += (_, arg) =>
            {
                logger.LogInformation("new file found: " + arg.FileName);
            };
        }

        internal void OnFileChanged(string fileName) => 
            FileChanged?.Invoke(_sender, new OnFileChangedEventArgs{ FileName = fileName });

        internal void OnNewFileFound(string fileName) =>
            NewFileFound?.Invoke(_sender, new OnFileChangedEventArgs { FileName = fileName });
        
        private sealed class OnFileChangedEventArgs : EventArgs 
        { 
            public string FileName { get; init; }
        }
    }
}
