using System;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace FileChangeWorker.Internal
{
    internal static class Configuration
    {
        private static readonly IConfigurationRoot ConfigurationRoot;
        private static readonly string Env;
        private const string WorkerSectionName = "Worker";
        
        private static string SettingsFile
        {
            get
            {
                var env = string.IsNullOrEmpty(Env) ? string.Empty : $".{Env}";
                return $"appsettings{env}.json";
            }
        }

        static Configuration()
        {
            Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(Env))
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development", EnvironmentVariableTarget.Process);
                Env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", EnvironmentVariableTarget.Process);
            }

            ConfigurationRoot = LoadConfiguration(Directory.GetCurrentDirectory());
            
            ValidateWorkerConfiguration();
        }
        
        public static IConfigurationSection WorkerConfigSection => ConfigurationRoot.GetSection(WorkerSectionName);
        
        private static IConfigurationRoot LoadConfiguration(string directory)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile(SettingsFile, optional: true, reloadOnChange: true);

            return builder.Build();
        }

        private static void ValidateWorkerConfiguration()
        {
            var section = ConfigurationRoot.GetSection(WorkerSectionName);
            var options = new WorkerOptions();
            section.Bind(options);
            
            if (!Directory.Exists(options.ScanPath))
            {
                throw new InvalidConfigurationException($"scan directory was not found in path `${options.ScanPath}`");
            }
            
            if (options.WaitMs < 0)
            {
                throw new InvalidConfigurationException($"`WaitMs` option must be positive, provided `${options.WaitMs}`");
            }
        }
    }
}
