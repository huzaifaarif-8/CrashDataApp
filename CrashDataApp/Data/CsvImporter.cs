using System.Globalization;
using CrashDataApp.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace CrashDataApp.Data;

public static class CsvImporter
{
    private const string CsvFileName = "Data/Airplane_Crashes_and_Fatalities_Since_1908.csv";

    public static void SeedIfEmpty(CrashContext context)
    {
        context.Database.EnsureCreated();

        if (context.Crashes.Any())
        {
            return;
        }

        if (!File.Exists(CsvFileName))
        {
            Console.WriteLine($"Warning: {CsvFileName} not found. Skipping seed.");
            return;
        }

        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null,
        };

        using var reader = new StreamReader(CsvFileName);
        using var csv = new CsvReader(reader, config);

        csv.Read();
        csv.ReadHeader();

        var batch = new List<Crash>();
        int id = 1;

        while (csv.Read())
        {
            var dateStr = csv.GetField("Date");
            int? year = null;
            if (DateTime.TryParseExact(dateStr, "M/d/yyyy", CultureInfo.InvariantCulture,
                    DateTimeStyles.None, out var parsedDate))
            {
                year = parsedDate.Year;
            }

            var crash = new Crash
            {
                Id = id++,
                Date = dateStr,
                Time = csv.GetField("Time"),
                Location = csv.GetField("Location"),
                Operator = csv.GetField("Operator"),
                FlightNumber = csv.GetField("Flight #"),
                Route = csv.GetField("Route"),
                AircraftType = csv.GetField("Type"),
                Registration = csv.GetField("Registration"),
                CnIn = csv.GetField("cn/In"),
                Aboard = ParseNullableInt(csv.GetField("Aboard")),
                Fatalities = ParseNullableInt(csv.GetField("Fatalities")),
                Ground = ParseNullableInt(csv.GetField("Ground")),
                Summary = csv.GetField("Summary"),
                Year = year,
            };

            batch.Add(crash);

            if (batch.Count >= 500)
            {
                context.Crashes.AddRange(batch);
                context.SaveChanges();
                batch.Clear();
            }
        }

        if (batch.Count > 0)
        {
            context.Crashes.AddRange(batch);
            context.SaveChanges();
        }

        Console.WriteLine($"Seeded {context.Crashes.Count()} rows into the database.");
    }

    private static int? ParseNullableInt(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        // source file stores these as floats like "12.0"
        if (double.TryParse(raw, NumberStyles.Any, CultureInfo.InvariantCulture, out var d))
        {
            return (int)d;
        }
        return null;
    }
}
