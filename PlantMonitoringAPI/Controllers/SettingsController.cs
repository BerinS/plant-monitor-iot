using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.DTOs;
using PlantMonitoringAPI.Models;
using PlantMonitoringAPI.Services;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SettingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsController> _logger;

        // When a new setting needs to be added, here, SettingKeys and DB
        private static readonly HashSet<string> _knownKeys = new()
        {
            SettingKeys.MailEnabled,
            SettingKeys.MailHost,
            SettingKeys.MailPort,
            SettingKeys.MailUsername,
            SettingKeys.MailPassword,
            SettingKeys.MailFromAddress,
            SettingKeys.MailFromName,
            SettingKeys.MailToAddress
        };

        public SettingsController(
            AppDbContext context,
            ISettingsService settingsService,
            ILogger<SettingsController> logger)
        {
            _context = context;
            _settingsService = settingsService;
            _logger = logger;
        }

        // GET: api/settings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SettingDto>>> GetAll()
        {
            var settings = await _context.SystemSettings
                .OrderBy(s => s.Key)
                .Select(s => new SettingDto
                {
                    Key = s.Key,
                    Value = s.Value,
                    Description = s.Description,
                    UpdatedAt = s.UpdatedAt
                })
                .ToListAsync();

            return Ok(settings);
        }

        // GET: api/settings/{key}
        [HttpGet("{key}")]
        public async Task<ActionResult<SettingDto>> GetByKey(string key)
        {
            var setting = await _context.SystemSettings.FindAsync(key);

            if (setting == null)
                return NotFound(new { message = $"Setting '{key}' not found." });

            return Ok(new SettingDto
            {
                Key = setting.Key,
                Value = setting.Value,
                Description = setting.Description,
                UpdatedAt = setting.UpdatedAt
            });
        }

        // PUT: api/settings/{key}
        [HttpPut("{key}")]
        public async Task<IActionResult> UpdateSetting(
            string key,
            [FromBody] UpdateSettingDto request)
        {
            if (!_knownKeys.Contains(key))
                return BadRequest(new { message = $"'{key}' is not a recognised setting key." });

            var success = await _settingsService.SetAsync(key, request.Value);

            if (!success)
            {
                _logger.LogError("Failed to persist setting update — key: {Key}", key);
                return StatusCode(503, new { message = "Failed to save setting. Check server logs." });
            }

            return Ok(new { message = $"Setting '{key}' updated successfully." });
        }

        // PUT: api/settings used by the settings page form.
        [HttpPut]
        public async Task<IActionResult> UpdateSettingsBulk([FromBody] UpdateSettingsBulkDto request)
        {
            if (request.Settings == null || !request.Settings.Any())
                return BadRequest(new { message = "No settings provided." });

            var unknownKeys = request.Settings.Keys
                .Where(k => !_knownKeys.Contains(k))
                .ToList();

            if (unknownKeys.Any())
                return BadRequest(new
                {
                    message = "Unknown setting keys provided.",
                    unknownKeys
                });

            var success = await _settingsService.SetManyAsync(request.Settings);

            if (!success)
            {
                _logger.LogError("Failed to persist bulk settings update.");
                return StatusCode(503, new { message = "Failed to save settings. Check server logs." });
            }

            return Ok(new
            {
                message = $"{request.Settings.Count} setting(s) updated successfully.",
                updated = request.Settings.Count
            });
        }
    }
}