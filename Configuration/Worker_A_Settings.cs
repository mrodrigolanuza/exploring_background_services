using Microsoft.Extensions.Logging;

namespace exploring_background_services.Configuration
{
    public class Worker_A_Settings
    {
        public bool Enabled { get; set; }
        public TimeSpan AutoExecutionDelay { get; set; }
        public string LogFilePath { get; set; } = string.Empty;
        public string RollingInterval { get; set; } = "Day";
        public LogLevel LogLevel { get; set; } = LogLevel.Information;
    }
}
