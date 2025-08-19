using GTFSParser;
using System.IO.Compression;

namespace GTFSParser
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("GTFS Parser - Static Data Parser");
            Console.WriteLine("=================================");
            Console.WriteLine();

            var parser = new GTFSFeedParser();

            // Parse from sample directory
            Console.WriteLine("Parsing GTFS feed from sample directory...");
            var samplePath = Path.Combine("..", "sample");
            
            if (Directory.Exists(samplePath))
            {
                try
                {
                    var feed = await parser.ParseFromDirectoryAsync(samplePath);
                    
                    // Display statistics
                    Console.WriteLine("\nFeed Statistics:");
                    Console.WriteLine("================");
                    var stats = parser.GetFeedStatistics(feed);
                    foreach (var stat in stats)
                    {
                        Console.WriteLine($"{stat.Key}: {stat.Value}");
                    }

                    // Validate feed
                    Console.WriteLine("\nFeed Validation:");
                    Console.WriteLine("================");
                    var errors = parser.ValidateFeed(feed);
                    if (errors.Any())
                    {
                        Console.WriteLine("Validation errors found:");
                        foreach (var error in errors)
                        {
                            Console.WriteLine($"  - {error}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("✓ Feed validation passed!");
                    }

                    // Display some sample data
                    Console.WriteLine("\nSample Data:");
                    Console.WriteLine("=============");
                    
                    if (feed.Agencies.Any())
                    {
                        var agency = feed.Agencies.First();
                        Console.WriteLine($"Agency: {agency.AgencyName} ({agency.AgencyId})");
                        Console.WriteLine($"  URL: {agency.AgencyUrl}");
                        Console.WriteLine($"  Timezone: {agency.AgencyTimezone}");
                    }

                    if (feed.Routes.Any())
                    {
                        var route = feed.Routes.First();
                        Console.WriteLine($"Route: {route.RouteShortName} - {route.RouteLongName}");
                        Console.WriteLine($"  Type: {route.RouteType}");
                    }

                    if (feed.Stops.Any())
                    {
                        var stop = feed.Stops.First();
                        Console.WriteLine($"Stop: {stop.StopName} ({stop.StopId})");
                        Console.WriteLine($"  Location: {stop.StopLat}, {stop.StopLon}");
                    }

                    // GTFS Analysis demo
                    Console.WriteLine("\nGTFS Analysis:");
                    Console.WriteLine("===============");
                    var analyzer = new GTFSAnalyzer(feed);
                    
                    // Show routes by agency
                    if (feed.Agencies.Any())
                    {
                        var agency = feed.Agencies.First();
                        if (agency.AgencyId != null)
                        {
                            var routes = analyzer.GetRoutesByAgency(agency.AgencyId);
                            Console.WriteLine($"Routes for agency {agency.AgencyId}: {routes.Count}");
                            foreach (var r in routes.Take(3))
                            {
                                Console.WriteLine($"  - {r.RouteShortName}: {r.RouteLongName}");
                            }
                        }
                    }
                    
                    // Show trip itinerary example
                    if (feed.Trips.Any())
                    {
                        var trip = feed.Trips.First();
                        if (trip.TripId != null)
                        {
                            var itinerary = analyzer.GetTripItinerary(trip.TripId);
                            Console.WriteLine($"\nTrip {trip.TripId} itinerary: {itinerary.Count} stops");
                            foreach (var (s, st) in itinerary.Take(3))
                            {
                                Console.WriteLine($"  - {s.StopName}: {st.ArrivalTime} - {st.DepartureTime}");
                            }
                        }
                    }
                    
                    // Show service information for today
                    var today = DateTime.Today;
                    var activeServices = analyzer.GetActiveServices(today);
                    Console.WriteLine($"\nActive services for {today:yyyy-MM-dd}: {activeServices.Count}");
                    foreach (var serviceId in activeServices)
                    {
                        Console.WriteLine($"  - {serviceId}");
                    }

                    // Demonstrate writing GTFS back to disk and zip
                    Console.WriteLine("\nWriting feed to ./out directory and ./out.zip...");
                    var writer = new GTFSFeedWriter();
                    var outDir = Path.Combine(".", "out");
                    await writer.WriteToDirectoryAsync(feed, outDir, overwrite: true);
                    var outZip = Path.Combine(".", "out.zip");
                    await writer.WriteToZipAsync(feed, outZip, overwrite: true);
                    Console.WriteLine("✓ Write complete.");

                    // Create a ZIP file from sample data for testing
                    Console.WriteLine("\nCreating sample ZIP file for testing...");
                    var zipPath = Path.Combine("..", "sample.zip");
                    await CreateSampleZipFileAsync(samplePath, zipPath);
                    
                    if (File.Exists(zipPath))
                    {
                        Console.WriteLine($"✓ Sample ZIP file created: {zipPath}");
                        
                        // Test parsing from ZIP file
                        Console.WriteLine("\nTesting ZIP file parsing...");
                        var zipFeed = await parser.ParseFromZipAsync(zipPath);
                        
                        Console.WriteLine($"ZIP Feed Statistics:");
                        var zipStats = parser.GetFeedStatistics(zipFeed);
                        foreach (var stat in zipStats)
                        {
                            Console.WriteLine($"  {stat.Key}: {stat.Value}");
                        }
                        
                        var zipErrors = parser.ValidateFeed(zipFeed);
                        if (!zipErrors.Any())
                        {
                            Console.WriteLine("✓ ZIP feed validation passed!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing sample data: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("Sample directory not found. Please ensure the sample GTFS files are available.");
                Console.WriteLine($"Looking for: {Path.GetFullPath(samplePath)}");
            }

            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }

        private static async Task CreateSampleZipFileAsync(string sourceDir, string zipPath)
        {
            if (File.Exists(zipPath))
                File.Delete(zipPath);

            using var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create);
            var files = Directory.GetFiles(sourceDir, "*.txt");
            
            foreach (var file in files)
            {
                var fileName = Path.GetFileName(file);
                var entry = archive.CreateEntry(fileName);
                
                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(file);
                await fileStream.CopyToAsync(entryStream);
            }
        }
    }
}
