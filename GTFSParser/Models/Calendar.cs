using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class Calendar
    {
        [Name("service_id")]
        public string? ServiceId { get; set; }
        
        [Name("monday")]
        public bool? Monday { get; set; }
        
        [Name("tuesday")]
        public bool? Tuesday { get; set; }
        
        [Name("wednesday")]
        public bool? Wednesday { get; set; }
        
        [Name("thursday")]
        public bool? Thursday { get; set; }
        
        [Name("friday")]
        public bool? Friday { get; set; }
        
        [Name("saturday")]
        public bool? Saturday { get; set; }
        
        [Name("sunday")]
        public bool? Sunday { get; set; }
        
        [Name("start_date")]
        public string? StartDate { get; set; }
        
        [Name("end_date")]
        public string? EndDate { get; set; }
    }
}
