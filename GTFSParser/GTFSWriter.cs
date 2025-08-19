using System.Globalization;
using System.IO.Compression;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace GTFSParser
{
    public class GTFSFeedWriter
    {
        private readonly CsvConfiguration _csvConfig;

        public GTFSFeedWriter()
        {
            _csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                TrimOptions = TrimOptions.Trim,
            };
        }

        public async Task WriteToDirectoryAsync(GTFSFeed feed, string directoryPath, bool overwrite = true)
        {
            Directory.CreateDirectory(directoryPath);

            // Helper local function
            async Task WriteFileAsync<T>(string fileName, IEnumerable<T> records)
            {
                var list = records?.ToList() ?? new List<T>();
                if (list.Count == 0) return; // skip empty files

                var filePath = Path.Combine(directoryPath, fileName);
                if (File.Exists(filePath) && !overwrite) return;

                using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var writer = new StreamWriter(stream, new UTF8Encoding(false));
                using var csv = new CsvWriter(writer, _csvConfig);
                await csv.WriteRecordsAsync(list);
                await writer.FlushAsync();
            }

            // Required + optional files, only write when there is data
            await WriteFileAsync("agency.txt", feed.Agencies);
            await WriteFileAsync("stops.txt", feed.Stops);
            await WriteFileAsync("routes.txt", feed.Routes);
            await WriteFileAsync("trips.txt", feed.Trips);
            await WriteFileAsync("stop_times.txt", feed.StopTimes);
            await WriteFileAsync("calendar.txt", feed.Calendars);
            await WriteFileAsync("calendar_dates.txt", feed.CalendarDates);

            await WriteFileAsync("fare_attributes.txt", feed.FareAttributes);
            await WriteFileAsync("fare_rules.txt", feed.FareRules);
            await WriteFileAsync("shapes.txt", feed.Shapes);
            await WriteFileAsync("frequencies.txt", feed.Frequencies);
            await WriteFileAsync("transfers.txt", feed.Transfers);
            await WriteFileAsync("feed_info.txt", feed.FeedInfos);
        }

        public async Task WriteToZipAsync(GTFSFeed feed, string zipFilePath, bool overwrite = true)
        {
            if (File.Exists(zipFilePath))
            {
                if (!overwrite) return;
                File.Delete(zipFilePath);
            }

            using var archive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create);

            async Task WriteEntryAsync<T>(string entryName, IEnumerable<T> records)
            {
                var list = records?.ToList() ?? new List<T>();
                if (list.Count == 0) return;

                var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                await using var entryStream = entry.Open();
                await using var writer = new StreamWriter(entryStream, new UTF8Encoding(false));
                using var csv = new CsvWriter(writer, _csvConfig);
                await csv.WriteRecordsAsync(list);
                await writer.FlushAsync();
            }

            await WriteEntryAsync("agency.txt", feed.Agencies);
            await WriteEntryAsync("stops.txt", feed.Stops);
            await WriteEntryAsync("routes.txt", feed.Routes);
            await WriteEntryAsync("trips.txt", feed.Trips);
            await WriteEntryAsync("stop_times.txt", feed.StopTimes);
            await WriteEntryAsync("calendar.txt", feed.Calendars);
            await WriteEntryAsync("calendar_dates.txt", feed.CalendarDates);

            await WriteEntryAsync("fare_attributes.txt", feed.FareAttributes);
            await WriteEntryAsync("fare_rules.txt", feed.FareRules);
            await WriteEntryAsync("shapes.txt", feed.Shapes);
            await WriteEntryAsync("frequencies.txt", feed.Frequencies);
            await WriteEntryAsync("transfers.txt", feed.Transfers);
            await WriteEntryAsync("feed_info.txt", feed.FeedInfos);
        }
    }
}
