using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SensorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> PostReading([FromBody] CreateSensorDataDto request)
        {
            // security check via valid api token
            var device = await _context.Devices
                .FirstOrDefaultAsync(d => d.ApiToken == request.Token);

            if (device == null)
            {
                return Unauthorized(new { message = "Invalid API Token" });
            }

            if (device.CurrentPlantId == null)
            {
                return BadRequest(new { message = "Device is not assigned to any plant" });
            }

            //  mapping the DTO to model
            var dataPoint = new SensorData
            {
                PlantId = device.CurrentPlantId.Value, 
                MoistureValue = request.Value,
                MeasuredAt = DateTime.UtcNow // server time
            };

            _context.SensorData.Add(dataPoint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data saved", id = dataPoint.Id });
        }

        [HttpGet("{plantId}/history")]
        public async Task<ActionResult> GetHistory(int plantId)
        {
            var plant = await _context.Plants.FindAsync(plantId);
            if (plant == null)
            {
                return NotFound(new { message = "Plant not found" });
            }

            var history = await _context.SensorData
                .Where(s => s.PlantId == plantId)
                .OrderByDescending(s => s.MeasuredAt)
                .Take(50)
                .Select(s => new SensorHistoryDto
                {
                    Value = s.MoistureValue,
                    Time = s.MeasuredAt
                })
                .ToListAsync();

            return Ok(history);
        }

        [HttpGet("{plantId}/latest")]
        public async Task<IActionResult> GetLatest(int plantId) 
        { 
            var reading = await _context.SensorData
                .Where(s => s.PlantId == plantId)
                .OrderByDescending(s => s.MeasuredAt)
                .Select(s => new SensorHistoryDto
                {
                    Value = s.MoistureValue,
                    Time = s.MeasuredAt
                })
                .FirstOrDefaultAsync();

            if (reading == null)
            {
                return NotFound(new { message = "No data found for this plant" });
            }

            return Ok(reading);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReading(long id)
        {
            var reading = await _context.SensorData.FindAsync(id);

            if (reading == null)
            {
                return NotFound();
            }

            _context.SensorData.Remove(reading);
            await _context.SaveChangesAsync();

            return NoContent(); 
        }
    }
}
