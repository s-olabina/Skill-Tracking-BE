using Microsoft.EntityFrameworkCore;
using SkillTrackingApp.Data;

namespace SkillTrackingApp.Services;

public class EmailNotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmailNotificationService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromHours(1); // Check every hour

    public EmailNotificationService(
        IServiceProvider serviceProvider,
        ILogger<EmailNotificationService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Email Notification Service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessEmailNotificationsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in Email Notification Service: {ex.Message}");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Email Notification Service stopped.");
    }

    private async Task ProcessEmailNotificationsAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
        var skillService = scope.ServiceProvider.GetRequiredService<ISkillService>();

        var now = DateTime.UtcNow;
        var dayOfWeek = now.DayOfWeek;

        // Send weekly summaries on Monday at 9 AM (configurable)
        if (dayOfWeek == DayOfWeek.Monday && now.Hour == 9)
        {
            await SendWeeklySummariesAsync(context, emailService, skillService);
        }

        // Send reminders for users who haven't updated skills in 30 days
        await SendInactivityRemindersAsync(context, emailService);
    }

    private async Task SendWeeklySummariesAsync(
        ApplicationDbContext context,
        IEmailService emailService,
        ISkillService skillService)
    {
        var usersWithNotifications = await context.Users
            .Where(u => u.EmailNotificationsEnabled)
            .ToListAsync();

        _logger.LogInformation($"Sending weekly summaries to {usersWithNotifications.Count} users");

        foreach (var user in usersWithNotifications)
        {
            try
            {
                var summary = await skillService.GetSkillSummaryAsync(user.Id);
                
                if (summary.TotalSkills > 0)
                {
                    await emailService.SendSkillSummaryEmailAsync(
                        user.Email,
                        $"{user.FirstName} {user.LastName}",
                        summary);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send summary email to {user.Email}: {ex.Message}");
            }
        }
    }

    private async Task SendInactivityRemindersAsync(
        ApplicationDbContext context,
        IEmailService emailService)
    {
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

        var inactiveUsers = await context.Users
            .Where(u => u.EmailNotificationsEnabled)
            .Where(u => u.Skills.Any()) // Has at least one skill
            .Where(u => !u.Skills.Any(s => s.LastUpdated > thirtyDaysAgo)) // No updates in 30 days
            .ToListAsync();

        _logger.LogInformation($"Sending inactivity reminders to {inactiveUsers.Count} users");

        foreach (var user in inactiveUsers)
        {
            try
            {
                var lastUpdate = await context.Skills
                    .Where(s => s.UserId == user.Id)
                    .MaxAsync(s => s.LastUpdated);

                await emailService.SendReminderEmailAsync(
                    user.Email,
                    $"{user.FirstName} {user.LastName}",
                    lastUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send reminder email to {user.Email}: {ex.Message}");
            }
        }
    }
}
