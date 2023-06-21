namespace FileChangeWorker
{
    public sealed class WorkerOptions
    {
        public string ScanPath { get; set; }
        public int WaitMs { get; set; }
    }
}
