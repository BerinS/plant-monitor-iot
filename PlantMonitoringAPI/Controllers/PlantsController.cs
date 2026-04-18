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
            {
                return NotFound();
            }

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
                .ToListAsync();

            return Ok(plants);
        }

        [HttpPost]
        public async Task<IActionResult> PostPlant([FromBody] CreatePlantDto request) 
        {
            // group check
            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId);
            if (!groupExists)
            {
                return BadRequest(new { message = $"Group ID {request.GroupId} does not exist" });
            }

            var plant = new Plant
            {
                Name = request.Name,
                Description = request.Description,
                GroupId = request.GroupId,
                CreatedAt = DateTime.UtcNow 
            };

            _context.Plants.Add(plant);
            await _context.SaveChangesAsync();

            // output response DTO

            var group = await _context.Groups.FindAsync(plant.GroupId);

            var responseDto = new PlantDto
            {
                Id = plant.Id,
                Name = plant.Name,
                Description = plant.Description,
                CreatedAt = plant.CreatedAt,
                GroupName = group?.Name ?? "No Group"
            };

            return CreatedAtAction(nameof(GetPlant), new { id = plant.Id }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutPlant(int id, [FromBody] UpdatePlantDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("Mismatch in URL and body");
            }

            var plant = await _context.Plants.FindAsync(id);
            if (plant == null) {
                return NotFound($"Plant with ID {id} not found");
            }

            var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId);
            if (!groupExists)
            {
                return BadRequest("Group does not exist");
            }

            plant.Name = request.Name;
            plant.Description = request.Description;
            plant.GroupId = request.GroupId;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlant(int id)
        {
            var plant = await _context.Plants.FindAsync(id);

            if (plant == null)
            {
                return NotFound(new { message = $"Plant with ID {id} not found" });
            }

            _context.Plants.Remove(plant);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
