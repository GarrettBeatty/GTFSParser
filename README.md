# GTFS Parser

A comprehensive .NET library for parsing GTFS (General Transit Feed Specification) static data feeds. This parser supports both ZIP file and directory-based GTFS feeds, with full validation and comprehensive data models.

## Features

- **Complete GTFS Support**: Parses all standard GTFS files including required and optional files
- **Multiple Input Formats**: Supports both ZIP files and individual CSV files
- **Data Validation**: Built-in validation for GTFS feed integrity and required fields
- **Comprehensive Models**: Full C# models for all GTFS entities
- **Error Handling**: Robust error handling with detailed error reporting
- **Statistics**: Built-in feed statistics and data analysis

## Supported GTFS Files

### Required Files
- `agency.txt` - Transit agencies
- `stops.txt` - Transit stops/stations
- `routes.txt` - Transit routes
- `trips.txt` - Trips for each route
- `stop_times.txt` - Times that vehicles arrive/depart at stops
- `calendar.txt` - Service dates using weekly schedule
- `calendar_dates.txt` - Service dates using exceptions

### Optional Files
- `fare_attributes.txt` - Fare information
- `fare_rules.txt` - Rules for applying fare information
- `shapes.txt` - Geographic shapes for routes
- `frequencies.txt` - Headway-based service
- `transfers.txt` - Transfer rules between stops
- `feed_info.txt` - Feed metadata
- `levels.txt` - Building levels
- `pathways.txt` - Station pathways
- `translations.txt` - Multi-language support

## Getting Started

### Prerequisites
- .NET 10.0 or later
- Visual Studio 2022 or VS Code

### Installation

1. Clone the repository:
   ```bash
   git clone <repository-url>
   cd GTFS
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

3. Run the project:
   ```bash
   dotnet run --project GTFSParser
   ```

## Usage

### Basic Usage

```csharp
using GTFSParser;

// Create parser instance
var parser = new GTFSFeedParser();

// Parse from ZIP file
var feed = await parser.ParseFromZipAsync("path/to/gtfs.zip");

// Parse from directory containing CSV files
var feed = await parser.ParseFromDirectoryAsync("path/to/gtfs/files");

// Validate the feed
var errors = parser.ValidateFeed(feed);
if (errors.Any())
{
    foreach (var error in errors)
    {
        Console.WriteLine($"Validation error: {error}");
    }
}

// Get feed statistics
var stats = parser.GetFeedStatistics(feed);
foreach (var stat in stats)
{
    Console.WriteLine($"{stat.Key}: {stat.Value}");
}
```

### Working with Parsed Data

```csharp
// Access agencies
foreach (var agency in feed.Agencies)
{
    Console.WriteLine($"Agency: {agency.AgencyName} ({agency.AgencyId})");
    Console.WriteLine($"  URL: {agency.AgencyUrl}");
    Console.WriteLine($"  Timezone: {agency.AgencyTimezone}");
}

// Access routes
foreach (var route in feed.Routes)
{
    Console.WriteLine($"Route: {route.RouteShortName} - {route.RouteLongName}");
    Console.WriteLine($"  Type: {route.RouteType}");
}

// Access stops
foreach (var stop in feed.Stops)
{
    Console.WriteLine($"Stop: {stop.StopName} ({stop.StopId})");
    Console.WriteLine($"  Location: {stop.StopLat}, {stop.StopLon}");
}

// Access trips and stop times
foreach (var trip in feed.Trips)
{
    Console.WriteLine($"Trip: {trip.TripId} on route {trip.RouteId}");
    
    var stopTimes = feed.StopTimes
        .Where(st => st.TripId == trip.TripId)
        .OrderBy(st => st.StopSequence);
    
    foreach (var stopTime in stopTimes)
    {
        var stop = feed.Stops.First(s => s.StopId == stopTime.StopId);
        Console.WriteLine($"  {stop.StopName}: {stopTime.ArrivalTime} - {stopTime.DepartureTime}");
    }
}
```

## Data Models

All GTFS entities are represented as C# classes with nullable properties to handle optional fields:

- `Agency` - Transit agency information
- `Route` - Transit route information
- `Stop` - Transit stop/station information
- `Trip` - Trip information for routes
- `StopTime` - Arrival/departure times at stops
- `Calendar` - Service schedule information
- `CalendarDate` - Service date exceptions
- `FareAttribute` - Fare information
- `FareRule` - Fare application rules
- `Shape` - Geographic route shapes
- `Frequency` - Headway-based service
- `Transfer` - Transfer rules
- `FeedInfo` - Feed metadata

## Validation

The parser includes comprehensive validation that checks:

- Required files are present
- Required fields contain data
- Referential integrity (e.g., trips reference valid routes)
- Data consistency across files

## Error Handling

The parser gracefully handles:
- Missing or malformed CSV files
- Invalid data formats
- Missing required fields
- Referential integrity issues

Errors are logged and reported without stopping the parsing process.

## Sample Data

The project includes sample GTFS data files in the `sample/` directory for testing and development purposes.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## License

This project is licensed under the MIT License.

## References

- [GTFS Specification](https://gtfs.org/schedule/reference/)
- [GTFS Overview](https://gtfs.org/documentation/overview/)
- [GTFS Best Practices](https://gtfs.org/schedule/best-practices/)

## Support

For issues, questions, or contributions, please use the GitHub issue tracker or submit a pull request.
