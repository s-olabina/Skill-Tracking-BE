using System.Net;
using System.Net.Mail;
using SkillTrackingApp.DTOs;

namespace SkillTrackingApp.Services;

public interface IEmailService
{
    Task<bool> SendSkillSummaryEmailAsync(string toEmail, string userName, SkillSummaryDto summary);
    Task<bool> SendReminderEmailAsync(string toEmail, string userName, DateTime lastUpdateDate);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendSkillSummaryEmailAsync(string toEmail, string userName, SkillSummaryDto summary)
    {
        var subject = "Your Skills Summary - Skill Tracking App";
        
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; }}
                    .content {{ background-color: #f9f9f9; padding: 20px; }}
                    .stats {{ display: flex; justify-content: space-around; margin: 20px 0; }}
                    .stat-box {{ text-align: center; padding: 15px; background: white; border-radius: 5px; }}
                    .stat-number {{ font-size: 24px; font-weight: bold; color: #4CAF50; }}
                    .skill-list {{ margin-top: 20px; }}
                    .skill-item {{ background: white; margin: 10px 0; padding: 10px; border-left: 3px solid #4CAF50; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Skills Summary</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        <p>Here's your current skills summary:</p>
                        
                        <div class='stats'>
                            <div class='stat-box'>
                                <div class='stat-number'>{summary.TotalSkills}</div>
                                <div>Total Skills</div>
                            </div>
                        </div>

                        <h3>Skills by Category:</h3>
                        <ul>
                            {string.Join("", summary.ByCategory.Select(c => $"<li><strong>{c.Key}</strong>: {c.Value} skills</li>"))}
                        </ul>

                        <h3>Skills by Level:</h3>
                        <ul>
                            {string.Join("", summary.ByLevel.Select(l => $"<li><strong>{l.Key}</strong>: {l.Value} skills</li>"))}
                        </ul>

                        <h3>Recently Updated:</h3>
                        <div class='skill-list'>
                            {string.Join("", summary.RecentlyUpdated.Select(s => 
                                $"<div class='skill-item'><strong>{s.Name}</strong> ({s.Category}) - {s.LevelName}<br><small>Updated: {s.LastUpdated:yyyy-MM-dd}</small></div>"))}
                        </div>

                        <p style='margin-top: 20px;'>
                            <a href='http://localhost:3000/dashboard' style='background-color: #4CAF50; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                View Your Dashboard
                            </a>
                        </p>
                    </div>
                    <div class='footer'>
                        <p>You're receiving this email because you have notifications enabled in Skill Tracking App.</p>
                        <p>To stop receiving these emails, update your preferences in your profile settings.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<bool> SendReminderEmailAsync(string toEmail, string userName, DateTime lastUpdateDate)
    {
        var daysSinceUpdate = (DateTime.UtcNow - lastUpdateDate).Days;
        var subject = "Time to Update Your Skills! - Skill Tracking App";
        
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #FF9800; color: white; padding: 20px; text-align: center; }}
                    .content {{ background-color: #f9f9f9; padding: 20px; }}
                    .reminder-box {{ background: white; padding: 20px; margin: 20px 0; border-left: 4px solid #FF9800; }}
                    .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>⏰ Skill Update Reminder</h1>
                    </div>
                    <div class='content'>
                        <p>Hi {userName},</p>
                        
                        <div class='reminder-box'>
                            <p>It's been <strong>{daysSinceUpdate} days</strong> since you last updated your skills!</p>
                            <p>Keeping your skills up-to-date helps you:</p>
                            <ul>
                                <li>Track your learning progress</li>
                                <li>Stay motivated on your development journey</li>
                                <li>Identify areas for growth</li>
                            </ul>
                        </div>

                        <p>Take a moment to review and update your skills today!</p>

                        <p style='margin-top: 20px;'>
                            <a href='http://localhost:3000/dashboard' style='background-color: #FF9800; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;'>
                                Update My Skills
                            </a>
                        </p>
                    </div>
                    <div class='footer'>
                        <p>You're receiving this email because you have notifications enabled in Skill Tracking App.</p>
                        <p>To stop receiving these emails, update your preferences in your profile settings.</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body);
    }

    private async Task<bool> SendEmailAsync(string toEmail, string subject, string body)
    {
        try
        {
            var smtpServer = _configuration["Email:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var senderEmail = _configuration["Email:SenderEmail"];
            var senderPassword = _configuration["Email:SenderPassword"];
            var senderName = _configuration["Email:SenderName"] ?? "Skill Tracking App";
            var enableSsl = bool.Parse(_configuration["Email:EnableSsl"] ?? "true");

            if (string.IsNullOrEmpty(senderEmail) || string.IsNullOrEmpty(senderPassword))
            {
                _logger.LogWarning("Email configuration is missing. Email not sent.");
                return false;
            }

            using var smtpClient = new SmtpClient(smtpServer, smtpPort)
            {
                EnableSsl = enableSsl,
                Credentials = new NetworkCredential(senderEmail, senderPassword)
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail, senderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {toEmail}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to send email: {ex.Message}");
            return false;
        }
    }
}
