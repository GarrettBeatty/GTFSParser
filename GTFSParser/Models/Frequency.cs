using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class Frequency
    {
        [Name("trip_id")]
        public string? TripId { get; set; }
        
        [Name("start_time")]
        public string? StartTime { get; set; }
        
        [Name("end_time")]
        public string? EndTime { get; set; }
        
        [Name("headway_secs")]
        public int? HeadwaySecs { get; set; }
        
        [Name("exact_times")]
        public int? ExactTimes { get; set; }
    }
}
