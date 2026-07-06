using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventLogController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Number of rows returned for the default "recent" range
        private const int RecentCount = 10;

        // Safety cap so the "all" range can never return an unbounded result set
        private const int MaxRows = 500;

        public EventLogController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/eventlog?range=recent|day|week|all
        // recent (default) = latest 10 entries, day = last 24h, week = last 7 days, all = everything (capped)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventLogDto>>> GetEvents([FromQuery] string range = "recent")
        {
            // Normalise so any unexpected value falls back to the safe default
            range = (range ?? "recent").ToLowerInvariant();
            if (range != "day" && range != "week" && range != "all")
                range = "recent";

            IQueryable<Models.EventLog> query = _context.EventLog;

            var now = DateTime.UtcNow;
            switch (range)
            {
                case "day":
                    query = query.Where(e => e.CreatedAt >= now.AddDays(-1));
                    break;
                case "week":
                    query = query.Where(e => e.CreatedAt >= now.AddDays(-7));
                    break;
                case "all":
                    // no time filter — capped by MaxRows below
                    break;
            }

            var take = range == "recent" ? RecentCount : MaxRows;

            var events = await query
                .OrderByDescending(e => e.CreatedAt)
                .Take(take)
                .Select(e => new EventLogDto
                {
                    Id = e.Id,
                    EventType = e.EventType,
                    PlantId = e.PlantId,
                    PlantName = e.PlantName,
                    DeviceId = e.DeviceId,
                    TriggeredBy = e.TriggeredBy,
                    MoistureAtTime = e.MoistureAtTime,
                    DurationSeconds = e.DurationSeconds,
                    Notes = e.Notes,
                    CreatedAt = e.CreatedAt
                })
                .ToListAsync();

            return Ok(events);
        }
    }
}
