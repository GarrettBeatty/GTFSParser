using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class Stop
    {
        [Name("stop_id")]
        public string? StopId { get; set; }
        
        [Name("stop_code")]
        public string? StopCode { get; set; }
        
        [Name("stop_name")]
        public string? StopName { get; set; }
        
        [Name("stop_desc")]
        public string? StopDesc { get; set; }
        
        [Name("stop_lat")]
        public double? StopLat { get; set; }
        
        [Name("stop_lon")]
        public double? StopLon { get; set; }
        
        [Name("zone_id")]
        public string? ZoneId { get; set; }
        
        [Name("stop_url")]
        public string? StopUrl { get; set; }
        
        [Name("location_type")]
        public int? LocationType { get; set; }
        
        [Name("parent_station")]
        public string? ParentStation { get; set; }
        
        [Name("wheelchair_boarding")]
        public string? WheelchairBoarding { get; set; }
        
        [Name("level_id")]
        public string? LevelId { get; set; }
        
        [Name("platform_code")]
        public string? PlatformCode { get; set; }
    }
}
