using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class FeedInfo
    {
        [Name("feed_publisher_name")]
        public string? FeedPublisherName { get; set; }
        
        [Name("feed_publisher_url")]
        public string? FeedPublisherUrl { get; set; }
        
        [Name("feed_lang")]
        public string? FeedLang { get; set; }
        
        [Name("default_lang")]
        public string? DefaultLang { get; set; }
        
        [Name("feed_start_date")]
        public string? FeedStartDate { get; set; }
        
        [Name("feed_end_date")]
        public string? FeedEndDate { get; set; }
        
        [Name("feed_version")]
        public string? FeedVersion { get; set; }
        
        [Name("feed_contact_email")]
        public string? FeedContactEmail { get; set; }
        
        [Name("feed_contact_url")]
        public string? FeedContactUrl { get; set; }
    }
}
