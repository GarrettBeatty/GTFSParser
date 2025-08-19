using Xunit;
using GTFSParser;
using GTFSParser.Models;
using System.IO.Compression;

namespace GTFSParser.Tests
{
    public class GTFSWriterTests
    {
        private readonly GTFSFeedWriter _writer;
        private readonly GTFSFeed _testFeed;

        public GTFSWriterTests()
        {
            _writer = new GTFSFeedWriter();
            _testFeed = CreateTestFeed();
        }

        [Fact]
        public async Task WriteToDirectoryAsync_WithValidFeed_ShouldCreateAllFiles()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_write_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                await _writer.WriteToDirectoryAsync(_testFeed, tempDir);

                // Assert
                var files = Directory.GetFiles(tempDir, "*.txt");
                Assert.Equal(6, files.Length); // Required files only (calendar_dates.txt is empty)

                // Check that required files exist
                var fileNames = Directory.GetFiles(tempDir, "*.txt").Select(Path.GetFileName).ToList();
                Assert.Contains("agency.txt", fileNames);
                Assert.Contains("stops.txt", fileNames);
                Assert.Contains("routes.txt", fileNames);
                Assert.Contains("trips.txt", fileNames);
                Assert.Contains("stop_times.txt", fileNames);
                Assert.Contains("calendar.txt", fileNames);

