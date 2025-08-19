using Xunit;
using GTFSParser;
using GTFSParser.Models;

namespace GTFSParser.Tests
{
    public class GTFSAnalyzerTests
    {
        private readonly GTFSFeed _testFeed;
        private readonly GTFSAnalyzer _analyzer;

        public GTFSAnalyzerTests()
        {
            _testFeed = CreateTestFeed();
            _analyzer = new GTFSAnalyzer(_testFeed);
        }

        [Fact]
        public void GetRoutesByAgency_WithValidAgencyId_ShouldReturnCorrectRoutes()
        {
            // Act
            var routes = _analyzer.GetRoutesByAgency("AGENCY1");

            // Assert
            Assert.Equal(2, routes.Count);
            Assert.All(routes, r => Assert.Equal("AGENCY1", r.AgencyId));
        }

        [Fact]
        public void GetRoutesByAgency_WithInvalidAgencyId_ShouldReturnEmptyList()
        {
            // Act
            var routes = _analyzer.GetRoutesByAgency("INVALID");

            // Assert
            Assert.Empty(routes);
        }

        [Fact]
        public void GetStopsByRoute_WithValidRouteId_ShouldReturnCorrectStops()
        {
            // Act
            var stops = _analyzer.GetStopsByRoute("ROUTE1");

            // Assert
            Assert.Equal(2, stops.Count);
            Assert.Contains(stops, s => s.StopId == "STOP1");
            Assert.Contains(stops, s => s.StopId == "STOP2");
        }

        [Fact]
        public void GetStopsByRoute_WithInvalidRouteId_ShouldReturnEmptyList()
        {
            // Act
            var stops = _analyzer.GetStopsByRoute("INVALID");

            // Assert
            Assert.Empty(stops);
        }

        [Fact]
        public void GetTripsByRoute_WithValidRouteId_ShouldReturnCorrectTrips()
        {
            // Act
            var trips = _analyzer.GetTripsByRoute("ROUTE1");

            // Assert
            Assert.Equal(2, trips.Count);
            Assert.All(trips, t => Assert.Equal("ROUTE1", t.RouteId));
        }

        [Fact]
        public void GetTripItinerary_WithValidTripId_ShouldReturnCorrectItinerary()
        {
            // Act
            var itinerary = _analyzer.GetTripItinerary("TRIP1");

            // Assert
            Assert.Equal(2, itinerary.Count);
            Assert.Equal("STOP1", itinerary[0].Stop.StopId);
            Assert.Equal("STOP2", itinerary[1].Stop.StopId);
            Assert.Equal(1, itinerary[0].StopTime.StopSequence);
            Assert.Equal(2, itinerary[1].StopTime.StopSequence);
        }

        [Fact]
        public void GetStopsNearLocation_WithinRange_ShouldReturnCorrectStops()
        {
            // Act - Search near STOP1 (40.7128, -74.0060)
            var nearbyStops = _analyzer.GetStopsNearLocation(40.7128, -74.0060, 1.0);

            // Assert
            Assert.Single(nearbyStops);
            Assert.Equal("STOP1", nearbyStops[0].StopId);
        }

        [Fact]
        public void GetStopsNearLocation_OutsideRange_ShouldReturnEmptyList()
        {
            // Act - Search far from any stop
            var nearbyStops = _analyzer.GetStopsNearLocation(0.0, 0.0, 1.0);

            // Assert
            Assert.Empty(nearbyStops);
        }

        [Fact]
        public void GetRoutesByStop_WithValidStopId_ShouldReturnCorrectRoutes()
        {
            // Act
            var routes = _analyzer.GetRoutesByStop("STOP1");

            // Assert
            Assert.Equal(2, routes.Count);
            Assert.Contains(routes, r => r.RouteId == "ROUTE1");
            Assert.Contains(routes, r => r.RouteId == "ROUTE2");
        }

        [Fact]
        public void GetActiveServices_OnWeekday_ShouldReturnCorrectServices()
        {
            // Arrange - Monday
            var monday = new DateTime(2024, 1, 1); // Monday

            // Act
            var activeServices = _analyzer.GetActiveServices(monday);

            // Assert
            Assert.Single(activeServices);
            Assert.Contains("SERVICE1", activeServices);
        }

        [Fact]
        public void GetActiveServices_OnWeekend_ShouldReturnCorrectServices()
        {
            // Arrange - Saturday
            var saturday = new DateTime(2024, 1, 6); // Saturday

            // Act
            var activeServices = _analyzer.GetActiveServices(saturday);

            // Assert
            Assert.Single(activeServices);
            Assert.Contains("SERVICE2", activeServices);
        }

        [Fact]
        public void GetActiveServices_WithException_ShouldHandleCorrectly()
        {
            // Arrange - Add a service exception
            _testFeed.CalendarDates.Add(new CalendarDate
            {
                ServiceId = "SERVICE1",
                Date = "20240101",
                ExceptionType = 2 // Service removed
            });

            var monday = new DateTime(2024, 1, 1);

            // Act
            var activeServices = _analyzer.GetActiveServices(monday);

            // Assert
            Assert.Empty(activeServices); // Service should be removed
        }

