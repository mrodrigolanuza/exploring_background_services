namespace exploring_background_services.Configuration
{
    public class Worker_B_Settings
    {
        public bool Enabled { get; set; }
        public TimeSpan AutoExecutionDelay { get; set; }
        public int NumberOfDays { get; set; }
    }
}