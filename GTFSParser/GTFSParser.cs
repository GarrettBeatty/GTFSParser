using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO.Compression;
using GTFSParser.Models;

namespace GTFSParser
{
    public class GTFSFeed
    {
        public List<Agency> Agencies { get; set; } = new();
        public List<Route> Routes { get; set; } = new();
        public List<Stop> Stops { get; set; } = new();
        public List<Trip> Trips { get; set; } = new();
        public List<StopTime> StopTimes { get; set; } = new();
        public List<GTFSParser.Models.Calendar> Calendars { get; set; } = new();
        public List<CalendarDate> CalendarDates { get; set; } = new();
        public List<FareAttribute> FareAttributes { get; set; } = new();
        public List<FareRule> FareRules { get; set; } = new();
        public List<Shape> Shapes { get; set; } = new();
        public List<Frequency> Frequencies { get; set; } = new();
        public List<Transfer> Transfers { get; set; } = new();
        public List<FeedInfo> FeedInfos { get; set; } = new();
    }

    public class GTFSFeedParser
    {
        private readonly CsvConfiguration _csvConfig;

        public GTFSFeedParser()
        {
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                Delimiter = ","
            };
        }

        /// <summary>
        /// Parses a GTFS feed from a ZIP file
        /// </summary>
        /// <param name="zipFilePath">Path to the GTFS ZIP file</param>
        /// <returns>Parsed GTFS feed</returns>
        public async Task<GTFSFeed> ParseFromZipAsync(string zipFilePath)
        {
            var feed = new GTFSFeed();

            using var archive = ZipFile.OpenRead(zipFilePath);
            
            foreach (var entry in archive.Entries)
            {
                if (entry.Name.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
                {
                    await ParseCsvEntryAsync(entry, feed);
                }
            }

            return feed;
        }

        /// <summary>
        /// Parses a GTFS feed from a directory containing CSV files
        /// </summary>
        /// <param name="directoryPath">Path to the directory containing GTFS CSV files</param>
        /// <returns>Parsed GTFS feed</returns>
        public async Task<GTFSFeed> ParseFromDirectoryAsync(string directoryPath)
        {
            var feed = new GTFSFeed();
            var csvFiles = Directory.GetFiles(directoryPath, "*.txt");

            foreach (var file in csvFiles)
            {
                await ParseCsvFileAsync(file, feed);
            }

            return feed;
        }

        private async Task ParseCsvEntryAsync(ZipArchiveEntry entry, GTFSFeed feed)
        {
            using var reader = new StreamReader(entry.Open());
            using var csv = new CsvReader(reader, _csvConfig);

            var fileName = entry.Name.ToLowerInvariant();
            await ParseCsvContentAsync(csv, fileName, feed);
        }

        private async Task ParseCsvFileAsync(string filePath, GTFSFeed feed)
        {
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, _csvConfig);

            var fileName = Path.GetFileName(filePath).ToLowerInvariant();
            await ParseCsvContentAsync(csv, fileName, feed);
        }