        [Fact]
        public void GetFaresByRoute_WithValidRouteId_ShouldReturnCorrectFares()
        {
            // Act
            var fares = _analyzer.GetFaresByRoute("ROUTE1");

            // Assert
            Assert.Single(fares);
            Assert.Equal("FARE1", fares[0].FareId);
        }

        [Fact]
        public void GetFaresByRoute_WithInvalidRouteId_ShouldReturnEmptyList()
        {
            // Act
            var fares = _analyzer.GetFaresByRoute("INVALID");

            // Assert
            Assert.Empty(fares);
        }

        [Fact]
        public void GetFeedSummary_ShouldReturnFormattedSummary()
        {
            // Act
            var summary = _analyzer.GetFeedSummary();

            // Assert
            Assert.NotNull(summary);
            Assert.Contains("GTFS Feed Summary Report", summary);
            Assert.Contains("Agencies:", summary);
            Assert.Contains("AGENCY1", summary);
            Assert.Contains("Routes:", summary);
            Assert.Contains("Stops:", summary);
            Assert.Contains("Trips:", summary);
        }

        [Fact]
        public void CalculateDistance_WithValidCoordinates_ShouldReturnCorrectDistance()
        {
            // Arrange - Two points in NYC area
            var lat1 = 40.7128; // NYC
            var lon1 = -74.0060;
            var lat2 = 40.7589; // Times Square
            var lon2 = -73.9851;

            // Act
            var distance = CalculateDistance(lat1, lon1, lat2, lon2);

            // Assert
            Assert.True(distance > 0);
            Assert.True(distance < 10); // Should be less than 10 km
        }

        private GTFSFeed CreateTestFeed()
        {
            return new GTFSFeed
            {
                Agencies = new List<Agency>
                {
                    new Agency
                    {
                        AgencyId = "AGENCY1",
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
                    },
                    new Stop
                    {
                        StopId = "STOP2",
                        StopName = "Test Stop 2",
                        StopLat = 40.7589,
                        StopLon = -73.9851
                    }
                },
                Routes = new List<Route>
                {
                    new Route
                    {
                        RouteId = "ROUTE1",
                        AgencyId = "AGENCY1",
                        RouteShortName = "1",
                        RouteLongName = "Test Route 1",
                        RouteType = 3
                    },
                    new Route
                    {
                        RouteId = "ROUTE2",
                        AgencyId = "AGENCY1",
                        RouteShortName = "2",
                        RouteLongName = "Test Route 2",
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
                    },
                    new Trip
                    {
                        TripId = "TRIP2",
                        RouteId = "ROUTE1",
                        ServiceId = "SERVICE1"
                    },
                    new Trip
                    {
                        TripId = "TRIP3",
                        RouteId = "ROUTE2",
                        ServiceId = "SERVICE2"
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
                    },
                    new StopTime
                    {
                        TripId = "TRIP1",
                        StopId = "STOP2",
                        StopSequence = 2,
                        ArrivalTime = "08:10:00",
                        DepartureTime = "08:10:00"
                    },
                    new StopTime
                    {
                        TripId = "TRIP2",
                        StopId = "STOP1",
                        StopSequence = 1,
                        ArrivalTime = "09:00:00",
                        DepartureTime = "09:00:00"
                    },
                    new StopTime
                    {
                        TripId = "TRIP2",
                        StopId = "STOP2",
                        StopSequence = 2,
                        ArrivalTime = "09:10:00",
                        DepartureTime = "09:10:00"
                    },
                    new StopTime
                    {
                        TripId = "TRIP3",
                        StopId = "STOP1",
                        StopSequence = 1,
                        ArrivalTime = "10:00:00",
                        DepartureTime = "10:00:00"
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
                    },
                    new GTFSParser.Models.Calendar
                    {
                        ServiceId = "SERVICE2",
                        Monday = false,
                        Tuesday = false,
                        Wednesday = false,
                        Thursday = false,
                        Friday = false,
                        Saturday = true,
                        Sunday = true,
                        StartDate = "20240101",
                        EndDate = "20241231"
                    }
                },
                FareAttributes = new List<FareAttribute>
                {
                    new FareAttribute
                    {
                        FareId = "FARE1",
                        Price = 2.75m,
                        CurrencyType = "USD",
                        PaymentMethod = 1,
                        Transfers = 0
                    }
                },
                FareRules = new List<FareRule>
                {
                    new FareRule
                    {
                        FareId = "FARE1",
                        RouteId = "ROUTE1"
                    }
                }
            };
        }

        // Helper method to test distance calculation
        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double earthRadius = 6371; // Earth's radius in kilometers
            
            var dLat = ToRadians(lat2 - lat1);
            var dLon = ToRadians(lon2 - lon1);
            
            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            
            return earthRadius * c;
        }

        private double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }
    }
}
