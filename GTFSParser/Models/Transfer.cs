using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class Transfer
    {
        [Name("from_stop_id")]
        public string? FromStopId { get; set; }
        
        [Name("to_stop_id")]
        public string? ToStopId { get; set; }
        
        [Name("transfer_type")]
        public int? TransferType { get; set; }
        
        [Name("min_transfer_time")]
        public int? MinTransferTime { get; set; }
        
        [Name("from_route_id")]
        public string? FromRouteId { get; set; }
        
        [Name("to_route_id")]
        public string? ToRouteId { get; set; }
        
        [Name("from_trip_id")]
        public string? FromTripId { get; set; }
        
        [Name("to_trip_id")]
        public string? ToTripId { get; set; }
    }
}
