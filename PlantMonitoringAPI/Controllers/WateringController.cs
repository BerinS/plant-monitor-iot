using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Services;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WateringController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MqttBackgroundService _mqttService;
        private readonly ILogger<WateringController> _logger;

        // Default duration if caller does not specify one
        private const int DEFAULT_DURATION_SECONDS = 5;

        // Pump running cap, also implemented in the ESP code
        private const int MAX_DURATION_SECONDS = 8;

        public WateringController(AppDbContext context, MqttBackgroundService mqttService, ILogger<WateringController> logger)
        {
            _context = context;
            _mqttService = mqttService;
            _logger = logger;
        }

        [HttpPost("{plantId}")]
        public async Task<IActionResult> TriggerWatering(int plantId, [FromBody] DTOs.WateringRequestDto? request)
        {
            // find plant
            var plant = await _context.Plants
                .Include(p => p.ActiveDevice)
                .FirstOrDefaultAsync(p => p.Id == plantId);

            if (plant == null)
                return NotFound(new { message = $"Plant with ID {plantId} not found." });

            if (plant.ActiveDevice == null)
                return BadRequest(new
                {
                    message = $"Plant '{plant.Name}' has no device assigned. Attach a sensor with a pump before watering."
                });

            var device = plant.ActiveDevice;

            // if there is no body or durationSeconds, use default
            var requestedDuration = request?.DurationSeconds ?? DEFAULT_DURATION_SECONDS;

            if (requestedDuration <= 0)
                return BadRequest(new { message = "Duration must be greater than 0 seconds." });

            // clamp to max duration
            var duration = Math.Min(requestedDuration, MAX_DURATION_SECONDS);

            if (duration < requestedDuration)
            {
                _logger.LogWarning(
                    "Watering request for plant {PlantId} clamped from {Requested}s to {Capped}s",
                    plantId, requestedDuration, duration);
            }

            //  send MQTT command 
            var command = new
            {
                action = "activate_pump",
                duration_seconds = duration
            };

            var sent = await _mqttService.SendCommandAsync(device.Id, command);

            if (!sent)
            {
                _logger.LogWarning(
                    "Watering command failed for plant {PlantId} — device {DeviceId} unreachable",
                    plantId, device.Id);

                return StatusCode(503, new
                {
                    message = "Could not reach the device. The broker may be offline or the device is disconnected."
                });
            }

            _logger.LogInformation(
                "Watering triggered for plant {PlantId} via device {DeviceId} for {Duration}s",
                plantId, device.Id, duration);

            return Ok(new
            {
                message = $"Watering started for '{plant.Name}'.",
                plantId = plant.Id,
                deviceId = device.Id,
                durationSeconds = duration
            });
        }
    }
}

