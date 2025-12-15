namespace PlantMonitoringAPI.DTOs
{
    public class SensorHistoryDto
    {
        public double Value { get; set; }
        public DateTime Time { get; set; }
        public string FormattedTime => Time.ToLocalTime().ToString("g");
    }
}
