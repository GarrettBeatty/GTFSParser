using Xunit;
using GTFSParser;
using GTFSParser.Models;
using System.IO.Compression;

namespace GTFSParser.Tests
{
    public class IntegrationTests
    {
        [Fact]
        public async Task FullWorkflow_ParseWriteParse_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var parser = new GTFSFeedParser();
            var writer = new GTFSFeedWriter();
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_integration_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create original test data
                var originalFeed = CreateComprehensiveTestFeed();

                // Act 1: Write the feed to directory
                await writer.WriteToDirectoryAsync(originalFeed, tempDir);

                // Act 2: Parse the written files back
                var parsedFeed = await parser.ParseFromDirectoryAsync(tempDir);

                // Assert: Data integrity should be maintained
                Assert.Equal(originalFeed.Agencies.Count, parsedFeed.Agencies.Count);
                Assert.Equal(originalFeed.Stops.Count, parsedFeed.Stops.Count);
                Assert.Equal(originalFeed.Routes.Count, parsedFeed.Routes.Count);
                Assert.Equal(originalFeed.Trips.Count, parsedFeed.Trips.Count);
                Assert.Equal(originalFeed.StopTimes.Count, parsedFeed.StopTimes.Count);
                Assert.Equal(originalFeed.Calendars.Count, parsedFeed.Calendars.Count);

                // Check specific data integrity
                var originalAgency = originalFeed.Agencies.First();
                var parsedAgency = parsedFeed.Agencies.First();
                Assert.Equal(originalAgency.AgencyId, parsedAgency.AgencyId);
                Assert.Equal(originalAgency.AgencyName, parsedAgency.AgencyName);
                Assert.Equal(originalAgency.AgencyUrl, parsedAgency.AgencyUrl);
                Assert.Equal(originalAgency.AgencyTimezone, parsedAgency.AgencyTimezone);

