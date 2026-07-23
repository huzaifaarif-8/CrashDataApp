using CrashDataApp.Data;
using CrashDataApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrashDataApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrashesController : ControllerBase
{
    private readonly CrashContext _context;

    public CrashesController(CrashContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var total = await _context.Crashes.CountAsync();
        var items = await _context.Crashes
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var crash = await _context.Crashes.FindAsync(id);
        return crash is null ? NotFound() : Ok(crash);
    }

    [HttpGet("summary")]
    public IActionResult GetSummary()
    {
        var total = _context.Crashes.Count();
        var totalAboard = _context.Crashes.Sum(c => c.Aboard ?? 0);
        var totalFatalities = _context.Crashes.Sum(c => c.Fatalities ?? 0);
        var fatalityRate = totalAboard == 0 ? 0 : Math.Round(100.0 * totalFatalities / totalAboard, 2);

        return Ok(new
        {
            totalCrashes = total,
            totalAboard,
            totalFatalities,
            fatalityRatePct = fatalityRate
        });
    }

    [HttpGet("by-decade")]
    public IActionResult GetByDecade()
    {
        var result = _context.Crashes
            .Where(c => c.Year != null)
            .AsEnumerable()
            .GroupBy(c => (c.Year!.Value / 10) * 10)
            .Select(g => new
            {
                decade = g.Key,
                crashes = g.Count(),
                fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderBy(x => x.decade);

        return Ok(result);
    }

    [HttpGet("top-operators")]
    public IActionResult GetTopOperators([FromQuery] int top = 10)
    {
        var result = _context.Crashes
            .Where(c => c.Operator != null)
            .GroupBy(c => c.Operator)
            .Select(g => new
            {
                @operator = g.Key,
                crashes = g.Count(),
                fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.fatalities)
            .Take(top);

        return Ok(result);
    }

    [HttpGet("deadliest-per-decade")]
    public IActionResult GetDeadliestPerDecade()
    {
        var result = _context.Crashes
            .Where(c => c.Year != null && c.Fatalities != null)
            .AsEnumerable()
            .GroupBy(c => (c.Year!.Value / 10) * 10)
            .Select(g => g.OrderByDescending(c => c.Fatalities).First())
            .OrderBy(c => c.Year)
            .Select(c => new
            {
                decade = (c.Year!.Value / 10) * 10,
                date = c.Date,
                location = c.Location,
                @operator = c.Operator,
                fatalities = c.Fatalities
            });

        return Ok(result);
    }

    [HttpGet("top-aircraft-types")]
    public IActionResult GetTopAircraftTypes([FromQuery] int top = 8)
    {
        var result = _context.Crashes
            .Where(c => c.AircraftType != null)
            .GroupBy(c => c.AircraftType)
            .Select(g => new
            {
                aircraftType = g.Key,
                crashes = g.Count(),
                fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.crashes)
            .Take(top);

        return Ok(result);
    }

    [HttpGet("military-vs-civilian")]
    public IActionResult GetMilitaryVsCivilian()
    {
        var result = _context.Crashes
            .AsEnumerable()
            .GroupBy(c => c.Operator != null && c.Operator.StartsWith("Military") ? "Military" : "Civilian/Other")
            .Select(g => new
            {
                category = g.Key,
                crashes = g.Count(),
                fatalities = g.Sum(c => c.Fatalities ?? 0),
                avgFatalitiesPerCrash = Math.Round(g.Average(c => c.Fatalities ?? 0), 2)
            });

        return Ok(result);
    }

    [HttpGet("engine-failure")]
    public IActionResult GetEngineFailureYears([FromQuery] int top = 10)
    {
        var result = _context.Crashes
            .Where(c => c.Year != null && c.Summary != null && c.Summary.Contains("engine failure"))
            .GroupBy(c => c.Year)
            .Select(g => new { year = g.Key, count = g.Count() })
            .OrderByDescending(x => x.count)
            .Take(top);

        return Ok(result);
    }

    [HttpGet("cumulative-fatalities")]
    public IActionResult GetCumulativeFatalities([FromQuery] int lastYears = 10)
    {
        var yearly = _context.Crashes
            .Where(c => c.Year != null)
            .GroupBy(c => c.Year!.Value)
            .Select(g => new { year = g.Key, fatalities = g.Sum(c => c.Fatalities ?? 0) })
            .OrderBy(x => x.year)
            .ToList();

        long running = 0;
        var withCumulative = yearly
            .Select(y =>
            {
                running += y.fatalities;
                return new { y.year, y.fatalities, cumulativeFatalities = running };
            })
            .OrderByDescending(x => x.year)
            .Take(lastYears);

        return Ok(withCumulative);
    }

    [HttpGet("year-over-year")]
    public IActionResult GetYearOverYear([FromQuery] int lastYears = 10)
    {
        var yearly = _context.Crashes
            .Where(c => c.Year != null)
            .GroupBy(c => c.Year!.Value)
            .Select(g => new { year = g.Key, crashes = g.Count() })
            .OrderBy(x => x.year)
            .ToList();

        var result = yearly
            .Select((y, i) => new
            {
                y.year,
                y.crashes,
                previousYearCrashes = i > 0 ? yearly[i - 1].crashes : (int?)null,
                pctChange = i > 0 && yearly[i - 1].crashes != 0
                    ? Math.Round(100.0 * (y.crashes - yearly[i - 1].crashes) / yearly[i - 1].crashes, 1)
                    : (double?)null
            })
            .OrderByDescending(x => x.year)
            .Take(lastYears);

        return Ok(result);
    }

    [HttpGet("top-regions")]
    public IActionResult GetTopRegions([FromQuery] int top = 10)
    {
        var result = _context.Crashes
            .Where(c => c.Location != null)
            .AsEnumerable()
            .Where(c => c.Location!.Contains(','))
            .GroupBy(c => c.Location!.Substring(c.Location.LastIndexOf(',') + 1).Trim())
            .Select(g => new
            {
                region = g.Key,
                crashes = g.Count(),
                fatalities = g.Sum(c => c.Fatalities ?? 0)
            })
            .OrderByDescending(x => x.fatalities)
            .Take(top);

        return Ok(result);
    }
}
