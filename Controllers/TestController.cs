using Microsoft.AspNetCore.Mvc;
using SkillTrackingApp.Services;

namespace SkillTrackingApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<TestController> _logger;

        public TestController(IEmailService emailService, ILogger<TestController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Sends a test email to verify email configuration
        /// </summary>
        /// <param name="toEmail">Email address to send test email to</param>
        /// <returns>Success or error message</returns>
        [HttpPost("send-test-email")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string toEmail)
        {
            if (string.IsNullOrEmpty(toEmail))
            {
                return BadRequest("Email address is required");
            }

            // Basic email validation
            if (!toEmail.Contains("@") || !toEmail.Contains("."))
            {
                return BadRequest("Invalid email address format");
            }

            try
            {
                _logger.LogInformation($"Attempting to send test email to: {toEmail}");

                var subject = "Test Email - Skill Tracking App";

                var htmlBody = $@"
                <!DOCTYPE html>
                <html>
                <head>
                    <style>
                        body {{
                            font-family: Arial, sans-serif;
                            line-height: 1.6;
                            color: #333;
                        }}
                        .container {{
                            max-width: 600px;
                            margin: 0 auto;
                            padding: 20px;
                        }}
                        .header {{
                            background-color: #4299e1;
                            color: white;
                            padding: 20px;
                            text-align: center;
                            border-radius: 8px 8px 0 0;
                        }}
                        .content {{
                            background-color: #f7fafc;
                            padding: 30px;
                            border: 1px solid #e2e8f0;
                        }}
                        .success-box {{
                            background-color: #c6f6d5;
                            border-left: 4px solid #48bb78;
                            padding: 15px;
                            margin: 20px 0;
                        }}
                        .footer {{
                            background-color: #edf2f7;
                            padding: 15px;
                            text-align: center;
                            font-size: 12px;
                            color: #718096;
                            border-radius: 0 0 8px 8px;
                        }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h1>📧 Test Email</h1>
                        </div>
                        <div class='content'>
                            <h2>Email Service Test</h2>
                            <p>This is a <strong>test email</strong> from the Skill Tracking Application.</p>
                            
                            <div class='success-box'>
                                <strong>✅ Success!</strong><br>
                                If you're reading this, your email service is configured correctly!
                            </div>
                            
                            <p><strong>Configuration Details:</strong></p>
                            <ul>
                                <li>SMTP Server: Configured</li>
                                <li>SSL/TLS: Enabled</li>
                                <li>Email Service: Active</li>
                            </ul>
                            
                            <p>You can now use the email functionality in your Skill Tracking application.</p>
                        </div>
                        <div class='footer'>
                            <p>Sent on: {DateTime.Now:yyyy-MM-dd HH:mm:ss}</p>
                            <p>Skill Tracking Application © 2025</p>
                        </div>
                    </div>
                </body>
                </html>";

                // Koristi postojeći SendEmailAsync metod (koji prima 3 parametra)
                var success = await _emailService.SendEmailAsync(toEmail, subject, htmlBody);

                if (success)
                {
                    _logger.LogInformation($"✅ Test email sent successfully to: {toEmail}");
                    return Ok("Email sent successfully! Check your inbox (and spam folder).");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Email service returned false for: {toEmail}");
                    return BadRequest("Failed to send email. Check email configuration in appsettings.json");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"❌ Error sending test email to: {toEmail}");
                return BadRequest($"Failed to send email: {ex.Message}");
            }
        }

        /// <summary>
        /// Gets the current email configuration status (without exposing sensitive data)
        /// </summary>
        [HttpGet("email-config-status")]
        public IActionResult GetEmailConfigStatus()
        {
            try
            {
                // This just checks if the service is available, doesn't expose config
                var isConfigured = _emailService != null;

                return Ok(new
                {
                    isConfigured = isConfigured,
                    message = isConfigured
                        ? "Email service is configured and ready"
                        : "Email service is not configured"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email configuration");
                return StatusCode(500, "Error checking email configuration");
            }
        }
    }
}