using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using PlantMonitoringAPI.Models;
using PlantMonitoringAPI.Services;

namespace PlantMonitoringAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailController : ControllerBase
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(ISettingsService settingsService, ILogger<EmailController> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        // POST: api/email/test sends a test email using current mail settings
        [HttpPost("test")]
        public async Task<IActionResult> SendTestEmail()
        {
            var mailEnabled = await _settingsService.GetBoolAsync(SettingKeys.MailEnabled);
            if (!mailEnabled)
                return BadRequest(new { message = "Mail is disabled. Enable it in settings first." });

            var host = await _settingsService.GetAsync(SettingKeys.MailHost);
            var port = await _settingsService.GetIntAsync(SettingKeys.MailPort, 465);
            var username = await _settingsService.GetAsync(SettingKeys.MailUsername);
            var password = await _settingsService.GetAsync(SettingKeys.MailPassword);
            var fromAddress = await _settingsService.GetAsync(SettingKeys.MailFromAddress);
            var fromName = await _settingsService.GetAsync(SettingKeys.MailFromName) ?? "Plant Monitor";
            var toAddress = await _settingsService.GetAsync(SettingKeys.MailToAddress);

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromAddress) ||
                string.IsNullOrWhiteSpace(toAddress))
                return BadRequest(new { message = "Mail settings incomplete. Check host, username, password, from and to addresses." });

            try
            {
                var email = new MimeMessage();
                email.From.Add(new MailboxAddress(fromName, fromAddress));
                email.To.Add(MailboxAddress.Parse(toAddress));
                email.Subject = "[Plant Monitor] Test Email";

                email.Body = new BodyBuilder
                {
                    TextBody =
                        $"This is a test email from Plant Monitor.\n\n" +
                        $"If you received this, your mail settings are configured correctly.\n\n" +
                        $"Sent at: {DateTime.UtcNow:dd MMM yyyy HH:mm} UTC"
                }.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, SecureSocketOptions.SslOnConnect);
                await client.AuthenticateAsync(username, password);
                await client.SendAsync(email);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Test email sent successfully to {ToAddress}", toAddress);
                return Ok(new { message = $"Test email sent to {toAddress}." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test email failed.");
                return StatusCode(503, new { message = $"Failed to send test email: {ex.Message}" });
            }
        }
    }
}