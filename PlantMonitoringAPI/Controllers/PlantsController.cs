using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlantsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PlantsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlantDto>> GetPlant(int id)
        {
            var plantDto = await _context.Plants
                .Where(p => p.Id == id)
                .Select(p => new PlantDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    MoistureThreshold = p.MoistureThreshold,
                    GroupName = p.Group != null ? p.Group.Name : "No Group",
                    CurrentMoisture = p.SensorReadings
                                       .OrderByDescending(r => r.MeasuredAt)
                                       .Select(r => (double?)r.MoistureValue)
                                       .FirstOrDefault(),
                    LastUpdate = p.SensorReadings
                                  .OrderByDescending(r => r.MeasuredAt)
                                  .Select(r => (DateTime?)r.MeasuredAt)
                                  .FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (plantDto == null)
                return NotFound(new { message = $"Plant with ID {id} not found." });

            return Ok(plantDto);
        }

        // GET: api/plants
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PlantDto>>> GetPlants()
        {
            var plants = await _context.Plants
                .Select(p => new PlantDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    CreatedAt = p.CreatedAt,
                    MoistureThreshold = p.MoistureThreshold,
                    GroupName = p.Group != null ? p.Group.Name : "No Group",
                    SensorName = p.ActiveDevice != null ? p.ActiveDevice.Name : "No Sensor Assigned",
                    SensorId = p.ActiveDevice != null ? p.ActiveDevice.Id.ToString() : null,
                    CurrentMoisture = p.SensorReadings
                                       .OrderByDescending(r => r.MeasuredAt)
                                       .Select(r => (double?)r.MoistureValue)
                                       .FirstOrDefault(),
                    LastUpdate = p.SensorReadings
                                  .OrderByDescending(r => r.MeasuredAt)
                                  .Select(r => (DateTime?)r.MeasuredAt)
                                  .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(plants);
        }

        // POST: api/plants
        [HttpPost]
        public async Task<IActionResult> PostPlant([FromBody] CreatePlantDto request)
        {
            // GroupId is nullable so only validate if provided
            if (request.GroupId.HasValue)
            {
                var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId.Value);
                if (!groupExists)
                    return BadRequest(new { message = $"Group with ID {request.GroupId} does not exist." });
            }

            var plant = new Plant
            {
                Name = request.Name,
                Description = request.Description,
                GroupId = request.GroupId,
                MoistureThreshold = request.MoistureThreshold,
                CreatedAt = DateTime.UtcNow
            };

            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();

            var group = plant.GroupId.HasValue
                ? await _context.Groups.FindAsync(plant.GroupId.Value)
                : null;

            var responseDto = new PlantDto
            {
                Id = plant.Id,
                Name = plant.Name,
                Description = plant.Description,
                CreatedAt = plant.CreatedAt,
                MoistureThreshold = plant.MoistureThreshold,
                GroupName = group?.Name ?? "No Group"
            };

            return CreatedAtAction(nameof(GetPlant), new { id = plant.Id }, responseDto);
        }

        // PUT: api/plants/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlant(int id, [FromBody] UpdatePlantDto request)
        {
            if (id != request.Id)
                return BadRequest(new { message = "ID mismatch between URL and request body." });

            var plant = await _context.Plants.FindAsync(id);
            if (plant == null)
                return NotFound(new { message = $"Plant with ID {id} not found." });

            // Only validate group if one is being assigned
            if (request.GroupId.HasValue)
            {
                var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId.Value);
                if (!groupExists)
                    return BadRequest(new { message = $"Group with ID {request.GroupId} does not exist." });
            }

            plant.Name = request.Name;
            plant.Description = request.Description;
            plant.GroupId = request.GroupId;
            plant.MoistureThreshold = request.MoistureThreshold;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/plants/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlant(int id)
        {
            var plant = await _context.Plants.FindAsync(id);
            if (plant == null)
                return NotFound(new { message = $"Plant with ID {id} not found." });

            _context.Plants.Remove(plant);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}