using Xunit;
using GTFSParser;
using GTFSParser.Models;
using System.Text;

namespace GTFSParser.Tests
{
    public class GTFSFeedParserTests
    {
        private readonly GTFSFeedParser _parser;

        public GTFSFeedParserTests()
        {
            _parser = new GTFSFeedParser();
        }

        [Fact]
        public async Task ParseFromDirectoryAsync_WithValidFiles_ShouldParseCorrectly()
        {
            // Arrange
            var tempDir = CreateTempGTFSFiles();

            try
            {
                // Act
                var feed = await _parser.ParseFromDirectoryAsync(tempDir);

                // Assert
                Assert.NotNull(feed);
                Assert.Single(feed.Agencies);
                Assert.Single(feed.Stops);
                Assert.Single(feed.Routes);
                Assert.Single(feed.Trips);
                Assert.Single(feed.StopTimes);
                Assert.Single(feed.Calendars);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task ParseFromZipAsync_WithValidZip_ShouldParseCorrectly()
        {
            // Arrange
            var tempDir = CreateTempGTFSFiles();
            var zipPath = Path.Combine(Path.GetTempPath(), $"gtfs_test_{Guid.NewGuid()}.zip");

            try
            {
                // Create ZIP file
                using (var archive = System.IO.Compression.ZipFile.Open(zipPath, System.IO.Compression.ZipArchiveMode.Create))
                {
                    foreach (var file in Directory.GetFiles(tempDir, "*.txt"))
                    {
                        var entry = archive.CreateEntry(Path.GetFileName(file));
                        using var entryStream = entry.Open();
                        using var fileStream = File.OpenRead(file);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }

                // Act
                var feed = await _parser.ParseFromZipAsync(zipPath);

                // Assert
                Assert.NotNull(feed);
                Assert.Single(feed.Agencies);
                Assert.Single(feed.Stops);
                Assert.Single(feed.Routes);
                Assert.Single(feed.Trips);
                Assert.Single(feed.StopTimes);
                Assert.Single(feed.Calendars);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
        }

        [Fact]
        public async Task ParseFromDirectoryAsync_WithMissingFiles_ShouldHandleGracefully()
        {
            // Arrange
            var tempDir = CreateTempGTFSFiles();
            File.Delete(Path.Combine(tempDir, "agency.txt")); // Remove required file

            try
            {
                // Act
                var feed = await _parser.ParseFromDirectoryAsync(tempDir);

                // Assert
                Assert.NotNull(feed);
                Assert.Empty(feed.Agencies); // Should be empty since file was deleted
                Assert.Single(feed.Stops);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public void ValidateFeed_WithValidFeed_ShouldPassValidation()
        {
            // Arrange
            var feed = CreateValidGTFSFeed();

            // Act
            var errors = _parser.ValidateFeed(feed);

            // Assert
            Assert.Empty(errors);
        }

        [Fact]
        public void ValidateFeed_WithMissingRequiredFiles_ShouldReturnErrors()
        {
            // Arrange
            var feed = new GTFSFeed(); // Empty feed

            // Act
            var errors = _parser.ValidateFeed(feed);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("agency.txt"));
            Assert.Contains(errors, e => e.Contains("stops.txt"));
            Assert.Contains(errors, e => e.Contains("routes.txt"));
            Assert.Contains(errors, e => e.Contains("trips.txt"));
            Assert.Contains(errors, e => e.Contains("stop_times.txt"));
        }

        [Fact]
        public void ValidateFeed_WithOrphanedReferences_ShouldReturnErrors()
        {
            // Arrange
            var feed = CreateValidGTFSFeed();
            feed.Routes.Clear(); // Remove routes but keep trips that reference them

            // Act
            var errors = _parser.ValidateFeed(feed);

            // Assert
            Assert.NotEmpty(errors);
            Assert.Contains(errors, e => e.Contains("trips with non-existent route_id"));
        }

        [Fact]
        public void GetFeedStatistics_WithValidFeed_ShouldReturnCorrectCounts()
        {
            // Arrange
            var feed = CreateValidGTFSFeed();

            // Act
            var stats = _parser.GetFeedStatistics(feed);

            // Assert
            Assert.Equal(1, stats["Agencies"]);
            Assert.Equal(1, stats["Routes"]);
            Assert.Equal(1, stats["Stops"]);
            Assert.Equal(1, stats["Trips"]);
            Assert.Equal(1, stats["Stop Times"]);
            Assert.Equal(1, stats["Calendars"]);
        }

        private string CreateTempGTFSFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            // Create agency.txt
            var agencyContent = "agency_id,agency_name,agency_url,agency_timezone\n" +
                              "TEST,Test Agency,http://test.com,America/New_York";
            File.WriteAllText(Path.Combine(tempDir, "agency.txt"), agencyContent);

            // Create stops.txt
            var stopsContent = "stop_id,stop_name,stop_lat,stop_lon\n" +
                             "STOP1,Test Stop,40.7128,-74.0060";
            File.WriteAllText(Path.Combine(tempDir, "stops.txt"), stopsContent);

            // Create routes.txt
            var routesContent = "route_id,agency_id,route_short_name,route_long_name,route_type\n" +
                              "ROUTE1,TEST,1,Test Route,3";
            File.WriteAllText(Path.Combine(tempDir, "routes.txt"), routesContent);

            // Create trips.txt
            var tripsContent = "route_id,service_id,trip_id\n" +
                             "ROUTE1,SERVICE1,TRIP1";
            File.WriteAllText(Path.Combine(tempDir, "trips.txt"), tripsContent);

            // Create stop_times.txt
            var stopTimesContent = "trip_id,arrival_time,departure_time,stop_id,stop_sequence\n" +
                                 "TRIP1,08:00:00,08:00:00,STOP1,1";
            File.WriteAllText(Path.Combine(tempDir, "stop_times.txt"), stopTimesContent);

            // Create calendar.txt
            var calendarContent = "service_id,monday,tuesday,wednesday,thursday,friday,saturday,sunday,start_date,end_date\n" +
                                "SERVICE1,1,1,1,1,1,0,0,20240101,20241231";
            File.WriteAllText(Path.Combine(tempDir, "calendar.txt"), calendarContent);

            return tempDir;
        }

        private GTFSFeed CreateValidGTFSFeed()
        {
            return new GTFSFeed
            {
                Agencies = new List<Agency>
                {
                    new Agency
                    {
                        AgencyId = "TEST",
                        AgencyName = "Test Agency",
                        AgencyUrl = "http://test.com",
                        AgencyTimezone = "America/New_York"
                    }
                },
                Stops = new List<Stop>
                {
                    new Stop
                    {
                        StopId = "STOP1",
                        StopName = "Test Stop",
                        StopLat = 40.7128,
                        StopLon = -74.0060
                    }
                },
                Routes = new List<Route>
                {
                    new Route
                    {
                        RouteId = "ROUTE1",
                        AgencyId = "TEST",
                        RouteShortName = "1",
                        RouteLongName = "Test Route",
                        RouteType = 3
                    }
                },
                Trips = new List<Trip>
                {
                    new Trip
                    {
                        TripId = "TRIP1",
                        RouteId = "ROUTE1",
                        ServiceId = "SERVICE1"
                    }
                },
                StopTimes = new List<StopTime>
                {
                    new StopTime
                    {
                        TripId = "TRIP1",
                        StopId = "STOP1",
                        StopSequence = 1,
                        ArrivalTime = "08:00:00",
                        DepartureTime = "08:00:00"
                    }
                },
                Calendars = new List<GTFSParser.Models.Calendar>
                {
                    new GTFSParser.Models.Calendar
                    {
                        ServiceId = "SERVICE1",
                        Monday = true,
                        Tuesday = true,
                        Wednesday = true,
                        Thursday = true,
                        Friday = true,
                        Saturday = false,
                        Sunday = false,
                        StartDate = "20240101",
                        EndDate = "20241231"
                    }
                }
            };
        }
    }
}
