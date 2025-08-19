using GTFSParser;
using GTFSParser.Models;

namespace GTFSParser.Examples
{
    public static class UsageExamples
    {
        /// <summary>
        /// Example: Basic GTFS feed parsing
        /// </summary>
        public static async Task BasicParsingExample()
        {
            var parser = new GTFSFeedParser();
            
            // Parse from directory
            var feed = await parser.ParseFromDirectoryAsync("path/to/gtfs/files");
            
            // Parse from ZIP file
            var zipFeed = await parser.ParseFromZipAsync("path/to/gtfs.zip");
            
            // Validate feeds
            var errors = parser.ValidateFeed(feed);
            if (errors.Any())
            {
                Console.WriteLine("Validation errors:");
                foreach (var error in errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }

        /// <summary>
        /// Example: Working with parsed GTFS data
        /// </summary>
        public static void WorkingWithDataExample(GTFSFeed feed)
        {
            // Find all routes for a specific agency
            var agencyId = "DTA";
            var routes = feed.Routes.Where(r => r.AgencyId == agencyId).ToList();
            
            // Find all stops for a specific route
            var routeId = "AB";
            var tripIds = feed.Trips.Where(t => t.RouteId == routeId).Select(t => t.TripId).ToHashSet();
            var stopIds = feed.StopTimes
                .Where(st => tripIds.Contains(st.TripId))
                .Select(st => st.StopId)
                .ToHashSet();
            var stops = feed.Stops.Where(s => stopIds.Contains(s.StopId)).ToList();
            
            // Get trip itinerary
            var tripId = "AB1";
            var stopTimes = feed.StopTimes
                .Where(st => st.TripId == tripId)
                .OrderBy(st => st.StopSequence)
                .ToList();
            
            foreach (var stopTime in stopTimes)
            {
                var stop = feed.Stops.First(s => s.StopId == stopTime.StopId);
                Console.WriteLine($"{stop.StopName}: {stopTime.ArrivalTime} - {stopTime.DepartureTime}");
            }
        }

        /// <summary>
        /// Example: Using the GTFS Analyzer
        /// </summary>
        public static void AnalyzerExample(GTFSFeed feed)
        {
            var analyzer = new GTFSAnalyzer(feed);
            
            // Get routes by agency
            var routes = analyzer.GetRoutesByAgency("DTA");
            
            // Get stops near a location
            var nearbyStops = analyzer.GetStopsNearLocation(36.425288, -117.133162, 10.0); // 10km radius
            
            // Get active services for a date
            var activeServices = analyzer.GetActiveServices(DateTime.Today);
            
            // Get fare information for a route
            var fares = analyzer.GetFaresByRoute("AB");
            
            // Get complete feed summary
            var summary = analyzer.GetFeedSummary();
            Console.WriteLine(summary);
        }

        /// <summary>
        /// Example: Advanced data analysis
        /// </summary>
        public static void AdvancedAnalysisExample(GTFSFeed feed)
        {
            // Find routes with the most stops
            var routeStopCounts = feed.Routes.Select(route =>
            {
                var tripIds = feed.Trips.Where(t => t.RouteId == route.RouteId).Select(t => t.TripId).ToHashSet();
                var stopCount = feed.StopTimes
                    .Where(st => tripIds.Contains(st.TripId))
                    .Select(st => st.StopId)
                    .Distinct()
                    .Count();
                
                return new { Route = route, StopCount = stopCount };
            })
            .OrderByDescending(x => x.StopCount)
            .ToList();
            
            Console.WriteLine("Routes by number of stops:");
            foreach (var item in routeStopCounts)
            {
                Console.WriteLine($"{item.Route.RouteShortName}: {item.StopCount} stops");
            }
            
            // Find stops with the most routes
            var stopRouteCounts = feed.Stops.Select(stop =>
            {
                var tripIds = feed.StopTimes
                    .Where(st => st.StopId == stop.StopId)
                    .Select(st => st.TripId)
                    .ToHashSet();
                
                var routeCount = feed.Trips
                    .Where(t => tripIds.Contains(t.TripId))
                    .Select(t => t.RouteId)
                    .Distinct()
                    .Count();
                
                return new { Stop = stop, RouteCount = routeCount };
            })
            .OrderByDescending(x => x.RouteCount)
            .ToList();
            
            Console.WriteLine("\nStops by number of routes:");
            foreach (var item in stopRouteCounts.Take(5))
            {
                Console.WriteLine($"{item.Stop.StopName}: {item.RouteCount} routes");
            }
        }

        /// <summary>
        /// Example: Exporting data to different formats
        /// </summary>
        public static void ExportExample(GTFSFeed feed)
        {
            // Export routes to CSV
            var routesCsv = "route_id,route_short_name,route_long_name,route_type\n";
            foreach (var route in feed.Routes)
            {
                routesCsv += $"{route.RouteId},{route.RouteShortName},{route.RouteLongName},{route.RouteType}\n";
            }
            File.WriteAllText("routes_export.csv", routesCsv);
            
            // Export stops to GeoJSON
            var geoJson = "{\n  \"type\": \"FeatureCollection\",\n  \"features\": [\n";
            foreach (var stop in feed.Stops.Where(s => s.StopLat.HasValue && s.StopLon.HasValue))
            {
                geoJson += $"    {{\n      \"type\": \"Feature\",\n      \"geometry\": {{\n        \"type\": \"Point\",\n        \"coordinates\": [{stop.StopLon}, {stop.StopLat}]\n      }},\n      \"properties\": {{\n        \"stop_id\": \"{stop.StopId}\",\n        \"stop_name\": \"{stop.StopName}\"\n      }}\n    }},\n";
            }
            geoJson = geoJson.TrimEnd(',', '\n') + "\n  ]\n}";
            File.WriteAllText("stops_export.geojson", geoJson);
        }

        /// <summary>
        /// Example: Error handling and validation
        /// </summary>
        public static async Task ErrorHandlingExample()
        {
            var parser = new GTFSFeedParser();
            
            try
            {
                var feed = await parser.ParseFromDirectoryAsync("path/to/gtfs/files");
                
                // Validate the feed
                var errors = parser.ValidateFeed(feed);
                if (errors.Any())
                {
                    Console.WriteLine("GTFS validation errors found:");
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    
                    // Try to fix common issues
                    if (errors.Any(e => e.Contains("agency.txt")))
                    {
                        Console.WriteLine("Missing agency.txt - this is a critical error");
                    }
                    
                    if (errors.Any(e => e.Contains("orphaned")))
                    {
                        Console.WriteLine("Found orphaned references - data integrity issues detected");
                    }
                }
                else
                {
                    Console.WriteLine("GTFS feed is valid!");
                }
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine($"GTFS files not found: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing GTFS feed: {ex.Message}");
            }
        }
    }
}
