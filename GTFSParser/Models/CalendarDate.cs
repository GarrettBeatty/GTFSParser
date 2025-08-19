using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class CalendarDate
    {
        [Name("service_id")]
        public string? ServiceId { get; set; }
        
        [Name("date")]
        public string? Date { get; set; }
        
        [Name("exception_type")]
        public int? ExceptionType { get; set; }
    }
}
