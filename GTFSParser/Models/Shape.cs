using CsvHelper.Configuration.Attributes;

namespace GTFSParser.Models
{
    public class Shape
    {
        [Name("shape_id")]
        public string? ShapeId { get; set; }
        
        [Name("shape_pt_lat")]
        public double? ShapePtLat { get; set; }
        
        [Name("shape_pt_lon")]
        public double? ShapePtLon { get; set; }
        
        [Name("shape_pt_sequence")]
        public int? ShapePtSequence { get; set; }
        
        [Name("shape_dist_traveled")]
        public double? ShapeDistTraveled { get; set; }
    }
}
