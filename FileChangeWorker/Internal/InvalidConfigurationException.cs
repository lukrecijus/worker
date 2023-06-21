using System;

namespace FileChangeWorker.Internal
{
    [Serializable]
    internal class InvalidConfigurationException : Exception
    {
        internal InvalidConfigurationException(string message) : base(message)
        {
        }
    }
}
