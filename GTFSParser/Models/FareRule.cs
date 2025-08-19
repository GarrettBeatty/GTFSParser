using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class FareRule
    {
        [Name("fare_id")]
        public string? FareId { get; set; }
        
        [Name("route_id")]
        public string? RouteId { get; set; }
        
        [Name("origin_id")]
        public string? OriginId { get; set; }
        
        [Name("destination_id")]
        public string? DestinationId { get; set; }
        
        [Name("contains_id")]
        public string? ContainsId { get; set; }
    }
}
