using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using PlantMonitoringAPI.Data;
using PlantMonitoringAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace PlantMonitoringAPI.Services
{
    public class EmailBackgroundService : BackgroundService
    {
        private readonly ILogger<EmailBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ISettingsService _settingsService;

        // Poll every 5 minutes
        private static readonly TimeSpan _pollInterval = TimeSpan.FromMinutes(5);

        public EmailBackgroundService(
            ILogger<EmailBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            ISettingsService settingsService)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settingsService = settingsService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Email background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessPendingEmailsAsync(stoppingToken);
                }
                catch (Exception ex)
                {                  
                    _logger.LogError(ex, "Unexpected error in email background service.");
                }

                await Task.Delay(_pollInterval, stoppingToken);
            }

            _logger.LogInformation("Email background service stopped.");
        }

        private async Task ProcessPendingEmailsAsync(CancellationToken stoppingToken)
        {
            // Check if mail is enabled
            var mailEnabled = await _settingsService.GetBoolAsync(SettingKeys.MailEnabled);
            if (!mailEnabled)
            {
                _logger.LogDebug("Mail is disabled — skipping email cycle.");
                return;
            }

            var host = await _settingsService.GetAsync(SettingKeys.MailHost);
            var port = await _settingsService.GetIntAsync(SettingKeys.MailPort, 465);
            var username = await _settingsService.GetAsync(SettingKeys.MailUsername);
            var password = await _settingsService.GetAsync(SettingKeys.MailPassword);
            var fromAddress = await _settingsService.GetAsync(SettingKeys.MailFromAddress);
            var fromName = await _settingsService.GetAsync(SettingKeys.MailFromName) ?? "Plant Monitor";
            var toAddress = await _settingsService.GetAsync(SettingKeys.MailToAddress);

            // Validate required settings are configured
            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromAddress) ||
                string.IsNullOrWhiteSpace(toAddress))
            {
                _logger.LogWarning("Mail settings error " +
                    "Check host, username, password, from and to addresses.");
                return;
            }

            // Fetch unsent notifications
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var pending = await context.Notifications
                .Where(n => !n.SentEmail)
                .OrderBy(n => n.CreatedAt)
                .ToListAsync(stoppingToken);

            if (!pending.Any())
            {
                _logger.LogDebug("No pending notifications to email.");
                return;
            }

            _logger.LogInformation(
                "Found {Count} unsent notification(s) — attempting to send.", pending.Count);

            // Oone email per notification, marks sent individually.  
            foreach (var notification in pending)
            {
                if (stoppingToken.IsCancellationRequested) break;

                var sent = await SendEmailAsync(
                    notification,
                    host, port, username, password,
                    fromAddress, fromName, toAddress);

                if (sent)
                {
                    notification.SentEmail = true;
                    await context.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation(
                        "Email sent for notification {Id} — plant: {PlantName}",
                        notification.Id, notification.PlantName ?? "unknown");
                }
                else
                {
                    // Log and continue 
                    _logger.LogWarning(
                        "Failed to send email for notification {Id} — will retry next cycle.",
                        notification.Id);
                }
            }
        }

        private async Task<bool> SendEmailAsync(
            Notification notification,
            string host, int port, string username, string password,
            string fromAddress, string fromName, string toAddress)
        {
            try
            {
                // MimeKit email build
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromAddress));
                email.To.Add(MailboxAddress.Parse(toAddress));
                email.Subject = $"[Plant Monitor] {notification.Title}";

                // Plain text body 
                var bodyBuilder = new BodyBuilder
                {
                    TextBody = BuildEmailBody(notification)
                };
                email.Body = bodyBuilder.ToMessageBody();

                // MailKit send
                using var client = new SmtpClient();

                // SslOnConnect = implicit TLS (port 465), STARTTLS negotiation
                await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to send email for notification {NotificationId}",
                    notification.Id);
                return false;
            }
        }

        private static string BuildEmailBody(Notification notification)
        {
            var lines = new List<string>
            {
                $"Plant Monitor Notification",
                $"",
                $"Severity:  {notification.Severity.ToUpper()}",
                $"Title:     {notification.Title}",
            };

            if (!string.IsNullOrWhiteSpace(notification.PlantName))
                lines.Add($"Plant:     {notification.PlantName}");

            lines.Add($"");
            lines.Add(notification.Message);
            lines.Add($"");
            lines.Add($"Time:      {notification.CreatedAt:dd MMM yyyy HH:mm} UTC");
            lines.Add($"");
            lines.Add($"---");
            lines.Add($"This message was sent automatically by your Plant Monitor system.");

            return string.Join(Environment.NewLine, lines);
        }
    }
}