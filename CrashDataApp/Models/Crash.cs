using System.ComponentModel.DataAnnotations;

namespace CrashDataApp.Models;

public class Crash
{
    [Key]
    public int Id { get; set; }

    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Location { get; set; }
    public string? Operator { get; set; }
    public string? FlightNumber { get; set; }
    public string? Route { get; set; }
    public string? AircraftType { get; set; }
    public string? Registration { get; set; }
    public string? CnIn { get; set; }

    public int? Aboard { get; set; }
    public int? Fatalities { get; set; }
    public int? Ground { get; set; }

    public string? Summary { get; set; }

    public int? Year { get; set; }
}
