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

        // POST: api/sensor/data
        [HttpPost("data")]
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

            //  mapping DTO to model
            var dataPoint = new SensorData
            {
                PlantId = device.CurrentPlantId.Value, 
                MoistureValue = request.Value,
                MeasuredAt = DateTime.UtcNow 
            };

            _context.SensorData.Add(dataPoint);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Data saved", id = dataPoint.Id });
        }

        // POST: api/sensor - make new sensor
        [HttpPost]
        [HttpPost]
        public async Task<IActionResult> CreateSensor([FromBody] SensorDto request)
        {
            if (!string.IsNullOrEmpty(request.MacAddress))
            {
                var existingDevice = await _context.Devices
                    .FirstOrDefaultAsync(d => d.MacAddress == request.MacAddress);

                if (existingDevice != null)
                {
                    return BadRequest(new { message = "A sensor with this MAC Address already exists." });
                }
            }

            var newDevice = new Device
            {
                Name = request.Name,
                MacAddress = request.MacAddress,
                Description = request.Description,
                CurrentPlantId = request.CurrentPlantId,
                GroupId = request.GroupId,
                ApiToken = Guid.NewGuid()
            };

            _context.Devices.Add(newDevice);
            await _context.SaveChangesAsync();

            var createdSensorDto = new SensorDto
            {
                Id = newDevice.Id,
                Name = newDevice.Name,
                MacAddress = newDevice.MacAddress,
                Description = newDevice.Description,
                CurrentPlantId = newDevice.CurrentPlantId,
                GroupId = newDevice.GroupId,
                ApiToken = newDevice.ApiToken
            };

            return Ok(createdSensorDto);
        }

        // GET: api/sensor
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorDto>>> GetAllSensors()
        {
            var sensors = await _context.Devices
                .Select(d => new SensorDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    MacAddress = d.MacAddress,
                    ApiToken = d.ApiToken,
                    CurrentPlantId = d.CurrentPlantId,
                    GroupId = d.GroupId,
                    Description = d.Description
                })
                .ToListAsync();

            return Ok(sensors);
        }

        [HttpGet("{plantId}/history")]
        public async Task<ActionResult> GetHistory(int plantId, [FromQuery] int hours = 24)
        {
            var plant = await _context.Plants.FindAsync(plantId);
            if (plant == null)
            {
                return NotFound(new { message = "Plant not found" });
            }

            // calculate cutoff time     
            var cutoffTime = DateTime.UtcNow.AddHours(-hours);

            var history = await _context.SensorData
                .Where(s => s.PlantId == plantId && s.MeasuredAt >= cutoffTime) // filter by time
                .OrderByDescending(s => s.MeasuredAt) 
                .Select(s => new SensorHistoryDto
                {
                    Value = s.MoistureValue,
                    Time = s.MeasuredAt
                })
                .ToListAsync();

            return Ok(history);
        }

        // get the latest sensor reading for a plant        
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

        // used for system health and determining if a sensor is offline
        [HttpGet("health")]
        public async Task<ActionResult<IEnumerable<DeviceHealthDto>>> GetSystemHealth()
        {
            var devices = await _context.Devices
                .Select(d => new DeviceHealthDto
                {
                    Id = d.Id,
                    MacAddress = d.MacAddress,

                    AssignedPlant = d.Plant != null ? d.Plant.Name : "Idle",

                    LastContact = d.Plant != null
                ? d.Plant.SensorReadings
                    .OrderByDescending(r => r.MeasuredAt)
                    .Select(r => (DateTime?)r.MeasuredAt)
                    .FirstOrDefault()
                : null
                })
                .ToListAsync();

            return Ok(devices);
        }

        // PUT: api/sensor/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSensor(int id, [FromBody] SensorDto request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound(new { message = "Sensor not found" });
            }

            if (request.CurrentPlantId.HasValue)
            {
                var plantExists = await _context.Plants.AnyAsync(p => p.Id == request.CurrentPlantId.Value);
                if (!plantExists)
                {
                    return BadRequest(new { message = $"Cannot assign sensor: Plant with ID {request.CurrentPlantId.Value} does not exist." });
                }
            }

            if (request.GroupId.HasValue)
            {
                var groupExists = await _context.Groups.AnyAsync(g => g.Id == request.GroupId.Value);
                if (!groupExists)
                {
                    return BadRequest(new { message = $"Cannot assign sensor: Group with ID {request.GroupId.Value} does not exist." });
                }
            }

            // Update fields
            device.Name = request.Name;
            device.MacAddress = request.MacAddress;
            device.Description = request.Description;
            device.CurrentPlantId = request.CurrentPlantId;
            device.GroupId = request.GroupId;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Sensor updated successfully" });
        }

        // DELETE: api/sensor/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSensor(int id)
        {
            var device = await _context.Devices.FindAsync(id);
            if (device == null)
            {
                return NotFound(new { message = "Sensor not found" });
            }

            _context.Devices.Remove(device);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