        private async Task ParseCsvContentAsync(CsvReader csv, string fileName, GTFSFeed feed)
        {
            try
            {
                switch (fileName)
                {
                    case "agency.txt":
                        feed.Agencies = await csv.GetRecordsAsync<Agency>().ToListAsync();
                        break;
                    case "routes.txt":
                        feed.Routes = await csv.GetRecordsAsync<Route>().ToListAsync();
                        break;
                    case "stops.txt":
                        feed.Stops = await csv.GetRecordsAsync<Stop>().ToListAsync();
                        break;
                    case "trips.txt":
                        feed.Trips = await csv.GetRecordsAsync<Trip>().ToListAsync();
                        break;
                    case "stop_times.txt":
                        feed.StopTimes = await csv.GetRecordsAsync<StopTime>().ToListAsync();
                        break;
                    case "calendar.txt":
                        feed.Calendars = await csv.GetRecordsAsync<GTFSParser.Models.Calendar>().ToListAsync();
                        break;
                    case "calendar_dates.txt":
                        feed.CalendarDates = await csv.GetRecordsAsync<CalendarDate>().ToListAsync();
                        break;
                    case "fare_attributes.txt":
                        feed.FareAttributes = await csv.GetRecordsAsync<FareAttribute>().ToListAsync();
                        break;
                    case "fare_rules.txt":
                        feed.FareRules = await csv.GetRecordsAsync<FareRule>().ToListAsync();
                        break;
                    case "shapes.txt":
                        feed.Shapes = await csv.GetRecordsAsync<Shape>().ToListAsync();
                        break;
                    case "frequencies.txt":
                        feed.Frequencies = await csv.GetRecordsAsync<Frequency>().ToListAsync();
                        break;
                    case "transfers.txt":
                        feed.Transfers = await csv.GetRecordsAsync<Transfer>().ToListAsync();
                        break;
                    case "feed_info.txt":
                        feed.FeedInfos = await csv.GetRecordsAsync<FeedInfo>().ToListAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Validates the GTFS feed for required files and basic data integrity
        /// </summary>
        /// <param name="feed">The parsed GTFS feed to validate</param>
        /// <returns>Validation result with any errors found</returns>
        public List<string> ValidateFeed(GTFSFeed feed)
        {
            var errors = new List<string>();

            // Check required files
            if (!feed.Agencies.Any())
                errors.Add("agency.txt is required and must contain at least one agency");

            if (!feed.Stops.Any())
                errors.Add("stops.txt is required and must contain at least one stop");

            if (!feed.Routes.Any())
                errors.Add("routes.txt is required and must contain at least one route");

            if (!feed.Trips.Any())
                errors.Add("trips.txt is required and must contain at least one trip");

            if (!feed.StopTimes.Any())
                errors.Add("stop_times.txt is required and must contain at least one stop time");

            if (!feed.Calendars.Any() && !feed.CalendarDates.Any())
                errors.Add("Either calendar.txt or calendar_dates.txt (or both) must be present");

            // Check for orphaned references
            var routeIds = feed.Routes.Select(r => r.RouteId).ToHashSet();
            var stopIds = feed.Stops.Select(s => s.StopId).ToHashSet();
            var tripIds = feed.Trips.Select(t => t.TripId).ToHashSet();

            var orphanedTrips = feed.Trips.Where(t => !routeIds.Contains(t.RouteId)).ToList();
            if (orphanedTrips.Any())
                errors.Add($"Found {orphanedTrips.Count} trips with non-existent route_id");

            var orphanedStopTimes = feed.StopTimes.Where(st => !tripIds.Contains(st.TripId) || !stopIds.Contains(st.StopId)).ToList();
            if (orphanedStopTimes.Any())
                errors.Add($"Found {orphanedStopTimes.Count} stop times with non-existent trip_id or stop_id");

            return errors;
        }

        /// <summary>
        /// Gets summary statistics about the parsed GTFS feed
        /// </summary>
        /// <param name="feed">The parsed GTFS feed</param>
        /// <returns>Summary statistics</returns>
        public Dictionary<string, int> GetFeedStatistics(GTFSFeed feed)
        {
            return new Dictionary<string, int>
            {
                ["Agencies"] = feed.Agencies.Count,
                ["Routes"] = feed.Routes.Count,
                ["Stops"] = feed.Stops.Count,
                ["Trips"] = feed.Trips.Count,
                ["Stop Times"] = feed.StopTimes.Count,
                ["Calendars"] = feed.Calendars.Count,
                ["Calendar Dates"] = feed.CalendarDates.Count,
                ["Fare Attributes"] = feed.FareAttributes.Count,
                ["Fare Rules"] = feed.FareRules.Count,
                ["Shapes"] = feed.Shapes.Count,
                ["Frequencies"] = feed.Frequencies.Count,
                ["Transfers"] = feed.Transfers.Count,
                ["Feed Info"] = feed.FeedInfos.Count
            };
        }
    }
}