                // Validate the parsed feed
                var errors = parser.ValidateFeed(parsedFeed);
                Assert.Empty(errors);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task FullWorkflow_ParseWriteZipParse_ShouldMaintainDataIntegrity()
        {
            // Arrange
            var parser = new GTFSFeedParser();
            var writer = new GTFSFeedWriter();
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_integration_zip_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            var zipPath = Path.Combine(Path.GetTempPath(), $"gtfs_integration_test_{Guid.NewGuid()}.zip");

            try
            {
                // Create original test data
                var originalFeed = CreateComprehensiveTestFeed();

                // Act 1: Write the feed to directory
                await writer.WriteToDirectoryAsync(originalFeed, tempDir);

                // Act 2: Write the feed to ZIP
                await writer.WriteToZipAsync(originalFeed, zipPath);

                // Act 3: Parse the ZIP file
                var parsedFromZipFeed = await parser.ParseFromZipAsync(zipPath);

                // Assert: Data integrity should be maintained
                Assert.Equal(originalFeed.Agencies.Count, parsedFromZipFeed.Agencies.Count);
                Assert.Equal(originalFeed.Stops.Count, parsedFromZipFeed.Stops.Count);
                Assert.Equal(originalFeed.Routes.Count, parsedFromZipFeed.Routes.Count);
                Assert.Equal(originalFeed.Trips.Count, parsedFromZipFeed.Trips.Count);
                Assert.Equal(originalFeed.StopTimes.Count, parsedFromZipFeed.StopTimes.Count);
                Assert.Equal(originalFeed.Calendars.Count, parsedFromZipFeed.Calendars.Count);

                // Validate the parsed feed
                var errors = parser.ValidateFeed(parsedFromZipFeed);
                Assert.Empty(errors);
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
        public async Task RoundTrip_WithRealSampleData_ShouldWorkCorrectly()
        {
            // Arrange
            var parser = new GTFSFeedParser();
            var writer = new GTFSFeedWriter();
            var samplePath = Path.Combine("..", "sample");
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_sample_roundtrip_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Skip if sample data doesn't exist
                if (!Directory.Exists(samplePath))
                {
                    // Skip test if sample data not available
                    return;
                }

                // Act 1: Parse original sample data
                var originalFeed = await parser.ParseFromDirectoryAsync(samplePath);

                // Act 2: Write to temp directory
                await writer.WriteToDirectoryAsync(originalFeed, tempDir);

                // Act 3: Parse back from temp directory
                var roundTripFeed = await parser.ParseFromDirectoryAsync(tempDir);

                // Assert: Data integrity should be maintained
                Assert.Equal(originalFeed.Agencies.Count, roundTripFeed.Agencies.Count);
                Assert.Equal(originalFeed.Stops.Count, roundTripFeed.Stops.Count);
                Assert.Equal(originalFeed.Routes.Count, roundTripFeed.Routes.Count);
                Assert.Equal(originalFeed.Trips.Count, roundTripFeed.Trips.Count);
                Assert.Equal(originalFeed.StopTimes.Count, roundTripFeed.StopTimes.Count);
                Assert.Equal(originalFeed.Calendars.Count, roundTripFeed.Calendars.Count);

                // Validate the round-trip feed
                var errors = parser.ValidateFeed(roundTripFeed);
                Assert.Empty(errors);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        [Fact]
        public async Task DataTransformation_ShouldPreserveAllFields()
        {
            // Arrange
            var parser = new GTFSFeedParser();
            var writer = new GTFSFeedWriter();
            var tempDir = Path.Combine(Path.GetTempPath(), $"gtfs_transformation_test_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // Create feed with all optional fields populated
                var originalFeed = CreateFeedWithAllFields();

                // Act 1: Write to directory
                await writer.WriteToDirectoryAsync(originalFeed, tempDir);

                // Act 2: Parse back
                var parsedFeed = await parser.ParseFromDirectoryAsync(tempDir);

                // Assert: All fields should be preserved
                Assert.Equal(originalFeed.FareAttributes.Count, parsedFeed.FareAttributes.Count);
                Assert.Equal(originalFeed.FareRules.Count, parsedFeed.FareRules.Count);
                Assert.Equal(originalFeed.Shapes.Count, parsedFeed.Shapes.Count);
                Assert.Equal(originalFeed.Frequencies.Count, parsedFeed.Frequencies.Count);
                Assert.Equal(originalFeed.Transfers.Count, parsedFeed.Transfers.Count);
                Assert.Equal(originalFeed.FeedInfos.Count, parsedFeed.FeedInfos.Count);

                // Check specific field preservation
                var originalFare = originalFeed.FareAttributes.First();
                var parsedFare = parsedFeed.FareAttributes.First();
                Assert.Equal(originalFare.FareId, parsedFare.FareId);
                Assert.Equal(originalFare.Price, parsedFare.Price);
                Assert.Equal(originalFare.CurrencyType, parsedFare.CurrencyType);
            }
            finally
            {
                // Cleanup
                Directory.Delete(tempDir, true);
            }
        }

        private GTFSFeed CreateComprehensiveTestFeed()
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
                        AgencyTimezone = "America/New_York",
                        AgencyLang = "en",
                        AgencyPhone = "555-1234",
                        AgencyFareUrl = "http://test.com/fares",
                        AgencyEmail = "info@test.com"
                    }
                },
                Stops = new List<Stop>
                {
                    new Stop
                    {
                        StopId = "STOP1",
                        StopCode = "S1",
                        StopName = "Test Stop 1",
                        StopDesc = "A test stop",
                        StopLat = 40.7128,
                        StopLon = -74.0060,
                        ZoneId = "ZONE1",
                        StopUrl = "http://test.com/stops/1",
                        LocationType = 0,
                        ParentStation = null,
                        WheelchairBoarding = "1",
                        LevelId = null,
                        PlatformCode = null
                    },
                    new Stop
                    {
                        StopId = "STOP2",
                        StopCode = "S2",
                        StopName = "Test Stop 2",
                        StopDesc = "Another test stop",
                        StopLat = 40.7589,
                        StopLon = -73.9851,
                        ZoneId = "ZONE1",
                        StopUrl = "http://test.com/stops/2",
                        LocationType = 0,
                        ParentStation = null,
                        WheelchairBoarding = "1",
                        LevelId = null,
                        PlatformCode = null
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
                        RouteDesc = "A test route",
                        RouteType = 3,
                        RouteUrl = "http://test.com/routes/1",
                        RouteColor = "FF0000",
                        RouteTextColor = "FFFFFF",
                        RouteSortOrder = 1,
                        ContinuousPickup = "0",
                        ContinuousDropOff = "0",
                        NetworkId = null
                    }
                },
                Trips = new List<Trip>
                {
                    new Trip
                    {
                        TripId = "TRIP1",
                        RouteId = "ROUTE1",
                        ServiceId = "SERVICE1",
                        TripHeadsign = "To Stop 2",
                        TripShortName = "T1",
                        DirectionId = 0,
                        BlockId = "BLOCK1",
                        ShapeId = null,
                        WheelchairAccessible = 1,
                        BikesAllowed = 1
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
                        DepartureTime = "08:00:00",
                        StopHeadsign = null,
                        PickupType = 0,
                        DropOffType = 0,
                        ShapeDistTraveled = null,
                        Timepoint = 1,
                        ContinuousPickup = 0,
                        ContinuousDropOff = 0
                    },
                    new StopTime
                    {
                        TripId = "TRIP1",
                        StopId = "STOP2",
                        StopSequence = 2,
                        ArrivalTime = "08:10:00",
                        DepartureTime = "08:10:00",
                        StopHeadsign = null,
                        PickupType = 0,
                        DropOffType = 0,
                        ShapeDistTraveled = null,
                        Timepoint = 1,
                        ContinuousPickup = 0,
                        ContinuousDropOff = 0
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

        private GTFSFeed CreateFeedWithAllFields()
        {
            var baseFeed = CreateComprehensiveTestFeed();
            
            baseFeed.FareAttributes = new List<FareAttribute>
            {
                new FareAttribute
                {
                    AgencyId = "TEST",
                    FareId = "FARE1",
                    Price = 2.75m,
                    CurrencyType = "USD",
                    PaymentMethod = 1,
                    Transfers = 0,
                    TransferDuration = "3600"
                }
            };

            baseFeed.FareRules = new List<FareRule>
            {
                new FareRule
                {
                    FareId = "FARE1",
                    RouteId = "ROUTE1",
                    OriginId = null,
                    DestinationId = null,
                    ContainsId = null
                }
            };

            baseFeed.Shapes = new List<Shape>
            {
                new Shape
                {
                    ShapeId = "SHAPE1",
                    ShapePtLat = 40.7128,
                    ShapePtLon = -74.0060,
                    ShapePtSequence = 1,
                    ShapeDistTraveled = 0.0
                },
                new Shape
                {
                    ShapeId = "SHAPE1",
                    ShapePtLat = 40.7589,
                    ShapePtLon = -73.9851,
                    ShapePtSequence = 2,
                    ShapeDistTraveled = 5.0
                }
            };

            baseFeed.Frequencies = new List<Frequency>
            {
                new Frequency
                {
                    TripId = "TRIP1",
                    StartTime = "08:00:00",
                    EndTime = "18:00:00",
                    HeadwaySecs = 1800,
                    ExactTimes = 0
                }
            };

            baseFeed.Transfers = new List<Transfer>
            {
                new Transfer
                {
                    FromStopId = "STOP1",
                    ToStopId = "STOP2",
                    TransferType = 2,
                    MinTransferTime = 300,
                    FromRouteId = null,
                    ToRouteId = null,
                    FromTripId = null,
                    ToTripId = null
                }
            };

            baseFeed.FeedInfos = new List<FeedInfo>
            {
                new FeedInfo
                {
                    FeedPublisherName = "Test Publisher",
                    FeedPublisherUrl = "http://test.com",
                    FeedLang = "en",
                    DefaultLang = "en",
                    FeedStartDate = "20240101",
                    FeedEndDate = "20241231",
                    FeedVersion = "1.0",
                    FeedContactEmail = "info@test.com",
                    FeedContactUrl = "http://test.com/contact"
                }
            };

            return baseFeed;
        }
    }
}
