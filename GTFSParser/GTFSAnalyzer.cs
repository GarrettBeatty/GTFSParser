using GTFSParser.Models;

namespace GTFSParser
{
    public class GTFSAnalyzer
    {
        private readonly GTFSFeed _feed;

        public GTFSAnalyzer(GTFSFeed feed)
        {
            _feed = feed;
        }

        /// <summary>
        /// Gets all routes for a specific agency
        /// </summary>
        public List<Route> GetRoutesByAgency(string agencyId)
        {
            return _feed.Routes.Where(r => r.AgencyId == agencyId).ToList();
        }

        /// <summary>
        /// Gets all stops for a specific route
        /// </summary>
        public List<Stop> GetStopsByRoute(string routeId)
        {
            var tripIds = _feed.Trips.Where(t => t.RouteId == routeId).Select(t => t.TripId).ToHashSet();
            var stopIds = _feed.StopTimes
                .Where(st => tripIds.Contains(st.TripId))
                .Select(st => st.StopId)
                .ToHashSet();
            
            return _feed.Stops.Where(s => stopIds.Contains(s.StopId)).ToList();
        }

        /// <summary>
        /// Gets all trips for a specific route
        /// </summary>
        public List<Trip> GetTripsByRoute(string routeId)
        {
            return _feed.Trips.Where(t => t.RouteId == routeId).ToList();
        }

        /// <summary>
        /// Gets the complete itinerary for a specific trip
        /// </summary>
        public List<(Stop Stop, StopTime StopTime)> GetTripItinerary(string tripId)
        {
            var stopTimes = _feed.StopTimes
                .Where(st => st.TripId == tripId)
                .OrderBy(st => st.StopSequence)
                .ToList();

            var result = new List<(Stop Stop, StopTime StopTime)>();
            
            foreach (var stopTime in stopTimes)
            {
                var stop = _feed.Stops.FirstOrDefault(s => s.StopId == stopTime.StopId);
                if (stop != null)
                {
                    result.Add((stop, stopTime));
                }
            }

            return result;
        }

        /// <summary>
        /// Gets all stops within a certain distance from a point
        /// </summary>
        public List<Stop> GetStopsNearLocation(double lat, double lon, double maxDistanceKm)
        {
            var stopsWithCoordinates = _feed.Stops
                .Where(s => s.StopLat.HasValue && s.StopLon.HasValue)
                .Select(s => new { Stop = s, Lat = s.StopLat!.Value, Lon = s.StopLon!.Value })
                .Where(s => CalculateDistance(lat, lon, s.Lat, s.Lon) <= maxDistanceKm)
                .Select(s => s.Stop)
                .ToList();
            
            return stopsWithCoordinates;
        }

        /// <summary>
        /// Gets routes that serve a specific stop
        /// </summary>
        public List<Route> GetRoutesByStop(string stopId)
        {
            var tripIds = _feed.StopTimes
                .Where(st => st.StopId == stopId)
                .Select(st => st.TripId)
                .ToHashSet();
            
            var routeIds = _feed.Trips
                .Where(t => tripIds.Contains(t.TripId))
                .Select(t => t.RouteId)
                .ToHashSet();
            
            return _feed.Routes.Where(r => routeIds.Contains(r.RouteId)).ToList();
        }

        /// <summary>
        /// Gets service information for a specific date
        /// </summary>
        public List<string> GetActiveServices(DateTime date)
        {
            var dayOfWeek = date.DayOfWeek;
            var dateString = date.ToString("yyyyMMdd");
            
            var activeServices = new List<string>();
            
            // Check calendar.txt for regular service
            foreach (var calendar in _feed.Calendars)
            {
                bool isActive = dayOfWeek switch
                {
                    DayOfWeek.Monday => calendar.Monday ?? false,
                    DayOfWeek.Tuesday => calendar.Tuesday ?? false,
                    DayOfWeek.Wednesday => calendar.Wednesday ?? false,
                    DayOfWeek.Thursday => calendar.Thursday ?? false,
                    DayOfWeek.Friday => calendar.Friday ?? false,
                    DayOfWeek.Saturday => calendar.Saturday ?? false,
                    DayOfWeek.Sunday => calendar.Sunday ?? false,
                    _ => false
                };
                
                if (isActive && 
                    calendar.StartDate != null && 
                    calendar.EndDate != null &&
                    calendar.ServiceId != null &&
                    string.Compare(dateString, calendar.StartDate) >= 0 &&
                    string.Compare(dateString, calendar.EndDate) <= 0)
                {
                    activeServices.Add(calendar.ServiceId);
                }
            }
            
            // Check calendar_dates.txt for exceptions
            var exceptions = _feed.CalendarDates
                .Where(cd => cd.Date == dateString)
                .ToList();
            
            foreach (var exception in exceptions)
            {
                if (exception.ServiceId != null)
                {
                    if (exception.ExceptionType == 1) // Service added
                    {
                        if (!activeServices.Contains(exception.ServiceId))
                            activeServices.Add(exception.ServiceId);
                    }
                    else if (exception.ExceptionType == 2) // Service removed
                    {
                        activeServices.Remove(exception.ServiceId);
                    }
                }
            }
            
            return activeServices;
        }

        /// <summary>
        /// Gets fare information for a specific route
        /// </summary>
        public List<FareAttribute> GetFaresByRoute(string routeId)
        {
            var fareIds = _feed.FareRules
                .Where(fr => fr.RouteId == routeId)
                .Select(fr => fr.FareId)
                .ToHashSet();
            
            return _feed.FareAttributes
                .Where(fa => fareIds.Contains(fa.FareId))
                .ToList();
        }

        /// <summary>
        /// Calculates the distance between two coordinates using the Haversine formula
        /// </summary>
        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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

        private static double ToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        /// <summary>
        /// Gets a summary report of the GTFS feed
        /// </summary>
        public string GetFeedSummary()
        {
            var summary = new System.Text.StringBuilder();
            
            summary.AppendLine("GTFS Feed Summary Report");
            summary.AppendLine("=========================");
            summary.AppendLine();
            
            // Agency information
            summary.AppendLine("Agencies:");
            foreach (var agency in _feed.Agencies)
            {
                summary.AppendLine($"  - {agency.AgencyName} ({agency.AgencyId})");
                summary.AppendLine($"    URL: {agency.AgencyUrl}");
                summary.AppendLine($"    Timezone: {agency.AgencyTimezone}");
                summary.AppendLine();
            }
            
            // Route information
            summary.AppendLine($"Routes: {_feed.Routes.Count}");
            summary.AppendLine($"Stops: {_feed.Stops.Count}");
            summary.AppendLine($"Trips: {_feed.Trips.Count}");
            summary.AppendLine($"Stop Times: {_feed.StopTimes.Count}");
            summary.AppendLine();
            
            // Service coverage
            var serviceIds = _feed.Calendars.Select(c => c.ServiceId).ToHashSet();
            summary.AppendLine($"Service IDs: {serviceIds.Count}");
            foreach (var serviceId in serviceIds)
            {
                var calendar = _feed.Calendars.FirstOrDefault(c => c.ServiceId == serviceId);
                if (calendar != null)
                {
                    summary.AppendLine($"  - {serviceId}: {calendar.StartDate} to {calendar.EndDate}");
                }
            }
            
            return summary.ToString();
        }
    }
}
