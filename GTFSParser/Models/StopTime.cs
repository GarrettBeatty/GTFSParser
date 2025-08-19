using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class StopTime
    {
        [Name("trip_id")]
        public string? TripId { get; set; }
        
        [Name("arrival_time")]
        public string? ArrivalTime { get; set; }
        
        [Name("departure_time")]
        public string? DepartureTime { get; set; }
        
        [Name("stop_id")]
        public string? StopId { get; set; }
        
        [Name("stop_sequence")]
        public int? StopSequence { get; set; }
        
        [Name("stop_headsign")]
        public string? StopHeadsign { get; set; }
        
        [Name("pickup_type")]
        public int? PickupType { get; set; }
        
        [Name("drop_off_type")]
        public int? DropOffType { get; set; }
        
        [Name("shape_dist_traveled")]
        public double? ShapeDistTraveled { get; set; }
        
        [Name("timepoint")]
        public int? Timepoint { get; set; }
        
        [Name("continuous_pickup")]
        public int? ContinuousPickup { get; set; }
        
        [Name("continuous_drop_off")]
        public int? ContinuousDropOff { get; set; }
    }
}
