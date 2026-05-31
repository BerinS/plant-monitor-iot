using Microsoft.EntityFrameworkCore;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Models;

namespace PlantMonitoringAPI.Services
{
    public interface ISettingsService
    {
        // Returns value for a key or null if not set
        Task<string?> GetAsync(string key);

        // helpers
        Task<bool> GetBoolAsync(string key, bool defaultValue = false);
        Task<int> GetIntAsync(string key, int defaultValue = 0);

        // Updates a single value, writes to DB, reloads cache, returns false if the save failed
        Task<bool> SetAsync(string key, string? value);

        // Updates multiple values in one call, returns false if save failed
        Task<bool> SetManyAsync(Dictionary<string, string?> settings);

        // Returns all settings
        Task<IReadOnlyDictionary<string, string?>> GetAllAsync();
    }

    public class SettingsService : ISettingsService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<SettingsService> _logger;

        // cache populated on first access via EnsureLoadedAsync
        private Dictionary<string, string?> _cache = new();

        // SemaphoreSlim(1,1) as a mutex so only one thread can enter the critical section at a time
        private readonly SemaphoreSlim _lock = new(1, 1);

        // volatile ensures every thread reads the current value from memory rather than a cached register copy 
        private volatile bool _loaded = false;

        public SettingsService(
            IServiceScopeFactory scopeFactory,
            ILogger<SettingsService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<string?> GetAsync(string key)
        {
            await EnsureLoadedAsync();
            _cache.TryGetValue(key, out var value);
            return value;
        }

        public async Task<bool> GetBoolAsync(string key, bool defaultValue = false)
        {
            var value = await GetAsync(key);
            if (string.IsNullOrWhiteSpace(value)) return defaultValue;
            return value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<int> GetIntAsync(string key, int defaultValue = 0)
        {
            var value = await GetAsync(key);
            return int.TryParse(value, out var result) ? result : defaultValue;
        }

        public async Task<bool> SetAsync(string key, string? value)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var setting = await context.SystemSettings.FindAsync(key);

                if (setting == null)
                {
                    _logger.LogWarning("Attempted to update unknown setting key: {Key}", key);
                    return false;
                }

                setting.Value = value;
                setting.UpdatedAt = DateTime.UtcNow;

                await context.SaveChangesAsync();
                _logger.LogInformation("Setting updated — key: {Key}", key);

                // reload cache after write
                _loaded = false;
                await EnsureLoadedAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update setting — key: {Key}", key);
                return false;
            }
        }

        public async Task<bool> SetManyAsync(Dictionary<string, string?> settings)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                foreach (var (key, value) in settings)
                {
                    var setting = await context.SystemSettings.FindAsync(key);

                    if (setting == null)
                    {
                        _logger.LogWarning("Attempted to update unknown setting key: {Key}", key);
                        continue;
                    }

                    setting.Value = value;
                    setting.UpdatedAt = DateTime.UtcNow;
                }

                // One SaveChangesAsync for all changes 
                await context.SaveChangesAsync();
                _logger.LogInformation(
                    "Bulk settings update — {Count} settings saved.", settings.Count);

                // One cache reload for all changes
                _loaded = false;
                await EnsureLoadedAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to bulk update settings.");
                return false;
            }
        }

        public async Task<IReadOnlyDictionary<string, string?>> GetAllAsync()
        {
            await EnsureLoadedAsync();

            // Return a copy of live cache
            return new Dictionary<string, string?>(_cache).AsReadOnly();
        }

        // Double checked locking pattern:
        // First check: no lock overhead on every read after load
        // Second check inside lock prevents duplicate loads when multiple threads pass the first check simultaneously on a cold cache
        private async Task EnsureLoadedAsync()
        {
            if (_loaded) return;

            await _lock.WaitAsync();
            try
            {
                if (_loaded) return;

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var settings = await context.SystemSettings.ToListAsync();
                _cache = settings.ToDictionary(s => s.Key, s => s.Value);
                _loaded = true;

                _logger.LogInformation(
                    "Settings cache loaded — {Count} settings.", _cache.Count);
            }
            finally
            {
                _lock.Release();
            }
        }
    }
}