                // Check file contents
                var agencyContent = await File.ReadAllTextAsync(Path.Combine(tempDir, "agency.txt"));
                Assert.Contains("agency_id,agency_name,agency_url,agency_timezone", agencyContent);
                Assert.Contains("TEST,Test Agency,http://test.com,America/New_York", agencyContent);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task WriteToDirectoryAsync_WithEmptyCollections_ShouldSkipEmptyFiles()
        {
            // Arrange
            var emptyFeed = new GTFSFeed
            {
                Agencies = new List<Agency>(), // Empty
                Stops = new List<Stop>(),      // Empty
                Routes = new List<Route>(),    // Empty
                Trips = new List<Trip>(),      // Empty
                StopTimes = new List<StopTime>(), // Empty
                Calendars = new List<GTFSParser.Models.Calendar>() // Empty
            };

            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_write_empty_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                await _writer.WriteToDirectoryAsync(emptyFeed, tempDir);

                // Assert
                var files = Directory.GetFiles(tempDir, "*.txt");
                Assert.Empty(files); // No files should be created for empty collections
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task WriteToDirectoryAsync_WithOverwriteFalse_ShouldNotOverwriteExistingFiles()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_write_overwrite_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            // Create an existing file
            var existingFile = Path.Combine(tempDir, "agency.txt");
            await File.WriteAllTextAsync(existingFile, "existing content");

            try
            {
                // Act
                await _writer.WriteToDirectoryAsync(_testFeed, tempDir, overwrite: false);

                // Assert
                var content = await File.ReadAllTextAsync(existingFile);
                Assert.Equal("existing content", content); // Should not be overwritten
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task WriteToZipAsync_WithValidFeed_ShouldCreateValidZip()
        {
            // Arrange
            var zipPath = Path.Combine(Path.GetTempPath(), $"gtfs_write_test_{Guid.NewGuid()}.zip");

            try
            {
                // Act
                await _writer.WriteToZipAsync(_testFeed, zipPath);

                // Assert
                Assert.True(File.Exists(zipPath));

                using var archive = ZipFile.OpenRead(zipPath);
                var entries = archive.Entries.ToList();
                Assert.Equal(6, entries.Count); // Required files only (calendar_dates.txt is empty)

                // Check that required files exist in zip
                var entryNames = entries.Select(e => e.Name).ToList();
                Assert.Contains("agency.txt", entryNames);
                Assert.Contains("stops.txt", entryNames);
                Assert.Contains("routes.txt", entryNames);
                Assert.Contains("trips.txt", entryNames);
                Assert.Contains("stop_times.txt", entryNames);
                Assert.Contains("calendar.txt", entryNames);

                // Check file contents in zip
                var agencyEntry = entries.First(e => e.Name == "agency.txt");
                using var reader = new StreamReader(agencyEntry.Open());
                var content = await reader.ReadToEndAsync();
                Assert.Contains("agency_id,agency_name,agency_url,agency_timezone", content);
                Assert.Contains("TEST,Test Agency,http://test.com,America/New_York", content);
            }
            finally
            {
                // Cleanup
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
        }

        [Fact]
        public async Task WriteToZipAsync_WithOverwriteFalse_ShouldNotOverwriteExistingZip()
        {
            // Arrange
            var zipPath = Path.Combine(Path.GetTempPath(), $"gtfs_write_overwrite_test_{Guid.NewGuid()}.zip");
            await File.WriteAllTextAsync(zipPath, "existing content");

            try
            {
                // Act
                await _writer.WriteToZipAsync(_testFeed, zipPath, overwrite: false);

                // Assert
                var content = await File.ReadAllTextAsync(zipPath);
                Assert.Equal("existing content", content); // Should not be overwritten
            }
            finally
            {
                // Cleanup
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
        }

        [Fact]
        public async Task WriteToZipAsync_WithOverwriteTrue_ShouldOverwriteExistingZip()
        {
            // Arrange
            var zipPath = Path.Combine(Path.GetTempPath(), $"gtfs_write_overwrite_test_{Guid.NewGuid()}.zip");
            await File.WriteAllTextAsync(zipPath, "existing content");

            try
            {
                // Act
                await _writer.WriteToZipAsync(_testFeed, zipPath, overwrite: true);

                // Assert
                Assert.True(File.Exists(zipPath));

                using var archive = ZipFile.OpenRead(zipPath);
                var entries = archive.Entries.ToList();
                Assert.Equal(6, entries.Count); // Should contain new content (calendar_dates.txt is empty)
            }
            finally
            {
                // Cleanup
                if (File.Exists(zipPath))
                    File.Delete(zipPath);
            }
        }

        [Fact]
        public async Task WriteToDirectoryAsync_WithOptionalFiles_ShouldIncludeThem()
        {
            // Arrange
            var feedWithOptionals = CreateTestFeed();
            feedWithOptionals.FareAttributes = new List<FareAttribute>
            {
                new FareAttribute
                {
                    FareId = "FARE1",
                    Price = 2.75m,
                    CurrencyType = "USD",
                    PaymentMethod = 1,
                    Transfers = 0
                }
            };
            feedWithOptionals.FareRules = new List<FareRule>
            {
                new FareRule
                {
                    FareId = "FARE1",
                    RouteId = "ROUTE1"
                }
            };

            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_write_optional_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                await _writer.WriteToDirectoryAsync(feedWithOptionals, tempDir);

                // Assert
                var files = Directory.GetFiles(tempDir, "*.txt");
                Assert.Equal(8, files.Length); // Required + optional files (calendar_dates.txt is empty)

                Assert.True(File.Exists(Path.Combine(tempDir, "fare_attributes.txt")));
                Assert.True(File.Exists(Path.Combine(tempDir, "fare_rules.txt")));
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task WriteToDirectoryAsync_ShouldCreateValidCSVFiles()
        {
            // Arrange
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_write_csv_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Act
                await _writer.WriteToDirectoryAsync(_testFeed, tempDir);

                // Assert - Check CSV format
                var stopsContent = await File.ReadAllTextAsync(Path.Combine(tempDir, "stops.txt"));
                var lines = stopsContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                
                Assert.Equal(2, lines.Length); // Header + 1 data row
                Assert.Contains("stop_id", lines[0]); // Header contains stop_id
                Assert.Contains("stop_name", lines[0]); // Header contains stop_name
                Assert.Contains("stop_lat", lines[0]); // Header contains stop_lat
                Assert.Contains("stop_lon", lines[0]); // Header contains stop_lon
                Assert.Contains("STOP1", lines[1]); // Data contains stop ID
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        private GTFSFeed CreateTestFeed()
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
                        StopName = "Test Stop 1",
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
                        RouteLongName = "Test Route 1",
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
