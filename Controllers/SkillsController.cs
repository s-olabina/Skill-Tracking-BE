using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillTrackingApp.DTOs;
using SkillTrackingApp.Models;
using SkillTrackingApp.Services;
using System.Security.Claims;

namespace SkillTrackingApp.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly IEmailService _emailService;
    private readonly ILogger<SkillsController> _logger;

    public SkillsController(
        ISkillService skillService,
        IEmailService emailService,
        ILogger<SkillsController> logger)
    {
        _skillService = skillService;
        _emailService = emailService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllSkills()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetAllSkillsAsync(userId.Value);
        return Ok(skills);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetSkillById(int id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.GetSkillByIdAsync(id, userId.Value);
        if (skill == null)
        {
            return NotFound();
        }

        return Ok(skill);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSkill([FromBody] CreateSkillDto createSkillDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.CreateSkillAsync(createSkillDto, userId.Value);
        return CreatedAtAction(nameof(GetSkillById), new { id = skill!.Id }, skill);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSkill(int id, [FromBody] UpdateSkillDto updateSkillDto)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skill = await _skillService.UpdateSkillAsync(id, updateSkillDto, userId.Value);
        if (skill == null)
        {
            return NotFound();
        }

        return Ok(skill);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteSkill(int id)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var result = await _skillService.DeleteSkillAsync(id, userId.Value);
        if (!result)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSkillSummary()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var summary = await _skillService.GetSkillSummaryAsync(userId.Value);
        return Ok(summary);
    }

    [HttpGet("category/{category}")]
    public async Task<IActionResult> GetSkillsByCategory(string category)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetSkillsByCategoryAsync(category, userId.Value);
        return Ok(skills);
    }

    [HttpGet("level/{level}")]
    public async Task<IActionResult> GetSkillsByLevel(SkillLevel level)
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        var skills = await _skillService.GetSkillsByLevelAsync(level, userId.Value);
        return Ok(skills);
    }

    /// <summary>
    /// Send skills report via email to the authenticated user
    /// </summary>
    [HttpPost("send-report")]
    public async Task<IActionResult> SendSkillsReport()
    {
        var userId = GetUserIdFromClaims();
        if (userId == null)
        {
            return Unauthorized();
        }

        try
        {
            // Get user email and name from claims
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var userName = User.FindFirst(ClaimTypes.Name)?.Value ?? "User";

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("User email not found");
            }

            _logger.LogInformation($"Sending skills report to user {userId} at {userEmail}");

            // Get skills summary
            var summary = await _skillService.GetSkillSummaryAsync(userId.Value);

            if (summary.TotalSkills == 0)
            {
                return BadRequest("You don't have any skills to send in the report");
            }

            // Send email
            var emailSent = await _emailService.SendSkillSummaryEmailAsync(
                userEmail,
                userName,
                summary
            );

            if (emailSent)
            {
                _logger.LogInformation($"✅ Skills report sent successfully to {userEmail}");
                return Ok(new { message = "Skills report sent successfully! Check your email." });
            }
            else
            {
                _logger.LogWarning($"⚠️ Failed to send skills report to {userEmail}");
                return StatusCode(500, "Failed to send email. Please check email configuration.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ Error sending skills report to user {userId}");
            return StatusCode(500, $"Error sending skills report: {ex.Message}");
        }
    }

    private int? GetUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        return null;
    }
}