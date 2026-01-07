using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;

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
    }
}
